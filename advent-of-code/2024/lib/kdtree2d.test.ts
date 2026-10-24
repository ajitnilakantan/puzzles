import { beforeEach, describe, expect, test } from "bun:test";
import type { Point2D } from "./kdtree2d";
import { KDTree2D, ManhattanMetric } from "./kdtree2d";

// Adjust path to file location

describe("KDTree2D Tests", () => {
  let initialPoints: Point2D[];

  beforeEach(() => {
    // Standard test dataset
    initialPoints = [
      { x: 2, y: 3 },
      { x: 5, y: 4 },
      { x: 9, y: 6 },
      { x: 4, y: 7 },
      { x: 8, y: 1 },
      { x: 7, y: 2 },
    ];
  });

  test("should build an empty tree without errors", () => {
    const tree = new KDTree2D([]);
    expect(tree.root).toBeNull();
    expect(tree.nearest({ x: 0, y: 0 })).toBeNull();
    expect(tree.withinRange({ x: 0, y: 0 }, 10)).toEqual([]);
  });

  test("should find the exact nearest neighbor (Euclidean)", () => {
    const tree = new KDTree2D(initialPoints);

    // Closer to (2,3)
    const result1 = tree.nearest({ x: 2, y: 2 });
    expect(result1).toEqual({ x: 2, y: 3 });

    // Closer to (9,6)
    const result2 = tree.nearest({ x: 9, y: 5 });
    expect(result2).toEqual({ x: 9, y: 6 });
  });

  test("should find all points within range (Euclidean)", () => {
    const tree = new KDTree2D(initialPoints);

    // Circle centered at (5,4) with radius 2.5
    // Should catch (5,4) itself and (7,2) because dist is sqrt(4+4) = 2.82 (Wait, radius 2.5 means distSq <= 6.25)
    // Let's test target (6,3) radius 2:
    // (5,4) distSq = 1+1 = 2 <= 4 (Yes)
    // (7,2) distSq = 1+1 = 2 <= 4 (Yes)
    // (2,3) distSq = 16+0 = 16 > 4 (No)
    const results = tree.withinRange({ x: 6, y: 3 }, 2);

    expect(results).toHaveLength(2);
    expect(results).toContainEqual({ x: 5, y: 4 });
    expect(results).toContainEqual({ x: 7, y: 2 });
  });

  test("should work with Manhattan distance metric", () => {
    const tree = new KDTree2D(initialPoints, ManhattanMetric);

    // Manhattan target (4,6) max distance 2
    // (4,7) -> |4-4| + |7-6| = 1 <= 2 (Yes)
    // (5,4) -> |5-4| + |4-6| = 3 > 2 (No)
    const results = tree.withinRange({ x: 4, y: 6 }, 2);

    expect(results).toEqual([{ x: 4, y: 7 }]);
  });

  test("should support dynamic node insertions", () => {
    const tree = new KDTree2D(initialPoints);

    // Initially, this area is sparse
    expect(tree.nearest({ x: 100, y: 100 })).toEqual({ x: 9, y: 6 });

    // Insert a deep outlier point
    tree.insert({ x: 100, y: 100 });

    expect(tree.nearest({ x: 101, y: 101 })).toEqual({ x: 100, y: 100 });
  });

  test("should support deleting leaf and internal nodes", () => {
    const tree = new KDTree2D(initialPoints);

    // Delete a point
    tree.delete({ x: 9, y: 6 });

    // Verify it can no longer be evaluated or returned
    const results = tree.withinRange({ x: 9, y: 6 }, 0.1);
    expect(results).toEqual([]);

    // Closest should fallback to next nearest
    expect(tree.nearest({ x: 9, y: 6 })).toEqual({ x: 7, y: 2 });
  });

  test("should automatically fire rebalance after threshold is breached", () => {
    // Set a very low threshold to force a rebuild quickly
    const tree = new KDTree2D([{ x: 1, y: 1 }], undefined, 3);

    // Counter sits at 0.
    tree.insert({ x: 2, y: 2 }); // Counter = 1
    tree.insert({ x: 3, y: 3 }); // Counter = 2

    // Third operation hits threshold = 3, causing a deep median-split restructuring
    tree.insert({ x: 4, y: 4 });

    // Verify structural integrity still handles searches perfectly after the hot rebuild
    expect(tree.nearest({ x: 3.9, y: 3.9 })).toEqual({ x: 4, y: 4 });
    expect(tree.withinRange({ x: 2, y: 2 }, 0)).toEqual([{ x: 2, y: 2 }]);
  });
});
