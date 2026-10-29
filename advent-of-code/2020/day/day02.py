"""Advent of Code 2020, day 02: Password Philosophy."""

from pathlib import Path

from registry import aoc_day

INPUT = Path(__file__).with_name("day02.txt")


def parse() -> list[tuple[int, int, str, str]]:
    """Each line is '<min>-<max> <char>: <password>'."""
    entries = []
    for line in INPUT.read_text().splitlines():
        line = line.strip()
        if not line:
            continue
        counts, char, password = line.split(" ")
        low, high = counts.split("-")
        entries.append((int(low), int(high), char[0], password))
    return entries


def part1() -> int:
    """Count passwords where the character occurs within the allowed range."""
    return sum(
        1
        for low, high, char, password in parse()
        if low <= password.count(char) <= high
    )


def part2() -> int:
    """Count passwords where exactly one of the two positions holds the character."""
    valid = 0
    for low, high, char, password in parse():
        first = password[low - 1] == char
        second = password[high - 1] == char
        if first != second:
            valid += 1
    return valid


def solve() -> str:
    return f"Part 1: {part1()}\nPart 2: {part2()}"


main = aoc_day("day02")(solve)
