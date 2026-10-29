"""Advent of Code 2020, day 11: Seating System."""

from pathlib import Path

from registry import aoc_day

INPUT = Path(__file__).with_name("day11.txt")

NEIGHBOURS = ((-1, -1), (-1, 0), (-1, 1), (0, -1), (0, 1), (1, -1), (1, 0), (1, 1))


def grid() -> list[list[str]]:
    return [list(line) for line in INPUT.read_text().splitlines() if line.strip()]


def occupied(
    seats: list[list[str]], row: int, col: int, dr: int, dc: int, reach: int
) -> bool:
    """True when an occupied seat is visible along one direction."""
    rows, cols = len(seats), len(seats[0])
    for step in range(1, reach):
        r, c = row + dr * step, col + dc * step
        if not (0 <= r < rows and 0 <= c < cols):
            return False
        if seats[r][c] == "L":
            return False
        if seats[r][c] == "#":
            return True
    return False


def step(seats: list[list[str]], reach: int, tolerance: int) -> list[list[str]]:
    nxt = [row[:] for row in seats]
    for r in range(len(seats)):
        for c in range(len(seats[0])):
            count = sum(occupied(seats, r, c, dr, dc, reach) for dr, dc in NEIGHBOURS)
            if seats[r][c] == "L" and count == 0:
                nxt[r][c] = "#"
            elif seats[r][c] == "#" and count >= tolerance:
                nxt[r][c] = "L"
    return nxt


def settle(reach: int, tolerance: int) -> int:
    seats = grid()
    while True:
        nxt = step(seats, reach, tolerance)
        if nxt == seats:
            return sum(row.count("#") for row in seats)
        seats = nxt


def part1() -> int:
    """Adjacent seats only, empty out at 4 or more neighbours."""
    return settle(reach=2, tolerance=4)


def part2() -> int:
    """Look past floor to the first seat in each direction, empty out at 5 or more."""
    return settle(reach=max(len(grid()), len(grid()[0])), tolerance=5)


def solve() -> str:
    return f"Part 1: {part1()}\nPart 2: {part2()}"


main = aoc_day("day11")(solve)
