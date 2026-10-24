import { describe, expect, test } from "bun:test";
import assert from "node:assert";
import { Poset } from "../lib/poset";
import { debug, fileio } from "../lib/prelude";
import { __test__ } from "./day05";

describe("day05", () => {
  const data = `47|53
                97|13
                97|61
                97|47
                75|29
                61|13
                75|53
                29|13
                97|29
                53|29
                61|53
                97|53
                61|29
                47|13
                75|47
                97|75
                47|61
                75|61
                47|29
                75|13
                53|13

                75,47,61,53,29
                97,61,53,29,13
                75,29,13
                75,97,47,61,53
                61,13,29
                97,13,75,29,47`;

  const lines = fileio.linesFromString(data);

  test("part1", async () => {
    const [adjacencies, updates] = __test__.parseData(lines);
    const solution = updates.reduce<number>((acc, update) => {
      if (__test__.checkUpdate(update, adjacencies)) {
        assert(update.length % 2 === 1); // Should be odd to choose the middle item
        return acc + update[Math.trunc(update.length / 2)];
      } else {
        return acc;
      }
    }, 0);
    expect(solution).toBe(143);
  });

  test("part2", async () => {
    const [adjacencies, updates] = __test__.parseData(lines);
    const badUpdates = updates.filter((val) => !__test__.checkUpdate(val, adjacencies));

    let solution = 0;

    // Create posets for each update only.
    // Creating one for the entire set or rules fails because they are inconsistent.
    for (const badUpdate of badUpdates) {
      const usedPages = new Set<number>(badUpdate);
      const poset = new Poset<number>();
      for (const p of usedPages) {
        if (adjacencies.has(p)) {
          for (const v of adjacencies.get(p) ?? []) {
            if (usedPages.has(v)) {
              poset.addConstraint(p, v);
            }
          }
        }
      }
      const fixedUpdate = poset.sort(badUpdate);
      assert(__test__.checkUpdate(fixedUpdate, adjacencies));
      assert(fixedUpdate.length % 2 === 1); // Should be odd to choose the middle item
      solution += fixedUpdate[Math.trunc(fixedUpdate.length / 2)];
    }

    expect(solution).toBe(123);
  });
});
