import assert from "node:assert";

export class Grid<T> {
  private data: T[];
  public readonly numrows: number;
  public readonly numcols: number;
  // The 4 possible directions: [rowOffset, colOffset]
  public static readonly Directions4 = [
    [-1, 0], // Up
    [1, 0], // Down
    [0, -1], // Left
    [0, 1], // Right
  ];
  // The 8 possible directions: [rowOffset, colOffset]
  public static readonly Directions8 = [
    [-1, 0], // Up
    [1, 0], // Down
    [0, -1], // Left
    [0, 1], // Right
    [-1, -1], // Up-Left
    [-1, 1], // Up-Right
    [1, -1], // Down-Left
    [1, 1], // Down-Right
  ];
  // Type signature to allow the literal grid[row][col] bracket syntax
  [row: number]: Record<number, T>;

  constructor(rows: number, cols: number, initialValue: T) {
    if (rows <= 0 || cols <= 0) {
      throw new Error("Grid dimensions must be greater than zero.");
    }
    this.numrows = rows;
    this.numcols = cols;
    this.data = new Array<T>(rows * cols).fill(initialValue);

    // Proxy wrapper to intercept bracket index access
    return new Proxy(this, {
      get: (target, rowKey): unknown => {
        // Forward standard methods and property lookups (like rows, cols, map, get)
        if (typeof rowKey === "symbol" || isNaN(Number(rowKey))) {
          return Reflect.get(target, rowKey);
        }

        const row = Number(rowKey);
        if (row < 0 || row >= target.numrows) {
          throw new RangeError(`Row index ${row} is out of bounds.`);
        }

        // Return an ephemeral row proxy to handle column extraction
        return new Proxy(Object.create(null) as object, {
          get: (_, colKey): T => {
            const col = Number(colKey);
            if (isNaN(col) || col < 0 || col >= target.numcols) {
              throw new RangeError(`Column index ${colKey.toString()} is out of bounds.`);
            }
            return target.data[row * target.numcols + col];
          },
          set: (_, colKey, value: T): boolean => {
            const col = Number(colKey);
            if (isNaN(col) || col < 0 || col >= target.numcols) {
              throw new RangeError(`Column index ${colKey.toString()} is out of bounds.`);
            }
            target.data[row * target.numcols + col] = value;
            return true;
          },
        });
      },
    });
  }

  /**
   * Safe boundary validation helper.
   */
  public isValidCoord(row: number, col: number): boolean {
    return row >= 0 && row < this.numrows && col >= 0 && col < this.numcols;
  }

  /**
   * Helper method alternative to look up data.
   */
  public get(row: number, col: number): T {
    if (!this.isValidCoord(row, col)) {
      throw new RangeError(`Coordinates (${row}, ${col}) are out of bounds.`);
    }
    return this.data[row * this.numcols + col];
  }

  /**
   * Helper method alternative to set data.
   */
  public set(row: number, col: number, value: T): void {
    if (!this.isValidCoord(row, col)) {
      throw new RangeError(`Coordinates (${row}, ${col}) are out of bounds.`);
    }
    this.data[row * this.numcols + col] = value;
  }

  /**
   * Creates a deep copy of this grid and its values.
   */
  public copy(): Grid<T> {
    const copiedGrid = new Grid<T>(this.numrows, this.numcols, this.data[0]);
    copiedGrid.data = structuredClone(this.data);
    return copiedGrid;
  }

  /**
   * Executes a provided function once for each element in the grid.
   */
  public forEach(callback: (value: T, row: number, col: number, grid: Grid<T>) => void): void {
    for (let r = 0; r < this.numrows; r++) {
      const rowOffset = r * this.numcols;
      for (let c = 0; c < this.numcols; c++) {
        callback(this.data[rowOffset + c], r, c, this);
      }
    }
  }

  /**
   * Get the neighbours of a node
   */
  public *neighbours4(row: number, col: number): Generator<[number, number, T]> {
    for (const [dRow, dCol] of Grid.Directions4) {
      const [r, c] = [row + dRow, col + dCol];
      if (r >= 0 && r < this.numrows && c >= 0 && c < this.numcols) {
        yield [r, c, this.get(r, c)];
      }
    }
  }

  /**
   * Get the neighbours of a node
   */
  public *neighbours8(row: number, col: number): Generator<[number, number, T]> {
    for (const [dRow, dCol] of Grid.Directions8) {
      const [r, c] = [row + dRow, col + dCol];
      if (r >= 0 && r < this.numrows && c >= 0 && c < this.numcols) {
        yield [r, c, this.get(r, c)];
      }
    }
  }

  /**
   * Find element that matches a provided function
   */
  public find(
    callback: (value: T, row: number, col: number, grid: Grid<T>) => boolean
  ): [T, number, number] | undefined {
    for (let r = 0; r < this.numrows; r++) {
      const rowOffset = r * this.numcols;
      for (let c = 0; c < this.numcols; c++) {
        if (callback(this.data[rowOffset + c], r, c, this)) {
          return [this.data[rowOffset + c], r, c];
        }
      }
    }
    return undefined;
  }

  /**
   * Creates a new Grid populated with the results of calling
   * a provided function on every element in this grid.
   */
  public map<U>(
    callback: (value: T, row: number, col: number, grid: Grid<T>) => U,
    initialValueForNewGrid: U
  ): Grid<U> {
    const newGrid = new Grid<U>(this.numrows, this.numcols, initialValueForNewGrid);
    for (let r = 0; r < this.numrows; r++) {
      const rowOffset = r * this.numcols;
      for (let c = 0; c < this.numcols; c++) {
        const newValue = callback(this.data[rowOffset + c], r, c, this);
        newGrid.set(r, c, newValue);
      }
    }
    return newGrid;
  }

  /**
   * Clear or fill the entire grid efficiently.
   */
  public fill(value: T): void {
    this.data.fill(value);
  }
  /**
   * Print out a grid
   */
  public print(): void {
    for (const r of Grid.range(this.numrows)) {
      let row = "";
      for (const c of Grid.range(this.numcols)) {
        row += `${this.get(r, c)}`;
      }
      console.log(row);
    }
  }
  /**
   * Range generator [0..start)  or  [start..end)
   */
  static *range(start: number, end?: number, step = 1): Generator<number> {
    const actualStart = end === undefined ? 0 : start;
    const actualEnd = end ?? start;

    for (let i = actualStart; i < actualEnd; i += step) {
      yield i;
    }
  }
  /**
   * Product of two iterables
   * genX, genY are either iterator factories (to avoid iterator exhaution when re-using iterators)
   * or number (which are converted to [0..n) ranges.
   */
  public static *product<T, U>(
    inputX: number | (() => Iterable<T>),
    inputY: number | (() => Iterable<U>)
  ): Generator<[T | number, U | number]> {
    // Type guard
    const isNumber = (val: unknown): val is number => typeof val === "number";
    const getX = isNumber(inputX) ? (): Iterable<number> => Grid.range(inputX) : inputX;
    const getY = isNumber(inputY) ? (): Iterable<number> => Grid.range(inputY) : inputY;

    for (const x of getX()) {
      for (const y of getY()) {
        yield [x, y];
      }
    }
  }
  /**
   * Create a grid from a list of strings. Optionally pad with a 1-border all around.
   */
  public static fromLines<T>(lines: string[], defaultValue: T, padded = false): Grid<T> {
    assert(lines.length >= 1);
    const numrows: number = lines.length;
    const numcols: number = lines[0].length;
    // Check all lines have same length
    lines.forEach((val) => {
      assert(val.length == numcols);
    });

    let grid: Grid<T>;
    if (padded) {
      grid = new Grid(numrows + 2, numcols + 2, defaultValue);
      for (const [r, c] of Grid.product<number, number>(numrows, numcols)) {
        grid.set(r + 1, c + 1, lines[r][c] as T);
      }
    } else {
      grid = new Grid(numrows, numcols, defaultValue);
      for (const [r, c] of Grid.product<number, number>(numrows, numcols)) {
        grid.set(r, c, lines[r][c] as T);
      }
    }
    return grid;
  }
}
