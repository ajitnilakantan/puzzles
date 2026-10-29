"""Advent of Code 2020 runner.

Usage:
    python main.py day01    # run a single day
    python main.py all      # run every registered day
"""

import argparse
import importlib
import pkgutil

import day as day_package
from registry import AOC_REGISTRY


def discover_days() -> None:
    """Import every module under ``day/`` so its ``@aoc_day`` decorator runs."""
    for _, module_name, _ in pkgutil.iter_modules(day_package.__path__):
        importlib.import_module(f"day.{module_name}")


def main() -> None:
    discover_days()

    parser = argparse.ArgumentParser(description="Advent of Code Runner CLI")
    parser.add_argument(
        "day",
        type=str,
        help="Specify the day to run (e.g., 'day01') or 'all' to run all registered days.",
    )
    args = parser.parse_args()
    target = args.day.lower()

    if target == "all":
        print("=== Running All Solved Advent of Code Days ===")
        for name in sorted(AOC_REGISTRY):
            print(f"[{name}] -> {AOC_REGISTRY[name]()}")
    elif target in AOC_REGISTRY:
        print(f"=== Running {target} ===")
        print(f"Result: {AOC_REGISTRY[target]()}")
    else:
        print(f"Error: '{args.day}' has not been implemented or registered yet.")
        print(f"Available options: 'all', {', '.join(sorted(AOC_REGISTRY))}")


if __name__ == "__main__":
    main()
