import { describe, expect, test } from "bun:test";
import { parseArgs } from "../src/cli";

describe("parseArgs", () => {
  test("parses help and verbose aliases", () => {
    expect(parseArgs(["-h", "-v"]).options).toEqual({ help: true, verbose: true, day: null });
  });

  test("parses a day", () => {
    expect(parseArgs(["day01"]).options.day).toBe("day01");
  });

  test("rejects invalid and duplicate arguments", () => {
    expect(parseArgs(["day1"]).errors.length).toBe(1);
    expect(parseArgs(["day01", "day02"]).errors.length).toBe(1);
    expect(parseArgs(["--unknown"]).errors.length).toBe(1);
  });
});
