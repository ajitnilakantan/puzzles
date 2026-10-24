import assert from "node:assert";
import { debug, fileio } from "../lib/prelude";
import { Grid }  from "../lib/grid";
import type { DayPlugin } from "../src/plugin";

// export private functions for testing
export const __test__ = {parseData, countNoOverlap};

function parseData(lines: string[]) : [locks: number[][], keys: number[][], height: number, width: number] {
  const chunks = fileio.chunkLines(lines);
  const locks: number[][] = [];
  const keys: number[][] = []
  for (const chunk of chunks) {
    const grid =  Grid.fromLines(chunk, ".");
    assert(grid.numrows === 7 && grid.numcols === 5);
    const values = Array(grid.numcols).fill(-1);
    grid.forEach((val, _row, col, _grid):void => {if (val === "#") values[col]++;});
    if (Grid.range(grid.numcols).every(c => grid[0][c] === "#")) {
      locks.push(values);
    } else {
      keys.push(values);
    }
  }
  return [locks, keys, 7, 5];
}

function countNoOverlap(locks: number[][], keys: number[][], height: number): number {
  let count = 0;
  for (const [locknum, keynum] of Grid.product<number,number>(locks.length, keys.length)) {
    if (locks[locknum].every((_v, idx) => locks[locknum][idx] + keys[keynum][idx] < height-1)) {
      count++;
    }
  }
  return count;
}

function solvePart1(lines: string[]): number {
  const [locks, keys, height, _width] = parseData(lines);
  const solution = countNoOverlap(locks, keys, height);

  return solution;
}

function solvePart2(_lines: string[]): number {
  const solution = 0;

  return solution;
}

const plugin: DayPlugin = {
  name: "Day 25",
  async run({ verbose }) {
    console.log(`Running ${this.name} verbose=${verbose}`);
    const data = await fileio.linesFromFile(`${import.meta.dir}/day25.txt`);

    const start1 = performance.now();
    const solution1 = solvePart1(data);
    const duration1 = (performance.now() - start1).toFixed(3);
    console.log(`${this.name}.Part1 = ${solution1} : Elapsed time: ${duration1}ms`);
    assert.strictEqual(solution1, 3451, `${this.name}.Part1`);

    const start2 = performance.now();
    const solution2 = solvePart2(data);
    const duration2 = (performance.now() - start2).toFixed(3);
    console.log(`${this.name}.Part2 = ${solution2} : Elapsed time: ${duration2}ms`);
    assert.strictEqual(solution2, 0, `${this.name}.Part2`);
  },
};

export default plugin;
