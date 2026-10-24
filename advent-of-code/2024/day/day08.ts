import assert from "node:assert";
import { Grid } from "../lib/grid";
import { debug, fileio } from "../lib/prelude";
import type { DayPlugin } from "../src/plugin";

// export private functions for testing
export const __test__ = { allPairs, getAntennas, getAntinodes, getAntinodesPart2 };

// Generator for allPairs combinations
function* allPairs<T>(arr: T[]): Generator<[T, T], void, unknown> {
  for (let i = 0; i < arr.length; i++) {
    for (let j = i + 1; j < arr.length; j++) {
      yield [arr[i], arr[j]];
    }
  }
}

// Get a Map of antenna name to list of [row, col]
function getAntennas(grid: Grid<string>): Map<string, [number, number][]> {
  const antennas = new Map<string, [number, number][]>();
  const callback = (value: string, row: number, col: number, _grid: Grid<string>): void => {
    if (value !== ".") {
      const current = antennas.get(value) ?? [];
      current.push([row, col]);
      antennas.set(value, current);
    }
  };
  grid.forEach(callback);
  return antennas;
}

// Get a list of [row, col] of the "antinodes"
function getAntinodes(antennas: Map<string, [number, number][]>, _grid: Grid<string>): [number, number][] {
  const coords: [number, number][] = [];
  for (const [_key, value] of antennas) {
    for (const pair of allPairs(value)) {
      const [[r1, c1], [r2, c2]] = pair;
      const [dr, dc] = [r2 - r1, c2 - c1];
      coords.push([r1 - dr, c1 - dc]);
      coords.push([r2 + dr, c2 + dc]);
    }
  }
  return coords;
}

// Get a list of [row, col] of the "antinodes"
function getAntinodesPart2(antennas: Map<string, [number, number][]>, grid: Grid<string>): [number, number][] {
  const coords: [number, number][] = [];
  for (const [_key, value] of antennas) {
    for (const pair of allPairs(value)) {
      const [[r1, c1], [r2, c2]] = pair;
      const [dr, dc] = [r2 - r1, c2 - c1];
      coords.push([r1, c1]);
      coords.push([r2, c2]);
      let r, c;
      [r, c] = [r1 - dr, c1 - dc];
      while (r >= 0 && r < grid.numrows && c >= 0 && c < grid.numcols) {
        coords.push([r, c]);
        [r, c] = [r - dr, c - dc];
      }
      [r, c] = [r2 + dr, c2 + dc];
      while (r >= 0 && r < grid.numrows && c >= 0 && c < grid.numcols) {
        coords.push([r, c]);
        [r, c] = [r + dr, c + dc];
      }
    }
  }
  return coords;
}

function solvePart1(lines: string[]): number {
  const grid = Grid.fromLines(lines, ".");
  const antennas = getAntennas(grid);
  const antinodes = getAntinodes(antennas, grid);
  const toKey = (row: number, col: number): string => `${row},${col}`;
  const validAntinodes = antinodes.filter(([r, c]) => r >= 0 && r < grid.numrows && c >= 0 && c < grid.numcols);
  // Sets need primitive key
  const uniqueAntinodes = new Set(validAntinodes.map(([r, c]) => toKey(r, c)));
  const solution = uniqueAntinodes.size;

  return solution;
}

function solvePart2(lines: string[]): number {
  const grid = Grid.fromLines(lines, ".");
  const antennas = getAntennas(grid);
  const antinodes = getAntinodesPart2(antennas, grid);
  const toKey = (row: number, col: number): string => `${row},${col}`;
  const validAntinodes = antinodes.filter(([r, c]) => r >= 0 && r < grid.numrows && c >= 0 && c < grid.numcols);
  // Sets need primitive key
  const uniqueAntinodes = new Set(validAntinodes.map(([r, c]) => toKey(r, c)));
  const solution = uniqueAntinodes.size;

  return solution;
}

const plugin: DayPlugin = {
  name: "Day 08",
  async run({ verbose }) {
    console.log(`Running ${this.name} verbose=${verbose}`);
    const data = await fileio.linesFromFile(`${import.meta.dir}/day08.txt`);

    const start1 = performance.now();
    const solution1 = solvePart1(data);
    const duration1 = (performance.now() - start1).toFixed(3);
    console.log(`${this.name} verbose=${verbose}.Part1 = ${solution1} : Elapsed time: ${duration1}ms`);
    assert.strictEqual(solution1, 318, `${this.name} verbose=${verbose}.Part1`);

    const start2 = performance.now();
    const solution2 = solvePart2(data);
    const duration2 = (performance.now() - start2).toFixed(3);
    console.log(`${this.name} verbose=${verbose}.Part2 = ${solution2} : Elapsed time: ${duration2}ms`);
    assert.strictEqual(solution2, 1126, `${this.name} verbose=${verbose}.Part2`);
  },
};

export default plugin;
