import { describe, expect, test } from "bun:test";
import { debug, fileio } from "../lib/prelude";
import { __test__ } from "./day25";

describe("day25", () => {
  const data = `#####
                .####
                .####
                .####
                .#.#.
                .#...
                .....

                #####
                ##.##
                .#.##
                ...##
                ...#.
                ...#.
                .....

                .....
                #....
                #....
                #...#
                #.#.#
                #.###
                #####

                .....
                .....
                #.#..
                ###..
                ###.#
                ###.#
                #####

                .....
                .....
                .....
                #....
                #.#..
                #.#.#
                #####`;

  const lines = fileio.linesFromString(data);

  test("part1", async () => {
    const [locks, keys, height, _width] = __test__.parseData(lines);
    const solution = __test__.countNoOverlap(locks, keys, height);
    expect(solution).toBe(3);
  });

  test("part2", async () => {
    const solution = 0;
    expect(solution).toBe(0);
  });
});
