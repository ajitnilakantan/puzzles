import { describe, expect, test } from "bun:test";
import _ from "lodash";
import { Grid } from "../lib/grid";
import { debug, fileio } from "../lib/prelude";
import { __test__ } from "./day06";

describe("day06", () => {
  const data = `....#.....
                .........#
                ..........
                ..#.......
                .......#..
                ..........
                .#..^.....
                ........#.
                #.........
                ......#...`;

  const lines = fileio.linesFromString(data);

  test("lodash", async () => {
    const nums = [10, 20, 30, 40, 50];
    let idx: number;

    idx = _.sortedIndex(nums, 8);
    expect(idx).toBe(0);
    idx = _.sortedIndex(nums, 30);
    expect(idx).toBe(2);
    idx = _.sortedIndex(nums, 48);
    expect(idx).toBe(4);
    idx = _.sortedIndex(nums, 58);
    expect(idx).toBe(5);
    expect(idx).toBe(nums.length);
    const nums0 = [0];
    const ids = [];
    for (let i = 0; i < 8; i++) {
      ids.push(_.sortedIndex(nums0, i));
    }
    expect(ids).toEqual([0, 1, 1, 1, 1, 1, 1, 1]);
  });

  test("part1", async () => {
    const grid = Grid.fromLines(lines, ".");
    let [row, col, dir] = __test__.parseGrid(grid);
    const [rows, cols, rowIndices, colIndices] = __test__.gridIndices(grid);
    const markedGrid = grid.copy();

    do {
      const [newRow, newCol, newDir] = __test__.move(
        row,
        col,
        dir,
        grid,
        markedGrid,
        rows,
        cols,
        rowIndices,
        colIndices
      );
      [row, col, dir] = [newRow, newCol, newDir];
    } while (row >= 0 && row < grid.numrows && col >= 0 && col < grid.numcols);

    let solution = 0;
    for (const [row, col] of Grid.product<number, number>(markedGrid.numrows, markedGrid.numcols)) {
      if (markedGrid[row][col] === "X") {
        solution++;
      }
    }

    expect(solution).toBe(41);
  });

  test("part2", async () => {
    // Run part1 to get the markedGrid
    const grid = Grid.fromLines(lines, ".");
    let [row, col, dir] = __test__.parseGrid(grid);
    const [rows, cols, rowIndices, colIndices] = __test__.gridIndices(grid);
    const markedGrid = grid.copy();

    // Solve as in part1
    do {
      const [newRow, newCol, newDir] = __test__.move(
        row,
        col,
        dir,
        grid,
        markedGrid,
        rows,
        cols,
        rowIndices,
        colIndices
      );
      [row, col, dir] = [newRow, newCol, newDir];
    } while (row >= 0 && row < grid.numrows && col >= 0 && col < grid.numcols);

    // Mark all the visited "X" coords
    const coords: [number, number][] = [];
    for (const [row, col] of Grid.product<number, number>(markedGrid.numrows, markedGrid.numcols)) {
      if (markedGrid[row][col] === "X") {
        coords.push([row, col]);
      }
    }

    // Count the looping paths
    let solution = 0;

    // Create a newGrid with a new blockage at each of the "coords"
    const newGrid = grid.copy();
    const [startRow, startCol, startDir] = __test__.parseGrid(newGrid);
    for ([row, col] of coords) {
      const oldCell = newGrid[row][col];
      const [oldsinglerow, oldsinglecol] = [rows[row], cols[col]];
      const [oldsinglerowIndices, oldsinglecolIndices] = [rowIndices[row], colIndices[col]];

      // push
      newGrid[row][col] = "#";
      const [singlerow, singlecol, singlerowIndices, singlecolIndices] = __test__.rcIndices(newGrid, row, col);

      // const [rows, cols, rowIndices, colIndices] = __test__.gridIndices(newGrid);
      rows[row] = singlerow;
      cols[col] = singlecol;
      rowIndices[row] = singlerowIndices;
      colIndices[col] = singlecolIndices;

      if (__test__.isLoop(startRow, startCol, startDir, newGrid, rows, cols, rowIndices, colIndices)) {
        solution++;
      }

      // pop
      newGrid[row][col] = oldCell;
      rows[row] = oldsinglerow;
      cols[col] = oldsinglecol;
      rowIndices[row] = oldsinglerowIndices;
      colIndices[col] = oldsinglecolIndices;
    }

    expect(solution).toBe(6);
  });
});
