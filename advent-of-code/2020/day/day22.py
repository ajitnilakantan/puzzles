"""Advent of Code 2020, day 22: Crab Combat."""

from collections import deque
from pathlib import Path

from registry import aoc_day

INPUT = Path(__file__).with_name("day22.txt")


def parse() -> tuple[deque[int], deque[int]]:
    sections = [block for block in INPUT.read_text().split("\n\n") if block.strip()]
    decks = []
    for block in sections:
        cards = [int(line) for line in block.splitlines()[1:] if line.strip()]
        decks.append(deque(cards))
    return decks[0], decks[1]


def score(deck: deque[int]) -> int:
    """Highest card counts once, the next twice, and so on from the bottom."""
    return sum(
        card * multiplier for multiplier, card in enumerate(reversed(deck), start=1)
    )


def play_combat(first: deque[int], second: deque[int]) -> deque[int]:
    while first and second:
        a, b = first.popleft(), second.popleft()
        winner = first if a > b else second
        winner.extend(sorted((a, b), reverse=True))
    return first or second


def play_recursive(first: deque[int], second: deque[int], depth: int = 0) -> int:
    """Play a game of Recursive Combat; return 1 if player one wins, else 2."""
    seen: set[tuple[tuple[int, ...], tuple[int, ...]]] = set()
    while first and second:
        state = (tuple(first), tuple(second))
        if state in seen:
            return 1  # Repeated position: player one wins immediately.
        seen.add(state)

        a, b = first.popleft(), second.popleft()
        if len(first) >= a and len(second) >= b:
            winner = play_recursive(
                deque(list(first)[:a]), deque(list(second)[:b]), depth + 1
            )
        else:
            winner = 1 if a > b else 2

        if winner == 1:
            first.extend((a, b))
        else:
            second.extend((b, a))
    return 1 if first else 2


def part1() -> int:
    first, second = parse()
    return score(play_combat(first, second))


def part2() -> int:
    first, second = parse()
    play_recursive(first, second)
    return score(first or second)


def solve() -> str:
    return f"Part 1: {part1()}\nPart 2: {part2()}"


main = aoc_day("day22")(solve)
