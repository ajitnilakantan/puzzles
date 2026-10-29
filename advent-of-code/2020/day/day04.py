"""Advent of Code 2020, day 04: Passport Processing."""

from pathlib import Path

from registry import aoc_day

INPUT = Path(__file__).with_name("day04.txt")

REQUIRED = {"byr", "iyr", "eyr", "hgt", "hcl", "ecl", "pid"}
EYE_COLORS = {"amb", "blu", "brn", "gry", "grn", "hzl", "oth"}


def parse() -> list[dict[str, str]]:
    """Blank-line separated passports, one 'key:value' pair per whitespace token."""
    passports = []
    for block in INPUT.read_text().split("\n\n"):
        fields = {}
        for token in block.split():
            key, _, value = token.partition(":")
            if key:
                fields[key] = value
        if fields:
            passports.append(fields)
    return passports


def has_required_fields(passport: dict[str, str]) -> bool:
    return REQUIRED <= passport.keys()


def is_valid(passport: dict[str, str]) -> bool:
    if not has_required_fields(passport):
        return False
    if not 1920 <= int(passport["byr"]) <= 2002:
        return False
    if not 2010 <= int(passport["iyr"]) <= 2020:
        return False
    if not 2020 <= int(passport["eyr"]) <= 2030:
        return False

    height = passport["hgt"]
    unit, amount = height[-2:], height[:-2]
    if unit not in ("in", "cm") or not amount.isdigit():
        return False
    if unit == "in" and not 59 <= int(amount) <= 76:
        return False
    if unit == "cm" and not 150 <= int(amount) <= 193:
        return False

    hair = passport["hcl"]
    if (
        len(hair) != 7
        or hair[0] != "#"
        or any(c not in "0123456789abcdef" for c in hair[1:])
    ):
        return False
    if passport["ecl"] not in EYE_COLORS:
        return False

    pid = passport["pid"]
    if len(pid) != 9 or not pid.isdigit():
        return False
    return True


def part1() -> int:
    return sum(1 for passport in parse() if has_required_fields(passport))


def part2() -> int:
    return sum(1 for passport in parse() if is_valid(passport))


def solve() -> str:
    return f"Part 1: {part1()}\nPart 2: {part2()}"


main = aoc_day("day04")(solve)
