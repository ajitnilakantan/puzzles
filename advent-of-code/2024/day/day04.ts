import assert from "node:assert";
import { Counter } from "../lib/counter";
import { Grid } from "../lib/grid";
import { debug, fileio } from "../lib/prelude";
import type { DayPlugin } from "../src/plugin";

// export private functions for testing
export const __test__ = { wordSearch };

export interface Position {
  row: number;
  col: number;
}
export interface SearchResult {
  count: number;
  paths: Position[][];
}

function wordSearch(grid: Grid<string>, word: string, allowSelfOverlap = true, colinear = true): SearchResult {
  const results: Position[][] = [];
  const numRows = grid.numrows;
  if (numRows === 0 || word.length === 0) return { count: 0, paths: [] };
  const numCols = grid.numcols;

  const upperWord = word.toUpperCase();

  // The 8 possible offset directions
  const directions = Grid.Directions8;

  // Recursive helper function to traverse the grid
  function findPaths(
    row: number,
    col: number,
    index: number,
    currentPath: Position[],
    visitedKeySet: Set<string>
  ): void {
    // Boundary check
    if (row < 0 || row >= numRows || col < 0 || col >= numCols) {
      return;
    }

    const coordKey = `${row},${col}`;

    // Overlap check: Block if self-overlap is disabled and tile was already used
    if (!allowSelfOverlap && visitedKeySet.has(coordKey)) {
      return;
    }

    // Character mismatch check
    if (grid[row][col].toUpperCase() !== upperWord[index]) {
      return;
    }

    // Record the current position
    const newPath = [...currentPath, { row, col }];

    // Base case: Full word successfully found
    if (index === upperWord.length - 1) {
      results.push(newPath);
      return;
    }

    // Branch tracking for strict paths
    let nextVisitedSet = visitedKeySet;
    if (!allowSelfOverlap) {
      nextVisitedSet = new Set(visitedKeySet);
      nextVisitedSet.add(coordKey);
    }

    // Explore all 8 adjacent directions
    if (colinear && index >= 1) {
      const dRow = newPath[1].row - newPath[0].row;
      const dCol = newPath[1].col - newPath[0].col;
      findPaths(row + dRow, col + dCol, index + 1, newPath, nextVisitedSet);
    } else {
      for (const [dRow, dCol] of directions) {
        findPaths(row + dRow, col + dCol, index + 1, newPath, nextVisitedSet);
      }
    }
  }

  // Begin search from every tile in the grid
  for (let row = 0; row < numRows; row++) {
    for (let col = 0; col < numCols; col++) {
      findPaths(row, col, 0, [], new Set<string>());
    }
  }

  return {
    count: results.length,
    paths: results,
  };
}

function solvePart1(lines: string[]): number {
  const grid = Grid.fromLines(lines, ".");
  const result: SearchResult = wordSearch(grid, "XMAS", false);
  const solution = result.count;

  return solution;
}

function solvePart2(lines: string[]): number {
  const grid = Grid.fromLines(lines, ".");
  const result: SearchResult = wordSearch(grid, "MAS", false);
  const counter = new Counter<string>();
  const toKey = (pos: Position): string => `${pos.row},${pos.col}`;
  for (const p of result.paths) {
    // Add the position of "A" (midpoint)
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
  return solution;
}

const plugin: DayPlugin = {
  name: "Day 04",
  async run({ verbose }) {
    console.log(`Running ${this.name} verbose=${verbose}`);
    const data = await fileio.linesFromFile(`${import.meta.dir}/day04.txt`);

    const start1 = performance.now();
    const solution1 = solvePart1(data);
    const duration1 = (performance.now() - start1).toFixed(3);
    console.log(`${this.name}.Part1 = ${solution1} : Elapsed time: ${duration1}ms`);
    assert.strictEqual(solution1, 2397, `${this.name}.Part1`);

    const start2 = performance.now();
    const solution2 = solvePart2(data);
    const duration2 = (performance.now() - start2).toFixed(3);
    console.log(`${this.name}.Part2 = ${solution2} : Elapsed time: ${duration2}ms`);
    assert.strictEqual(solution2, 1824, `${this.name}.Part2`);
  },
};

export default plugin;
