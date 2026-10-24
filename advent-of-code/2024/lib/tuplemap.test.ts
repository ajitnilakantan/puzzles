import { beforeEach, describe, expect, test } from "bun:test";
import { TupleMap } from "./tuplemap";

// Assuming your class is in tuple-map.ts

describe("TupleMap", () => {
  let registry: TupleMap<[string, string], string[]>;

  beforeEach(() => {
    registry = new TupleMap<[string, string], string[]>();
  });

  test("should store and retrieve values by structural tuple value", () => {
    registry.set(["USA", "California"], ["Los Angeles", "San Francisco"]);

    // Looks up using a completely new array reference with identical values
    const result = registry.get(["USA", "California"]);

    expect(result).toEqual(["Los Angeles", "San Francisco"]);
    expect(registry.size).toBe(1);
  });

  test("should return true for .has() with identical structural values", () => {
    registry.set(["Canada", "BC"], ["Vancouver"]);

    expect(registry.has(["Canada", "BC"])).toBe(true);
    expect(registry.has(["Canada", "Ontario"])).toBe(false);
  });

  test("should handle deletion correctly", () => {
    registry.set(["UK", "London"], ["Camden"]);

    const deleted = registry.delete(["UK", "London"]);

    expect(deleted).toBe(true);
    expect(registry.has(["UK", "London"])).toBe(false);
    expect(registry.size).toBe(0);
  });

  test("should support iteration using for...of", () => {
    registry.set(["Japan", "Tokyo"], ["Shibuya"]);
    registry.set(["France", "IDF"], ["Paris"]);

    const keys: string[][] = [];
    for (const [key, _value] of registry) {
      keys.push(key);
    }

    expect(keys).toContainEqual(["Japan", "Tokyo"]);
    expect(keys).toContainEqual(["France", "IDF"]);
  });
});
