import assert from "node:assert";
import { Heap } from "../lib/heap";
import type { Comparator } from "../lib/heap";
import { debug, fileio } from "../lib/prelude";
import type { DayPlugin } from "../src/plugin";

// export private functions for testing
export const __test__ = { parseLine, fillGaps, toString, toArray, fillGapsPart2 };

export interface Space {
  id: number;
  position: number;
  count: number;
}

// Parse one line containing alternative blocks/free counts
function parseLine(line: string): [Space[], Space[]] {
  const blocks: Space[] = [];
  const freespace: Space[] = [];
  let id = 0;
  let position = 0;
  for (let i = 0; i < line.length; i += 2) {
    const count = Number(line[i]);
    const gap = i + 1 < line.length ? Number(line[i + 1]) : 0;
    assert(count !== 0); // Not sure what this means if it happens
    const s: Space = { id: id, position: position, count: count };
    blocks.push(s);
    id += 1;
    position += count;
    if (gap !== 0) {
      const s: Space = { id: -1, position: position, count: gap };
      freespace.push(s);
      position += gap;
    }
  }
  return [blocks, freespace];
}

function fillGaps(blocks: Space[], freespace: Space[]): [Space[], Space[]] {
  blocks = structuredClone(blocks); // make a copy
  freespace = structuredClone(freespace); // make a copy
  const newBlocks: Space[] = [];
  let currentBlock = blocks.length - 1;
  let currentGap = 0;
  do {
    if (blocks[currentBlock].count <= freespace[currentGap].count) {
      const count = blocks[currentBlock].count;
      const position = freespace[currentGap].position;
      // Add new block
      newBlocks.push({ id: blocks[currentBlock].id, position: position, count: count });
      // Update counts
      freespace[currentGap].position += count;
      freespace[currentGap].count -= count;
      blocks[currentBlock].count = 0;
      currentBlock -= 1;
      if (freespace[currentGap].count === 0) {
        currentGap += 1;
      }
    } else {
      const count = freespace[currentGap].count;
      const position = freespace[currentGap].position;
      // Add new block
      newBlocks.push({ id: blocks[currentBlock].id, position: position, count: count });
      // Update counts
      freespace[currentGap].position += count;
      freespace[currentGap].count -= count;
      blocks[currentBlock].count -= count;
      if (freespace[currentGap].count === 0) {
        currentGap += 1;
      }
    }
  } while (freespace[currentGap].position < blocks[currentBlock].position);

  // Return updates blocks and the newly defragged blocks
  return [blocks, newBlocks];
}

function fillGapsPart2(blocks: Space[], freespace: Space[]): [Space[], Space[]] {
  blocks = structuredClone(blocks); // make a copy
  freespace = structuredClone(freespace); // make a copy

  const freeBlocks = new Map<number, Heap<Space>>();
  const compareFn: Comparator<Space> = (a, b) => a.position - b.position;

  // Put all the free space blocks in a Map (keyed by freespace) for quick lookup
  let max_count = 0;
  freespace.forEach((v) => {
    const current = freeBlocks.get(v.count) ?? new Heap<Space>(compareFn);
    current.push(v);
    if (max_count < v.count) {
      max_count = v.count;
    }
    freeBlocks.set(v.count, current);
  });

  // Go backwards across the allocated blocks, try to find free space earlier
  const newBlocks: Space[] = [];
  let currentBlockIndex = blocks.length - 1;
  // Available gap size. Used to skip unnecessary search.
  let maxAvailable = Number.MAX_SAFE_INTEGER;
  do {
    const currentBlock = blocks[currentBlockIndex];
    let freeBlock = undefined;
    for (let key = currentBlock.count; currentBlock.count < maxAvailable && key <= max_count; key++) {
      const countList = freeBlocks.get(key) ?? new Heap<Space>(compareFn);
      const foundFreeBlock = countList.peek();
      if (countList.size() && foundFreeBlock !== undefined) {
        // Found one at >= size and < position
        if (
          foundFreeBlock.position < currentBlock.position &&
          (!freeBlock || foundFreeBlock.position < freeBlock.position)
        ) {
          // Found a free space. Pop it.
          freeBlock = foundFreeBlock;
        }
      }
    } // end-for
    assert(
      freeBlock === undefined || (freeBlock.position < currentBlock.position && freeBlock.count >= currentBlock.count)
    );
    if (!freeBlock && currentBlock.count < maxAvailable) {
      maxAvailable = currentBlock.count;
    }

    if (freeBlock) {
      // Found a space
      // Remove from map
      const countList = freeBlocks.get(freeBlock.count) ?? new Heap<Space>(compareFn);
      const _freeBlock = countList.pop();
      assert(_freeBlock === freeBlock);
      // Update positions
      const position = freeBlock.position;
      freeBlock.count -= currentBlock.count;
      freeBlock.position += currentBlock.count;
      assert(freeBlock.count >= 0);
      if (freeBlock.count > 0) {
        // Update the freeBlock with the smaller count and new position
        const current = freeBlocks.get(freeBlock.count) ?? new Heap<Space>(compareFn);
        current.push(freeBlock);
        freeBlocks.set(freeBlock.count, current);
      }
      // Update
      newBlocks.push({ id: currentBlock.id, position: position, count: currentBlock.count });
      currentBlock.id = -1;
      currentBlock.count = 0;
      assert(blocks[currentBlockIndex].id === -1);
    }

    currentBlockIndex -= 1;
  } while (currentBlockIndex > 1);

  // Return updates blocks and the newly defragged blocks
  return [blocks, newBlocks];
}

// Convert to array of ids.  Note, the trailing freespace is ignored
function toArray(blocks: Space[], freespace: Space[]): number[] {
  const allBlocks = [...blocks, ...freespace].toSorted((a, b) => a.position - b.position);
  const result = [];
  for (let i = 0; i < allBlocks.length; i++) {
    if (i > 0) {
      // Fill in gaps with -1
      const gapSize = allBlocks[i].position - (allBlocks[i - 1].position + allBlocks[i - 1].count);
      assert(gapSize >= 0);
      result.push(...Array<number>(gapSize).fill(-1));
    }
    result.push(...Array<number>(allBlocks[i].count).fill(allBlocks[i].id));
  }
  return result;
}

// Convert to string. Note the trailing "." (i.e. freespace is ignored)
// 00..111...  output print as 00...111
function toString(blocks: Space[], freespace: Space[]): string {
  const result = toArray(blocks, freespace)
    .map<string>((val) => {
      return val === -1 ? "." : String(val);
    })
    .join("");
  return result;
}

function solvePart1(lines: string[]): number {
  const line = lines[0];
  const [blocks, freespace] = parseLine(line);
  const [newBlocks, newDefrag] = fillGaps(blocks, freespace);
  const defrag = [...newBlocks, ...newDefrag].toSorted((a, b) => a.position - b.position).filter((a) => a.count > 0);
  const result = toArray(defrag, []);
  const solution = result.reduce((acc, val, index) => {
    assert(val !== -1);
    return acc + val * index;
  }, 0);

  return solution;
}

function solvePart2(lines: string[]): number {
  const line = lines[0];
  const [blocks, freespace] = parseLine(line);
  const [newBlocks, newDefrag] = fillGapsPart2(blocks, freespace);
  const defrag = [...newBlocks, ...newDefrag].toSorted((a, b) => a.position - b.position).filter((a) => a.count > 0);
  const result = toArray(defrag, []);
  const solution = result.reduce((acc, val, index) => {
    return acc + (val === -1 ? 0 : val) * index;
  }, 0);

  return solution;
}

const plugin: DayPlugin = {
  name: "Day 09",
  async run({ verbose }) {
    console.log(`Running ${this.name} verbose=${verbose}`);
    const data = await fileio.linesFromFile(`${import.meta.dir}/day09.txt`);

    const start1 = performance.now();
    const solution1 = solvePart1(data);
    const duration1 = (performance.now() - start1).toFixed(3);
    console.log(`${this.name} verbose=${verbose}.Part1 = ${solution1} : Elapsed time: ${duration1}ms`);
    assert.strictEqual(solution1, 6154342787400, `${this.name} verbose=${verbose}.Part1`);

    const start2 = performance.now();
    const solution2 = solvePart2(data);
    const duration2 = (performance.now() - start2).toFixed(3);
    console.log(`${this.name} verbose=${verbose}.Part2 = ${solution2} : Elapsed time: ${duration2}ms`);
    assert.strictEqual(solution2, 6183632723350, `${this.name} verbose=${verbose}.Part2`);
  },
};

export default plugin;
