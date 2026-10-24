import assert from "node:assert";
import { getAllPaths } from "../lib/graph";
import { Grid } from "../lib/grid";
import { debug, fileio } from "../lib/prelude";
import type { DayPlugin } from "../src/plugin";

export type Node = Record<"row" | "col", number>;

const START = 0;
const GOAL = 9;

// Lambda generator
// const counter = ((start = 0) => (function*() { while(true) yield start++; })())();
// const createGen = (start: number) => (function* () { while (true) yield start++; })();
// Double closure to capture grid
const getNeighbours =
  (grid: Grid<number>) =>
  (node: Node): Generator<Node, void, unknown> =>
    (function* (): Generator<Node, void, unknown> {
      for (const [_r, _c, v] of grid.neighbours4(node.row, node.col)) {
        if (v - grid[node.row][node.col] === 1) {
          yield { row: _r, col: _c };
        }
      }
    })();

// Double closure to capture grid
const isGoal =
  (grid: Grid<number>) =>
  (node: Node): boolean => {
    return grid[node.row][node.col] === GOAL;
  };

const toKey = (node: Node): string => `${node.row},${node.col}`;

// export private functions for testing (Typescript limitation - as to be after variable declarations)
export const __test__ = { toString, getNeighbours, isGoal, toKey, START, GOAL };

function toString(grid: Grid<number>): string {
  let ret = "";
  for (const r of Grid.range(grid.numrows)) {
    let l = "";
    for (const c of Grid.range(grid.numrows)) {
      l += grid[r][c] < 0 ? "." : String(grid[r][c]);
    }
    ret += l + "\n";
  }
  return ret;
}

function solvePart1(lines: string[]): number {
  const rawGrid = Grid.fromLines(lines, ".");
  const grid = rawGrid.map((val, _row, _col, _grid) => (val === "." ? -1 : Number(val)), 0);
  const starts: Node[] = [];
  grid.forEach((val, row, col, _grid) => {
    if (val === START) {
      starts.push({ row: row, col: col });
    }
  });
  let solution = 0;
  starts.forEach((start, _) => {
    const trails = new Set<string>();
    for (const p of getAllPaths(start, isGoal(grid), getNeighbours(grid))) {
      if (p.length === 10) {
        trails.add(toKey({ row: p[9].row, col: p[9].col }));
      }
    }
    solution += trails.size;
  });

  return solution;
}

function solvePart2(lines: string[]): number {
  const rawGrid = Grid.fromLines(lines, ".");
  const grid = rawGrid.map((val, _row, _col, _grid) => (val === "." ? -1 : Number(val)), 0);
  const starts: Node[] = [];
  grid.forEach((val, row, col, _grid) => {
    if (val === START) {
      starts.push({ row: row, col: col });
    }
  });
  let solution = 0;
  starts.forEach((start, _) => {
    for (const p of getAllPaths(start, isGoal(grid), getNeighbours(grid))) {
      if (p.length === 10) {
        solution += 1;
      }
    }
  });

  return solution;
}

const plugin: DayPlugin = {
  name: "Day 10",
  async run({ verbose }) {
    console.log(`Running ${this.name} verbose=${verbose}`);
    const data = await fileio.linesFromFile(`${import.meta.dir}/day10.txt`);

    const start1 = performance.now();
    const solution1 = solvePart1(data);
    const duration1 = (performance.now() - start1).toFixed(3);
    console.log(`${this.name} verbose=${verbose}.Part1 = ${solution1} : Elapsed time: ${duration1}ms`);
    assert.strictEqual(solution1, 737, `${this.name} verbose=${verbose}.Part1`);

    const start2 = performance.now();
    const solution2 = solvePart2(data);
    const duration2 = (performance.now() - start2).toFixed(3);
    console.log(`${this.name} verbose=${verbose}.Part2 = ${solution2} : Elapsed time: ${duration2}ms`);
    assert.strictEqual(solution2, 1619, `${this.name} verbose=${verbose}.Part2`);
  },
};

export default plugin;
