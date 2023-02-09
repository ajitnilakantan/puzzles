package internal

import (
	"math"
	"testing"

	"github.com/stretchr/testify/assert"
)

func TestDay23(t *testing.T) {
	data :=
		`.....
		..##.
		..#..
		.....
		..##.
		.....`
	lines := split_lines(data)
	grid := read23_grid(lines)
	debug_print23_grid(grid)
	var rounds int
	grid, _, rounds = make23_moves(grid, 10)
	debug_print23_grid(grid)
	assert.Equal(t, 4, rounds+1)

	data = `..............
	..............
	.......#......
	.....###.#....
	...#...#.#....
	....#...##....
	...#.###......
	...##.#.##....
	....#..#......
	..............
	..............
	..............`

	lines = split_lines(data)
	grid = read23_grid(lines)
	debug_print23_grid(grid)
	var count int
	grid, count, _ = make23_moves(grid, 10)
	debug_print23_grid(grid)
	assert.Equal(t, 110, count)

	lines = split_lines(data)
	grid = read23_grid(lines)
	_, _, rounds = make23_moves(grid, math.MaxInt16)
	assert.Equal(t, 20, rounds+1)
}
