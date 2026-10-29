## 2020 Advent of Code in Python

Each day lives in `day/dayNN.py` and registers itself with the `@aoc_day`
decorator from `registry.py`. `main.py` imports every module in `day/`, so it
can run one day or all of them.

## Running
- Install uv

- Initialization
```
# uv python install --reinstall # To reinstall uv-managed python
uv init # --bare no src directory

uv add scipy
uv add numpy
uv add --dev mypy
uv tool install ruff
uv sync
```

- Running
```
uv run ruff check day/dayNN.py
uv run ruff format day/dayNN.py
uv run python -mmypy day/dayNN.py
uv run python -mmypy main.py registry.py day
```

- Running through the CLI
```
uv run python main.py day01      # a single day
uv run python main.py all        # every day, in order
uv run python main.py day31      # not implemented -> lists available days
```

## Layout
```
2020/
├── day/
│   ├── __init__.py
│   ├── day01.py       # solution for day 1
│   ├── day01.txt      # its puzzle input
│   ├── day09_sample.txt   # extra inputs are suffixed _sample/_sample_b/...
│   └── ...
├── registry.py        # @aoc_day decorator and the shared registry
└── main.py            # CLI entry point
```

## Notes

### Day01

Sort the list and walk inwards with two pointers to find the pair. Part 2 is a
brute-force triple loop (only 200 numbers).

### Day02

Part 1 counts character occurrences and checks the range. Part 2 is an XOR of
the two position checks.

### Day03

The column is `(row // down) * right % width`, so the slope repeats without
materialising a widened grid. Part 2 multiplies the tree counts of all slopes.

### Day04

Split into blank-line separated passports. Part 1 is a subset test on required
keys; part 2 validates each field against its documented range/format.

### Day05

Replace `F`/`B`/`L`/`R` with binary digits and parse the whole string as base 2:
the seat id falls out directly. Part 2 finds the id whose two neighbours are taken.

### Day06

Parse groups as lists of member answers. Part 1 is the size of the union, part 2
the size of the intersection.

### Day07

Part 1 grows a set of bag colours that can hold a shiny gold bag to a fixed
point. Part 2 recurses, multiplying `Counter`s by the number of bags at each level.

### Day08

A small interpreter over `(op, value)` with a `seen` set detects the loop. Part 2
re-runs the program once per `jmp`/`nop` until one terminates.

### Day09

Part 1 checks each number against the previous 25. Part 2 grows a window from
each start until the sum meets the target, then adds its min and max.

### Day10

Add 0 and `max + 3`, sort, and count the 1 and 3 jolt gaps. Part 2 is a simple DP
over "number of ways to reach each adapter".

### Day11

One cellular automaton for both parts. Part 1 uses adjacent seats and a
tolerance of 4; part 2 walks each direction until the first seat and uses 5.

### Day12

Part 1 rotates a heading index through the four compass directions. Part 2 keeps
a waypoint that steps move and turns rotate around the ship.

### Day13

Part 1 is a modulo sweep for the earliest bus. Part 2 is the Chinese remainder
theorem over the schedules, using `pow(x, -1, m)` for the modular inverse.

### Day14

Part 1 applies the mask with clear/set bits. Part 2 expands the floating `X` bits
over the product of all combinations.

### Day15

Keep a map of the last turn each number was spoken; a new number scores a 0.
Part 2 just runs the same loop to 30,000,000.

### Day16

Part 1 sums nearby values that no rule accepts. Part 2 intersects the set of
possible field names per position over all valid tickets, then eliminates to a
single assignment.

### Day17

A set of active cubes and a neighbour counter, parameterised by dimension, so
part 1 (3-D) and part 2 (4-D) share the same code.

### Day18

Recursive-descent evaluator with a `plus_first` flag: part 1 treats `+` and `*`
as equal precedence left to right, part 2 gives `+` the tighter binding.

### Day19

Memoised span matching: `ends(rule, position)` returns every position where a
rule can finish. Part 2 uses the pre-modified input where rules 8 and 11 are
recursive, so no grammar expansion is needed.

### Day20

Find edge matches across tiles, orient a corner, then DFS the grid by matching
right-to-left and bottom-to-top edges. Strip the borders, stitch the image, and
scan all 8 orientations for sea monsters. `#` count minus 15 per monster.

### Day21

For each allergen, intersect the ingredients of every food listing it. Then
assign greedily by fewest remaining options. Part 1 counts occurrences of the
ingredients left with no allergen.

### Day22

Plain deque Combat for part 1. Part 2 recurses on sub-decks and keeps a set of
seen `(deck1, deck2)` positions — a repeat means player one wins.

### Day23

One circular linked list, stored as a successor array, shared by both parts.
Part 2 extends to 1,000,000 cups and runs 10,000,000 rounds (~19 sec, the
slowest day of the set).

### Day24

Hex tiles in cube coordinates. Part 1 flips the landing tile of each line. Part 2
runs the cellular automaton for 100 days, counting neighbours with a `Counter`.

### Day25

Brute force the loop size by repeated multiplication, then `pow(other_key,
loop_size, 20201227)`. There is no part 2.
