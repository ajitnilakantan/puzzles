"""Advent of Code 2020, day 23: Crab Cups."""

from pathlib import Path

from registry import aoc_day

INPUT = Path(__file__).with_name("day23.txt")

ROUNDS_PART1 = 100
ROUNDS_PART2 = 10_000_000
CUPS_PART2 = 1_000_000


def starting_cups() -> list[int]:
    return [int(char) for char in INPUT.read_text().strip()]


class Cups:
    """Circular linked list over cup labels, stored as a successor array."""

    def __init__(self, order: list[int]) -> None:
        self.next: dict[int, int] = {}
        for current, following in zip(order, order[1:] + order[:1]):
            self.next[current] = following
        self.current = order[0]
        self.highest = max(order)

    def move(self) -> None:
        current = self.current
        picked = [self.next[current]]
        picked.append(self.next[picked[0]])
        picked.append(self.next[picked[1]])

        # Splice the three cups out of the circle.
        self.next[current] = self.next[picked[2]]

        destination = current - 1
        while destination in picked or destination < 1:
            destination = destination - 1 if destination > 1 else self.highest

        self.next[picked[2]] = self.next[destination]
        self.next[destination] = picked[0]
        self.current = self.next[current]

    def play(self, rounds: int) -> None:
        for _ in range(rounds):
            self.move()

    def after_one(self) -> list[int]:
        labels = []
        cup = self.next[1]
        while cup != 1:
            labels.append(cup)
            cup = self.next[cup]
        return labels


def part1() -> str:
    cups = Cups(starting_cups())
    cups.play(ROUNDS_PART1)
    return "".join(str(label) for label in cups.after_one())


def part2() -> int:
    order = starting_cups()
    order += list(range(len(order) + 1, CUPS_PART2 + 1))
    cups = Cups(order)
    cups.play(ROUNDS_PART2)
    first = cups.next[1]
    second = cups.next[first]
    return first * second


def solve() -> str:
    return f"Part 1: {part1()}\nPart 2: {part2()}"


main = aoc_day("day23")(solve)
