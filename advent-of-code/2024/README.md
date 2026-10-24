# Advent of Code 2024 in TypeScript

## Prerequisites

- Bun

```sh
$ bun --version
1.4.3
```

## Run

```sh
bun install
bun run src/main.ts --help  # List options
bun run src/main.ts         # list available "plugins"
bun run src/main.ts day01   # Run dayNN
bun run src/main.ts all     # Run all days
```

<!--
## Dev setup
```sh
bun init -y
bun create @eslint/config@latest
bun add --dev prettier eslint-config-prettier
bun add --dev @trivago/prettier-plugin-sort-imports
bun add lodash
bun add --dev @types/lodash
bun add z3-solver
```
-->

## Tests/Devel

Bun includes a test runner, so no test dependency is required:

```sh
bun test                    # Run all tests
bun test day04              # Run all tests for file pattern *day04*
bun format                  # Format code
bun lint                    # Run lint checks
bun lint:fix                # Lint and try to fix issues
bun check                   # Syntax check with bun
bun syntax                  # Syntax check with tsc
```

## Notes

### Day05

Use a partially ordered set (poset). Note: the constraints given in the first section of the puzzle are inconsistent.
You end up with circular loops in the graph. To work around this, build a poset of the subset of page numbers given in each line.

### Day06

Store the positions of "#" in sorted lists for each row/column for binary lookups instead of linear searches.
Part 2 is too slow (~2min) so instead pre-compute the next obstacle for each cell in the grid - per row and per-column. This gets the time to ~1sec

### Day07

Recursively build solutions. Part 2 is a bit slow (~7sec)

### Day08

Pretty straingforward placing antinodes.

### Day09

Create list of blocks and freenodes. Moving backwards for the blocks and forward for the freenodes, fill up space.
For part2 sort the freenodes by space/position and store in Map<blockSize, Heap_of_freespace> to find the first block with a required space; as
before traveres the blocks backwards. Keep track of failures to place block of size "n", to avoid processing blocks of size >=n.

### Day10

Straightforward path counting.

### Day11

Order of the stones don't matter. Just keep a map of stone number and count.

### Day12

BFS to find regions. Scan to find boundaries in 4 directions + area for each region. For part 2 have to distinguish "inside" and "outside" edges when counting.

### Day13

Straightforward, use Cramer's rule to solve 2 linear equations of 2 variables.

### Day14

Part2 much harder. Not sure what the xmas tree is supposed to look like. Wasted a lot of time because I thought all the robots would arrange in am xmas tree pattern. Failed methods used Hough transforms, linear modulo/chinese remainder calcs, checking for trianglular distributions.
Actual solution is more manual - process the robots 1 sec at a time and see if the upper left and right triangles are approximately empty. For each _manually_ double check the results.
I guess in restrospect, you can automate by finding the rectangular frame around the tree.
Get solutions at 6493, 16896

### Day15

Part2: Left,Right similar to Part1, but Up and Down can affect multiple disjoint regions. Also walls can be missed. E.g.

```
.....#.
...#...
[]...[]
[]..[].
.[][]..
..[]...
...@...  ^ Moving up
```

### Day16

Simple graph search. For part2, extend dijkstra to return all shortest paths.

### Day17

Use a z3 optmizer to solve the constraints. Manually decode one iteration of the loop and run it for the length of the program.
The are probably much more elegant ways to do this.

### Day18

Simple binary search for first failure

### Day19

Recursively check if a word can be composed of the "prefixes".

### Day20

Part1: Find the shortest path. Then look at each of the # walls -- if on either side there is a path-point, we can remove it for a short-cut.
We don't have to recalculate the new shortest path -- just remove the count of nodes that was removed from the short-cut.\
Part2: For each node on the shortest path find another node, ahead, within "n" manhattan distance and see if time is improve. If n=2, should match part1.
Part2 is a bit slow (~3sec). Can probably speed up using a kdtree.

### Day21

We have to do a lookahead and not just greedily choose any or shortest path immediately.
E.g. If we go ^>> instead of >>^ on Control1, the number of moves on Control2 can differ.
In part1, I "flattened" the possible sequences, but this is not feasible in part2.\
Instead recursively find shortest sequences at each depth. Since a subsequence like "^^>A" puts all
buttons back at "A", the calculations repeat. Cache the values at each depth.
This is _much_ less code than the iterative approach I used in part1.

### Day22

`16_777_216 = 0x100_0000` so `x % 16777216 == x & 0xFF_FFFF`
For part2, just brute forced it, and get solution in ~5sec. I feel there must be a cleverer way to solve it using GF(2) to simplify the expression.

### Day23

Part1: just need to handle 3-cliques (triangles). Can use the "Neighborhood Intersection Algorithm" for this.
Part2: need to find largest clique (Bron–Kerbosch Algorithm with Pivoting)

### Day24

Part2: Start as the least significant bit z00, and work up to see the first incorrect bit. Backtrace each output "z" to see which gates drive it. Since this is an addition circuit, I assume that for zN, gates that drive zN-1, zN-2,...,z0 should not be toggled, otherwise they will affect the previous bits. This reduces the search space. Find all possible pairs that give the correct result for zN (as well as all previous bits z00..zN-1), and recursively move on to the next highest bit that is incorrect.

## Typescript annoyances

- No built in Result/Option types. Have to have useless Type|undefined all over the place
- Discriminated unions are weak. Call members need to share a field. Cannot have unrelated objects
- No pattern matching. Switch is still C-like. Exhausiveness checking is a bit kludgy
- Debug printing is weird.
  - No easy way to add `__repr__` with annotations.
  - Nested arrays print out flat.
  - Cannot print out type of an object. Everything is elided.
  - Printing any Set misleadingly outputs "{}".\
  - Workaround: Added utility ``debug.log`...${var}...` ``
- Cannot redeclare variables. Can only shadow in a block. Oddly, the repl lets you redeclare.
- for in/of confusion. Often either can be used without a syntax error
- No range
- Type system is powerful, but seems a bit useless in practice. Rust's & F#'s idea of making illegal states unrepresentable is better.
- Miss currying/pipes from F#. Can simulate currying using double closures.
- Maps/Sets are limited. Basically only numbers and strings can be used as strings. No structural equality like F#
- `const a=[0, 1, 2, 3]; a[3/2];` --> silent failure, returns `undefined`. Worse, `a[3/2] = 5` succeeds and gives: `[ 0, 1, 2, 3, "1.5": 5 ]`
  - I guess arrays don't really exist and are syntactic sugar around objects. E.g. see: `Object.getOwnPropertyNames(a)`

## Typescript good points

- There is surprisingly little cruft/boilerplate in TS code. Get used to its idiosyncrasies.
- Reasonable functional programming. Allows top level functions + also classes.
- Very quick edit/run experience. `bun repl` is useful.
- Bun is a nice environment with built in package management, testing, hooks for linting, formatting.
