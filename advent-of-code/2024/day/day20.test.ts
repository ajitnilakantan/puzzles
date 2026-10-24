import { describe, expect, test } from "bun:test";
import { debug, fileio } from "../lib/prelude";
import { __test__ } from "./day20";

describe("day20", () => {
  const data = `###############
                #...#...#.....#
                #.#.#.#.#.###.#
                #S#...#.#.#...#
                #######.#.#.###
                #######.#.#...#
                #######.#.###.#
                ###..E#...#...#
                ###.#######.###
                #...###...#...#
                #.#####.#.###.#
                #.#...#.#.#...#
                #.#.#.#.#.#.###
                #...#...#...###
                ###############`;

  const lines = fileio.linesFromString(data);

  test("part1", async () => {
    const [grid, start, goal] = __test__.parseData(lines);
    const [height, width] = [grid.numrows, grid.numcols];

    const shortestPath = __test__.getShortestPath(grid, height, width, start, goal);
    expect(shortestPath).not.toBeNull();
    expect(shortestPath!.length - 1).toBe(84); // Pathlength = numNodes - 1

    const solution = __test__.getShortcutPaths(grid, height, width, shortestPath!, 0);
    expect(solution).toBe(14 + 14 + 2 + 4 + 2 + 3 + 5);
  });

  test("part2", async () => {
    const [grid, start, goal] = __test__.parseData(lines);
    const [height, width] = [grid.numrows, grid.numcols];

    const shortestPath = __test__.getShortestPath(grid, height, width, start, goal)!;
    const solution = __test__.getExtendedShortcutPaths(grid, height, width, shortestPath, 20, 50);
    expect(solution).toBe(32 + 31 + 29 + 39 + 25 + 23 + 20 + 19 + 12 + 14 + 12 + 22 + 4 + 3);
  });
});
