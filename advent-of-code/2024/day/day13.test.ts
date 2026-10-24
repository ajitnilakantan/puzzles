import { describe, expect, test } from "bun:test";
import { debug, fileio } from "../lib/prelude";
import { __test__ } from "./day13";

describe("day13", () => {
  const data = `Button A: X+94, Y+34
                Button B: X+22, Y+67
                Prize: X=8400, Y=5400

                Button A: X+26, Y+66
                Button B: X+67, Y+21
                Prize: X=12748, Y=12176

                Button A: X+17, Y+86
                Button B: X+84, Y+37
                Prize: X=7870, Y=6450

                Button A: X+69, Y+23
                Button B: X+27, Y+71
                Prize: X=18641, Y=10279`;

  const lines = fileio.linesFromString(data);
  const chunks = fileio.chunkLines(lines);

  test("part1_equation", async () => {
    // Single unique intersection
    // 2x + 1y = 7
    // 1x - 1y = 2
    const result1 = __test__.solveLinearSystem(2, 1, 7, 1, -1, 2);
    expect(result1).toEqual([3, 1]);

    // Parallel lines (No solution)
    // 1x + 2y = 5
    // 2x + 4y = 12
    const result2 = __test__.solveLinearSystem(1, 2, 5, 2, 4, 12);
    expect(result2).toEqual(undefined);

    // Overlapping lines (No solution)
    // 1x + 2y = 5
    // 1x + 2y = 5
    const result3 = __test__.solveLinearSystem(1, 2, 5, 1, 2, 5);
    expect(result3).toEqual(undefined);
    const solution = 0;
    expect(solution).toBe(0);
  });

  test("part1", async () => {
    let solution = 0;
    for (const chunk of chunks) {
      const [aX, aY, bX, bY, prizeX, prizeY] = __test__.parseChunk(chunk);
      const result = __test__.solveLinearSystem(aX, bX, prizeX, aY, bY, prizeY);
      if (result) {
        const [a, b] = result;
        if (a >= 0 && b >= 0 && a === Math.trunc(a) && b === Math.trunc(b)) {
          solution += 3 * a + 1 * b;
        }
      }
    }
    expect(solution).toBe(480);
  });
  test("part2", async () => {
    let solution = 0;
    for (const chunk of chunks) {
      const [aX, aY, bX, bY, prizeX, prizeY] = __test__.parseChunk(chunk);
      const newPrizeX = prizeX + 10000000000000;
      const newPrizeY = prizeY + 10000000000000;
      const result = __test__.solveLinearSystem(aX, bX, newPrizeX, aY, bY, newPrizeY);
      if (result) {
        const [a, b] = result;
        if (a >= 0 && b >= 0 && a === Math.trunc(a) && b === Math.trunc(b)) {
          solution += 3 * a + 1 * b;
        }
      }
    }
    expect(solution).toBe(875318608908);
  });
});
