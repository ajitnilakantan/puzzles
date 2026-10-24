import assert from "node:assert";
import { debug, fileio } from "../lib/prelude";
import { RingBuffer } from "../lib/ringbuffer";
import type { DayPlugin } from "../src/plugin";

// export private functions for testing
export const __test__ = { getNthPseudo, createFirstTable };

function getNthPseudo(val: number, count: number): number {
  let result = val;
  for (let c = 0; c < count; c++) {
    result = ((result << 6) ^ result) & 0xff_ffff;
    result = ((result >> 5) ^ result) & 0xff_ffff;
    result = ((result << 11) ^ result) & 0xff_ffff;
  }
  return result;
}

class PatternMap extends Map<number, Map<number, number>> {
  allKeys = new Set<number>();

  // eslint-disable-next-line @typescript-eslint/no-useless-constructor
  constructor(entries?: readonly (readonly [number, Map<number, number>])[] | null) {
    super(entries);
  }

  addEntry(v: number[], column: number, position: number): void {
    const toKey = (v0: number, v1: number, v2: number, v3: number): number =>
      v0 + 9 + (v1 + 9) * 20 + (v2 + 9) * 20 * 20 + (v3 + 9) * 20 * 20 * 20;
    const key = toKey(v[0], v[1], v[2], v[3]);
    const current = this.get(key);
    if (current === undefined) {
      const newVal = new Map<number, number>();
      newVal.set(column, position);
      this.set(key, newVal);
      this.allKeys.add(key);
    } else {
      if (current.get(column) === undefined) {
        current.set(column, position);
      }
    }
  }
}

function createFirstTable(seeds: number[]): [number, number] {
  const firstTable = new PatternMap();
  for (const [index, seed] of seeds.entries()) {
    let result = seed;
    let oldResult = 0;
    const rb = new RingBuffer<number>(4);
    for (let k = 0; k < 2000; k++) {
      rb.push((result % 10) - (oldResult % 10));
      oldResult = result;
      result = ((result << 6) ^ result) & 0xff_ffff;
      result = ((result >> 5) ^ result) & 0xff_ffff;
      result = ((result << 11) ^ result) & 0xff_ffff;
      if (k >= 4) {
        assert(rb.size === 4);
        firstTable.addEntry(rb.toArray(), index, oldResult % 10);
      }
    }
  }

  let maxScore = 0;
  let maxKey = 0;
  for (const key of firstTable.allKeys) {
    const total = Array.from(firstTable.get(key)!.entries()).reduce((acc, [_key, value]) => {
      return acc + value;
    }, 0);
    if (total > maxScore) {
      maxScore = total;
      maxKey = key;
    }
  }

  return [maxScore, maxKey];
}

function solvePart1(lines: string[]): number {
  const initial = lines.map((l) => Number(l));
  const after2000 = initial.map((n) => getNthPseudo(n, 2000));
  const solution = after2000.reduce((acc, val) => acc + val, 0);

  return solution;
}

function solvePart2(lines: string[]): number {
  const seeds = lines.map((l) => Number(l));
  const [maxScore, _maxKey] = createFirstTable(seeds);
  const solution = maxScore;

  return solution;
}

const plugin: DayPlugin = {
  name: "Day 22",
  async run({ verbose }) {
    console.log(`Running ${this.name}, verbose=${verbose}`);
    const data = await fileio.linesFromFile(`${import.meta.dir}/day22.txt`);

    const start1 = performance.now();
    const solution1 = solvePart1(data);
    const duration1 = (performance.now() - start1).toFixed(3);
    console.log(`${this.name}.Part1 = ${solution1} : Elapsed time: ${duration1}ms`);
    assert.strictEqual(solution1, 17724064040, `${this.name}.Part1`);

    const start2 = performance.now();
    const solution2 = solvePart2(data);
    const duration2 = (performance.now() - start2).toFixed(3);
    console.log(`${this.name}.Part2 = ${solution2} : Elapsed time: ${duration2}ms`);
    assert.strictEqual(solution2, 1998, `${this.name}.Part2`);
  },
};

export default plugin;
