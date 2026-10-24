import { describe, expect, test } from "bun:test";
import { dijkstraAllPaths, dijkstraPath } from "../lib/graph";
import { debug, fileio } from "../lib/prelude";
import { __test__ } from "./day16";
import type { Node } from "./day16";

describe("day16", () => {
  const datas = [
    `###############
     #.......#....E#
     #.#.###.#.###.#
     #.....#.#...#.#
     #.###.#####.#.#
     #.#.#.......#.#
     #.#.#####.###.#
     #...........#.#
     ###.#.#####.#.#
     #...#.....#.#.#
     #.#.#.###.#.#.#
     #.....#...#.#.#
     #.###.#.#.#.#.#
     #S..#.....#...#
     ###############`,
    `#################
     #...#...#...#..E#
     #.#.#.#.#.#.#.#.#
     #.#.#.#...#...#.#
     #.#.#.#.###.#.#.#
     #...#.#.#.....#.#
     #.#.#.#.#.#####.#
     #.#...#.#.#.....#
     #.#.#####.#.###.#
     #.#.#.......#...#
     #.#.###.#####.###
     #.#.#...#.....#.#
     #.#.#.#####.###.#
     #.#.#.........#.#
     #.#.#.#########.#
     #S#.............#
     #################`,
  ];

  const expected1 = [7036, 11048];
  const expected2 = [45, 64];

  test("part1", async () => {
    for (const [index, data] of datas.entries()) {
      const lines = fileio.linesFromString(data);
      const [grid, start, goal] = __test__.parseData(lines);
      const dijkstraResult = dijkstraPath(__test__.getNeighbours(grid), __test__.getCost, start, __test__.isGoal(goal));
      expect(dijkstraResult).not.toBeNull();
      const [cost, path] = dijkstraResult ?? [0, []];
      expect(path).not.toBeNull();
      expect(path.length).not.toBe(0);
      expect(cost).toBe(expected1[index]);
    }
  });

  test("part2", async () => {
    for (const [index, data] of datas.entries()) {
      const lines = fileio.linesFromString(data);
      const [grid, start, goal] = __test__.parseData(lines);
      const dijkstraResult = dijkstraAllPaths(
        __test__.getNeighbours(grid),
        __test__.getCost,
        start,
        __test__.isGoal(goal)
      );
      const [minimumCost, paths] = dijkstraResult ?? [0, []];
      expect(minimumCost).toBe(expected1[index]);
      const toKey = (node: Node): string => `${node.row},${node.col}`;
      const visited = new Set<string>();
      for (const path of paths) {
        path.forEach((p) => visited.add(toKey(p)));
      }
      const solution = visited.size;
      expect(solution).toBe(expected2[index]);
    }
  });
});
