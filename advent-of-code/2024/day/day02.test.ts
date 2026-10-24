import { describe, expect, test } from "bun:test";
import { fileio } from "../lib/prelude";
import { __test__ } from "./day02";

describe("day02", () => {
  const data = `7 6 4 2 1
                  1 2 7 8 9
                  9 7 6 2 1
                  1 3 2 4 5
                  8 6 4 4 1
                  1 3 6 7 9`;

  const lines = fileio.linesFromString(data);
  const records = __test__.linesToRecords(lines);

  test("part1", async () => {
    const solution = records.reduce<number>((acc, val) => {
      const [ismono] = __test__.isMonotonic(val);
      if (ismono) {
        acc += 1;
      }
      return acc;
    }, 0);
    expect(solution).toBe(2);
  });

  test("part2", async () => {
    const arr = [12, 14, 13, 10, 9];
    const ismono = __test__.isMonotonicPart2(arr);
    expect(ismono).toBe(true);
    const solution = records.reduce<number>((acc, val) => {
      const ismono = __test__.isMonotonicPart2(val);
      if (ismono) {
        acc += 1;
      }
      return acc;
    }, 0);
    expect(solution).toBe(4);
  });
});
