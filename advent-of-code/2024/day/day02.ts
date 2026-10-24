import assert from "node:assert";
import { fileio } from "../lib/prelude";
import type { DayPlugin } from "../src/plugin";

// export private functions for testing
export const __test__ = {
  linesToRecords,
  isMonotonic,
  isMonotonicPart2,
};

function linesToRecords(lines: string[]): number[][] {
  const records = lines.map<number[]>((line) => {
    const tokens = fileio.tokenize(line);
    const record = tokens.map<number>((val) => Number(val));
    return record;
  });
  return records;
}

function isMonotonic(nums: number[], min = 1, max = 3): [isMonotic: boolean, failureIndex: number] {
  let hasIncreased = false;
  let hasDecreased = false;

  const checkChanged = (val: number, index: number, nums: number[], min = 1, max = 3): boolean => {
    if (index === 0) return true; // Skip the first element as there is no previous element

    const prev = nums[index - 1];
    if (val > prev) hasIncreased = true;
    if (val < prev) hasDecreased = true;
    const diff = Math.abs(val - prev);
    const diffOk = diff >= min && diff <= max;

    // It is monotonic as long as it hasn't BOTH increased AND decreased
    return !(hasIncreased && hasDecreased) && diffOk;
  };

  const index = nums.findIndex((val, idx, arr) => {
    return !checkChanged(val, idx, arr, min, max);
  });
  return [index == -1, index];
}

function isMonotonicPart2(nums: number[], min = 1, max = 3): boolean {
  const [ismono, failIndex] = isMonotonic(nums, min, max);
  if (ismono) {
    return true;
  } else {
    // Must have a failure
    assert.notEqual(failIndex, -1);
    // Faulure cannot be the first index
    assert.notEqual(failIndex, 0);
    // Try removing the failpoint, and the previous
    for (let i = 0; i <= 2; i++) {
      if (failIndex - i >= 0) {
        const [ismono] = isMonotonic(nums.toSpliced(failIndex - i, 1), min, max);
        if (ismono) {
          return true;
        }
      }
    }

    return false;
  }
}

function solvePart1(lines: string[]): number {
  const records = linesToRecords(lines);
  const solution = records.reduce<number>((acc, val) => {
    const [ismono] = isMonotonic(val);
    if (ismono) {
      acc += 1;
    }
    return acc;
  }, 0);

  return solution;
}

function solvePart2(lines: string[]): number {
  const records = linesToRecords(lines);
  const solution = records.reduce<number>((acc, val) => {
    const ismono = isMonotonicPart2(val);
    if (ismono) {
      acc += 1;
    }
    return acc;
  }, 0);
  return solution;
}

const plugin: DayPlugin = {
  name: "Day 02",
  async run({ verbose }) {
    console.log(`Running ${this.name} verbose=${verbose}`);
    const data = await fileio.linesFromFile(`${import.meta.dir}/day02.txt`);

    const start1 = performance.now();
    const solution1 = solvePart1(data);
    const duration1 = (performance.now() - start1).toFixed(3);
    console.log(`${this.name}.Part1 = ${solution1} : Elapsed time: ${duration1}ms`);
    assert.strictEqual(solution1, 510, `${this.name}.Part1`);

    const start2 = performance.now();
    const solution2 = solvePart2(data);
    const duration2 = (performance.now() - start2).toFixed(3);
    console.log(`${this.name}.Part2 = ${solution2} : Elapsed time: ${duration2}ms`);
    assert.strictEqual(solution2, 553, `${this.name}.Part2`);
  },
};

export default plugin;
