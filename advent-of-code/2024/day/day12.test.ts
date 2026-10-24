import { describe, expect, test } from "bun:test";
import { Grid } from "../lib/grid";
import { debug, fileio } from "../lib/prelude";
import { __test__ } from "./day12";

describe("day12", () => {
  const data = [
    `RRRRIICCFF
                 RRRRIICCCF
                 VVRRRCCFFF
                 VVRCCCJFFF
                 VVVVCJJCFE
                 VVIVCCJJEE
                 VVIIICJJEE
                 MIIIIIJJEE
                 MIIISIJEEE
                 MMMISSJEEE`,
    `EEEEE
                 EXXXX
                 EEEEE
                 EXXXX
                 EEEEE`,
    `AAAAAA
                 AAABBA
                 AAABBA
                 ABBAAA
                 ABBAAA
                 AAAAAA`,
  ];

  test("part1", async () => {
    const lines = fileio.linesFromString(data[0]);
    const grid = Grid.fromLines(lines, ".");
    const regions = __test__.splitRegions(grid);
    const solution = regions
      .map((region) => __test__.measureRegion(grid, region))
      .reduce((acc, [area, perimeter, _rows_cols]) => acc + area * perimeter, 0);
    expect(solution).toBe(1930);
  });

  test("part2", async () => {
    const expected = [1206, 236, 368];
    data.forEach((_d, index) => {
      const lines = fileio.linesFromString(data[index]);
      const grid = Grid.fromLines(lines, ".");
      const regions = __test__.splitRegions(grid);
      const solution = regions
        .map((region) => __test__.measureRegion(grid, region))
        .reduce((acc, [area, _perimeter, rows_cols]) => acc + area * __test__.numEdges(rows_cols), 0);
      expect(solution).toBe(expected[index]);
    });
  });
});
