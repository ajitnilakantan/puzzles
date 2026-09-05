import { describe, expect, test } from "bun:test";
import { discoverPlugins, loadPlugin } from "../src/registry";

describe("plugin registry", () => {
  test("discovers sorted day plugins", async () => {
    expect(await discoverPlugins()).toEqual(["day01", "day02"]);
  });

  test("loads a plugin through dynamic import", async () => {
    const plugin = await loadPlugin("day01");
    expect(plugin.name).toBe("Day 01 example");
  });

  test("reports missing plugins", async () => {
    await expect(loadPlugin("day99")).rejects.toThrow("was not found");
  });
});
