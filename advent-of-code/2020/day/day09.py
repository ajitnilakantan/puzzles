"""Advent of Code 2020, day 09: Encoding Error."""

from pathlib import Path

from registry import aoc_day

INPUT = Path(__file__).with_name("day09.txt")

PREAMBLE = 25


def numbers() -> list[int]:
    return [int(line) for line in INPUT.read_text().splitlines() if line.strip()]


def is_valid(window: list[int], value: int) -> bool:
    seen = set()
    for candidate in window:
        if value - candidate in seen and value - candidate != candidate:
            return True
        seen.add(candidate)
    return False


def find_invalid(nums: list[int], preamble: int = PREAMBLE) -> int:
    for index in range(preamble, len(nums)):
        if not is_valid(nums[index - preamble : index], nums[index]):
            return nums[index]
    raise ValueError("no invalid number found")


def part1() -> int:
    return find_invalid(numbers())


def part2() -> int:
    """The smallest + largest value in the contiguous run summing to the part 1 answer."""
    nums = numbers()
    target = find_invalid(nums)
    for start in range(len(nums)):
        total = nums[start]
        for end in range(start + 1, len(nums)):
            total += nums[end]
            if total == target:
                run = nums[start : end + 1]
                return min(run) + max(run)
            if total > target:
                break
    raise ValueError("no contiguous run found")


def solve() -> str:
    return f"Part 1: {part1()}\nPart 2: {part2()}"


main = aoc_day("day09")(solve)
