import { describe, expect, test } from "bun:test";
import { parseArgs } from "../src/cli";

describe("parseArgs", () => {
  test("parses help and verbose aliases", () => {
    expect(parseArgs(["-h", "-v"]).options).toEqual({ help: true, verbose: true, days: [] });
  });

  test("parses a single day", () => {
    expect(parseArgs(["day01"]).options.days).toEqual(["day01"]);
  });

  test("parses multiple days in order", () => {
    expect(parseArgs(["day01", "day03"]).options.days).toEqual(["day01", "day03"]);
  });

  test("parses all", () => {
    expect(parseArgs(["all"]).options.days).toEqual(["all"]);
  });

  test("rejects mixing all with specific days", () => {
    expect(parseArgs(["day01", "all"]).errors.length).toBe(1);
    expect(parseArgs(["all", "day03"]).errors.length).toBe(1);
  });

  test("rejects invalid and unknown arguments", () => {
    expect(parseArgs(["day1"]).errors.length).toBe(1);
    expect(parseArgs(["--unknown"]).errors.length).toBe(1);
  });
});
