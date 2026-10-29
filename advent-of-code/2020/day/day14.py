"""Advent of Code 2020, day 14: Docking Data."""

from itertools import product
from pathlib import Path

from registry import aoc_day

INPUT = Path(__file__).with_name("day14.txt")

MASK_BITS = 36


def parse() -> list[tuple[str, int, int, str]]:
    """Each instruction is ('mask', 0, 0, mask) or ('mem', address, value, '')."""
    program = []
    for line in INPUT.read_text().splitlines():
        if not line.strip():
            continue
        name, _, raw = line.partition(" = ")
        if name == "mask":
            program.append(("mask", 0, 0, raw.strip()))
        else:
            program.append(("mem", int(name[4:-1]), int(raw), ""))
    return program  # type: ignore[return-value]


def apply_mask(value: int, mask: str) -> int:
    """Part 1: 0 bits clear, 1 bits set, X bits leave the value alone."""
    for i, bit in enumerate(reversed(mask)):
        if bit == "0":
            value &= ~(1 << i)
        elif bit == "1":
            value |= 1 << i
    return value


def addresses(address: int, mask: str) -> list[int]:
    """Part 2: 1 bits set, X bits float to every combination."""
    base = address
    floating = []
    for i, bit in enumerate(reversed(mask)):
        if bit == "1":
            base |= 1 << i
        elif bit == "X":
            floating.append(i)

    result = []
    for bits in product((0, 1), repeat=len(floating)):
        addr = base
        for offset, set_bit in zip(floating, bits):
            if set_bit:
                addr |= 1 << offset
            else:
                addr &= ~(1 << offset)
        result.append(addr)
    return result


def part1() -> int:
    memory: dict[int, int] = {}
    mask = "X" * MASK_BITS
    for op, address, value, new_mask in parse():
        if op == "mask":
            mask = new_mask
        else:
            memory[address] = apply_mask(value, mask)
    return sum(memory.values())


def part2() -> int:
    memory: dict[int, int] = {}
    mask = "X" * MASK_BITS
    for op, address, value, new_mask in parse():
        if op == "mask":
            mask = new_mask
        else:
            for addr in addresses(address, mask):
                memory[addr] = value
    return sum(memory.values())


def solve() -> str:
    return f"Part 1: {part1()}\nPart 2: {part2()}"


main = aoc_day("day14")(solve)
