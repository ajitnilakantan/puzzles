"""Advent of Code 2020, day 13: Shuttle Search."""

from functools import reduce
from pathlib import Path

from registry import aoc_day

INPUT = Path(__file__).with_name("day13.txt")


def parse() -> tuple[int, list[str]]:
    lines = INPUT.read_text().splitlines()
    return int(lines[0].strip()), [token.strip() for token in lines[1].split(",")]


def part1() -> int:
    """Earliest bus after the departure time, times the wait."""
    depart, schedule = parse()
    best_wait, best_bus = min(
        ((bus - depart % bus) % bus, bus)
        for bus in (int(t) for t in schedule if t != "x")
    )
    return best_wait * best_bus


def part2() -> int:
    """Earliest timestamp where bus i departs i minutes after the first bus.

    Solved with the Chinese remainder theorem over pairwise-coprime schedules.
    """
    _, schedule = parse()
    buses = [(int(bus), offset) for offset, bus in enumerate(schedule) if bus != "x"]

    # x = -offset (mod bus) for every bus
    residues = [(-offset) % bus for bus, offset in buses]
    moduli = [bus for bus, _ in buses]

    total = reduce(lambda a, b: a * b, moduli, 1)
    result = 0
    for residue, modulus in zip(residues, moduli):
        partial = total // modulus
        result += residue * pow(partial, -1, modulus) * partial
    return result % total


def solve() -> str:
    return f"Part 1: {part1()}\nPart 2: {part2()}"


main = aoc_day("day13")(solve)
