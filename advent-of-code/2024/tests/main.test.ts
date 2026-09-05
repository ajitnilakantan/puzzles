import { expect, test } from "bun:test";
import { runCli } from "../src/main";

test("help exits successfully", async () => {
  expect(await runCli(["--help"])).toBe(0);
});

test("unknown day exits with an error", async () => {
  expect(await runCli(["day99"])).toBe(1);
});
