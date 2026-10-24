import { describe, expect, test } from "bun:test";
import { Grid } from "../lib/grid";
import { debug, fileio } from "../lib/prelude";
import { __test__ } from "./day08";

describe("day08", () => {
  const data = `............
                ........0...
                .....0......
                .......0....
                ....0.......
                ......A.....
                ............
                ............
                ........A...
                .........A..
                ............
                ............`;

  const lines = fileio.linesFromString(data);

  test("allPairs", async () => {
    const items = ["a", "b", "c", "d"];
    const pairs = [...__test__.allPairs(items)];
    expect(pairs).toEqual([
      ["a", "b"],
      ["a", "c"],
      ["a", "d"],
      ["b", "c"],
      ["b", "d"],
      ["c", "d"],
    ]);
    const pairs2 = [];
    for (const p of __test__.allPairs(items)) {
      pairs2.push(p);
    }
    expect(pairs2).toEqual([
      ["a", "b"],
      ["a", "c"],
      ["a", "d"],
      ["b", "c"],
      ["b", "d"],
      ["c", "d"],
    ]);
  });

  test("part1", async () => {
    const grid = Grid.fromLines(lines, ".");
    const antennas = __test__.getAntennas(grid);
    const antinodes = __test__.getAntinodes(antennas, grid);
    const toKey = (row: number, col: number): string => `${row},${col}`;
    const validAntinodes = antinodes.filter(([r, c]) => r >= 0 && r < grid.numrows && c >= 0 && c < grid.numcols);
    // Sets need primitive key
    const uniqueAntinodes = new Set(validAntinodes.map(([r, c]) => toKey(r, c)));
    const solution = uniqueAntinodes.size;
    expect(solution).toBe(14);
  });

  test("part2", async () => {
    const grid = Grid.fromLines(lines, ".");
    const antennas = __test__.getAntennas(grid);
    const antinodes = __test__.getAntinodesPart2(antennas, grid);
    const toKey = (row: number, col: number): string => `${row},${col}`;
    const validAntinodes = antinodes.filter(([r, c]) => r >= 0 && r < grid.numrows && c >= 0 && c < grid.numcols);
    // Sets need primitive key
    const uniqueAntinodes = new Set(validAntinodes.map(([r, c]) => toKey(r, c)));
    const solution = uniqueAntinodes.size;
    expect(solution).toBe(34);
  });
});
