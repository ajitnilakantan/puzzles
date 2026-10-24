import assert from "node:assert";
import { debug, fileio } from "../lib/prelude";
import type { DayPlugin } from "../src/plugin";

// export private functions for testing
export const __test__ = { solveLinearSystem, parseChunk };

/**
 * Solves a system of two linear equations with two variables using Cramer's Rule.
 * Equation 1: a1*x + b1*y = c1
 * Equation 2: a2*x + b2*y = c2
 */
function solveLinearSystem(
  a1: number,
  b1: number,
  c1: number,
  a2: number,
  b2: number,
  c2: number
): [number, number] | undefined {
  // Calculate the main determinant (D)
  const determinant = a1 * b2 - b1 * a2;

  // If the determinant is zero, lines are parallel or coincident
  if (determinant === 0) {
    // Check if they are the exact same line (infinite solutions)
    const determinantX = c1 * b2 - b1 * c2;
    if (determinantX === 0) {
      console.log("Infinite solutions (lines are coincident)");
      return undefined;
    }
    console.log("No solution (lines are parallel)");
    return undefined;
  }

  // Calculate determinants for X and Y using Cramer's Rule
  const x = (c1 * b2 - b1 * c2) / determinant;
  const y = (a1 * c2 - c1 * a2) / determinant;

  return [x, y];
}

function parseChunk(lines: string[]): [aX: number, aY: number, bX: number, bY: number, prizeX: number, prizeY: number] {
  assert(lines.length === 3);
  // Find consecutive digits (number)
  const regex = /(\d+)/gm;

  let matches: RegExpExecArray[] = Array.from(lines[0].matchAll(regex));
  assert(matches.length === 2);
  const [aX, aY] = [Number(matches[0][0]), Number(matches[1][0])];

  matches = Array.from(lines[1].matchAll(regex));
  assert(matches.length === 2);
  const [bX, bY] = [Number(matches[0][0]), Number(matches[1][0])];

  matches = Array.from(lines[2].matchAll(regex));
  assert(matches.length === 2);
  const [prizeX, prizeY] = [Number(matches[0][0]), Number(matches[1][0])];

  return [aX, aY, bX, bY, prizeX, prizeY];
}

function solvePart1(lines: string[]): number {
  const chunks = fileio.chunkLines(lines);
  let solution = 0;
  for (const chunk of chunks) {
    const [aX, aY, bX, bY, prizeX, prizeY] = parseChunk(chunk);
    const result = solveLinearSystem(aX, bX, prizeX, aY, bY, prizeY);
    if (result) {
      const [a, b] = result;
      if (a >= 0 && b >= 0 && a === Math.trunc(a) && b === Math.trunc(b)) {
        solution += 3 * a + 1 * b;
      }
    }
  }

  return solution;
}

function solvePart2(lines: string[]): number {
  const chunks = fileio.chunkLines(lines);
  let solution = 0;
  for (const chunk of chunks) {
    const [aX, aY, bX, bY, prizeX, prizeY] = parseChunk(chunk);
    const newPrizeX = prizeX + 10000000000000;
    const newPrizeY = prizeY + 10000000000000;
    const result = solveLinearSystem(aX, bX, newPrizeX, aY, bY, newPrizeY);
    if (result) {
      const [a, b] = result;
      if (a >= 0 && b >= 0 && a === Math.trunc(a) && b === Math.trunc(b)) {
        solution += 3 * a + 1 * b;
      }
    }
  }

  return solution;
}

const plugin: DayPlugin = {
  name: "Day 13",
  async run({ verbose }) {
    console.log(`Running ${this.name} verbose=${verbose}`);
    const data = await fileio.linesFromFile(`${import.meta.dir}/day13.txt`);

    const start1 = performance.now();
    const solution1 = solvePart1(data);
    const duration1 = (performance.now() - start1).toFixed(3);
    console.log(`${this.name} verbose=${verbose}.Part1 = ${solution1} : Elapsed time: ${duration1}ms`);
    assert.strictEqual(solution1, 32067, `${this.name} verbose=${verbose}.Part1`);

    const start2 = performance.now();
    const solution2 = solvePart2(data);
    const duration2 = (performance.now() - start2).toFixed(3);
    console.log(`${this.name} verbose=${verbose}.Part2 = ${solution2} : Elapsed time: ${duration2}ms`);
    assert.strictEqual(solution2, 92871736253789, `${this.name} verbose=${verbose}.Part2`);
  },
};

export default plugin;
