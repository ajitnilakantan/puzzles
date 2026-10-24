import assert from "node:assert";
import { debug, fileio } from "../lib/prelude";
import type { DayPlugin } from "../src/plugin";

// export private functions for testing
export const __test__ = { parseData, can_form_word, can_form_word_count };

function parseData(lines: string[]): [Set<string>, string[]] {
  const chunks = fileio.chunkLines(lines);
  assert(chunks.length === 2);
  assert(chunks[0].length === 1); // list of syllables
  const syllables = new Set<string>(fileio.tokenize(chunks[0][0], ",\x20"));
  const words = chunks[1];

  return [syllables, words];
}

// Recursively check syllable-prefixes
function can_form_word(
  word: string,
  syllables: Set<string>,
  maxSyllableLength: number,
  cache: Map<string, boolean> | null = null
): boolean {
  // Keep a cache to quick exit failures
  cache ??= new Map<string, boolean>();

  // Check cache first
  if (cache.has(word)) {
    return cache.get(word)!;
  }
  // Base case: if the remaining word is empty, it's a perfect match
  if (word === "") {
    return true;
  }

  // Check every possible prefix length of the current word segment
  for (let k = 1; k <= maxSyllableLength; k++) {
    const prefix = word.slice(0, k); // first k characters
    if (syllables.has(prefix)) {
      // Recursively check the remainder of the word
      if (can_form_word(word.slice(k), syllables, maxSyllableLength, cache)) {
        cache.set(word, true);
        return true;
      }
    }
  }
  cache.set(word, false);
  return false;
}

// Extend to return count
function can_form_word_count(
  word: string,
  syllables: Set<string>,
  maxSyllableLength: number,
  cache: Map<string, number> | null = null
): number {
  // Keep a cache to quick exit failures
  cache ??= new Map<string, number>();

  // Check cache first
  if (cache.has(word)) {
    return cache.get(word)!;
  }
  // Base case: if the remaining word is empty, it's a perfect match
  if (word === "") {
    return 1;
  }

  // Check every possible prefix length of the current word segment
  let count = 0;
  let found = false;
  // Need the min, because e.g. if maxSyllableLength=3, and word = "a", then
  // the slice (0,1), (0,2), (0,3) will all return "a", causing e.g. a triple-count.
  for (let k = 1; k <= Math.min(maxSyllableLength, word.length); k++) {
    const prefix = word.slice(0, k); // first k characters
    if (syllables.has(prefix)) {
      found = true;
      // Recursively check the remainder of the word
      count += can_form_word_count(word.slice(k), syllables, maxSyllableLength, cache);
    }
  }

  if (!found) {
    cache.set(word, 0);
    return 0;
  } else {
    cache.set(word, count);
    return count;
  }
}

function solvePart1(lines: string[]): number {
  const [syllables, words] = parseData(lines);
  const maxSyllableLength = Math.max(...Array.from(syllables).map((s) => s.length));
  const cache = new Map<string, boolean>();
  const solution = words.reduce((acc, w) => (can_form_word(w, syllables, maxSyllableLength, cache) ? acc + 1 : acc), 0);

  return solution;
}

function solvePart2(lines: string[]): number {
  const [syllables, words] = parseData(lines);
  const maxSyllableLength = Math.max(...Array.from(syllables).map((s) => s.length));
  const cache = new Map<string, number>();
  const solution = words.reduce((acc, w) => {
    const count = can_form_word_count(w, syllables, maxSyllableLength, cache);
    return acc + count;
  }, 0);

  return solution;
}

const plugin: DayPlugin = {
  name: "Day 19",
  async run({ verbose }) {
    console.log(`Running ${this.name} verbose=${verbose}`);
    const data = await fileio.linesFromFile(`${import.meta.dir}/day19.txt`);

    const start1 = performance.now();
    const solution1 = solvePart1(data);
    const duration1 = (performance.now() - start1).toFixed(3);
    console.log(`${this.name} verbose=${verbose}.Part1 = ${solution1} : Elapsed time: ${duration1}ms`);
    assert.strictEqual(solution1, 344, `${this.name} verbose=${verbose}.Part1`);

    const start2 = performance.now();
    const solution2 = solvePart2(data);
    const duration2 = (performance.now() - start2).toFixed(3);
    console.log(`${this.name} verbose=${verbose}.Part2 = ${solution2} : Elapsed time: ${duration2}ms`);
    assert.strictEqual(solution2, 996172272010026, `${this.name} verbose=${verbose}.Part2`);
  },
};

export default plugin;
