import assert from "node:assert";
import { debug, fileio } from "../lib/prelude";
import type { DayPlugin } from "../src/plugin";

// export private functions for testing
export const __test__ = { parseData, getAllValues, getAllValuesPart2 };

// Parse data.  Return array of [number, [nums..]]
function parseData(lines: string[]): { target: number; vals: number[] }[] {
  return lines.map((line) => {
    const [strTarget, strVals] = fileio.tokenize(line, ":");
    const target = Number(strTarget);
    const vals = fileio.tokenize(strVals, "\x20").map<number>((val) => Number(val));
    return { target: target, vals: vals };
  });
}

/*
 * Recursively build up all possible expressions from an array inserting + or * in between
 * Apply operators stricly left to right (no * precedence)
 */
function getAllValues(arr: number[]): number[] {
  if (arr.length === 0) return [];

  const results = new Set<number>();

  function backtrack(index: number, currentVal: number): void {
    // Base case: if we have processed all numbers, save the final value
    if (index === arr.length) {
      results.add(currentVal);
      return;
    }

    const nextNum = arr[index];

    // Branch 1: Try addition
    backtrack(index + 1, currentVal + nextNum);

    // Branch 2: Try multiplication
    backtrack(index + 1, currentVal * nextNum);
  }

  // Start the recursion with the second element, initializing the total with the first element
  backtrack(1, arr[0]);

  return Array.from(results);
}

// Pre-calculated powers of 10 for O(1) array lookups
const POWERS_OF_10 = [
  1, 10, 100, 1_000, 10_000, 100_000, 1_000_000, 10_000_000, 100_000_000, 1_000_000_000, 10_000_000_000,
  100_000_000_000, 1_000_000_000_000, 10_000_000_000_000, 100_000_000_000_000, 1_000_000_000_000_000,
];

function closestCeilPowerOf10(num: number): number {
  if (num < 1) return 1; // 10^0 is the baseline for a single digit
  const digits = Math.floor(Math.log10(num)) + 1;
  return POWERS_OF_10[digits] ?? Math.pow(10, digits); // Fallback for extreme numbers
}

/*
 * Recursively build up all possible expressions from an array inserting + or * in between
 * Apply operators stricly left to right (no * precedence)
 * Handle inserting the || operator to concatenate numbers
 * Pass in target for an early exit
 */
function getAllValuesPart2(arr: number[], target = 0): number[] {
  if (arr.length === 0) return [];

  const results = new Set<number>();

  function backtrack(index: number, currentVal: number): void {
    // Base case: if we have processed all numbers, save the final value
    if (index === arr.length) {
      results.add(currentVal);
      return;
    }

    if (target !== 0 && currentVal > target) {
      return;
    }

    const nextNum = arr[index];

    // Handle || concatenate
    if (index < arr.length || index === 1) {
      // Branch 0: Try concatenate
      // backtrack(index + 1, Number(String(currentVal) + String(nextNum)));
      backtrack(index + 1, currentVal * closestCeilPowerOf10(nextNum) + nextNum);
    }

    // Branch 1: Try addition
    backtrack(index + 1, currentVal + nextNum);

    // Branch 2: Try multiplication
    backtrack(index + 1, currentVal * nextNum);
  }

  // Start the recursion with the second element, initializing the total with the first element
  backtrack(1, arr[0]);

  return Array.from(results);
}

// Example usage:
function solvePart1(lines: string[]): number {
  const data = parseData(lines);
  const solution = data.reduce<number>((acc, val) => {
    const target = val.target;
    const vals = val.vals;
    const allValues = new Set(getAllValues(vals));
    if (allValues.has(target)) {
      return acc + target;
    } else {
      return acc;
    }
  }, 0);
  return solution;
}

function solvePart2(lines: string[]): number {
  const data = parseData(lines);
  const solution = data.reduce<number>((acc, val) => {
    const target = val.target;
    const vals = val.vals;
    const allValues = new Set(getAllValuesPart2(vals));
    if (allValues.has(target)) {
      return acc + target;
    } else {
      return acc;
    }
  }, 0);

  return solution;
}

const plugin: DayPlugin = {
  name: "Day 07",
  async run({ verbose }) {
    console.log(`Running ${this.name} verbose=${verbose}`);
    const data = await fileio.linesFromFile(`${import.meta.dir}/day07.txt`);

    const start1 = performance.now();
    const solution1 = solvePart1(data);
    const duration1 = (performance.now() - start1).toFixed(3);
    console.log(`${this.name} verbose=${verbose}.Part1 = ${solution1} : Elapsed time: ${duration1}ms`);
    assert.strictEqual(solution1, 1298103531759, `${this.name} verbose=${verbose}.Part1`);

    const start2 = performance.now();
    const solution2 = solvePart2(data);
    const duration2 = (performance.now() - start2).toFixed(3);
    console.log(`${this.name} verbose=${verbose}.Part2 = ${solution2} : Elapsed time: ${duration2}ms`);
    assert.strictEqual(solution2, 140575048428831, `${this.name} verbose=${verbose}.Part2`);
  },
};

export default plugin;
