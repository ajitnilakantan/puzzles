import _ from "lodash";
import assert from "node:assert";
import { Grid } from "../lib/grid";
import { debug, fileio } from "../lib/prelude";
import type { DayPlugin } from "../src/plugin";

// export private functions for testing
export const __test__ = { parseGrid, move, isLoop, gridIndices, rcIndices };

export enum Direction {
  Up,
  Down,
  Left,
  Right,
}

function parseGrid(grid: Grid<string>): [startRow: number, startCol: number, startDir: Direction] {
  let [startRow, startCol, startDir] = [0, 0, Direction.Up];
  // Extract the sorted positions of "#" in the rows an columns
  // for quick lookups of next position.

  for (const [row, col] of Grid.product<number, number>(grid.numrows, grid.numcols)) {
    switch (grid[row][col]) {
      case "^":
        startRow = row;
        startCol = col;
        startDir = Direction.Up;
        break;
      case ">":
        startRow = row;
        startCol = col;
        startDir = Direction.Right;
        break;
      case "v":
        startRow = row;
        startCol = col;
        startDir = Direction.Down;
        break;
      case "<":
        startRow = row;
        startCol = col;
        startDir = Direction.Left;
        break;
      default:
        break;
    }
  }

  return [startRow, startCol, startDir];
}

function move(
  row: number,
  col: number,
  dir: Direction,
  grid: Grid<string>,
  markedGrid: Grid<string> | undefined,
  rows: number[][],
  cols: number[][],
  rowIndices: number[][],
  colIndices: number[][]
): [number, number, Direction] {
  let [newRow, newCol] = [row, col];
  let newDir: Direction;

  let idx: number;
  switch (dir) {
    case Direction.Up:
      newDir = Direction.Right;
      idx = colIndices[col][row] - 1;
      newRow = idx === -1 ? -1 : cols[col][idx] + 1;
      break;
    case Direction.Right:
      newDir = Direction.Down;
      idx = rowIndices[row][col];
      newCol = idx === rows[row].length ? grid.numcols : rows[row][idx] - 1;
      break;
    case Direction.Down:
      newDir = Direction.Left;
      idx = colIndices[col][row];
      newRow = idx === cols[col].length ? grid.numrows : cols[col][idx] - 1;
      break;
    case Direction.Left:
      newDir = Direction.Up;
      idx = rowIndices[row][col] - 1;
      newCol = idx === -1 ? -1 : rows[row][idx] + 1;
      break;
    default: {
      // Exhaustiveness check: TypeScript will throw a compile error here.
      const _exhaustiveCheck: never = dir;
      return _exhaustiveCheck;
    }
  }
  if (markedGrid !== undefined) {
    const [r1, r2] = [Math.min(row, newRow), Math.max(row, newRow)];
    const [c1, c2] = [Math.min(col, newCol), Math.max(col, newCol)];
    for (let r = r1; r <= r2; r++) {
      for (let c = c1; c <= c2; c++) {
        if (r >= 0 && r < markedGrid.numrows && c >= 0 && c < markedGrid.numcols) {
          markedGrid[r][c] = "X";
        }
      }
    }
  }
  return [newRow, newCol, newDir];
}

// Compute the positions of each "# by rows and by columns
// Precompute the _sortedIndex() for a single row & single col of the grid
function rcIndices(
  grid: Grid<string>,
  row: number,
  col: number
): [singlerow: number[], singlecol: number[], rIndices: number[], cIndices: number[]] {
  const singlerow: number[] = [];
  for (const col of Grid.range(grid.numcols)) {
    if (grid[row][col] === "#") {
      singlerow.push(col);
    }
  }
  const singlecol: number[] = [];
  for (const row of Grid.range(grid.numrows)) {
    if (grid[row][col] === "#") {
      singlecol.push(row);
    }
  }

  const singlerowIndices: number[] = Array.from({ length: grid.numcols }, () => -1);
  for (const col of Grid.range(grid.numcols)) {
    // Calculate the position of the next '#' -- the actual column number is in rows[row][idx]
    singlerowIndices[col] = _.sortedIndex(singlerow, col);
  }

  const singlecolIndices: number[] = Array.from({ length: grid.numrows }, () => -1);
  for (const row of Grid.range(grid.numrows)) {
    // Calculate the position of the next '#' -- the actual row number is in cols[col][idx]
    singlecolIndices[row] = _.sortedIndex(singlecol, row);
  }

  return [singlerow, singlecol, singlerowIndices, singlecolIndices];
}

// Compute the position of each "#" in the grid by rows and by cols
// Precompute the _sortedIndex() for each cell of the grid, by row and by column.
function gridIndices(
  grid: Grid<string>
): [rows: number[][], cols: number[][], rowIndices: number[][], colIndices: number[][]] {
  // Initializes a 2D array of numbers: [[], [], [], [], []]
  //  Rows contains the next index of "#" for each row.
  const rows: number[][] = Array.from({ length: grid.numrows }, () => []);
  for (const [row, col] of Grid.product<number, number>(grid.numrows, grid.numcols)) {
    if (grid[row][col] === "#") {
      rows[row].push(col);
    }
  }

  // Initializes a 2D array of numbers: [[], [], [], [], []]
  //  Cols contains the next index of "#" for each column.
  const cols: number[][] = Array.from({ length: grid.numcols }, () => []);
  for (const [col, row] of Grid.product<number, number>(grid.numcols, grid.numrows)) {
    if (grid[row][col] === "#") {
      cols[col].push(row);
    }
  }
  const rowIndices: number[][] = Array.from({ length: grid.numrows }, () => Array<number>(grid.numcols).fill(-1));
  const colIndices: number[][] = Array.from({ length: grid.numcols }, () => Array<number>(grid.numrows).fill(-1));

  for (const [row, col] of Grid.product<number, number>(grid.numrows, grid.numcols)) {
    // Calculate the position of the next '#' -- the actual column number is in rows[row][idx]
    rowIndices[row][col] = _.sortedIndex(rows[row], col);
  }

  for (const [col, row] of Grid.product<number, number>(grid.numcols, grid.numrows)) {
    // Calculate the position of the next '#' -- the actual row number is in cols[col][idx]
    colIndices[col][row] = _.sortedIndex(cols[col], row);
  }

  return [rows, cols, rowIndices, colIndices];
}

function isLoop(
  startRow: number,
  startCol: number,
  startDir: Direction,
  grid: Grid<string>,
  rows: number[][],
  cols: number[][],
  rowIndices: number[][],
  colIndices: number[][]
): boolean {
  let [row, col, dir] = [startRow, startCol, startDir];
  const toKey = (row: number, col: number, dir: Direction): string => `${row},${col},${dir}`;
  const visited = new Set<string>();

  visited.add(toKey(row, col, dir));
  do {
    const [newRow, newCol, newDir] = move(row, col, dir, grid, undefined, rows, cols, rowIndices, colIndices);
    [row, col, dir] = [newRow, newCol, newDir];
    if (visited.has(toKey(row, col, dir))) {
      return true;
    }
    visited.add(toKey(row, col, dir));
  } while (row >= 0 && row < grid.numrows && col >= 0 && col < grid.numcols);
  return false;
}

function solvePart1(lines: string[]): number {
  const grid = Grid.fromLines(lines, ".");
  let [row, col, dir] = parseGrid(grid);
  const [rows, cols, rowIndices, colIndices] = gridIndices(grid);
  const markedGrid = grid.copy();
  do {
    const [newRow, newCol, newDir] = move(row, col, dir, grid, markedGrid, rows, cols, rowIndices, colIndices);
    [row, col, dir] = [newRow, newCol, newDir];
  } while (row >= 0 && row < grid.numrows && col >= 0 && col < grid.numcols);

  let solution = 0;
  for (const [row, col] of Grid.product<number, number>(markedGrid.numrows, markedGrid.numcols)) {
    if (markedGrid[row][col] === "X") {
      solution++;
    }
  }

  return solution;
}

function solvePart2(lines: string[]): number {
  // Run part1 to get the markedGrid
  const grid = Grid.fromLines(lines, ".");
  let [row, col, dir] = parseGrid(grid);
  const [rows, cols, rowIndices, colIndices] = gridIndices(grid);
  const markedGrid = grid.copy();

  // Solve as in part1
  do {
    const [newRow, newCol, newDir] = move(row, col, dir, grid, markedGrid, rows, cols, rowIndices, colIndices);
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
  const [startRow, startCol, startDir] = parseGrid(newGrid);
  for ([row, col] of coords) {
    const oldCell = newGrid[row][col];
    const [oldsinglerow, oldsinglecol] = [rows[row], cols[col]];
    const [oldsinglerowIndices, oldsinglecolIndices] = [rowIndices[row], colIndices[col]];

    // push
    newGrid[row][col] = "#";
    const [singlerow, singlecol, singlerowIndices, singlecolIndices] = rcIndices(newGrid, row, col);

    // const [rows, cols, rowIndices, colIndices] = gridIndices(newGrid);
    rows[row] = singlerow;
    cols[col] = singlecol;
    rowIndices[row] = singlerowIndices;
    colIndices[col] = singlecolIndices;

    if (isLoop(startRow, startCol, startDir, newGrid, rows, cols, rowIndices, colIndices)) {
      solution++;
    }

    // pop
    newGrid[row][col] = oldCell;
    rows[row] = oldsinglerow;
    cols[col] = oldsinglecol;
    rowIndices[row] = oldsinglerowIndices;
    colIndices[col] = oldsinglecolIndices;
  }
  return solution;
}

const plugin: DayPlugin = {
  name: "Day 06",
  async run({ verbose }) {
    console.log(`Running ${this.name} verbose=${verbose}`);
    const data = await fileio.linesFromFile(`${import.meta.dir}/day06.txt`);

    const start1 = performance.now();
    const solution1 = solvePart1(data);
    const duration1 = (performance.now() - start1).toFixed(3);
    console.log(`${this.name} verbose=${verbose}.Part1 = ${solution1} : Elapsed time: ${duration1}ms`);
    assert.strictEqual(solution1, 5329, `${this.name} verbose=${verbose}.Part1`);

    const start2 = performance.now();
    const solution2 = solvePart2(data);
    const duration2 = (performance.now() - start2).toFixed(3);
    console.log(`${this.name} verbose=${verbose}.Part2 = ${solution2} : Elapsed time: ${duration2}ms`);
    assert.strictEqual(solution2, 2162, `${this.name} verbose=${verbose}.Part2`);
  },
};

export default plugin;
