import { describe, expect, test } from "bun:test";
import { Counter } from "./counter";

describe("counter", () => {
  // Usage:
  test("usage", async () => {
    const fruitCounter = new Counter(["apple", "banana", "apple"]);

    expect(fruitCounter.get("apple")).toBe(2); // 2
    expect(fruitCounter.get("pear")).toBe(0); // 0 (safe default!)
    expect(fruitCounter.mostCommon(1)).toEqual([["apple", 2]]); // [['apple', 2]]
  });
});
