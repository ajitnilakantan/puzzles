import assert from "node:assert";
import { dijkstraPath } from "../lib/graph";
import { Grid } from "../lib/grid";
import { debug, fileio } from "../lib/prelude";
import type { DayPlugin } from "../src/plugin";

// export private functions for testing
export const __test__ = { parseData, getShortestPath, getShortcutPaths, getExtendedShortcutPaths };

export interface Position {
  row: number;
  col: number;
}

function parseData(lines: string[]): [grid: Grid<string>, start: Position, goal: Position] {
  const grid = Grid.fromLines(lines, ".");
  const s = grid.find((val, _r, _c, _grid) => val === "S");
  assert(s?.[0] === "S");
  const start = { row: s[1], col: s[2], broken: false };
  grid[start.row][start.col] = ".";

  const g = grid.find((val, _r, _c, _grid) => val === "E");
  assert(g?.[0] === "E");
  const goal = { row: g[1], col: g[2], broken: false };
  grid[goal.row][goal.col] = ".";
  return [grid, start, goal];
}

// Use A* to find shortest path
function getShortestPath(
  grid: Grid<string>,
  height: number,
  width: number,
  start: Position,
  goal: Position
): Position[] | null {
  const getNeighbours =
    (grid: Grid<string>, height: number, width: number) =>
    (node: Position): Generator<Position, void, unknown> =>
      (function* (): Generator<Position & { broken: boolean }, void, unknown> {
        for (const [dr, dc] of Grid.Directions4) {
          const [r, c] = [node.row + dr, node.col + dc];
          if (r >= 0 && r < height && c >= 0 && c < width && grid[r][c] != "#") {
            yield { row: r, col: c, broken: false };
          }
        }
      })();
  const isGoal =
    (goal: Position) =>
    (node: Position): boolean => {
      return node.row === goal.row && node.col === goal.col;
    };
  const getCost = (from: Position, to: Position): number => {
    return Math.abs(from.row - to.row) + Math.abs(from.col - to.col);
  };

  const path = dijkstraPath(getNeighbours(grid, height, width), getCost, start, isGoal(goal));
  assert(path !== null);
  return path[1];
}

// Find all paths, with wall breaking
function getShortcutPaths(
  grid: Grid<string>,
  height: number,
  width: number,
  shortestPath: Position[],
  targetSavings: number
): number {
  // Map the node to the index along the path. Used to calculate how much time a shortcut saves.
  const shortestPathSet = new Set<string>();
  const shortestPathIndices = new Map<string, number>();
  const toKey = (row: number, col: number): string => `${row},${col}`;
  shortestPath.forEach((val, index) => shortestPathIndices.set(toKey(val.row, val.col), index));
  shortestPath.forEach((val) => shortestPathSet.add(toKey(val.row, val.col)));

  let total = 0;
  for (const [r, c] of Grid.product<number, number>(height, width)) {
    // up/down
    const keyup = toKey(r, c - 1);
    const keydown = toKey(r, c + 1);
    if (c > 0 && c < width - 1 && grid[r][c] === "#" && shortestPathSet.has(keyup) && shortestPathSet.has(keydown)) {
      // Subtract 1 from the savings to take into account move through the wall
      const savings = Math.abs(shortestPathIndices.get(keyup)! - shortestPathIndices.get(keydown)!) - 1;
      if (savings >= targetSavings) {
        total++;
      }
    }
    // left/right
    const keyleft = toKey(r - 1, c);
    const keyright = toKey(r + 1, c);
    if (
      r > 0 &&
      r < height - 1 &&
      grid[r][c] === "#" &&
      shortestPathSet.has(keyleft) &&
      shortestPathSet.has(keyright)
    ) {
      // Subtract 1 from the savings to take into account move through the wall
      const savings = Math.abs(shortestPathIndices.get(keyleft)! - shortestPathIndices.get(keyright)!) - 1;
      if (savings >= targetSavings) {
        total++;
      }
    }
  }
  return total;
}

// Find all paths, with wall breaking up to "maxShortCut" steps.  Filter those that save >= targetSavings
function getExtendedShortcutPaths(
  _grid: Grid<string>,
  _height: number,
  _width: number,
  shortestPath: Position[],
  maxShortCut: number,
  targetSavings: number
): number {
  // Map the node to the index along the path. Used to calculate how much time a shortcut saves.
  const shortestPathSet = new Set<string>();
  const shortestPathIndices = new Map<string, number>();
  const toKey = (row: number, col: number): string => `${row},${col}`;
  shortestPath.forEach((val, index) => shortestPathIndices.set(toKey(val.row, val.col), index));
  shortestPath.forEach((val) => shortestPathSet.add(toKey(val.row, val.col)));

  let total = 0;
  for (const node of shortestPath) {
    const currentIndex = shortestPathIndices.get(toKey(node.row, node.col))!;
    for (const [r, c] of getManhattanCoordinates(node.row, node.col, maxShortCut, true)) {
      const nextKey = toKey(r, c);
      if (shortestPathSet.has(nextKey)) {
        const nextIndex = shortestPathIndices.get(nextKey)!;
        if (nextIndex > currentIndex) {
          const distance = Math.abs(r - node.row) + Math.abs(c - node.col);
          if (nextIndex - currentIndex - distance >= targetSavings) {
            total++;
          }
        }
      }
    }
  }
  return total;
}

/**
 * Returns grid coordinates at a Manhattan distance of n from (r, c).
 * @param r - Center row coordinate
 * @param c - Center column coordinate
 * @param n - Manhattan distance
 * @param inclusive - If true, returns all coordinates within distance <= n. Otherwise, exactly n.
 */
function getManhattanCoordinates(r: number, c: number, n: number, inclusive = false): [number, number][] {
  const coords: [number, number][] = [];

  // Determine which distances to process
  const distances: number[] = [];
  if (inclusive) {
    for (let d = 0; d <= n; d++) distances.push(d);
  } else {
    distances.push(n);
  }

  for (const d of distances) {
    if (d === 0) {
      coords.push([r, c]);
      continue;
    }

    for (let dx = -d; dx <= d; dx++) {
      const dy = d - Math.abs(dx);
      coords.push([r + dx, c + dy]);
      if (dy !== 0) {
        coords.push([r + dx, c - dy]);
      }
    }
  }

  return coords;
}

function solvePart1(lines: string[]): number {
  const [grid, start, goal] = parseData(lines);
  const [height, width] = [grid.numrows, grid.numcols];

  const shortestPath = getShortestPath(grid, height, width, start, goal);
  const solution = getShortcutPaths(grid, height, width, shortestPath!, 100);
  return solution;
}

function solvePart2(lines: string[]): number {
  const [grid, start, goal] = __test__.parseData(lines);
  const [height, width] = [grid.numrows, grid.numcols];

  const shortestPath = getShortestPath(grid, height, width, start, goal)!;
  const solution = getExtendedShortcutPaths(grid, height, width, shortestPath, 20, 100);

  return solution;
}

const plugin: DayPlugin = {
  name: "Day 20",
  async run({ verbose }) {
    console.log(`Running ${this.name} verbose=${verbose}`);
    const data = await fileio.linesFromFile(`${import.meta.dir}/day20.txt`);

    const start1 = performance.now();
    const solution1 = solvePart1(data);
    const duration1 = (performance.now() - start1).toFixed(3);
    console.log(`${this.name} verbose=${verbose}.Part1 = ${solution1} : Elapsed time: ${duration1}ms`);
    assert.strictEqual(solution1, 1395, `${this.name} verbose=${verbose}.Part1`);

    const start2 = performance.now();
    const solution2 = solvePart2(data);
    const duration2 = (performance.now() - start2).toFixed(3);
    console.log(`${this.name} verbose=${verbose}.Part2 = ${solution2} : Elapsed time: ${duration2}ms`);
    assert.strictEqual(solution2, 993178, `${this.name} verbose=${verbose}.Part2`);
  },
};

export default plugin;
