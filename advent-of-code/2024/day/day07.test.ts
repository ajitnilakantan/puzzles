import { describe, expect, test } from "bun:test";
import { debug, fileio } from "../lib/prelude";
import { __test__ } from "./day07";

describe("day07", () => {
  const data = `190: 10 19
                3267: 81 40 27
                83: 17 5
                156: 15 6
                7290: 6 8 6 15
                161011: 16 10 13
                192: 17 8 14
                21037: 9 7 18 13
                292: 11 6 16 20`;

  const lines = fileio.linesFromString(data);

  test("part1_check recursive", async () => {
    const nums = [1, 2, 3];
    const allValues = __test__.getAllValues(nums);
    // (1+2+3=6, 1+2*3=9 left-to-right is 9, 1*2+3=5, 1*2*3=6)
    expect(allValues).toEqual([6, 9, 5]);
  });

  test("part1", async () => {
    const data = __test__.parseData(lines);
    const solution = data.reduce<number>((acc, val) => {
      const target = val.target;
      const vals = val.vals;
      const allValues = new Set(__test__.getAllValues(vals));
      if (allValues.has(target)) {
        return acc + target;
      } else {
        return acc;
      }
    }, 0);

    expect(solution).toBe(3749);
  });

  test("part2_check recursive", async () => {
    let nums = [15, 6];
    let allValues = new Set(__test__.getAllValuesPart2(nums));
    expect(allValues).toContain(156);

    nums = [6, 8, 6, 15];
    allValues = new Set(__test__.getAllValuesPart2(nums));
    expect(allValues).toContain(7290);

    nums = [17, 8, 14];
    allValues = new Set(__test__.getAllValuesPart2(nums));
    expect(allValues).toContain(192);
  });

  test("part2", async () => {
    const data = __test__.parseData(lines);
    const solution = data.reduce<number>((acc, val) => {
      const target = val.target;
      const vals = val.vals;
      const allValues = new Set(__test__.getAllValuesPart2(vals, target));
      if (allValues.has(target)) {
        return acc + target;
      } else {
        return acc;
      }
    }, 0);

    expect(solution).toBe(11387);
  });
});
