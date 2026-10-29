"""Advent of Code 2020, day 15: Rambunctious Recitation."""

from pathlib import Path

from registry import aoc_day

INPUT = Path(__file__).with_name("day15.txt")


def starting_numbers() -> list[int]:
    return [int(token) for token in INPUT.read_text().strip().split(",")]


def nth_spoken(target: int) -> int:
    """The number spoken on ``target`` (1-indexed) turn."""
    numbers = starting_numbers()
    last_seen = {value: turn for turn, value in enumerate(numbers, start=1)}
    last = numbers[-1]

    for turn in range(len(numbers) + 1, target + 1):
        previous = last_seen.get(last)
        last_seen[last] = turn - 1
        last = 0 if previous is None else turn - 1 - previous
    return last


def part1() -> int:
    return nth_spoken(2020)


def part2() -> int:
    return nth_spoken(30_000_000)


def solve() -> str:
    return f"Part 1: {part1()}\nPart 2: {part2()}"


main = aoc_day("day15")(solve)
