import { describe, expect, test } from "bun:test";
import { debug, fileio } from "../lib/prelude";
import { __test__ } from "./day15";

describe("day15", () => {
  const data1 = [
    `########
     #..O.O.#
     ##@.O..#
     #...O..#
     #.#.O..#
     #...O..#
     #......#
     ########

     <^^>>>vv<v>>v<<`,

    `##########
     #..O..O.O#
     #......O.#
     #.OO..O.O#
     #..O@..O.#
     #O#..O...#
     #O..O..O.#
     #.OO.O.OO#
     #....O...#
     ##########

     <vv>^<v^>v>^vv^v>v<>v^v<v<^vv<<<^><<><>>v<vvv<>^v^>^<<<><<v<<<v^vv^v>^
     vvv<<^>^v^^><<>>><>^<<><^vv^^<>vvv<>><^^v>^>vv<>v<<<<v<^v>^<^^>>>^<v<v
     ><>vv>v^v^<>><>>>><^^>vv>v<^^^>>v^v^<^^>v^^>v^<^v>v<>>v^v^<v>v^^<^^vv<
     <<v<^>>^^^^>>>v^<>vvv^><v<<<>^^^vv^<vvv>^>v<^^^^v<>^>vvvv><>>v^<<^^^^^
     ^><^><>>><>^^<<^^v>>><^<v>^<vv>>v>>>^v><>^v><<<<v>>v<v<v>vvv>^<><<>^><
     ^>><>^v<><^vvv<^^<><v<<<<<><^v<<<><<<^^<v<^^^><^>>^<v^><<<^>>^v<v^v<v^
     >^>>^v>vv>^<<^v<>><<><<v<<v><>v<^vv<<<>^^v^>^^>>><<^v>>v^v><^^>>^<>vv^
     <><^^>^^^<><vvvvv^v<v<<>^v<v>v<<^><<><<><<<^^<<<^<<>><<><^^^>^^<>^>v<>
     ^^>vv<^v^v<vv>^<><v<^v>^^^>>>^^vvv^>vvv<>>>^<^>>>>>^<<^v>^vvv<>^<><<v>
     v^^>>><<^^<>>^v^<v^vv<>v^<<>^<^v^v><^<<<><<^<v><v<>vv>>v><v^<vv<>v^<<^`,
  ];
  const data2 = [
    `#######
     #...#.#
     #.....#
     #..OO@#
     #..O..#
     #.....#
     #######

     <vv<<^^<<^^`,
  ];

  const expected1 = [2028, 10092];
  const expected2 = [618, 9021];

  test("part1", async () => {
    for (const [index, data] of data1.entries()) {
      const lines = fileio.linesFromString(data);
      const chunks = fileio.chunkLines(lines);
      const [grid, moves, pos] = __test__.parseData(chunks);
      let newpos = pos;
      for (const m of moves) {
        newpos = __test__.move(newpos, m, grid);
      }

      const solution = __test__.scoreGrid(grid);
      expect(solution).toBe(expected1[index]);
    }
  });

  test("part2", async () => {
    const datas = [data2[0], data1[1]];
    for (const [index, data] of datas.entries()) {
      const lines = fileio.linesFromString(data);
      const chunks = fileio.chunkLines(lines);
      const [grid, moves, pos] = __test__.parseData(chunks);
      const newGrid = __test__.expandGrid(grid);
      let newpos = { row: pos.row, col: 2 * pos.col };
      for (const m of moves) {
        newpos = __test__.move2(newpos, m, newGrid);
      }

      const solution = __test__.scoreGrid(newGrid);
      expect(solution).toBe(expected2[index]);
    }
  });
});
