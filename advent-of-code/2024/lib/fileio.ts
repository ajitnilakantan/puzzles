export async function linesFromFile(path: string): Promise<string[]> {
  const text = await Bun.file(path).text();
  if (text.length === 0) return [];

  const withoutTrailingNewline = text.endsWith("\n") ? text.slice(0, -1) : text;
  return withoutTrailingNewline.split("\n").map((line) => line.replace(/\r$/, "").trim());
}

export function linesFromString(text: string): string[] {
  if (text.length === 0) return [];

  const withoutTrailingNewline = text.endsWith("\n") ? text.slice(0, -1) : text;
  return withoutTrailingNewline.split("\n").map((line) => line.replace(/\r$/, "").trim());
}

// Break list of strings (lines) into a list of list of strings
// broken at empty lines
export function chunkLines(lines: string[]): string[][] {
  if (lines.length === 0) {
    return [];
  }

  const result: string[][] = [];
  let chunk: string[] = [];

  for (const line of lines) {
    if (line === "") {
      if (chunk.length > 0) {
        result.push(chunk);
      }
      chunk = [];
    } else {
      chunk.push(line);
    }
  }
  if (chunk.length > 0) {
    result.push(chunk);
  }
  return result;
}

export function tokenize(line: string, separators = ""): string[] {
  if (separators == "") {
    separators = "\x20\n\r\t";
  }
  const separatorSet = new Set(separators);
  const tokens: string[] = [];
  let tokenStart = 0;

  for (let index = 0; index < line.length; index += 1) {
    if (separatorSet.has(line[index])) {
      if (index > tokenStart) tokens.push(line.slice(tokenStart, index));
      tokenStart = index + 1;
    }
  }

  if (tokenStart < line.length) tokens.push(line.slice(tokenStart));
  return tokens;
}
