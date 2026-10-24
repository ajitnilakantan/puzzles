import { describe, expect, test } from "bun:test";
import { getAllPaths } from "../lib/graph";
import { Grid } from "../lib/grid";
import { debug, fileio } from "../lib/prelude";
import type { Node } from "./day10";
import { __test__ } from "./day10";

describe("day10", () => {
  const allData1 = [
    `0123
     1234
     8765
     9876`,
    `...0...
     ...1...
     ...2...
     6543456
     7.....7
     8.....8
     9.....9`,
    `..90..9
     ...1.98
     ...2..7
     6543456
     765.987
     876....
     987....`,
    `10..9..
     2...8..
     3...7..
     4567654
     ...8..3
     ...9..2
     .....01`,
    `89010123
     78121874
     87430965
     96549874
     45678903
     32019012
     01329801
     10456732`,
  ];
  const expected1 = [1, 2, 4, 3, 36];

  const allData2 = [
    `.....0.
       ..4321.
       ..5..2.
       ..6543.
       ..7..4.
       ..8765.
       ..9....`,
    `..90..9
       ...1.98
       ...2..7
       6543456
       765.987
       876....
       987....`,
    `012345
       123456
       234567
       345678
       4.6789
       56789.`,
    `89010123
       78121874
       87430965
       96549874
       45678903
       32019012
       01329801
       10456732`,
  ];
  const expected2 = [3, 13, 227, 81];

  test("part1", async () => {
    allData1.forEach((data, index) => {
      const lines = fileio.linesFromString(data);
      const rawGrid = Grid.fromLines(lines, ".");
      const grid = rawGrid.map((val, _row, _col, _grid) => (val === "." ? -1 : Number(val)), 0);
      const starts: Node[] = [];
      grid.forEach((val, row, col, _grid) => {
        if (val === __test__.START) {
          starts.push({ row: row, col: col });
        }
      });
      let solution = 0;
      starts.forEach((start, _) => {
        const trails = new Set<string>();
        for (const p of getAllPaths(start, __test__.isGoal(grid), __test__.getNeighbours(grid))) {
          if (p.length === 10) {
            trails.add(__test__.toKey({ row: p[9].row, col: p[9].col }));
          }
        }
        solution += trails.size;
      });
      expect(solution).toBe(expected1[index]);
    }); // forEach allData
  });

  test("part2", async () => {
    allData2.forEach((data, index) => {
      const lines = fileio.linesFromString(data);
      const rawGrid = Grid.fromLines(lines, ".");
      const grid = rawGrid.map((val, _row, _col, _grid) => (val === "." ? -1 : Number(val)), 0);
      const starts: Node[] = [];
      grid.forEach((val, row, col, _grid) => {
        if (val === __test__.START) {
          starts.push({ row: row, col: col });
        }
      });
      let solution = 0;
      starts.forEach((start, _) => {
        for (const p of getAllPaths(start, __test__.isGoal(grid), __test__.getNeighbours(grid))) {
          if (p.length === 10) {
            solution += 1;
          }
        }
      });
      expect(solution).toBe(expected2[index]);
    }); // forEach allData
  });
});
