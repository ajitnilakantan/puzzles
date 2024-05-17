package internal

import (
	"testing"

	"github.com/stretchr/testify/assert"
)

func TestDay17(t *testing.T) {
	input := `>>><<><>><<<>><>>><<<>>><<<><<<>><>><<>>`
	lines := split_lines(input)
	directions := read17_data(lines[0])
	grid := get17_grid()
	top := process17_moves(grid, directions, 2022)
	assert.Equal(t, 3068, top)

	grid = get17_grid()
	top = process17_moves(grid, directions, 1000000000000)
	assert.Equal(t, 1514285714288, top)
}
