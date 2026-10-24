// combinations.test.ts
import { describe, expect, test } from "bun:test";
import { type CombinationDecision, getCombinations, getRecursiveCombinations } from "./combinations";

describe("Combinations", () => {
  test("should resolve basic full combinations", () => {
    const config = {
      getChoicesForLevel: (_path: string[], level: number): string[] | null => {
        if (level === 0) return ["Red", "Blue"];
        if (level === 1) return ["Shirt"];
        return null;
      },
    };

    const outcome = getRecursiveCombinations(config, {});

    expect(outcome.done).toBe(false);
    expect(outcome.results).toEqual([
      ["Red", "Shirt"],
      ["Blue", "Shirt"],
    ]);
  });

  test("should change level 1 options dynamically based on level 0 choice", () => {
    const config = {
      getChoicesForLevel: (path: string[], level: number): string[] | null => {
        if (level === 0) return ["Mage", "Warrior"];
        if (level === 1) {
          // Dynamic branching context check
          return path[0] === "Mage" ? ["Staff"] : ["Sword"];
        }
        return null;
      },
    };

    const outcome = getRecursiveCombinations(config, {});

    expect(outcome.results).toEqual([
      ["Mage", "Staff"],
      ["Warrior", "Sword"],
    ]);
  });

  test("should correctly filter out configurations when 'skip' is used", () => {
    const config = {
      getChoicesForLevel: (_path: string[], level: number): string[] | null => {
        if (level === 0) return ["A", "B"];
        if (level === 1) return ["1", "2"];
        return null;
      },
      onStep: (path: string[]): CombinationDecision => {
        // Skip/Prune any branch containing the letter "B"
        return path.includes("B") ? "skip" : "continue";
      },
    };

    const outcome = getRecursiveCombinations(config, {});

    expect(outcome.results).toEqual([
      ["A", "1"],
      ["A", "2"],
    ]);
  });

  test("should break out completely when 'success' is flagged early", () => {
    const config = {
      getChoicesForLevel: (_path: number[], level: number): number[] | null => {
        if (level === 0) return [1, 2];
        if (level === 1) return [10, 20];
        return null;
      },
      onStep: (path: number[]): CombinationDecision => {
        // Stop the entire loop operations as soon as we match the first inner node under 1
        return path[0] === 1 && path[1] === 10 ? "success" : "continue";
      },
    };

    const outcome = getRecursiveCombinations(config, {});

    expect(outcome.done).toBe(true);
    // Should immediately abort without exploring, or nodes under 2 and 3
    expect(outcome.results).toEqual([[1, 10]]);
  });

  test("getCombinations", async () => {
    const items = ["a", "b", "c", "d"];
    const k = 3;
    const combos = getCombinations(items, k).map((a) => a.join(""));
    expect(combos).toEqual(["abc", "abd", "acd", "bcd"]);
  });
});
