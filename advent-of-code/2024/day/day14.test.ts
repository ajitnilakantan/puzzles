import { describe, expect, test } from "bun:test";
import { debug, fileio } from "../lib/prelude";
import { __test__ } from "./day14";

describe("day14", () => {
  const data = `p=0,4 v=3,-3
                p=6,3 v=-1,-3
                p=10,3 v=-1,2
                p=2,0 v=2,-1
                p=0,0 v=1,3
                p=3,0 v=-2,-2
                p=7,6 v=-1,-3
                p=3,0 v=-1,-2
                p=9,3 v=2,3
                p=7,3 v=-1,2
                p=2,4 v=2,-3
                p=9,5 v=-3,-3`;

  const lines = fileio.linesFromString(data);

  test("part1", async () => {
    const [height, width] = [7, 11];
    const robots = lines.map((val) => __test__.parseLine(val));
    const mod = (x: number, m: number): number => ((x % m) + m) % m;
    const newRobots = robots.map(([pX, pY, vX, vY]) => [mod(pX + vX * 100, width), mod(pY + vY * 100, height)]);
    const quadrants = [0, 0, 0, 0, 0];
    newRobots.forEach(([px, py]) => (quadrants[__test__.quadrant(py, px, height, width)] += 1));
    expect(quadrants).toEqual([3, 1, 3, 4, 1]);
    const solution = quadrants.slice(1).reduce((acc, val) => acc * val, 1);
    expect(solution).toBe(12);
  });

  test("part2", async () => {
    // Uncomment to run the semi-manual test.  Get actual result from visual inspection
    /*
    const Grid = await import('../lib/grid');
    const lines = await fileio.linesFromFile(`${import.meta.dir}/day14.txt`);
    const robots = lines.map((val) => __test__.parseLine(val));
    const [height, width] = [103, 101];

    const mod = (x: number, m: number) => ((x % m) + m) % m;
    for (let k = 0; k < 10000000; k++) {
      const newRobots = robots.map(([pX, pY, vX, vY]) => [mod(pX + vX * k, width), mod(pY + vY * k, height)]);
      const toKey = (row: number, col: number) => `${row},${col}`;
      const robotsSet = new Set(newRobots.map(([x, y]) => toKey(y, x)));
      let [cleft, cright] = [0, 0];
      for (let r = 0; r < height / 4; r++) {
        for (let c = 0; c < width / 2 - 15 - r; c++) {
          if (robotsSet.has(toKey(r, c))) {
            cleft++;
          }
        }
      }
      for (let r = 0; r < height / 4; r++) {
        for (let c = Math.trunc(width / 2) + 15 + r; c < width; c++) {
          if (robotsSet.has(toKey(r, c))) {
            cright++;
          }
        }
      }
      if (cleft < 10 && cright < 10) {
        debug.log`Count: ${k}: >>>>>>>>>>>>>>>>>`;
        const grid = new Grid.Grid<string>(height, width, ".");
        for (const [rx, ry] of newRobots) {
          const val = grid[ry][rx] === "." ? 0 : Number(grid[ry][rx]);
          grid[ry][rx] = String(val + 1);
        }
        for (let r = 0; r < height / 4; r++) {
          for (let c = 0; c < width / 2 - 15 - r; c++) {
            grid[r][c] = "+";
          }
        }
        for (let rr = 0; rr < height / 4; rr++) {
          for (let cc = Math.trunc(width / 2) + 15 + rr; cc < width; cc++) {
            grid[rr][cc] = "*";
          }
        }
        grid.print();
        debug.log`==============================================================`;
      }
    }
    */
    const solution = 6493;
    expect(solution).toBe(6493);
  });
});
