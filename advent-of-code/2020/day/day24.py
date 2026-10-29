"""Advent of Code 2020, day 24: Lobby Layout.

Hex tiles use cube coordinates, see https://www.redblobgames.com/grids/hexagons/
"""

from collections import Counter
from pathlib import Path

from registry import aoc_day

INPUT = Path(__file__).with_name("day24.txt")

# Cube-coordinate steps for each direction.
STEPS = {
    "e": (1, -1, 0),
    "w": (-1, 1, 0),
    "ne": (1, 0, -1),
    "sw": (-1, 0, 1),
    "nw": (0, 1, -1),
    "se": (0, -1, 1),
}

NEIGHBOURS = list(STEPS.values())

DAYS = 100


def landing(line: str) -> tuple[int, int, int]:
    x = y = z = 0
    index = 0
    while index < len(line):
        if line[index] in "ew":
            step = STEPS[line[index]]
            index += 1
        else:
            step = STEPS[line[index : index + 2]]
            index += 2
        x, y, z = x + step[0], y + step[1], z + step[2]
    return x, y, z


def initial_black() -> set[tuple[int, int, int]]:
    black: set[tuple[int, int, int]] = set()
    for line in INPUT.read_text().splitlines():
        tile = landing(line.strip())
        black ^= {tile}  # Flip: black becomes white and vice versa.
    return black


def tick(black: set[tuple[int, int, int]]) -> set[tuple[int, int, int]]:
    """One day of the hexagonal cellular automaton."""
    counts: Counter = Counter()
    for x, y, z in black:
        for dx, dy, dz in NEIGHBOURS:
            counts[(x + dx, y + dy, z + dz)] += 1
    return {
        tile
        for tile, count in counts.items()
        if count == 2 or (count == 1 and tile in black)
    }


def part1() -> int:
    return len(initial_black())


def part2() -> int:
    black = initial_black()
    for _ in range(DAYS):
        black = tick(black)
    return len(black)


def solve() -> str:
    return f"Part 1: {part1()}\nPart 2: {part2()}"


main = aoc_day("day24")(solve)
