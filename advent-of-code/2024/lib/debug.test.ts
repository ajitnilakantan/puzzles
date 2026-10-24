import { afterAll, beforeEach, describe, expect, spyOn, test } from "bun:test";
import { debug } from "../lib/prelude";

//
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

describe("debug", () => {
  test("log", async () => {
    debug.log`hex = ${255}:x`; // hex = ff
    expect(logSpy).toHaveBeenCalledWith(expect.stringContaining("ff"));
    debug.log`hex padded = ${255}:x:p:6`; // hex padded =     ff
    expect(logSpy).toHaveBeenCalledWith(expect.stringContaining("    ff"));
    debug.log`bin = ${42}:b`; // bin = 101010
    expect(logSpy).toHaveBeenCalledWith(expect.stringContaining("bin = 101010"));
    debug.log`bin padded = ${42}:b:p:16`; // bin padded =         101010
    expect(logSpy).toHaveBeenCalledWith(expect.stringContaining("bin padded =           101010"));
    debug.log`num = ${3.14159}:d:2:p:10`; // num =       3.14
    expect(logSpy).toHaveBeenCalledWith("num =       3.14");
    debug.log`num = ${3.14159}:d:3`; // num =       3.14
    expect(logSpy).toHaveBeenCalledWith("num = 3.142");
  });
});
