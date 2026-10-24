import { describe, expect, test } from "bun:test";
import { type GetNeighbours, findMaximalCliques } from "./max_clique";

// Adjust path to your source file

describe("Bron-Kerbosch Algorithm", () => {
  test("should find all maximal cliques in a diamond graph with a tail", () => {
    // Graph visual:
    //   1 --- 2
    //   |  X  |
    //   3 --- 4 --- 5
    const nodes = [1, 2, 3, 4, 5];

    const adjacencyList: Record<number, number[]> = {
      1: [2, 3, 4],
      2: [1, 3, 4],
      3: [1, 2, 4],
      4: [1, 2, 3, 5],
      5: [4],
    };

    const getNeighbours: GetNeighbours<number> = (node) => adjacencyList[node] ?? [];

    const result = findMaximalCliques(nodes, getNeighbours);

    // Sort inner arrays and the outer array so the assertion isn't flaky due to ordering
    const normalizedResult = result.map((clique) => clique.sort((a, b) => a - b)).sort((a, b) => a[0] - b[0]);

    expect(normalizedResult).toEqual([
      [1, 2, 3, 4], // The 4-clique diamond
      [4, 5], // The tail edge
    ]);
  });

  test("should handle an empty graph", () => {
    const result = findMaximalCliques([], () => []);
    expect(result).toEqual([]);
  });
});
