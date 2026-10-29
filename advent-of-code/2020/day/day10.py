"""Advent of Code 2020, day 10: Adapter Array."""

from collections import Counter
from pathlib import Path

from registry import aoc_day

INPUT = Path(__file__).with_name("day10.txt")


def adapters() -> list[int]:
    """Joltages including the 0 outlet and the built-in adapter."""
    nums = [int(line) for line in INPUT.read_text().splitlines() if line.strip()]
    nums += [0, max(nums) + 3]
    return sorted(nums)


def part1() -> int:
    """Number of 1-jolt gaps times the number of 3-jolt gaps."""
    gaps = Counter(b - a for a, b in zip(adapters(), adapters()[1:]))
    return gaps[1] * gaps[3]


def part2() -> int:
    """Number of distinct ways to chain the adapters together."""
    nums = adapters()
    paths = [0] * len(nums)
    paths[0] = 1
    for i in range(1, len(nums)):
        for j in range(i):
            if nums[i] - nums[j] <= 3:
                paths[i] += paths[j]
    return paths[-1]


def solve() -> str:
    return f"Part 1: {part1()}\nPart 2: {part2()}"


main = aoc_day("day10")(solve)
