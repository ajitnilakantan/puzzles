import { describe, expect, test } from "bun:test";
import { debug, fileio } from "../lib/prelude";
import { __test__ } from "./day22";

describe("day22", () => {
  const data1 = `1
                10
                100
                2024`;
  const data2 = `1
                 2
                 3
                 2024`;
  const expected1 = [8685429, 4700978, 15273692, 8667524];

  const lines1 = fileio.linesFromString(data1);
  const lines2 = fileio.linesFromString(data2);

  test("part1", async () => {
    const initial = lines1.map((l) => Number(l));
    const after2000 = initial.map((n) => __test__.getNthPseudo(n, 2000));
    expect(after2000).toEqual(expected1);
    const solution = after2000.reduce((acc, val) => acc + val, 0);
    expect(solution).toBe(37327623);
  });

  test("part2", async () => {
    const seeds = lines2.map((l) => Number(l));
    const [maxScore, _maxKey] = __test__.createFirstTable(seeds);
    const solution = maxScore;
    expect(solution).toBe(23);
  });
});
