import assert from "node:assert";
import { Grid } from "../lib/grid";
import { debug, fileio } from "../lib/prelude";
import type { DayPlugin } from "../src/plugin";

// export private functions for testing
export const __test__ = { parseData, move, expandGrid, move2, scoreGrid };

export enum Direction {
  Up = 0,
  Down = 1,
  Left = 2,
  Right = 3,
}

const dirs = [
  [-1, 0],
  [1, 0],
  [0, -1],
  [0, 1],
] as const; // delta row/col

interface Position {
  row: number;
  col: number;
}
function parseData(lines: string[][]): [Grid<string>, Direction[], Position] {
  assert(lines.length === 2);
  const grid = Grid.fromLines(lines[0], ".");
  const moves = [];
  for (const line of lines[1]) {
    const m = Array.from(line, (x) => {
      switch (x) {
        case "^":
          return Direction.Up;
        case ">":
          return Direction.Right;
        case "v":
          return Direction.Down;
        case "<":
          return Direction.Left;
        default: // Exhaustiveness check
          throw new Error(`Unhandled fallback`);
      }
    });
    moves.push(...m);
  }
  const robot = grid.find((val, _r, _c, _grid) => val === "@");
  assert(robot !== undefined);
  const pos = { row: robot[1], col: robot[2] };
  grid[pos.row][pos.col] = ".";
  return [grid, moves, pos];
}

function move(pos: Position, d: Direction, grid: Grid<string>): Position {
  const dir = dirs[d];
  const nextpos = { row: pos.row + dir[0], col: pos.col + dir[1] };
  const next = grid[nextpos.row][nextpos.col];
  if (next === "#") {
    return pos;
  } else if (next === ".") {
    return nextpos;
  } else {
    assert(next === "O");
    let nextpos2 = nextpos;
    let next2;
    for (;;) {
      nextpos2 = { row: nextpos2.row + dir[0], col: nextpos2.col + dir[1] };
      next2 = grid[nextpos2.row][nextpos2.col];
      if (next2 === "#") {
        return pos; // No space left
      } else if (next2 === ".") {
        // Shift evrything over.
        grid[nextpos.row][nextpos.col] = ".";
        grid[nextpos2.row][nextpos2.col] = "O";
        return nextpos;
      }
    }
  }
}

// Shift [startCol, endCol] on given row left or right. Replace vacated space with "."
function shiftRow(grid: Grid<string>, row: number, startCol: number, endCol: number, d: Direction): void {
  if (d === Direction.Left) {
    assert(endCol <= startCol); // moving left
    for (let c = endCol; c <= startCol; c++) {
      grid[row][c - 1] = grid[row][c];
    }
    grid[row][startCol] = ".";
  } else {
    assert(d === Direction.Right);
    assert(startCol <= endCol); // moving right
    for (let c = endCol; c >= startCol; c--) {
      grid[row][c + 1] = grid[row][c];
    }
    grid[row][startCol] = ".";
  }
}

// Shift array of "[]" boxes up or down. Replace vacated space with "."
function shiftCol(grid: Grid<string>, boxes: Position[], d: Direction): void {
  const dir = dirs[d];
  for (const { row, col } of boxes) {
    // Clear
    grid[row][col] = ".";
    grid[row][col + 1] = ".";
  }
  for (const { row, col } of boxes) {
    // Set
    assert(dir[1] === 0); // Moving vertically
    grid[row + dir[0]][col] = "[";
    grid[row + dir[0]][col + 1] = "]";
  }
}
// For part 2, handle the oversized boxes [].
function expandGrid(grid: Grid<string>): Grid<string> {
  const newGrid = new Grid<string>(grid.numrows, 2 * grid.numcols, ".");
  for (const [row, col] of Grid.product<number, number>(grid.numrows, grid.numcols)) {
    switch (grid[row][col]) {
      case ".":
        break; // already cleared
      case "@":
        newGrid[row][2 * col] = "@";
        break;
      case "#":
        newGrid[row][2 * col] = "#";
        newGrid[row][2 * col + 1] = "#";
        break;
      case "O":
        newGrid[row][2 * col] = "[";
        newGrid[row][2 * col + 1] = "]";
        break;
      default: // Exhaustiveness check
        throw new Error(`Unhandled fallback`);
    }
  }
  return newGrid;
}

// For part 2, handle the oversized boxes [].
function move2(pos: Position, d: Direction, grid: Grid<string>): Position {
  const dir = dirs[d];
  const nextpos = { row: pos.row + dir[0], col: pos.col + dir[1] };
  const next = grid[nextpos.row][nextpos.col];
  // As in part 1, look for first "." or "#"
  if (next === "#") {
    return pos;
  } else if (next === ".") {
    return nextpos;
  }
  if (d === Direction.Left || d === Direction.Right) {
    assert(next === "[" || next === "]");
    assert(dir[0] === 0); // Delta-row is zer0
    let nextpos2 = nextpos;
    let next2;
    for (;;) {
      nextpos2 = { row: nextpos2.row, col: nextpos2.col + dir[1] };
      next2 = grid[nextpos2.row][nextpos2.col];
      if (next2 === "#") {
        return pos; // No space left
      } else if (next2 === ".") {
        // Shift everything over.
        shiftRow(grid, nextpos.row, nextpos.col, nextpos2.col - dir[1], d);
        return nextpos;
      }
    }
  } else {
    assert(dir[1] === 0); // Delta-column is zert
    assert(next === "[" || next === "]");
    // Push [ ]  or [ ]
    //      @         @
    // Initialize with first box
    let nextRow: Position[] = [
      next === "[" ? { row: nextpos.row, col: nextpos.col } : { row: nextpos.row, col: nextpos.col - 1 },
    ];

    // Keep track of all boxes being pushed. Track with the position of the "[" (left) side.
    const boxes = [];

    // Loop up or down row by row until either
    // - All cells above the boxes are "." (can move one space)
    // - Any cell above the boxes are "#" (blocked)
    // - Else cells contain "[" or "]" --> add a new row of boxes and repeat.
    for (;;) {
      boxes.push(...nextRow);
      if (
        nextRow.every(({ row, col }) => {
          const n1: string = grid[row + dir[0]][col];
          const n2: string = grid[row + dir[0]][col + 1];
          return n1 === "." && n2 === ".";
        })
      ) {
        // Found blank space. Move the boxes.
        shiftCol(grid, boxes, d);
        return nextpos;
      }
      if (
        nextRow.some(({ row, col }) => {
          const [n1, n2] = [grid[row + dir[0]][col], grid[row + dir[0]][col + 1]];
          return n1 === "#" || n2 === "#";
        })
      ) {
        // Found "#" wall.  Cannot move boxes.
        return pos;
      }
      // Repeat for next row.
      const newBoxes = [];
      for (const { row: r, col: c } of nextRow) {
        // The next above/below the "[" and "]"
        const a1: string = grid[r + dir[0]][c];
        const a2: string = grid[r + dir[0]][c + 1];
        if (a1 === "]") {
          newBoxes.push({ row: r + dir[0], col: c - 1 }); // matching "["
        }
        if (a1 === "[") {
          newBoxes.push({ row: r + dir[0], col: c }); // same "[" above/below
        }
        if (a2 === "]") {
          // already handled with a1 === "["
          assert(a1 === "[");
        }
        if (a2 === "[") {
          newBoxes.push({ row: r + dir[0], col: c + 1 }); // same "[" above/below
        }
      }
      nextRow = newBoxes; // Swap out
    }
  }
}
function scoreGrid(grid: Grid<string>): number {
  let score = 0;
  grid.forEach((val, row, col, _grid) => {
    if (val === "O" || val === "[") {
      score += 100 * row + col;
    }
  });
  return score;
}

function solvePart1(lines: string[]): number {
  const chunks = fileio.chunkLines(lines);
  const [grid, moves, pos] = parseData(chunks);
  let newpos = pos;
  for (const m of moves) {
    newpos = move(newpos, m, grid);
  }

  const solution = scoreGrid(grid);
  return solution;
}

function solvePart2(lines: string[]): number {
  const chunks = fileio.chunkLines(lines);
  const [grid, moves, pos] = parseData(chunks);
  const newGrid = expandGrid(grid);
  let newpos = { row: pos.row, col: 2 * pos.col };
  for (const m of moves) {
    newpos = move2(newpos, m, newGrid);
  }

  const solution = scoreGrid(newGrid);

  return solution;
}

const plugin: DayPlugin = {
  name: "Day 15",
  async run({ verbose }) {
    console.log(`Running ${this.name} verbose=${verbose}`);
    const data = await fileio.linesFromFile(`${import.meta.dir}/day15.txt`);

    const start1 = performance.now();
    const solution1 = solvePart1(data);
    const duration1 = (performance.now() - start1).toFixed(3);
    console.log(`${this.name} verbose=${verbose}.Part1 = ${solution1} : Elapsed time: ${duration1}ms`);
    assert.strictEqual(solution1, 1495147, `${this.name} verbose=${verbose}.Part1`);

    const start2 = performance.now();
    const solution2 = solvePart2(data);
    const duration2 = (performance.now() - start2).toFixed(3);
    console.log(`${this.name} verbose=${verbose}.Part2 = ${solution2} : Elapsed time: ${duration2}ms`);
    assert.strictEqual(solution2, 1524905, `${this.name} verbose=${verbose}.Part2`);
  },
};

export default plugin;
