"""Advent of Code 2020, day 19: Monster Messages."""

from pathlib import Path

from registry import aoc_day

INPUT = Path(__file__).with_name("day19.txt")
INPUT_PART2 = Path(__file__).with_name("day19_modified.txt")


def parse(path: Path) -> tuple[dict[int, list[list[str]]], list[str]]:
    rules_section, messages_section = path.read_text().split("\n\n")
    rules: dict[int, list[list[str]]] = {}
    for line in rules_section.splitlines():
        if not line.strip():
            continue
        number, _, body = line.partition(":")
        options = []
        for option in body.split("|"):
            options.append([token.strip('"') for token in option.split()])
        rules[int(number)] = options
    messages = [line.strip() for line in messages_section.splitlines() if line.strip()]
    return rules, messages


class Matcher:
    """Matches rules against a message, remembering where each rule can end."""

    def __init__(self, rules: dict[int, list[list[str]]], text: str) -> None:
        self.rules = rules
        self.text = text
        self.cache: dict[tuple[int, int], frozenset[int]] = {}

    def ends(self, number: int, start: int) -> frozenset[int]:
        """Every position ``start``..end such that rule ``number`` matches text[start:end]."""
        key = (number, start)
        if key in self.cache:
            return self.cache[key]

        results: set[int] = set()
        for option in self.rules[number]:
            positions = {start}
            for token in option:
                next_positions: set[int] = set()
                for position in positions:
                    if token.isdigit():
                        next_positions |= self.ends(int(token), position)
                    elif position < len(self.text) and self.text[position] == token:
                        next_positions.add(position + 1)
                positions = next_positions
                if not positions:
                    break
            results |= positions

        self.cache[key] = frozenset(results)
        return self.cache[key]

    def matches(self, message: str, number: int = 0) -> bool:
        self.text = message
        self.cache.clear()
        return len(message) in self.ends(number, 0)


def count_valid(rules: dict[int, list[list[str]]], messages: list[str]) -> int:
    matcher = Matcher(rules, "")
    return sum(1 for message in messages if matcher.matches(message))


def part1() -> int:
    rules, messages = parse(INPUT)
    return count_valid(rules, messages)


def part2() -> int:
    rules, messages = parse(INPUT_PART2)
    return count_valid(rules, messages)


def solve() -> str:
    return f"Part 1: {part1()}\nPart 2: {part2()}"


main = aoc_day("day19")(solve)
