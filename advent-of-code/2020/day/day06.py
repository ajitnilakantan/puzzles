"""Advent of Code 2020, day 06: Custom Customs."""

from pathlib import Path

from registry import aoc_day

INPUT = Path(__file__).with_name("day06.txt")


def groups() -> list[list[str]]:
    """Each group is the list of its members' answered questions."""
    return [block.split() for block in INPUT.read_text().split("\n\n") if block.strip()]


def part1() -> int:
    """Questions answered by anyone in the group."""
    return sum(len(set().union(*map(set, group))) for group in groups())


def part2() -> int:
    """Questions answered by everyone in the group."""
    return sum(len(set.intersection(*map(set, group))) for group in groups())


def solve() -> str:
    return f"Part 1: {part1()}\nPart 2: {part2()}"


main = aoc_day("day06")(solve)
