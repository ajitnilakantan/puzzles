"""Advent of Code 2020, day 12: Rain Risk."""

from pathlib import Path

from registry import aoc_day

INPUT = Path(__file__).with_name("day12.txt")

# Clockwise from east: E, S, W, N
HEADINGS = [(1, 0), (0, -1), (-1, 0), (0, 1)]


def instructions() -> list[tuple[str, int]]:
    return [
        (line[0], int(line[1:]))
        for line in INPUT.read_text().splitlines()
        if line.strip()
    ]


def rotate(vector: tuple[int, int], degrees: int) -> tuple[int, int]:
    """Rotate (x, y) by a multiple of 90 degrees, counter-clockwise positive."""
    x, y = vector
    for _ in range(abs(degrees) // 90):
        x, y = (-y, x) if degrees > 0 else (y, -x)
    return x, y


def part1() -> int:
    """Ship moves itself; N/E/S/W are absolute, L/R turn the ship."""
    x = y = 0
    heading = 0
    for action, value in instructions():
        if action == "F":
            x += HEADINGS[heading][0] * value
            y += HEADINGS[heading][1] * value
        elif action in "NSEW":
            dx, dy = {"N": (0, 1), "S": (0, -1), "E": (1, 0), "W": (-1, 0)}[action]
            x += dx * value
            y += dy * value
        else:
            heading = (heading + (1 if action == "R" else -1) * (value // 90)) % 4
    return abs(x) + abs(y)


def part2() -> int:
    """Ship moves toward a waypoint; N/E/S/W and L/R move or turn the waypoint."""
    x = y = 0
    wx, wy = 10, 1
    for action, value in instructions():
        if action == "F":
            x += wx * value
            y += wy * value
        elif action in "NSEW":
            dx, dy = {"N": (0, 1), "S": (0, -1), "E": (1, 0), "W": (-1, 0)}[action]
            wx += dx * value
            wy += dy * value
        else:
            wx, wy = rotate((wx, wy), value if action == "L" else -value)
    return abs(x) + abs(y)


def solve() -> str:
    return f"Part 1: {part1()}\nPart 2: {part2()}"


main = aoc_day("day12")(solve)
