import { describe, expect, test } from "bun:test";
import { debug, fileio } from "../lib/prelude";
import { __test__ } from "./day11";

describe("day11", () => {
  const data = `0 1 10 99 999
                125 17`;

  const lines = fileio.linesFromString(data);

  test("part1", async () => {
    let line = lines[0];
    let tokens = fileio.tokenize(line);
    let nums = tokens.map<number>((val) => Number(val));
    // First example
    const result = __test__.expandNumbers(nums, 1);

    expect(result).toEqual([1, 2024, 1, 0, 9, 9, 2021976]);
    // Second example
    line = lines[1];
    tokens = fileio.tokenize(line);
    nums = tokens.map<number>((val) => Number(val));
    const solution = __test__.expandNumbers(nums, 25).length;
    expect(solution).toBe(55312);
  });

  test("part2", async () => {
    let line = lines[0];
    let tokens = fileio.tokenize(line);
    let nums = tokens.map<number>((val) => Number(val));
    let mapNums = new Map<number, number>();
    nums.forEach((val) => mapNums.set(val, (mapNums.get(val) ?? 0) + 1));
    // First example
    let result = __test__.expandNumbersPart2(mapNums, 1);
    let solution = 0;
    result.forEach((v, _k, _m) => {
      solution += v;
    });
    expect(solution).toBe(7); // [1,2024,1,0,9,9,2021976];

    // Second example
    line = lines[1];
    tokens = fileio.tokenize(line);
    nums = tokens.map<number>((val) => Number(val));
    mapNums = new Map<number, number>();
    nums.forEach((val) => mapNums.set(val, (mapNums.get(val) ?? 0) + 1));
    result = __test__.expandNumbersPart2(mapNums, 25);
    solution = 0;
    result.forEach((v, _k, _m) => (solution += v));
    expect(solution).toBe(55312);
  });
});
