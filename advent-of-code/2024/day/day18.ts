import assert from "node:assert";
import { dijkstraPath } from "../lib/graph";
import { Grid } from "../lib/grid";
import { debug, fileio } from "../lib/prelude";
import type { DayPlugin } from "../src/plugin";

// export private functions for testing
export const __test__ = { pathLength, firstFailure };

export interface Position {
  row: number;
  col: number;
}

// Return the path length, -1 if none found.
function pathLength(height: number, width: number, fallen: Position[], numFallen: number): number {
  const fallenSet = new Set<string>();
  const toKey = (pos: Position): string => `${pos.row},${pos.col}`;
  fallen.slice(0, numFallen).forEach((n) => fallenSet.add(toKey(n)));

  const getNeighbours =
    (height: number, width: number, fallenSet: Set<string>) =>
    (node: Position): Generator<Position, void, unknown> =>
      (function* (): Generator<Position, void, unknown> {
        for (const [dr, dc] of Grid.Directions4) {
          const [r, c] = [node.row + dr, node.col + dc];
          if (r >= 0 && r < height && c >= 0 && c < width && !fallenSet.has(toKey({ row: r, col: c }))) {
            yield { row: r, col: c };
          }
        }
      })();
  const isGoal =
    (height: number, width: number) =>
    (node: Position): boolean => {
      return node.row === height - 1 && node.col === width - 1;
    };
  const getCost = (from: Position, to: Position): number => {
    return Math.abs(from.row - to.row) + Math.abs(from.col - to.col);
  };
  const start = { row: 0, col: 0 };
  const dijkstraResult = dijkstraPath(getNeighbours(height, width, fallenSet), getCost, start, isGoal(height, width));
  if (dijkstraResult === null) {
    return -1;
  } else {
    const [totalCost, _pathNodes] = dijkstraResult;
    return totalCost;
  }
}

// Run a binary search for first failure. -1 on not found.
function firstFailure(height: number, width: number, fallen: Position[]): number {
  let [bottom, top, mid] = [0, fallen.length - 1, Math.trunc(fallen.length / 2)];
  while (Math.abs(bottom - top) > 1) {
    if (pathLength(height, width, fallen, mid) === -1) {
      // failed. look earlier.
      top = mid - 1;
      mid = Math.trunc((top + bottom) / 2);
    } else {
      // success. look later for failure.
      bottom = mid + 1;
      mid = Math.trunc((top + bottom) / 2);
    }
  }
  return top;
}

function solvePart1(lines: string[]): number {
  const [height, width] = [71, 71];
  const numFallen = 1024;
  const fallen: Position[] = lines.map((s) => {
    const toks = fileio.tokenize(s, ",");
    return { row: Number(toks[0]), col: Number(toks[1]) };
  });

  const solution = pathLength(height, width, fallen, numFallen);

  return solution;
}

function solvePart2(lines: string[]): string {
  const [height, width] = [71, 71];
  const fallen: Position[] = lines.map((s) => {
    const toks = fileio.tokenize(s, ",");
    return { row: Number(toks[0]), col: Number(toks[1]) };
  });

  const failPos = firstFailure(height, width, fallen);
  const solution = `${fallen[failPos].row},${fallen[failPos].col}`;

  return solution;
}

const plugin: DayPlugin = {
  name: "Day 18",
  async run({ verbose }) {
    console.log(`Running ${this.name} verbose=${verbose}`);
    const data = await fileio.linesFromFile(`${import.meta.dir}/day18.txt`);

    const start1 = performance.now();
    const solution1 = solvePart1(data);
    const duration1 = (performance.now() - start1).toFixed(3);
    console.log(`${this.name} verbose=${verbose}.Part1 = ${solution1} : Elapsed time: ${duration1}ms`);
    assert.strictEqual(solution1, 312, `${this.name} verbose=${verbose}.Part1`);

    const start2 = performance.now();
    const solution2 = solvePart2(data);
    const duration2 = (performance.now() - start2).toFixed(3);
    console.log(`${this.name} verbose=${verbose}.Part2 = ${solution2} : Elapsed time: ${duration2}ms`);
    assert.strictEqual(solution2, "28,26", `${this.name} verbose=${verbose}.Part2`);
  },
};

export default plugin;
