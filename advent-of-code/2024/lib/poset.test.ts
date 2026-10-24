import { describe, expect, test } from "bun:test";
import { Poset } from "./poset";

describe("Poset - Explicit Graph Relationships", () => {
  test("should handle basic reflexivity", () => {
    const poset = new Poset<string>();
    poset.add("A");

    expect(poset.isLessThanOrEqual("A", "A")).toBe(true);
  });

  test("should resolve transitive relations", () => {
    const poset = new Poset<string>();
    poset.addConstraint("A", "B");
    poset.addConstraint("B", "C");

    expect(poset.isLessThanOrEqual("A", "C")).toBe(true);
    expect(poset.isLessThanOrEqual("C", "A")).toBe(false);
  });

  test("should identify incomparable elements", () => {
    const poset = new Poset<string>();
    poset.addConstraint("Root", "Left Branch");
    poset.addConstraint("Root", "Right Branch");

    expect(poset.areComparable("Left Branch", "Right Branch")).toBe(false);
    expect(poset.areComparable("Root", "Left Branch")).toBe(true);
  });

  test("should throw an error if antisymmetry is broken (cycle injection)", () => {
    const poset = new Poset<string>();
    poset.addConstraint("A", "B");
    poset.addConstraint("B", "C");

    // Adding C <= A breaks antisymmetry because A <= B <= C implies A === C
    expect(() => {
      poset.addConstraint("C", "A");
    }).toThrow("Cannot add constraint: Would violate antisymmetry (creates a cycle).");
  });

  test("should generate a valid linear extension (topological sort)", () => {
    const poset = new Poset<string>();
    poset.addConstraint("Shirt", "Tie");
    poset.addConstraint("Tie", "Jacket");
    poset.addConstraint("Undershirt", "Shirt");

    const order = poset.getLinearExtension();

    // Validate indices to ensure ordering is maintained
    expect(order.indexOf("Undershirt")).toBeLessThan(order.indexOf("Shirt"));
    expect(order.indexOf("Shirt")).toBeLessThan(order.indexOf("Tie"));
    expect(order.indexOf("Tie")).toBeLessThan(order.indexOf("Jacket"));
  });
});

describe("Poset - Functional Relations (Divisibility Example)", () => {
  // Ordered by: a <= b IF b is divisible by a
  const divisibilityPoset = new Poset<number>((a, b) => b % a === 0);

  // Seed the numbers
  [2, 3, 4, 6, 8, 9, 12].forEach((n) => {
    divisibilityPoset.add(n);
  });

  test("should validate true relationships", () => {
    expect(divisibilityPoset.isLessThanOrEqual(2, 6)).toBe(true);
    expect(divisibilityPoset.isLessThanOrEqual(3, 12)).toBe(true);
  });

  test("should validate false relationships", () => {
    expect(divisibilityPoset.isLessThanOrEqual(3, 8)).toBe(false);
  });

  test("should recognize incomparable functional pairs", () => {
    expect(divisibilityPoset.areComparable(2, 3)).toBe(false);
    expect(divisibilityPoset.areComparable(4, 6)).toBe(false);
  });
});
