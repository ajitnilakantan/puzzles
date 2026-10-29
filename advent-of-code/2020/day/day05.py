"""Advent of Code 2020, day 05: Binary Boarding."""

from pathlib import Path

from registry import aoc_day

INPUT = Path(__file__).with_name("day05.txt")


def seat_id(boarding_pass: str) -> int:
    """First 7 characters pick the row, last 3 the column; F/L are 0 and B/R are 1."""
    bits = (
        boarding_pass.replace("F", "0")
        .replace("B", "1")
        .replace("L", "0")
        .replace("R", "1")
    )
    return int(bits, 2)


def seat_ids() -> list[int]:
    return [
        seat_id(line.strip()) for line in INPUT.read_text().splitlines() if line.strip()
    ]


def part1() -> int:
    return max(seat_ids())


def part2() -> int:
    """The one seat whose neighbours on both sides are taken."""
    ids = set(seat_ids())
    for seat in ids:
        if seat + 1 not in ids and seat + 2 in ids:
            return seat + 1
    raise ValueError("no missing seat found")


def solve() -> str:
    return f"Part 1: {part1()}\nPart 2: {part2()}"


main = aoc_day("day05")(solve)
