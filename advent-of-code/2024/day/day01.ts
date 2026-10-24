import assert from "node:assert";
import { Counter } from "../lib/counter";
import { fileio } from "../lib/prelude";
import type { DayPlugin } from "../src/plugin";

// export private functions for testing
export const __test__ = {
  splitLines,
};

function splitLines(lines: string[]): [number[], number[]] {
  let [lhs, rhs] = lines.reduce<[number[], number[]]>(
    (acc, line) => {
      const [str1, str2] = fileio.tokenize(line, " ");
      acc[0].push(Number(str1));
      acc[1].push(Number(str2));
      return acc;
    },
    [[], []]
  );
  lhs = lhs.toSorted((a, b) => a - b);
  rhs = rhs.toSorted((a, b) => a - b);
  return [lhs, rhs];
}

function solvePart1(lines: string[]): number {
  const [lhs, rhs] = splitLines(lines);
  const solution = lhs.reduce<number>((acc, lval, index) => {
    const rval = rhs[index];
    acc = acc + Math.abs(lval - rval);
    return acc;
  }, 0);

  return solution;
}

function solvePart2(lines: string[]): number {
  const [lhs, rhs] = splitLines(lines);
  const counter = new Counter(rhs);
  const solution = lhs.reduce<number>((acc, val) => {
    acc = acc + val * counter.get(val);
    return acc;
  }, 0);
  return solution;
}

const plugin: DayPlugin = {
  name: "Day 01",
  async run({ verbose }) {
    console.log(`Running ${this.name} verbose=${verbose}`);
    const data = await fileio.linesFromFile(`${import.meta.dir}/day01.txt`);

    const start1 = performance.now();
    const solution1 = solvePart1(data);
    const duration1 = (performance.now() - start1).toFixed(3);
    console.log(`${this.name} verbose=${verbose}.Part1 = ${solution1} : Elapsed time: ${duration1}ms`);
    assert.strictEqual(solution1, 1341714, `${this.name} verbose=${verbose}.Part1`);

    const start2 = performance.now();
    const solution2 = solvePart2(data);
    const duration2 = (performance.now() - start2).toFixed(3);
    console.log(`${this.name} verbose=${verbose}.Part2 = ${solution2} : Elapsed time: ${duration2}ms`);
    assert.strictEqual(solution2, 27384707, `${this.name} verbose=${verbose}.Part2`);
  },
};

export default plugin;
