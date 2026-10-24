import { describe, expect, test } from "bun:test";
import { debug, fileio } from "../lib/prelude";
import { __test__ } from "./day21";

describe("day21", () => {
  const data = `029A
                980A
                179A
                456A
                379A`;
  const expectedLength1 = [68, 60, 68, 64, 64];
  const expectedCode1 = [29, 980, 179, 456, 379];

  const lines = fileio.linesFromString(data);

  test("part1a", async () => {
    const [keypadMoves, controlMoves] = __test__.precalculateKeypadMoves();
    const cache = new Map<string, number>();
    let solution = 0;
    for (const [index, code] of lines.entries()) {
      const result = __test__.getMoves(code, 0, 2, cache, keypadMoves, controlMoves);
      expect(result).toBe(expectedLength1[index]);
      expect(__test__.codeToNumber(code)).toBe(expectedCode1[index]);
      solution += result * __test__.codeToNumber(code);
    }
    expect(solution).toBe(126384);
  });

  test("part2", async () => {
    const [keypadMoves, controlMoves] = __test__.precalculateKeypadMoves();
    const cache = new Map<string, number>();
    const result = __test__.getMoves("029A", 0, 2, cache, keypadMoves, controlMoves);
    debug.log`resuilt = ${result}`;
    const solution = 0;
    expect(solution).toBe(0);
  });
});
