import assert from "node:assert";
import { debug, fileio } from "../lib/prelude";
import type { DayPlugin } from "../src/plugin";

// export private functions for testing
export const __test__ = { parseLine, quadrant };

function parseLine(line: string): [pX: number, pY: number, vX: number, vY: number] {
  // Find consecutive digits (number) with optional "-" sign.
  const regex = /-?\d+/gm;

  const matches: RegExpExecArray[] = Array.from(line.matchAll(regex));
  assert(matches.length === 4);
  const [pX, pY, vX, vY] = [Number(matches[0][0]), Number(matches[1][0]), Number(matches[2][0]), Number(matches[3][0])];
  return [pX, pY, vX, vY];
}

// return 1..4,  0 for along the exact midpoint
function quadrant(row: number, col: number, height: number, width: number): number {
  // Assume odd width/height
  assert(height % 2 === 1);
  assert(width % 2 === 1);
  const midX = Math.floor(width / 2);
  const midY = Math.floor(height / 2);
  if (row == midY || col == midX) {
    return 0;
  } else if (row < midY && col < midX) {
    return 1;
  } else if (row < midY && col > midX) {
    return 2;
  } else if (row > midY && col < midX) {
    return 3;
  } else if (row > midY && col > midX) {
    return 4;
  } else {
    assert(false);
  }
}

function solvePart1(lines: string[]): number {
  const [height, width] = [103, 101];
  const robots = lines.map((val) => parseLine(val));
  const mod = (x: number, m: number): number => ((x % m) + m) % m;
  const newRobots = robots.map(([pX, pY, vX, vY]) => [mod(pX + vX * 100, width), mod(pY + vY * 100, height)]);
  const quadrants = [0, 0, 0, 0, 0];
  newRobots.forEach(([px, py]) => (quadrants[quadrant(py, px, height, width)] += 1));
  const solution = quadrants.slice(1).reduce((acc, val) => acc * val, 1);

  return solution;
}

function solvePart2(_lines: string[]): number {
  // Look at the test code for part2 to see how to get this result.
  // It is a semi-manual process.
  const solution = 6493;

  return solution;
}

const plugin: DayPlugin = {
  name: "Day 14",
  async run({ verbose }) {
    console.log(`Running ${this.name} verbose=${verbose}`);
    const data = await fileio.linesFromFile(`${import.meta.dir}/day14.txt`);

    const start1 = performance.now();
    const solution1 = solvePart1(data);
    const duration1 = (performance.now() - start1).toFixed(3);
    console.log(`${this.name} verbose=${verbose}.Part1 = ${solution1} : Elapsed time: ${duration1}ms`);
    assert.strictEqual(solution1, 228457125, `${this.name} verbose=${verbose}.Part1`);

    const start2 = performance.now();
    const solution2 = solvePart2(data);
    const duration2 = (performance.now() - start2).toFixed(3);
    console.log(`${this.name} verbose=${verbose}.Part2 = ${solution2} : Elapsed time: ${duration2}ms`);
    assert.strictEqual(solution2, 6493, `${this.name} verbose=${verbose}.Part2`);
  },
};

export default plugin;
