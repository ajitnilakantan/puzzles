"""Advent of Code 2020, day 16: Ticket Translation."""

from pathlib import Path

from registry import aoc_day

INPUT = Path(__file__).with_name("day16.txt")


def parse() -> tuple[dict[str, list[tuple[int, int]]], list[int], list[list[int]]]:
    rules_section, yours_section, nearby_section = INPUT.read_text().split("\n\n")

    rules: dict[str, list[tuple[int, int]]] = {}
    for line in rules_section.splitlines():
        if not line.strip():
            continue
        name, _, rest = line.partition(":")
        ranges = []
        for part in rest.split(" or "):
            low, _, high = part.strip().partition("-")
            ranges.append((int(low), int(high)))
        rules[name.strip()] = ranges

    your = [int(v) for v in yours_section.splitlines()[1].split(",")]
    nearby = [
        [int(v) for v in line.split(",")]
        for line in nearby_section.splitlines()[1:]
        if line.strip()
    ]
    return rules, your, nearby


def accepts(ranges: list[tuple[int, int]], value: int) -> bool:
    return any(low <= value <= high for low, high in ranges)


def part1() -> int:
    """Sum of every nearby-ticket value no rule accepts."""
    rules, _, nearby = parse()
    return sum(
        value
        for ticket in nearby
        for value in ticket
        if not any(accepts(ranges, value) for ranges in rules.values())
    )


def part2() -> int:
    """Product of the 'departure' fields on your ticket."""
    rules, yours, nearby = parse()
    valid = [
        ticket
        for ticket in nearby
        if all(
            any(accepts(ranges, value) for ranges in rules.values()) for value in ticket
        )
    ]

    # For each position, the field names that could legally sit there.
    candidates = []
    for position in range(len(yours)):
        candidates.append(
            {
                name
                for name, ranges in rules.items()
                if all(accepts(ranges, ticket[position]) for ticket in valid)
            }
        )

    # Eliminate: once a field has one position, remove it from the others.
    assigned: dict[str, int] = {}
    while len(assigned) < len(candidates):
        for position, possible in enumerate(candidates):
            remaining = possible - assigned.keys()
            if len(remaining) == 1:
                assigned[remaining.pop()] = position

    product = 1
    for name, position in assigned.items():
        if name.startswith("departure"):
            product *= yours[position]
    return product


def solve() -> str:
    return f"Part 1: {part1()}\nPart 2: {part2()}"


main = aoc_day("day16")(solve)
