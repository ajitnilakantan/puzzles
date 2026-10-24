import { afterEach, describe, expect, test } from "bun:test";
import * as fileio from "../lib/fileio";

const temporaryFiles: string[] = [];

async function writeTemporaryFile(content: string): Promise<string> {
  const path = `${import.meta.dir}/fileio-${crypto.randomUUID()}.txt`;
  temporaryFiles.push(path);
  await Bun.write(path, content);
  return path;
}

afterEach(async () => {
  await Promise.all(temporaryFiles.splice(0).map((path) => Bun.file(path).delete()));
});

describe("fileio.linesFromFile", () => {
  test("reads lines and normalizes line endings", async () => {
    const path = await writeTemporaryFile("first\r\n\r\nthird\n");
    expect(await fileio.linesFromFile(path)).toEqual(["first", "", "third"]);
    const text1 = "first\r\n\r\nthird\n";
    expect(fileio.linesFromString(text1)).toEqual(["first", "", "third"]);
    const text2 = "  first  \r\n\r\n  third";
    expect(fileio.linesFromString(text2)).toEqual(["first", "", "third"]);
  });

  test("returns an empty array for an empty file", async () => {
    const path = await writeTemporaryFile("");
    expect(await fileio.linesFromFile(path)).toEqual([]);
  });

  test("propagates missing-fiyyle errors", async () => {
    expect(fileio.linesFromFile(`${import.meta.dir}/missing-file.txt`)).rejects.toThrow();
  });
});

describe("fileio.chunks", () => {
  test("chunk lines", () => {
    const lines = fileio.linesFromString("abc\n  def\n\nx\ny\nz\n");
    expect(lines).toEqual(["abc", "def", "", "x", "y", "z"]);
    const chunks = fileio.chunkLines(lines);
    expect(chunks).toEqual([
      ["abc", "def"],
      ["x", "y", "z"],
    ]);
  });
});

describe("fileio.tokenize", () => {
  test("splits using every separator character", () => {
    expect(fileio.tokenize("alpha,beta;gamma", ",;")).toEqual(["alpha", "beta", "gamma"]);
  });

  test("omits empty tokens from adjacent and surrounding separators", () => {
    expect(fileio.tokenize(",,alpha,,,beta,", ",")).toEqual(["alpha", "beta"]);
  });

  test("returns the original line when no separators are supplied", () => {
    expect(fileio.tokenize("alpha,beta", "")).toEqual(["alpha,beta"]);
  });

  test("handles separator characters without regex escaping", () => {
    expect(fileio.tokenize("a-b|c", "-|")).toEqual(["a", "b", "c"]);
  });
});
