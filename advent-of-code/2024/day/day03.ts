import assert from "node:assert";
import { fileio } from "../lib/prelude";
import type { DayPlugin } from "../src/plugin";

// export private functions for testing.
export const __test__ = { solvePart1, solvePart2 };

function solvePart1(lines: string[]): number {
  const data: string = lines.join();
  const regex = /mul\((\d{1,3}),(\d{1,3})\)/g;
  const matches: RegExpExecArray[] = Array.from(data.matchAll(regex));
  const solution = matches.reduce<number>((acc, val) => acc + Number(val[1]) * Number(val[2]), 0);

  return solution;
}

function solvePart2(lines: string[]): number {
  const data = lines.join();
  const regex = /mul\((\d{1,3}),(\d{1,3})\)|don[']t|do/g;
  const matches = [...data.matchAll(regex)];
  let solution = 0;
  let calc = true;
  for (const match of matches) {
    switch (match[0]) {
      case "don't":
        calc = false;
        break;
      case "do":
        calc = true;
        break;
      default:
        assert.ok(match.length === 3, `match.length = ${match.length} expect 3 match=${match}`);
        assert.ok(match[0].length > 3);
        assert.strictEqual(match[0].slice(0, 3), "mul");
        if (calc) {
          solution += Number(match[1]) * Number(match[2]);
        }
    }
  }

  return solution;
}

const plugin: DayPlugin = {
  name: "Day 03",
  async run({ verbose }) {
    console.log(`Running ${this.name} verbose=${verbose}`);
    const data = await fileio.linesFromFile(`${import.meta.dir}/day03.txt`);

    const start1 = performance.now();
    const solution1 = solvePart1(data);
    const duration1 = (performance.now() - start1).toFixed(3);
    console.log(`${this.name}.Part1 = ${solution1} : Elapsed time: ${duration1}ms`);
    assert.strictEqual(solution1, 170807108, `${this.name}.Part1`);

    const start2 = performance.now();
    const solution2 = solvePart2(data);
    const duration2 = (performance.now() - start2).toFixed(3);
    console.log(`${this.name}.Part2 = ${solution2} : Elapsed time: ${duration2}ms`);
    assert.strictEqual(solution2, 74838033, `${this.name}.Part2`);
  },
};

export default plugin;
