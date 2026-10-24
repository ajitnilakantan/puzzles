import assert from "node:assert";
import { debug, fileio } from "../lib/prelude";
import { TupleMap } from "../lib/tuplemap";
import type { DayPlugin } from "../src/plugin";

// export private functions for testing
export const __test__ = { parseData, precalculateKeypadMoves, getMoves, codeToNumber };

interface Position {
  row: number;
  col: number;
}
interface KeyPad {
  type: "keypad";
  def: Map<string, Position>;
  pos: Position;
  key: string;
}
interface Control {
  type: "control";
  def: Map<string, Position>;
  pos: Position;
  key: string;
}
class MoveCache extends TupleMap<[string, string], string[]> {}

function parseData(lines: string[]): string[] {
  return lines;
}

function codeToNumber(code: string): number {
  return parseInt(code.replace(/A/g, ""), 10);
}

function createControllers(): [KeyPad, Control] {
  const p = (row: number, col: number): Position => ({ row, col });
  const keyPadDef = new Map([
    ["7", p(0, 0)],
    ["8", p(0, 1)],
    ["9", p(0, 2)],
    ["4", p(1, 0)],
    ["5", p(1, 1)],
    ["6", p(1, 2)],
    ["1", p(2, 0)],
    ["2", p(2, 1)],
    ["3", p(2, 2)],
    ["0", p(3, 1)],
    ["A", p(3, 2)],
  ]);
  const controlDef = new Map([
    // _ ^ A
    // < v >
    ["^", p(0, 1)],
    ["A", p(0, 2)],
    ["<", p(1, 0)],
    ["v", p(1, 1)],
    [">", p(1, 2)],
  ]);

  const keyPad: KeyPad = { type: "keypad", def: keyPadDef, pos: p(3, 2), key: "A" };
  const control: Control = { type: "control", def: controlDef, pos: p(0, 2), key: "A" };

  return [keyPad, control];
}

function precalculateKeypadMoves(): [keypadMoves: MoveCache, controlMoves: MoveCache] {
  const [keypad, control] = createControllers(); // temp usage

  // Cache moves for the keypad
  const keypadMoves = new MoveCache();
  const keyCodes = "0123456789A";
  for (const from of keyCodes) {
    for (const to of keyCodes) {
      const [fromPos, toPos] = [keypad.def.get(from)!, keypad.def.get(to)!];
      const [dr, dc] = [toPos.row - fromPos.row, toPos.col - fromPos.col];
      const dr_char = dr === 0 ? "" : dr < 0 ? "^" : "v";
      const dc_char = dc === 0 ? "" : dc < 0 ? "<" : ">";
      const [abs_dr, abs_dc] = [Math.abs(dr), Math.abs(dc)];

      let moves = getUniquePermutations(dr_char.repeat(abs_dr) + dc_char.repeat(abs_dc));
      // Filter invalid moves:
      /* eslint-disable-next-line @typescript-eslint/switch-exhaustiveness-check */
      switch (true) {
        case dr === 0 && dc === 0:
          moves = [""];
          break;
        case dc < 0 && fromPos.row === 3 && fromPos.col === 1: // Left from "0"
          moves = moves.filter((s) => !s.startsWith("<"));
          break;
        case dc < 0 && fromPos.row === 3 && fromPos.col === 2: // Left from "A"
          moves = moves.filter((s) => !s.startsWith("<<"));
          break;
        case dr > 0 && fromPos.col === 0 && fromPos.row === 0: // Down from "7"
          moves = moves.filter((s) => !s.startsWith("vvv"));
          break;
        case dr > 0 && fromPos.col === 0 && fromPos.row === 1: // Down from "4"
          moves = moves.filter((s) => !s.startsWith("vv"));
          break;
        case dr > 0 && fromPos.col === 0 && fromPos.row === 2: // Down from "1"
          moves = moves.filter((s) => !s.startsWith("v"));
          break;
        default:
          break; // Nothing to do
      }
      // Add final "A"
      moves = moves.map((s) => s + "A");
      // Cache it
      keypadMoves.set([from, to], moves);
    }
  }

  // Repeat for the controller
  const controlMoves = new MoveCache();
  const controlCodes = "^A<v>";
  for (const from of controlCodes) {
    for (const to of controlCodes) {
      const [fromPos, toPos] = [control.def.get(from)!, control.def.get(to)!];
      const [dr, dc] = [toPos.row - fromPos.row, toPos.col - fromPos.col];
      const dr_char = dr === 0 ? "" : dr < 0 ? "^" : "v";
      const dc_char = dc === 0 ? "" : dc < 0 ? "<" : ">";
      const [abs_dr, abs_dc] = [Math.abs(dr), Math.abs(dc)];

      let moves = getUniquePermutations(dr_char.repeat(abs_dr) + dc_char.repeat(abs_dc));
      // Filter invalid moves:
      /* eslint-disable-next-line @typescript-eslint/switch-exhaustiveness-check */
      switch (true) {
        case dr === 0 && dc === 0:
          moves = [""];
          break;
        case dc < 0 && fromPos.row === 0 && fromPos.col === 1: // Left from "^"
          moves = moves.filter((s) => !s.startsWith("<"));
          break;
        case dc < 0 && fromPos.row === 0 && fromPos.col === 2: // Left from "A"
          moves = moves.filter((s) => !s.startsWith("<<"));
          break;
        case dr < 0 && fromPos.col === 0 && fromPos.row === 1: // Up from "<"
          moves = moves.filter((s) => !s.startsWith("^"));
          break;
        default:
          break; // Nothing to do
      }
      // Add final "A"
      moves = moves.map((s) => s + "A");
      // Cache it
      controlMoves.set([from, to], moves);
    }
  }

  return [keypadMoves, controlMoves];
}

/**
 * Generates all unique permutations of a string.
 * @param str The input string to permute.
 * @returns An array of unique permutations.
 * --- Usage ---
 * console.log(getUniquePermutations("abc"));
 * // Output: [ 'abc', 'acb', 'bac', 'bca', 'cab', 'cba' ]
 * console.log(getUniquePermutations("aab"));
 * // Output: [ 'aab', 'aba', 'baa' ] (No duplicate entries)
 */
function getUniquePermutations(str: string): string[] {
  const results: string[] = [];

  // Create a frequency map of characters
  const charCounts: Record<string, number> = {};
  for (const char of str) {
    charCounts[char] = (charCounts[char] || 0) + 1;
  }

  // Recursive backtracking helper
  function backtrack(currentPermutation: string): void {
    // Base case: if the current string length matches the input length, we found a permutation
    if (currentPermutation.length === str.length) {
      results.push(currentPermutation);
      return;
    }

    // Try adding each unique available character
    for (const char in charCounts) {
      if (charCounts[char] > 0) {
        // Choose
        charCounts[char]--;

        // Recurse
        backtrack(currentPermutation + char);

        // Backtrack (Undo choice)
        charCounts[char]++;
      }
    }
  }

  backtrack("");
  return results;
}

// Recursively finds the minimum length required to type a sequence at a specific depth
function getMoves(
  sequence: string,
  depth: number,
  maxDepth: number,
  cache: Map<string, number>,
  keypadMoves: MoveCache,
  controlMoves: MoveCache
): number {
  // Memoization cache: "string_buttonDepth" -> minLength: cache = new Map<string, number>();
  // Base case: If we pass the robot controllers, human inputs 1-to-1
  if (depth > maxDepth) {
    return sequence.length;
  }

  // Check cache to avoid recalculating massive branches
  const cacheKey = `${sequence}_${depth}`;
  if (cache.has(cacheKey)) return cache.get(cacheKey)!;

  let totalLength = 0;
  // All movements start from the 'A' button position
  let currentPos = "A";

  // Split the sequence into step-by-step character transitions
  for (const nextPos of sequence) {
    // Get possible paths from current to next position
    const keypad = depth === 0 ? keypadMoves : controlMoves;
    const possiblePaths = keypad.get([currentPos, nextPos])!;

    // Try all valid paths for this transition, find the one that yields the absolute shortest length deeper down
    let minSubPath = Infinity;
    for (const path of possiblePaths) {
      // Recursively get possible moves.
      const length = getMoves(path, depth + 1, maxDepth, cache, keypadMoves, controlMoves);
      if (length < minSubPath) {
        // keep track of minimum
        minSubPath = length;
      }
    }

    totalLength += minSubPath;
    currentPos = nextPos; // Robot is now at the new button
  }

  // cache the result
  cache.set(cacheKey, totalLength);
  return totalLength;
}

function solvePart1(lines: string[]): number {
  const [keypadMoves, controlMoves] = precalculateKeypadMoves();
  let solution = 0;
  for (const code of lines) {
    const cache = new Map<string, number>();
    const result = getMoves(code, 0, 2, cache, keypadMoves, controlMoves);
    solution += result * codeToNumber(code);
  }

  return solution;
}

function solvePart2(lines: string[]): number {
  const [keypadMoves, controlMoves] = precalculateKeypadMoves();
  let solution = 0;
  for (const code of lines) {
    const cache = new Map<string, number>();
    const result = getMoves(code, 0, 25, cache, keypadMoves, controlMoves);
    solution += result * codeToNumber(code);
  }

  return solution;
}

const plugin: DayPlugin = {
  name: "Day 21",
  async run({ verbose }) {
    console.log(`Running ${this.name} verbose=${verbose}`);
    const data = await fileio.linesFromFile(`${import.meta.dir}/day21.txt`);

    const start1 = performance.now();
    const solution1 = solvePart1(data);
    const duration1 = (performance.now() - start1).toFixed(3);
    console.log(`${this.name} verbose=${verbose}.Part1 = ${solution1} : Elapsed time: ${duration1}ms`);
    assert.strictEqual(solution1, 231564, `${this.name} verbose=${verbose}.Part1`);

    const start2 = performance.now();
    const solution2 = solvePart2(data);
    const duration2 = (performance.now() - start2).toFixed(3);
    console.log(`${this.name} verbose=${verbose}.Part2 = ${solution2} : Elapsed time: ${duration2}ms`);
    assert.strictEqual(solution2, 281212077733592, `${this.name} verbose=${verbose}.Part2`);
  },
};

export default plugin;
