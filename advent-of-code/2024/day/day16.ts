import assert from "node:assert";
import { dijkstraAllPaths, dijkstraPath } from "../lib/graph";
import { Grid } from "../lib/grid";
import { debug, fileio } from "../lib/prelude";
import type { DayPlugin } from "../src/plugin";

export enum Direction {
  Up = 0,
  Right = 1,
  Down = 2,
  Left = 3,
}

export interface Node {
  row: number;
  col: number;
  dir: Direction;
}

// Lambda generator
// Double closure to capture grid
const getNeighbours =
  (grid: Grid<string>) =>
  (node: Node): Generator<Node, void, unknown> =>
    (function* (): Generator<Node, void, unknown> {
      for (const [r, c, v] of grid.neighbours4(node.row, node.col)) {
        if (v === ".") {
          const turns = getTurns(r, c, node);
          if (turns === 0 || turns === 1) {
            yield { row: r, col: c, dir: getHeading(r, c, node) };
          }
        }
      }
    })();

// Double closure to capture goal
const isGoal =
  (goal: Node) =>
  (node: Node): boolean => {
    return node.row === goal.row && node.col === goal.col;
  };

const getCost = (from: Node, to: Node): number => {
  const turns = getTurns(to.row, to.col, from);
  assert(turns !== 2); // We can only go straight or turn 90°
  const distance = Math.abs(to.row - from.row) + Math.abs(to.col - from.col);
  assert(distance === 1); // We move one step at a time
  return distance + 1000 * turns;
};

const getPathCost = (path: Node[]): number => {
  let cost = 0;
  for (let k = 0; k < path.length - 1; k++) {
    cost += getCost(path[k], path[k + 1]);
  }
  return cost;
};

// export private functions for testing
export const __test__ = { parseData, getNeighbours, isGoal, getCost, getPathCost };

function parseData(lines: string[]): [grid: Grid<string>, start: Node, goal: Node] {
  const grid = Grid.fromLines(lines, ".");

  const s = grid.find((val, _r, _c, _grid) => val === "S");
  assert(s?.[0] === "S");
  const start = { row: s[1], col: s[2], dir: Direction.Right };
  grid[start.row][start.col] = ".";

  const g = grid.find((val, _r, _c, _grid) => val === "E");
  assert(g?.[0] === "E");
  const goal = { row: g[1], col: g[2], dir: Direction.Right }; // Direction is irrelavent
  grid[goal.row][goal.col] = ".";

  return [grid, start, goal];
}

const getHeading = (to_row: number, to_col: number, from: Node): Direction => {
  const [dr, dc] = [to_row - from.row, to_col - from.col];
  /* eslint-disable-next-line @typescript-eslint/switch-exhaustiveness-check */
  switch (true) {
    case dr === -1 && dc === 0:
      return Direction.Up;
    case dr === 1 && dc === 0:
      return Direction.Down;
    case dr === 0 && dc === -1:
      return Direction.Left;
    case dr === 0 && dc === 1:
      return Direction.Right;
    default: // Exhaustiveness check
      throw new Error(`Unhandled fallback`);
  }
};

const getTurns = (to_row: number, to_col: number, from: Node): number => {
  const to_dir = getHeading(to_row, to_col, from);
  const turn_count = (to_dir - from.dir + 4) % 4;
  const turns = turn_count == 0 ? 0 : turn_count == 2 ? 2 : 1;
  return turns;
};

function solvePart1(lines: string[]): number {
  const [grid, start, goal] = parseData(lines);

  const dijkstraResult = dijkstraPath(getNeighbours(grid), getCost, start, isGoal(goal));
  assert(dijkstraResult !== null);
  const [cost, _path] = dijkstraResult;
  const solution = cost;

  return solution;
}

function solvePart2(lines: string[]): number {
  const [grid, start, goal] = parseData(lines);

  const dijkstraResult = dijkstraAllPaths(getNeighbours(grid), getCost, start, isGoal(goal));
  const [_minimumCost, paths] = dijkstraResult ?? [0, []];

  const toKey = (node: Node): string => `${node.row},${node.col}`;
  const visited = new Set<string>();
  for (const path of paths) {
    path.forEach((p) => visited.add(toKey(p)));
  }
  const solution = visited.size;

  return solution;
}

const plugin: DayPlugin = {
  name: "Day 16",
  async run({ verbose }) {
    console.log(`Running ${this.name} verbose=${verbose}`);
    const data = await fileio.linesFromFile(`${import.meta.dir}/day16.txt`);

    const start1 = performance.now();
    const solution1 = solvePart1(data);
    const duration1 = (performance.now() - start1).toFixed(3);
    console.log(`${this.name} verbose=${verbose}.Part1 = ${solution1} : Elapsed time: ${duration1}ms`);
    assert.strictEqual(solution1, 78428, `${this.name} verbose=${verbose}.Part1`);

    const start2 = performance.now();
    const solution2 = solvePart2(data);
    const duration2 = (performance.now() - start2).toFixed(3);
    console.log(`${this.name} verbose=${verbose}.Part2 = ${solution2} : Elapsed time: ${duration2}ms`);
    assert.strictEqual(solution2, 463, `${this.name} verbose=${verbose}.Part2`);
  },
};

export default plugin;
