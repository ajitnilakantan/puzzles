"""Advent of Code 2020, day 20: Jurassic Jigsaw."""

from pathlib import Path

from registry import aoc_day

INPUT = Path(__file__).with_name("day20.txt")

TILE_SIZE = 10

# The sea monster, as (row, col) offsets from the monster's top-left corner.
MONSTER = [
    (0, 18),
    (1, 0),
    (1, 5),
    (1, 6),
    (1, 11),
    (1, 12),
    (1, 17),
    (1, 18),
    (1, 19),
    (2, 1),
    (2, 4),
    (2, 7),
    (2, 10),
    (2, 13),
    (2, 16),
]
MONSTER_WIDTH = 20
MONSTER_HEIGHT = 3


def parse() -> dict[int, list[str]]:
    tiles: dict[int, list[str]] = {}
    current: int | None = None
    rows: list[str] = []
    for line in INPUT.read_text().splitlines():
        if line.startswith("Tile "):
            current = int(line[5:-1])
            rows = []
        elif line.strip():
            rows.append(line.strip())
        elif current is not None and rows:
            tiles[current] = rows
            current, rows = None, []
    if current is not None and rows:
        tiles[current] = rows
    return tiles


def edges(tile: list[str]) -> tuple[str, str, str, str]:
    """(top, right, bottom, left), clockwise from the top."""
    return (
        tile[0],
        "".join(row[-1] for row in tile),
        tile[-1],
        "".join(row[0] for row in tile),
    )


def rotate(tile: list[str]) -> list[str]:
    return ["".join(row[col] for row in reversed(tile)) for col in range(len(tile[0]))]


def flip(tile: list[str]) -> list[str]:
    return [row[::-1] for row in tile]


def orientations(tile: list[str]) -> list[list[str]]:
    """The 8 rotations/reflections of a tile, de-duplicated."""
    seen = []
    for flipped in (tile, flip(tile)):
        current = flipped
        for _ in range(4):
            if current not in seen:
                seen.append(current)
            current = rotate(current)
    return seen


def canonical(edge: str) -> str:
    return min(edge, edge[::-1])


def border_edges(tiles: dict[int, list[str]]) -> set[str]:
    """Edges that only one tile can produce (so they must be on the outside)."""
    counts: dict[str, int] = {}
    for tile in tiles.values():
        for edge in edges(tile):
            key = canonical(edge)
            counts[key] = counts.get(key, 0) + 1
    return {edge for edge, count in counts.items() if count == 1}


def borders(tile: list[str], border: set[str]) -> int:
    return sum(1 for edge in edges(tile) if canonical(edge) in border)


def corner_ids(tiles: dict[int, list[str]]) -> list[int]:
    border = border_edges(tiles)
    return [tile_id for tile_id, tile in tiles.items() if borders(tile, border) == 2]


def part1() -> int:
    tiles = parse()
    product = 1
    for tile_id in corner_ids(tiles):
        product *= tile_id
    return product


def assemble(tiles: dict[int, list[str]]) -> list[list[list[str]]]:
    """Lay the tiles out in a square grid, or raise if no arrangement exists."""
    border = border_edges(tiles)
    count = len(tiles)
    size = int(round(count**0.5))
    corner = corner_ids(tiles)[0]

    # Orient the corner so its two border edges are on the top and left.
    start = next(
        orientation
        for orientation in orientations(tiles[corner])
        if canonical(edges(orientation)[0]) in border
        and canonical(edges(orientation)[3]) in border
    )

    # Index every orientation of every tile by its left and top edges.
    by_left: dict[str, list[tuple[int, list[str]]]] = {}
    by_top: dict[str, list[tuple[int, list[str]]]] = {}
    for tile_id, tile in tiles.items():
        for orientation in orientations(tile):
            top, _, _, left = edges(orientation)
            by_left.setdefault(left, []).append((tile_id, orientation))
            by_top.setdefault(top, []).append((tile_id, orientation))

    grid: list[list[list[str] | None]] = [[None] * size for _ in range(size)]
    used: set[int] = set()

    def place(row: int, col: int) -> bool:
        if row == size:
            return True
        if col == size:
            return place(row + 1, 0)

        if row == 0 and col == 0:
            candidates = [(corner, start)]
        elif row == 0:
            candidates = by_left.get(edges(grid[0][col - 1])[1], [])  # type: ignore[arg-type]
        elif col == 0:
            candidates = by_top.get(edges(grid[row - 1][0])[2], [])  # type: ignore[arg-type]
        else:
            left_edge = edges(grid[row][col - 1])[1]  # type: ignore[arg-type]
            top_edge = edges(grid[row - 1][col])[2]  # type: ignore[arg-type]
            candidates = [
                (tile_id, orientation)
                for tile_id, orientation in by_left.get(left_edge, [])
                if edges(orientation)[0] == top_edge
            ]

        next_col, next_row = (0, row + 1) if col + 1 == size else (col + 1, row)
        for tile_id, orientation in candidates:
            if tile_id in used:
                continue
            used.add(tile_id)
            grid[row][col] = orientation
            if place(next_row, next_col):
                return True
            used.discard(tile_id)
            grid[row][col] = None
        return False

    if not place(0, 0):
        raise ValueError("could not assemble the tiles")
    return grid  # type: ignore[return-value]


def stitch(grid: list[list[list[str]]]) -> list[str]:
    """Remove the tile borders and glue the tile interiors into one image."""
    inner = TILE_SIZE - 2
    image: list[str] = []
    for tile_row in grid:
        for y in range(1, TILE_SIZE - 1):
            image.append("".join(tile[y][1:-1] for tile in tile_row))
    assert all(len(row) == len(grid) * inner for row in image)
    return image


def image_orientations(image: list[str]) -> list[list[str]]:
    return orientations(image)


def count_monsters(image: list[str]) -> int:
    height, width = len(image), len(image[0])
    found = 0
    for row in range(height - MONSTER_HEIGHT + 1):
        for col in range(width - MONSTER_WIDTH + 1):
            if all(image[row + dr][col + dc] == "#" for dr, dc in MONSTER):
                found += 1
    return found


def part2() -> int:
    image = stitch(assemble(parse()))
    for orientation in image_orientations(image):
        monsters = count_monsters(orientation)
        if monsters:
            roughness = sum(row.count("#") for row in orientation)
            return roughness - monsters * len(MONSTER)
    raise ValueError("no sea monsters found")


def solve() -> str:
    return f"Part 1: {part1()}\nPart 2: {part2()}"


main = aoc_day("day20")(solve)
