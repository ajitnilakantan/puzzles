"""Shared registry of Advent of Code solutions.

Each solution module under ``day/`` registers itself with :func:`aoc_day`, so the
CLI in ``main.py`` only needs to import the modules and then look days up by name.
"""

from collections.abc import Callable

AOC_REGISTRY: dict[str, Callable[[], str]] = {}


def aoc_day(name: str) -> Callable[[Callable[[], str]], Callable[[], str]]:
    """Decorator to automatically register an Advent of Code day."""

    def decorator(func: Callable[[], str]) -> Callable[[], str]:
        AOC_REGISTRY[name] = func
        return func

    return decorator
