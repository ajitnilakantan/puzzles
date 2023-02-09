package internal

import (
	"testing"

	"github.com/stretchr/testify/assert"
)

func TestDay12(t *testing.T) {
	data :=
		`Sabqponm
		abcryxxl
		accszExk
		acctuvwj
		abdefghi`
	lines := split_lines(data)

	grid, start, goal := read12_grid(lines)
	path := BFS[coord12](&grid, start, goal)
	assert.Equal(t, 31, len(path)-1)
	path = AStarSolve[coord12](&grid, start, goal)
	assert.Equal(t, 31, len(path)-1)
	path = DijkstraSolve[coord12](&grid, start, goal)
	assert.Equal(t, 31, len(path)-1)

	// Part b
	grid, _, goal = read12_grid(lines)
	shortest_path := len(grid) * len(grid[0])
	for r := 0; r < len(grid); r++ {
		for c := 0; c < len(grid[0]); c++ {
			if grid[r][c] == 'a' {
				start = coord12{row: r, col: c}
				path = AStarSolve[coord12](&grid, start, goal)
				if len(path) > 0 && len(path)-1 < shortest_path {
					shortest_path = len(path) - 1
				}
			}
		}
	}
	assert.Equal(t, 29, shortest_path)
}
