import { describe, expect, test } from "bun:test";
import { debug, fileio } from "../lib/prelude";
import { __test__ } from "./day09";

describe("day09", () => {
  const data = `2333133121414131402`;

  const lines = fileio.linesFromString(data);

  test("part1", async () => {
    const line = lines[0];
    const [blocks, freespace] = __test__.parseLine(line);
    const [newBlocks, newDefrag] = __test__.fillGaps(blocks, freespace);
    const defrag = [...newBlocks, ...newDefrag].toSorted((a, b) => a.position - b.position).filter((a) => a.count > 0);
    const strResult = __test__.toString(defrag, []);
    expect(strResult).toBe("0099811188827773336446555566");
    const result = __test__.toArray(defrag, []);
    expect(result).toEqual([0, 0, 9, 9, 8, 1, 1, 1, 8, 8, 8, 2, 7, 7, 7, 3, 3, 3, 6, 4, 4, 6, 5, 5, 5, 5, 6, 6]);
    const solution = result.reduce((acc, val, index) => {
      return acc + (val === -1 ? 0 : val) * index;
    }, 0);
    expect(solution).toBe(1928);
  });

  test("part2", async () => {
    const line = lines[0];
    const [blocks, freespace] = __test__.parseLine(line);
    const [newBlocks, newDefrag] = __test__.fillGapsPart2(blocks, freespace);
    const defrag = [...newBlocks, ...newDefrag].toSorted((a, b) => a.position - b.position).filter((a) => a.count > 0);
    const strResult = __test__.toString(defrag, []);
    expect(strResult).toBe("00992111777.44.333....5555.6666.....8888"); // We don't print trailing "."
    const result = __test__.toArray(defrag, []);
    expect(result).toEqual([
      0, 0, 9, 9, 2, 1, 1, 1, 7, 7, 7, -1, 4, 4, -1, 3, 3, 3, -1, -1, -1, -1, 5, 5, 5, 5, -1, 6, 6, 6, 6, -1, -1, -1,
      -1, -1, 8, 8, 8, 8,
    ]);
    const solution = result.reduce((acc, val, index) => {
      return acc + (val === -1 ? 0 : val) * index;
    }, 0);
    expect(solution).toBe(2858);
  });
});
