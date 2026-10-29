"""Advent of Code 2020, day 08: Handheld Halting."""

from pathlib import Path

from registry import aoc_day

INPUT = Path(__file__).with_name("day08.txt")


def parse() -> list[tuple[str, int]]:
    program = []
    for line in INPUT.read_text().splitlines():
        if line.strip():
            op, value = line.split()
            program.append((op, int(value)))
    return program


def run(program: list[tuple[str, int]], swap_at: int = -1) -> tuple[int, bool]:
    """Run the program, optionally flipping the n-th jmp/nop.

    Returns the accumulator when the program terminates, otherwise (-1, False)
    when it loops forever.
    """
    acc = 0
    pc = 0
    seen = set()
    swaps_left = swap_at
    while pc < len(program):
        if pc in seen:
            return -1, False
        seen.add(pc)
        op, value = program[pc]
        if swaps_left == 0 and op in ("jmp", "nop"):
            op = "jmp" if op == "nop" else "nop"
        if swaps_left >= 0 and op in ("jmp", "nop"):
            swaps_left -= 1
        if op == "acc":
            acc += value
            pc += 1
        elif op == "jmp":
            pc += value
        else:
            pc += 1
    return acc, True


def part1() -> int:
    acc = 0
    pc = 0
    seen = set()
    program = parse()
    while pc < len(program):
        if pc in seen:
            return acc
        seen.add(pc)
        op, value = program[pc]
        if op == "acc":
            acc += value
        elif op == "jmp":
            pc += value
            continue
        pc += 1
    return acc


def part2() -> int:
    program = parse()
    candidates = [i for i, (op, _) in enumerate(program) if op in ("jmp", "nop")]
    for index in candidates:
        acc, terminated = run(program, swap_at=index)
        if terminated:
            return acc
    raise ValueError("no single flip terminates the program")


def solve() -> str:
    return f"Part 1: {part1()}\nPart 2: {part2()}"


main = aoc_day("day08")(solve)
