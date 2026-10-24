import { describe, expect, test } from "bun:test";
import { discoverPlugins, loadPlugin } from "../src/registry";

describe("plugin registry", () => {
  test("discovers sorted day plugins", async () => {
    // just check first two
    expect((await discoverPlugins()).slice(0, 2)).toEqual(["day01", "day02"]);
  });

  test("loads a plugin through dynamic import", async () => {
    const plugin = await loadPlugin("day01");
    expect(plugin.name).toBe("Day 01");
  });

  test("reports missing plugins", async () => {
    const error = await loadPlugin("day99").then(
      () => null,
      (e: unknown) => e
    );
    expect(error).toBeInstanceOf(Error);
    expect((error as Error).message).toContain("was not found");
  });
});
