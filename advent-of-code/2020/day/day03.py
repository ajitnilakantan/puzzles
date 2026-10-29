"""Advent of Code 2020, day 03: Toboggan Trajectory."""

from functools import reduce
from pathlib import Path
from operator import mul

from registry import aoc_day

INPUT = Path(__file__).with_name("day03.txt")

SLOPES = [(1, 1), (3, 1), (5, 1), (7, 1), (1, 2)]


def count_trees(grid: list[str], right: int, down: int) -> int:
    width = len(grid[0])
    hits = 0
    for row in range(down, len(grid), down):
        col = (row // down) * right % width
        if grid[row][col] == "#":
            hits += 1
    return hits


def part1() -> int:
    return count_trees(INPUT.read_text().splitlines(), 3, 1)


def part2() -> int:
    grid = INPUT.read_text().splitlines()
    return reduce(mul, (count_trees(grid, right, down) for right, down in SLOPES), 1)


def solve() -> str:
    return f"Part 1: {part1()}\nPart 2: {part2()}"


main = aoc_day("day03")(solve)
