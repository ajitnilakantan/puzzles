import { describe, expect, spyOn, test } from "bun:test";
import { Grid } from "./grid";

describe("grid", () => {
  // Usage:
  test("grid tests", async () => {
    // Create a string grid
    const grid = new Grid<string>(2, 3, ".");
    expect(grid.numrows).toBe(2);
    expect(grid.numcols).toBe(3);
    expect(grid[0][1]).toBe(".");

    // get/set
    grid[0][1] = "A";
    expect(grid[0][1]).toBe("A");
    expect(grid.get(0, 1)).toBe("A");

    // [][] notation
    expect(grid[1][2]).toBe(".");
    grid.set(1, 2, "B");
    expect(grid[1][2]).toBe("B");
    expect(grid.get(1, 2)).toBe("B");

    // Deep copy
    const nestedGrid = new Grid<{ value: string }>(1, 2, { value: "" });
    nestedGrid.set(0, 0, { value: "A" });
    const copiedGrid = nestedGrid.copy();
    copiedGrid.get(0, 0).value = "B";
    expect(nestedGrid.get(0, 0).value).toBe("A");
    expect(copiedGrid.get(0, 0).value).toBe("B");
    copiedGrid.set(0, 1, { value: "C" });
    expect(nestedGrid.get(0, 1).value).toBe("");

    // Output
    const grid2 = Grid.fromLines(["abc", "def"], ".");

    const logSpy = spyOn(console, "log").mockImplementation(() => {
      /*empty*/
    });
    grid2.print();
    expect(logSpy).toHaveBeenCalledTimes(2);
    expect(logSpy).toHaveBeenNthCalledWith(1, "abc");
    expect(logSpy).toHaveBeenNthCalledWith(2, "def");
    logSpy.mockRestore();
  });

  test("grid iterators", async () => {
    const r1 = Array.from(Grid.range(0, 5));
    expect(r1).toEqual([0, 1, 2, 3, 4]);
    const r2 = Array.from(Grid.range(0, 5, 3));
    expect(r2).toEqual([0, 3]);

    let result = "";
    for (const [r, c] of Grid.product(
      () => Grid.range(2),
      () => Grid.range(3)
    )) {
      result += `${r},${c};`;
    }
    expect(result).toBe("0,0;0,1;0,2;1,0;1,1;1,2;");

    result = "";
    for (const [r, c] of Grid.product(3, 2)) {
      result += `${r},${c};`;
    }
    expect(result).toBe("0,0;0,1;1,0;1,1;2,0;2,1;");
  });
});
