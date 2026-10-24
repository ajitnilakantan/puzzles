import { describe, expect, test } from "bun:test";
import { aStar, bfs, dijkstra, dijkstraPath, findAllPaths, findAllPathsRecursive, getAllPaths } from "./graph";
import { StructuralMap } from "./graph";
import type { GetNeighbours, IsGoal } from "./graph";

// Mock Data for Testing (Contains multiple paths to 'D')
// Path 1: A -> B -> D
// Path 2: A -> C -> D
const mockGraph: Record<string, string[] | undefined> = {
  A: ["B", "C"],
  B: ["D"],
  C: ["D"],
  D: [],
};

describe("Graph Functions Suite", () => {
  describe("BFS Tests", () => {
    test("should work with a function returning an Array", () => {
      const getNeighboursArray: GetNeighbours<string> = (node) => mockGraph[node] ?? [];
      const result = bfs("A", getNeighboursArray);
      expect(result).toEqual(["A", "B", "C", "D"]);
    });
  });

  describe("getAllPaths Tests", () => {
    test("should yield all valid paths using an iterator", () => {
      const getNeighboursArray: GetNeighbours<string> = (node) => mockGraph[node] ?? [];
      const isGoal: IsGoal<string> = (node) => node === "D";

      // Obtain the iterator
      const pathsIterator = getAllPaths("A", isGoal, getNeighboursArray);

      // Collect all paths yielded by the iterator
      const allPaths = Array.from(pathsIterator);

      // Expecting both valid paths to 'D'
      expect(allPaths).toContainEqual(["A", "B", "D"]);
      expect(allPaths).toContainEqual(["A", "C", "D"]);
      expect(allPaths.length).toBe(2);
    });

    test("should work when getNeighbours is a Generator", () => {
      function* getNeighboursGenerator(node: string): Generator<string, void, unknown> {
        const neighbours = mockGraph[node] ?? [];
        for (const n of neighbours) {
          yield n;
        }
      }
      const isGoal: IsGoal<string> = (node) => node === "D";

      const pathsIterator = getAllPaths("A", isGoal, getNeighboursGenerator);
      const allPaths = Array.from(pathsIterator);

      expect(allPaths).toContainEqual(["A", "B", "D"]);
      expect(allPaths).toContainEqual(["A", "C", "D"]);
    });

    test("should handle cyclic graphs safely without infinite loops", () => {
      // Self-referencing cyclic graph: A <-> B
      const cyclicGraph: Record<string, string[]> = {
        A: ["B"],
        B: ["A", "C"],
        C: [],
      };
      const isGoal: IsGoal<string> = (node) => node === "C";
      const getNeighbours = (node: string): string[] => cyclicGraph[node] ?? [];

      const pathsIterator = getAllPaths("A", isGoal, getNeighbours);
      const allPaths = Array.from(pathsIterator);

      // It should successfully bypass the cycle and find the one clean path
      expect(allPaths).toEqual([["A", "B", "C"]]);
    });
  });
});

describe("structural_graphsearch_test", () => {
  test("test astar with structural array coordinate keys", () => {
    const graphStr = ".#....\n" + ".#....\n" + "......\n" + "#.....";

    const lines = graphStr.split("\n");
    const height = lines.length;
    const width = lines[0].length;

    const grid = new StructuralMap<[number, number], string>();
    for (let r = 0; r < height; r++) {
      for (let c = 0; c < width; c++) {
        grid.set([r, c], lines[r][c]);
      }
    }

    const start: [number, number] = [0, 0];
    const goal: [number, number] = [height - 1, width - 1];

    const getNeighbors = (rc: [number, number]): [number, number][] => {
      const [r, c] = rc;
      const potential: [number, number][] = [
        [r - 1, c],
        [r + 1, c],
        [r, c - 1],
        [r, c + 1],
      ];

      return potential.filter(([nr, nc]) => {
        const isInBounds = nr >= 0 && nr < height && nc >= 0 && nc < width;
        return isInBounds && grid.get([nr, nc]) !== "#";
      });
    };

    const getNeighborsWithHistory = (
      rc: [number, number],
      _cameFrom: StructuralMap<[number, number], [number, number]>
    ): [number, number][] => {
      return getNeighbors(rc);
    };

    const getHeuristic =
      (goalNode: [number, number]) =>
      (node: [number, number]): number => {
        return Math.abs(node[0] - goalNode[0]) + Math.abs(node[1] - goalNode[1]);
      };

    const getDistBetween = (node: [number, number], neighbor: [number, number]): number => {
      return Math.abs(node[0] - neighbor[0]) + Math.abs(node[1] - neighbor[1]);
    };

    const isGoal = (current: [number, number], target: [number, number]): boolean =>
      current[0] === target[0] && current[1] === target[1];

    // Execute A*
    const rawPath = aStar<[number, number]>(
      start,
      goal,
      getNeighborsWithHistory,
      getDistBetween,
      getHeuristic(goal),
      isGoal
    );

    expect(rawPath).not.toBeNull();

    expect(rawPath!.length).toEqual(9);

    // Execute Dijkstra
    const distances = dijkstra<[number, number]>(getNeighbors, getDistBetween, start);

    expect(distances.get(goal)).toBe(8.0);
    expect(distances.get(start)).toBe(0.0);

    // Execute DijkstraPath
    const isGoal2 = (node: [number, number]): boolean => node[0] === goal[0] && node[1] === goal[1];
    const dijkstraResult = dijkstraPath<[number, number]>(getNeighbors, getDistBetween, start, isGoal2);
    expect(dijkstraResult).not.toBeNull();

    const [totalCost, pathNodes] = dijkstraResult!;
    expect(totalCost).toBe(8.0);
    expect(pathNodes.length).toBe(9);
  });

  test("test getAllPaths with primitive string keys via StructuralMap", () => {
    const graph1 = new StructuralMap<string, string[]>([
      ["A", ["B", "C"]],
      ["B", ["D"]],
      ["C", ["B", "D"]],
    ]);

    const getNeighbors =
      (g: StructuralMap<string, string[]>) =>
      (node: string): string[] => {
        return g.get(node) ?? [];
      };

    const isTarget =
      (goal: string) =>
      (node: string): boolean =>
        goal === node;

    const pathSeqRec = Array.from(findAllPathsRecursive(getNeighbors(graph1), isTarget("D"), ["A"]));
    const pathSeqIterative = Array.from(findAllPaths(getNeighbors(graph1), isTarget("D"), "A"));

    const expectedPaths = [
      ["A", "B", "D"],
      ["A", "C", "B", "D"],
      ["A", "C", "D"],
    ];

    expect(pathSeqRec).toEqual(expectedPaths);
    expect(pathSeqIterative).toEqual(expectedPaths);
  });
});
