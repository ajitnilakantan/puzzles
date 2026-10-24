import { describe, expect, test } from "bun:test";
import { Counter } from "../lib/counter";
import { debug, fileio } from "../lib/prelude";
import { __test__ } from "./day01";

describe("day01", () => {
  const data = `3   4
                  4   3
                  2   5
                  1   3
                  3   9
                  3   3`;

  const lines = fileio.linesFromString(data);
  test("part1", async () => {
    const [lhs, rhs] = __test__.splitLines(lines);
    const solution = lhs.reduce<number>((acc, lval, index) => {
      const rval = rhs[index];
      acc = acc + Math.abs(lval - rval);
      return acc;
    }, 0);

    expect(solution).toBe(11);
  });

  test("part2", async () => {
    const [lhs, rhs] = __test__.splitLines(lines);
    const counter = new Counter(rhs);
    const solution = lhs.reduce<number>((acc, val) => {
      acc = acc + val * counter.get(val);
      return acc;
    }, 0);
    expect(solution).toBe(31);
  });
});
