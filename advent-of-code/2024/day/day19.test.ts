import { describe, expect, test } from "bun:test";
import { debug, fileio } from "../lib/prelude";
import { __test__ } from "./day19";

describe("day19", () => {
  const data = `r, wr, b, g, bwu, rb, gb, br

                brwrr
                bggr
                gbbr
                rrbgbr
                ubwu
                bwurrg
                brgr
                bbrgwb`;

  const lines = fileio.linesFromString(data);

  test("part1", async () => {
    const [syllables, words] = __test__.parseData(lines);
    const maxSyllableLength = Math.max(...Array.from(syllables).map((s) => s.length));
    const cache = new Map<string, boolean>();
    const solution = words.reduce(
      (acc, w) => (__test__.can_form_word(w, syllables, maxSyllableLength, cache) ? acc + 1 : acc),
      0
    );
    expect(solution).toBe(6);
  });

  test("part2", async () => {
    const [syllables, words] = __test__.parseData(lines);
    const maxSyllableLength = Math.max(...Array.from(syllables).map((s) => s.length));
    const cache = new Map<string, number>();
    const solution = words.reduce((acc, w) => {
      const count = __test__.can_form_word_count(w, syllables, maxSyllableLength, cache);
      return acc + count;
    }, 0);
    expect(solution).toBe(16);
  });
});
