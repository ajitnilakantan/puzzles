"""Advent of Code 2020, day 07: Handy Haversacks."""

from collections import Counter
from pathlib import Path

from registry import aoc_day

INPUT = Path(__file__).with_name("day07.txt")

TARGET = "shiny gold"


def parse() -> dict[str, list[tuple[int, str]]]:
    """Map each bag colour to the (count, colour) bags it directly contains."""
    rules: dict[str, list[tuple[int, str]]] = {}
    for line in INPUT.read_text().splitlines():
        line = line.strip().rstrip(".").replace(",", "")
        if not line:
            continue
        container, _, contents = line.partition(" contain ")
        colour = " ".join(container.split()[:2])
        if contents.startswith("no other"):
            rules[colour] = []
            continue
        items = contents.split()
        rules[colour] = [
            (int(items[i]), f"{items[i + 1]} {items[i + 2]}")
            for i in range(0, len(items), 4)
        ]
    return rules


def part1() -> int:
    """How many bag colours can eventually hold at least one shiny gold bag."""
    rules = parse()
    holders: set[str] = set()
    while True:
        grown = {
            colour
            for colour, contents in rules.items()
            if any(inner in holders or inner == TARGET for _, inner in contents)
        }
        if grown <= holders:
            return len(holders)
        holders |= grown


def count_inside(rules: dict[str, list[tuple[int, str]]], colour: str) -> Counter:
    """Total number of bags inside ``colour``, keyed by colour."""
    total: Counter = Counter()
    for quantity, inner in rules[colour]:
        total[inner] += quantity
        for descendant, count in count_inside(rules, inner).items():
            total[descendant] += count * quantity
    return total


def part2() -> int:
    """How many individual bags a shiny gold bag must contain."""
    return sum(count_inside(parse(), TARGET).values())


def solve() -> str:
    return f"Part 1: {part1()}\nPart 2: {part2()}"


main = aoc_day("day07")(solve)
