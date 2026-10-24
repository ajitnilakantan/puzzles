import assert from "node:assert";
import { debug, fileio } from "../lib/prelude";
import type { DayPlugin } from "../src/plugin";

// export private functions for testing
export const __test__ = { expandNumbers, expandNumbersPart2 };

function countDigits(num: number): number {
  if (num === 0) return 1;
  return (Math.log10(Math.abs(num)) | 0) + 1;
}

// Pre-calculated powers of 10 for O(1) array lookups
const POWERS_OF_10 = [
  1, 10, 100, 1_000, 10_000, 100_000, 1_000_000, 10_000_000, 100_000_000, 1_000_000_000, 10_000_000_000,
  100_000_000_000, 1_000_000_000_000, 10_000_000_000_000, 100_000_000_000_000, 1_000_000_000_000_000,
];

function powerOf10(num: number): number {
  if (num < 1) return 1; // 10^0 is the baseline for a single digit
  return POWERS_OF_10[num] ?? Math.pow(10, num); // Fallback for extreme numbers
}
function transformNumber(num: number): number[] {
  if (num === 0) {
    return [1];
  } else {
    const digits = countDigits(num);
    if (digits % 2 === 0) {
      const pow = powerOf10(digits / 2);
      return [(num / pow) | 0, num % pow]; // (x|0) is a quick way to truncate a float to int
    } else {
      return [2024 * num];
    }
  }
}

function expandNumbers(nums: number[], generations: number): number[] {
  let result = nums;
  for (let i = 0; i < generations; i++) {
    result = result.flatMap((val) => transformNumber(val));
  }
  return result;
}

// nums is a map of number to the repititions of that number
function expandNumbersPart2(nums: Map<number, number>, generations: number): Map<number, number> {
  let result = nums;
  for (let i = 0; i < generations; i++) {
    const newResult = new Map<number, number>();
    for (const [k, v] of result) {
      for (const val of transformNumber(k)) {
        newResult.set(val, (newResult.get(val) ?? 0) + v);
      }
    }
    result = newResult;
  }
  return result;
}

function solvePart1(lines: string[]): number {
  const line = lines[0];
  const tokens = fileio.tokenize(line);
  const nums = tokens.map<number>((val) => Number(val));
  const solution = expandNumbers(nums, 25).length;
  return solution;
}

function solvePart2(lines: string[]): number {
  const line = lines[0];
  const tokens = fileio.tokenize(line);
  const nums = tokens.map<number>((val) => Number(val));
  const mapNums = new Map<number, number>();
  nums.forEach((val) => mapNums.set(val, (mapNums.get(val) ?? 0) + 1));
  const result = expandNumbersPart2(mapNums, 75);
  let solution = 0;
  result.forEach((v, _k, _m) => (solution += v));

  return solution;
}

const plugin: DayPlugin = {
  name: "Day 11",
  async run({ verbose }) {
    console.log(`Running ${this.name} verbose=${verbose}`);
    const data = await fileio.linesFromFile(`${import.meta.dir}/day11.txt`);

    const start1 = performance.now();
    const solution1 = solvePart1(data);
    const duration1 = (performance.now() - start1).toFixed(3);
    console.log(`${this.name} verbose=${verbose}.Part1 = ${solution1} : Elapsed time: ${duration1}ms`);
    assert.strictEqual(solution1, 183248, `${this.name} verbose=${verbose}.Part1`);

    const start2 = performance.now();
    const solution2 = solvePart2(data);
    const duration2 = (performance.now() - start2).toFixed(3);
    console.log(`${this.name} verbose=${verbose}.Part2 = ${solution2} : Elapsed time: ${duration2}ms`);
    assert.strictEqual(solution2, 218811774248729, `${this.name} verbose=${verbose}.Part2`);
  },
};

export default plugin;
