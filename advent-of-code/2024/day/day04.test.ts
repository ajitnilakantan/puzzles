import { describe, expect, test } from "bun:test";
import { Counter } from "../lib/counter";
import { Grid } from "../lib/grid";
import { debug, fileio } from "../lib/prelude";
import { __test__ } from "./day04";
import type { Position, SearchResult } from "./day04";

describe("day04", () => {
  const data = `MMMSXXMASM
                MSAMXMSMSA
                AMXSXMAAMM
                MSAMASMSMX
                XMASAMXAMM
                XXAMMXXAMA
                SMSMSASXSS
                SAXAMASAAA
                MAMMMXMMMM
                MXMXAXMASX`;

  const lines = fileio.linesFromString(data);

  test("part1", async () => {
    const grid = Grid.fromLines(lines, ".");
    const result: SearchResult = __test__.wordSearch(grid, "XMAS", false);
    const solution = result.count;
    expect(solution).toBe(18);
  });

  test("part2", async () => {
    const grid = Grid.fromLines(lines, ".");
    const result: SearchResult = __test__.wordSearch(grid, "MAS", false);
    const counter = new Counter<string>();
    const toKey = (pos: Position): string => `${pos.row},${pos.col}`;
    for (const p of result.paths) {
      // Add the position of "A" (midpoint)
      expect(p.length).toBe(3);
      if (Math.abs(p[0].row - p[1].row) === 1 && Math.abs(p[0].col - p[1].col) === 1) {
        // Add the "A" coords when it is in a diagonal shape.
        counter.add(toKey(p[1]));
      }
    }
    const solution = [...counter.entries()].reduce((acc, [_k, v]) => {
      if (v === 2) {
        acc++;
      }
      return acc;
    }, 0);

    expect(solution).toBe(9);
  });
});
