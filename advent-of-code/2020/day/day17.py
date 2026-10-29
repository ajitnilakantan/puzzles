"""Advent of Code 2020, day 17: Conway Cubes."""

from itertools import product
from pathlib import Path

from registry import aoc_day

INPUT = Path(__file__).with_name("day17.txt")

CYCLES = 6


def active_cubes(dimensions: int) -> set[tuple[int, ...]]:
    """Initial active cubes, padded with zeros for the extra dimensions."""
    active = set()
    for y, line in enumerate(INPUT.read_text().splitlines()):
        for x, cell in enumerate(line):
            if cell == "#":
                active.add((x, y) + (0,) * (dimensions - 2))
    return active


def neighbours(cube: tuple[int, ...]) -> set[tuple[int, ...]]:
    return {
        tuple(coord + delta for coord, delta in zip(cube, offset))
        for offset in product((-1, 0, 1), repeat=len(cube))
        if any(offset)
    }


def run(dimensions: int) -> int:
    active = active_cubes(dimensions)
    for _ in range(CYCLES):
        counts: dict[tuple[int, ...], int] = {}
        for cube in active:
            for neighbour in neighbours(cube):
                counts[neighbour] = counts.get(neighbour, 0) + 1

        active = {
            cube
            for cube, count in counts.items()
            if count == 3 or (count == 2 and cube in active)
        }
    return len(active)


def part1() -> int:
    return run(3)


def part2() -> int:
    return run(4)


def solve() -> str:
    return f"Part 1: {part1()}\nPart 2: {part2()}"


main = aoc_day("day17")(solve)
