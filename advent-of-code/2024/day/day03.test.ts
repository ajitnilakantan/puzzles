import { describe, expect, test } from "bun:test";
import { fileio } from "../lib/prelude";
import { __test__ } from "./day03";

describe("day03", () => {
  test("part1", async () => {
    const data = `xmul(2,4)%&mul[3,7]!@^do_not_mul(5,5)+mul(32,64]then(mul(11,8)mul(8,5))`;
    const lines = fileio.linesFromString(data);
    const solution = __test__.solvePart1(lines);
    expect(solution).toBe(161);
  });

  test("part2", async () => {
    const data = `xmul(2,4)&mul[3,7]!^don't()_mul(5,5)+mul(32,64](mul(11,8)undo()?mul(8,5))`;
    const lines = fileio.linesFromString(data);
    const solution = __test__.solvePart2(lines);
    expect(solution).toBe(48);
  });
});
