package internal

import (
	"testing"

	"github.com/stretchr/testify/assert"
)

func TestDay24(t *testing.T) {
	data :=
		`#.######
		#>>.<^<#
		#.<..<<#
		#>v.><>#
		#<^v^^>#
		######.#`
	lines := split_lines(data)
	grid := read24_grid(lines)
	start := coord24{row: 0, col: 1, time: 0}
	goal := coord24{row: grid.height - 1, col: grid.width - 2, time: -1}
	// path := AStarSolve[coord24](&grid, start, goal)
	path := DijkstraSolve[coord24](&grid, start, goal)
	// fmt.Printf("path=%+v\n", path)
	time := len(path) - 1
	assert.Equal(t, 18, time)

	// back to start
	start = coord24{row: grid.height - 1, col: grid.width - 2, time: time}
	goal = coord24{row: 0, col: 1, time: -1}
	path = DijkstraSolve[coord24](&grid, start, goal)
	time += len(path) - 1

	// back to goal
	start = coord24{row: 0, col: 1, time: time}
	goal = coord24{row: grid.height - 1, col: grid.width - 2, time: -1}
	path = DijkstraSolve[coord24](&grid, start, goal)
	time += len(path) - 1
	assert.Equal(t, 54, time)

}
