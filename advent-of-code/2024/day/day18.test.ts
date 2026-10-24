import { describe, expect, test } from "bun:test";
import { debug, fileio } from "../lib/prelude";
import { __test__ } from "./day18";
import type { Position } from "./day18";

describe("day18", () => {
  const data = `5,4
                4,2
                4,5
                3,0
                2,1
                6,3
                2,4
                1,5
                0,6
                3,3
                2,6
                5,1
                1,2
                5,5
                2,5
                6,5
                1,4
                0,4
                6,4
                1,1
                6,1
                1,0
                0,5
                1,6
                2,0`;

  const lines = fileio.linesFromString(data);

  test("part1", async () => {
    const [height, width] = [7, 7];
    const numFallen = 12;
    const fallen: Position[] = lines.map((s) => {
      const toks = fileio.tokenize(s, ",");
      return { row: Number(toks[0]), col: Number(toks[1]) };
    });

    const solution = __test__.pathLength(height, width, fallen, numFallen);
    expect(solution).toBe(22);
  });

  test("part2", async () => {
    const [height, width] = [7, 7];
    const fallen: Position[] = lines.map((s) => {
      const toks = fileio.tokenize(s, ",");
      return { row: Number(toks[0]), col: Number(toks[1]) };
    });

    const failPos = __test__.firstFailure(height, width, fallen);
    const solution = `${fallen[failPos].row},${fallen[failPos].col}`;
    expect(solution).toBe("6,1");
  });
});
