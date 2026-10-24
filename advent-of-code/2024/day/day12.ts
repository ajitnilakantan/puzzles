import assert from "node:assert";
import { Grid } from "../lib/grid";
import { debug, fileio } from "../lib/prelude";
import type { DayPlugin } from "../src/plugin";

// export private functions for testing
export const __test__ = { splitRegions, measureRegion, numEdges };

export type Coord = [number, number];

// Use dfs to split into disjoint regions
function splitRegions(grid: Grid<string>): Coord[][] {
  const regions: Coord[][] = [];
  const visited = new Set<string>();
  const toKey = (r: number, c: number): string => `${r},${c}`;

  const dfs = (r: number, c: number, v: string, current: Coord[]): void => {
    if (r < 0 || r >= grid.numrows || c < 0 || c >= grid.numcols || grid[r][c] !== v || visited.has(toKey(r, c)))
      return;

    visited.add(toKey(r, c));
    current.push([r, c]);

    dfs(r + 1, c, v, current);
    dfs(r - 1, c, v, current);
    dfs(r, c + 1, v, current);
    dfs(r, c - 1, v, current);
  };

  for (const [r, c] of Grid.product<number, number>(grid.numrows, grid.numcols)) {
    if (!visited.has(toKey(r, c)) && grid[r][c] !== ".") {
      const region: Coord[] = [];
      const v = grid[r][c];
      dfs(r, c, v, region);
      regions.push(region);
    }
  }

  return regions;
}

// Scan the region and get the area, perimeter and list of vertices for the edges.
// The verticies are return in two maps for horizontal and vertical edges.
// rows is a map of row# and the column index of each horizontal 1-unit edge on that row.
// cols is a map of col# and the column index of each vertical 1-unit edge on that column.
function measureRegion(
  grid: Grid<string>,
  region: Coord[]
): [area: number, perimeter: number, rows_cols: Map<number, [number, number][]>[]] {
  let [area, perimeter] = [0, 0];
  // let vertices: Coord[] = [];
  const rows_up = new Map<number, [number, number][]>();
  const rows_down = new Map<number, [number, number][]>();
  const cols_left = new Map<number, [number, number][]>();
  const cols_right = new Map<number, [number, number][]>();
  const addToMap = (map: Map<number, [number, number][]>, key: number, value: [number, number]): void => {
    // Fetch the existing array, or default to an empty one if undefined
    const current = map.get(key) ?? [];
    // Push the new value
    current.push(value);
    // Save it back to the map
    map.set(key, current);
  };
  for (const [r, c] of region) {
    const val = grid.get(r, c);
    area++;
    for (const [dr, dc] of Grid.Directions4) {
      const [nr, nc] = [r + dr, c + dc];
      if (nr < 0 || nr >= grid.numrows || nc < 0 || nc >= grid.numcols || val !== grid.get(nr, nc)) {
        perimeter++;
        // Distinguish as per the example: "in particular, each section of fence has an in-side and an out-side,
        // so the fence does not connect across the middle of the region"
        // eslint-disable-next-line @typescript-eslint/switch-exhaustiveness-check
        switch (true) {
          case dr === -1 && dc === 0: // up
            addToMap(rows_up, r, [c, c + 1]);
            break;
          case dr === 1 && dc === 0: // down
            addToMap(rows_down, r + 1, [c, c + 1]);
            break;
          case dr === 0 && dc === -1: // left
            addToMap(cols_left, c, [r, r + 1]);
            break;
          case dr === 0 && dc === 1: // right
            addToMap(cols_right, c + 1, [r, r + 1]);
            break;
          default: // Exhaustiveness check
            throw new Error(`Unhandled fallback`);
        }
      }
    }
  }
  return [area, perimeter, [rows_up, rows_down, cols_left, cols_right]];
}

function numEdges(rows_cols: Map<number, [number, number][]>[]): number {
  let count = 0;
  for (const row_col of rows_cols) {
    for (const [_k, edges] of row_col) {
      const sorted = edges.toSorted(([a0, _a1], [b0, _b1]) => a0 - b0);
      assert(sorted.length > 0); // Should have at least 2 vertices
      let current = -10;
      for (const [v0, _v1] of sorted) {
        assert(_v1 - v0 === 1);
        if (v0 - current > 1) {
          count++;
        }
        current = v0;
      }
    }
  }

  return count;
}

function solvePart1(lines: string[]): number {
  const grid = Grid.fromLines(lines, ".");
  const regions = splitRegions(grid);
  const solution = regions
    .map((region) => measureRegion(grid, region))
    .reduce((acc, [area, perimeter, _rows_cols]) => acc + area * perimeter, 0);

  return solution;
}

function solvePart2(lines: string[]): number {
  const grid = Grid.fromLines(lines, ".");
  const regions = splitRegions(grid);
  const solution = regions
    .map((region) => measureRegion(grid, region))
    .reduce((acc, [area, _perimeter, rows_cols]) => acc + area * numEdges(rows_cols), 0);

  return solution;
}

const plugin: DayPlugin = {
  name: "Day 12",
  async run({ verbose }) {
    console.log(`Running ${this.name} verbose=${verbose}`);
    const data = await fileio.linesFromFile(`${import.meta.dir}/day12.txt`);

    const start1 = performance.now();
    const solution1 = solvePart1(data);
    const duration1 = (performance.now() - start1).toFixed(3);
    console.log(`${this.name} verbose=${verbose}.Part1 = ${solution1} : Elapsed time: ${duration1}ms`);
    assert.strictEqual(solution1, 1494342, `${this.name} verbose=${verbose}.Part1`);

    const start2 = performance.now();
    const solution2 = solvePart2(data);
    const duration2 = (performance.now() - start2).toFixed(3);
    console.log(`${this.name} verbose=${verbose}.Part2 = ${solution2} : Elapsed time: ${duration2}ms`);
    assert.strictEqual(solution2, 893676, `${this.name} verbose=${verbose}.Part2`); // 882166 low.
  },
};

export default plugin;
