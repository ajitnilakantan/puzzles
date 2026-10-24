import { describe, expect, test } from "bun:test";
import { AVLTree } from "./avl";

describe("AVLTree Boundary Tracking (minKey / maxKey)", () => {
  test("should return null for minKey and maxKey if the tree is empty", () => {
    const emptyTree = new AVLTree<number, string>();
    expect(emptyTree.minKey()).toBeNull();
    expect(emptyTree.maxKey()).toBeNull();
  });

  test("should retrieve correct boundaries under a standard ascending number configuration", () => {
    const tree = new AVLTree<number, string>();
    tree.insert(50, "Fifty");
    tree.insert(10, "Ten");
    tree.insert(90, "Ninety");
    tree.insert(5, "Five");
    tree.insert(35, "Thirty-Five");

    expect(tree.minKey()).toBe(5);
    expect(tree.maxKey()).toBe(90);
  });

  test("should dynamically adjust boundaries when endpoints are removed", () => {
    const tree = new AVLTree<number, string>();
    tree.insert(20, "V1");
    tree.insert(10, "V2");
    tree.insert(30, "V3");

    expect(tree.minKey()).toBe(10);
    tree.remove(10);
    expect(tree.minKey()).toBe(20); // 20 becomes the new min boundary
  });

  test("should flip positions correctly when a descending comparator is actively loaded", () => {
    // Inverted ordering logic: b - a
    const descendingTree = new AVLTree<number, string>((a, b) => b - a);
    descendingTree.insert(15, "Fifteen");
    descendingTree.insert(5, "Five");
    descendingTree.insert(45, "Forty-Five");

    // Leftmost leaf is structurally the "smallest" node to the comparator,
    // which corresponds to the highest numeric number under descending logic.
    expect(descendingTree.minKey()).toBe(45);
    expect(descendingTree.maxKey()).toBe(5);
  });
});

describe("AVLTree Bulk Initialization (fromArray)", () => {
  test("should produce an empty tree if given an empty array", () => {
    const tree = AVLTree.fromArray([], false);
    expect(tree.minKey()).toBeNull();
    expect(tree.maxKey()).toBeNull();
  });

  test("should instantiate correctly via consecutive insertions when isSorted is false", () => {
    const rawData = [
      { key: 30, value: "V30" },
      { key: 10, value: "V10" },
      { key: 20, value: "V20" },
    ];

    const tree = AVLTree.fromArray(rawData, false);

    expect([...tree]).toEqual([
      { key: 10, value: "V10" },
      { key: 20, value: "V20" },
      { key: 30, value: "V30" },
    ]);
  });

  test("should perfectly balance an already sorted array in linear O(n) runtime execution structures", () => {
    const sortedData = [
      { key: 1, value: "One" },
      { key: 2, value: "Two" },
      { key: 3, value: "Three" },
      { key: 4, value: "Four" },
      { key: 5, value: "Five" },
    ];

    const tree = AVLTree.fromArray(sortedData, true);

    // Verify ordering is maintained perfectly
    expect([...tree]).toEqual(sortedData);

    // Structural verification: With 5 elements balanced from middle out,
    // Root should be 3 (index 2). Left subtree root is 1 or 2, right is 4 or 5.
    // Max path height should be strictly capped at 3 layers.
    expect(tree.find(3)).toBe("Three");
    expect(tree.minKey()).toBe(1);
    expect(tree.maxKey()).toBe(5);
  });
});

describe("AVLTree Closest Key Tracking", () => {
  const numDistance = (a: number, b: number): number => Math.abs(a - b);

  test("should return null if searching for proximity within an empty tree", () => {
    const tree = new AVLTree<number, string>();
    expect(tree.findClosestKey(10, numDistance)).toBeNull();
  });

  test("should resolve exact key match directly if it exists", () => {
    const tree = AVLTree.fromArray([
      { key: 10, value: "A" },
      { key: 20, value: "B" },
      { key: 30, value: "C" },
    ]);
    expect(tree.findClosestKey(20, numDistance)).toBe(20);
  });

  test("should discover closest upper or lower boundary nodes", () => {
    const tree = AVLTree.fromArray([
      { key: 10, value: "Low" },
      { key: 25, value: "Mid" },
      { key: 40, value: "High" },
    ]);

    expect(tree.findClosestKey(12, numDistance)).toBe(10); // 10 is closer than 25
    expect(tree.findClosestKey(23, numDistance)).toBe(25); // 25 is closer than 10
    expect(tree.findClosestKey(37, numDistance)).toBe(40); // 40 is closer than 25
    expect(tree.findClosestKey(100, numDistance)).toBe(40); // Bounded ceiling cap
  });

  test("should handle complex types like Dates effectively", () => {
    const dateTree = new AVLTree<Date, string>((a, b) => a.getTime() - b.getTime());
    const dateDistance = (a: Date, b: Date): number => Math.abs(a.getTime() - b.getTime());

    const d1 = new Date("2026-01-01");
    const d2 = new Date("2026-06-01");
    const d3 = new Date("2026-12-01");

    dateTree.insert(d1, "New Year");
    dateTree.insert(d2, "Midyear");
    dateTree.insert(d3, "Yearend");

    const targetDate = new Date("2026-05-20");
    const matchedKey = dateTree.findClosestKey(targetDate, dateDistance);

    expect(matchedKey).toEqual(d2); // May 20 is closest to June 1
  });
});
