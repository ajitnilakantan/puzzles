"""Advent of Code 2020, day 25: Combo Breaker (there is no part 2)."""

from pathlib import Path

from registry import aoc_day

INPUT = Path(__file__).with_name("day25.txt")

SUBJECT_NUMBER = 7
MODULUS = 20201227


def public_keys() -> tuple[int, int]:
    card, door = (int(line) for line in INPUT.read_text().split() if line.strip())
    return card, door


def loop_size(public_key: int) -> int:
    value = 1
    size = 0
    while value != public_key:
        value = value * SUBJECT_NUMBER % MODULUS
        size += 1
    return size


def part1() -> int:
    """Encryption key: either public key raised to the other's loop size."""
    card, door = public_keys()
    return pow(door, loop_size(card), MODULUS)


def solve() -> str:
    return f"Part 1: {part1()}"


main = aoc_day("day25")(solve)
