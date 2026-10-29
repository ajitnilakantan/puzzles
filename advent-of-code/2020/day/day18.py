"""Advent of Code 2020, day 18: Operation Order."""

from pathlib import Path

from registry import aoc_day

INPUT = Path(__file__).with_name("day18.txt")


def tokenize(expression: str) -> list[str]:
    return expression.replace("(", "( ").replace(")", " )").split()


class Parser:
    """Recursive-descent evaluator; ``plus_first`` selects the precedence rules."""

    def __init__(self, tokens: list[str], plus_first: bool) -> None:
        self.tokens = tokens
        self.pos = 0
        self.plus_first = plus_first

    def peek(self) -> str | None:
        return self.tokens[self.pos] if self.pos < len(self.tokens) else None

    def take(self) -> str:
        token = self.tokens[self.pos]
        self.pos += 1
        return token

    def atom(self) -> int:
        token = self.take()
        if token == "(":
            value = self.expression()
            self.take()  # closing paren
            return value
        return int(token)

    def additive(self) -> int:
        """A run of additions, binding tighter than multiplication."""
        value = self.atom()
        while self.peek() == "+":
            self.take()
            value += self.atom()
        return value

    def expression(self) -> int:
        if not self.plus_first:
            # Part 1: + and * have equal precedence, evaluated left to right.
            value = self.atom()
            while self.peek() in ("+", "*"):
                operator = self.take()
                rhs = self.atom()
                value = value + rhs if operator == "+" else value * rhs
            return value

        # Part 2: + binds tighter than *.
        value = self.additive()
        while self.peek() == "*":
            self.take()
            value *= self.additive()
        return value


def evaluate(expression: str, plus_first: bool) -> int:
    return Parser(tokenize(expression), plus_first).expression()


def lines() -> list[str]:
    return [line for line in INPUT.read_text().splitlines() if line.strip()]


def part1() -> int:
    return sum(evaluate(line, plus_first=False) for line in lines())


def part2() -> int:
    return sum(evaluate(line, plus_first=True) for line in lines())


def solve() -> str:
    return f"Part 1: {part1()}\nPart 2: {part2()}"


main = aoc_day("day18")(solve)
