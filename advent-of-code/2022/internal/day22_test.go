package internal

import (
	"fmt"
	"testing"

	"github.com/stretchr/testify/assert"
)

func TestDay22(t *testing.T) {
	data :=
		`        ...#
        .#..
        #...
        ....
...#.......#
........#...
..#....#....
..........#.
        ...#....
        .....#..
        .#......
        ......#.

10R5L5R10L4R5L5`

	chunks := split_chunks(split_lines(data, false))
	grid, start := read22_grid(chunks[0])
	moves := read22_moves(chunks[1][0])
	pos, direction := play22_moves(grid, moves, start, nil, 0)
	result := 1000*(pos.row+1) + 4*(pos.col+1) + direction
	assert.Equal(t, 6032, result)

	print_scaled(grid, 4)
	// Part b
	{
		chunks := split_chunks(split_lines(data, false))
		grid, start := read22_grid(chunks[0])
		moves := read22_moves(chunks[1][0])
		pos, direction := play22_moves(grid, moves, start, test22_mapping, 4)
		fmt.Printf("end up at %+v dir=%v\n", pos, direction)
		result := 1000*(pos.row+1) + 4*(pos.col+1) + direction
		assert.Equal(t, 5031, result)
	}
}
