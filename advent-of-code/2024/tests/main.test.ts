import { afterAll, beforeEach, describe, expect, spyOn, test } from "bun:test";
import { runCli } from "../src/main";

// Silence console.log during a test
const logSpy = spyOn(console, "log").mockImplementation(() => {
  /*empty*/
});
const errSpy = spyOn(console, "error").mockImplementation(() => {
  /*empty*/
});
// Clear call history before *every* test block so they are independent
beforeEach(() => {
  logSpy.mockClear();
  errSpy.mockClear();
});
// Restore the original console behavior when the file is done
afterAll(() => {
  logSpy.mockRestore();
  errSpy.mockRestore();
});

describe("main cli", () => {
  test("help exits successfully", async () => {
    expect(await runCli(["--help"])).toBe(0);
    expect(logSpy).toHaveBeenCalled();
    expect(logSpy).toHaveBeenCalledWith(expect.stringContaining("Usage: bun run"));
    expect(logSpy).toHaveBeenCalledWith(expect.stringContaining("With no day argument, available plugins are listed."));
  });

  test("unknown day exits with an error", async () => {
    expect(await runCli(["day99"])).toBe(1);
    expect(errSpy).toHaveBeenCalled();
    expect(errSpy).toHaveBeenCalledWith(expect.stringContaining("Plugin day99 was not found. Available plugins:"));
  });
});
