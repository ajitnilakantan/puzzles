import assert from "node:assert";
import { Poset } from "../lib/poset";
import { debug, fileio } from "../lib/prelude";
import type { DayPlugin } from "../src/plugin";

// export private functions for testing
export const __test__ = { parseData, checkUpdate };

function parseData(lines: string[]): [adjacencies: Map<number, Set<number>>, updates: number[][]] {
  const chunks = fileio.chunkLines(lines);
  // Expect to have two sections
  assert(chunks.length === 2);

  // Create map of adjacency constraints (a < [b, c, d])
  const adjacencies = new Map<number, Set<number>>();
  for (const line of chunks[0]) {
    const tokens = fileio.tokenize(line, "|");
    assert(tokens.length === 2);
    const [a, b] = [Number(tokens[0]), Number(tokens[1])];
    const current = adjacencies.get(a) ?? new Set();
    current.add(b);
    adjacencies.set(a, current);
  }

  // Read the updates records
  const updates: number[][] = [];
  for (const line of chunks[1]) {
    const pages = fileio.tokenize(line, ",").map<number>((val) => Number(val));
    updates.push(pages);
  }

  return [adjacencies, updates];
}

function checkUpdate(update: number[], adjacencies: Map<number, Set<number>>): boolean {
  const positions = new Map<number, number>();
  // Map of page to its position
  for (const [index, page] of update.entries()) {
    positions.set(page, index);
  }
  for (const [index, page] of update.entries()) {
    const pageAdjacencies = adjacencies.get(page) ?? new Set();
    for (const adjacent of pageAdjacencies) {
      const adjindex = positions.get(adjacent);
      if (adjindex != undefined && adjindex < index) {
        return false;
      }
    }
  }
  // OK, all test pass
  return true;
}

function solvePart1(lines: string[]): number {
  const [adjacencies, updates] = parseData(lines);
  const solution = updates.reduce<number>((acc, update) => {
    if (checkUpdate(update, adjacencies)) {
      assert(update.length % 2 === 1); // Should be odd to choose the middle item
      return acc + update[Math.trunc(update.length / 2)];
    } else {
      return acc;
    }
  }, 0);

  return solution;
}

function solvePart2(lines: string[]): number {
  const [adjacencies, updates] = parseData(lines);
  const badUpdates = updates.filter((val) => !checkUpdate(val, adjacencies));

  let solution = 0;

  // Create posets for each update only.
  // Creating one for the entire set or rules fails because they are inconsistent.
  for (const badUpdate of badUpdates) {
    const usedPages = new Set<number>(badUpdate);
    const poset = new Poset<number>();
    for (const p of usedPages) {
      if (adjacencies.has(p)) {
        for (const v of adjacencies.get(p) ?? []) {
          if (usedPages.has(v)) {
            poset.addConstraint(p, v);
          }
        }
      }
    }
    const fixedUpdate = poset.sort(badUpdate);
    assert(checkUpdate(fixedUpdate, adjacencies));
    assert(fixedUpdate.length % 2 === 1); // Should be odd to choose the middle item
    solution += fixedUpdate[Math.trunc(fixedUpdate.length / 2)];
  }
  return solution;
}

const plugin: DayPlugin = {
  name: "Day 05",
  async run({ verbose }) {
    console.log(`Running ${this.name} verbose=${verbose}`);
    const data = await fileio.linesFromFile(`${import.meta.dir}/day05.txt`);

    const start1 = performance.now();
    const solution1 = solvePart1(data);
    const duration1 = (performance.now() - start1).toFixed(3);
    console.log(`${this.name} verbose=${verbose}.Part1 = ${solution1} : Elapsed time: ${duration1}ms`);
    assert.strictEqual(solution1, 7307, `${this.name} verbose=${verbose}.Part1`);

    const start2 = performance.now();
    const solution2 = solvePart2(data);
    const duration2 = (performance.now() - start2).toFixed(3);
    console.log(`${this.name} verbose=${verbose}.Part2 = ${solution2} : Elapsed time: ${duration2}ms`);
    assert.strictEqual(solution2, 4713, `${this.name} verbose=${verbose}.Part2`);
  },
};

export default plugin;
