"""Advent of Code 2020, day 21: Allergen Assessment."""

from pathlib import Path

from registry import aoc_day

INPUT = Path(__file__).with_name("day21.txt")


def parse() -> tuple[list[tuple[set[str], set[str]]], set[str]]:
    """Foods as (ingredients, allergens), plus every ingredient seen."""
    foods = []
    all_ingredients: set[str] = set()
    for line in INPUT.read_text().splitlines():
        if not line.strip():
            continue
        ingredients_text, _, allergens_text = line.partition("(contains ")
        ingredients = set(ingredients_text.replace(",", " ").split())
        allergens = set(allergens_text.rstrip(")").replace(",", " ").split())
        foods.append((ingredients, allergens))
        all_ingredients |= ingredients
    return foods, all_ingredients


def possible_pairs() -> dict[str, set[str]]:
    """Map each ingredient to the allergens it could be."""
    foods, all_ingredients = parse()
    possible: dict[str, set[str]] = {
        ingredient: set() for ingredient in all_ingredients
    }
    for ingredient in all_ingredients:
        for allergen in {a for _, allergens in foods for a in allergens}:
            if all(
                allergen not in allergens or ingredient in ingredients
                for ingredients, allergens in foods
            ):
                possible[ingredient].add(allergen)
    return possible


def resolve(possible: dict[str, set[str]]) -> dict[str, str]:
    """Assign allergens one at a time; each ingredient takes a unique allergen."""
    remaining = {
        ingredient: set(allergens)
        for ingredient, allergens in possible.items()
        if allergens
    }
    assigned: dict[str, str] = {}
    taken: set[str] = set()
    while remaining:
        # Prefer ingredients with the fewest still-free options.
        ingredient = min(remaining, key=lambda name: len(remaining[name] - taken))
        choice = min(remaining[ingredient] - taken)
        assigned[ingredient] = choice
        taken.add(choice)
        del remaining[ingredient]
    return assigned


def part1() -> int:
    """Occurrences of ingredients that contain no allergen."""
    foods, _ = parse()
    unsafe = resolve(possible_pairs())
    return sum(
        1
        for ingredients, _ in foods
        for ingredient in ingredients
        if ingredient not in unsafe
    )


def part2() -> str:
    """Unsafe ingredients ordered by their allergen, comma separated."""
    assigned = resolve(possible_pairs())
    return ",".join(sorted(assigned, key=lambda ingredient: assigned[ingredient]))


def solve() -> str:
    return f"Part 1: {part1()}\nPart 2: {part2()}"


main = aoc_day("day21")(solve)
