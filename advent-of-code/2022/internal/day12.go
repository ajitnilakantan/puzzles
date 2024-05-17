package internal

import (
	_ "embed"
	"fmt"
	"math"

	_ "github.com/stretchr/testify/assert"
)

var (
	//go:embed input/input12.txt
	input12 string
)

type grid12 [][]int
type coord12 struct{ row, col int }

// implement Graph for grid12
//
//lint:ignore U1000 Ignore unused function temporarily for debugging
func (g grid12) get_successors(node coord12) []coord12 {
	width := len(g[0])
	height := len(g)
	curVal := g[node.row][node.col]
	succ := make([]coord12, 0)
	dirs := [][]int{{-1, 0}, {1, 0}, {0, -1}, {0, 1}}

	for _, d := range dirs {
		r, c := node.row+d[0], node.col+d[1]
		if r >= 0 && r < height && c >= 0 && c < width && g[r][c]-curVal <= 1 {
			succ = append(succ, coord12{row: r, col: c})
		}
	}

	return succ
}

// implement GraphWithWeights for grid12
//
//lint:ignore U1000 Ignore unused function temporarily for debugging
func (g grid12) get_dist_between(node coord12, neighbor coord12) float64 {
	return math.Abs(float64(node.row-neighbor.row)) + math.Abs(float64(node.col-neighbor.col))
}

// implement GraphWithHeuristic for grid12
//
//lint:ignore U1000 Ignore unused function temporarily for debugging
func (g grid12) get_heuristic(node coord12, goal coord12) float64 {
	return g.get_dist_between(node, goal)
}

func read12_grid(lines []string) (grid grid12, start coord12, goal coord12) {
	width := len(lines[0])
	height := len(lines)
	grid = MakeMatrix2D[int](height, width)
	for row, line := range lines {
		for col := 0; col < width; col++ {
			grid[row][col] = int(line[col])
			if grid[row][col] == 'S' {
				grid[row][col] = int('a')
				start = coord12{row: row, col: col}
			}
			if grid[row][col] == 'E' {
				grid[row][col] = int('z')
				goal = coord12{row: row, col: col}
			}
		}
	}
	// fmt.Printf("w=%v h=%v s=%v g=%v\n", width, height, start, goal)
	return
}

func part12a(log Log) int {
	lines := split_lines(input12)
	grid, start, goal := read12_grid(lines)
	path := AStarSolve[coord12](grid, start, goal)
	result := len(path) - 1 // get length of path

	return result
}

func part12b(log Log) int {
	lines := split_lines(input12)
	grid, _, goal := read12_grid(lines)
	shortest_path := len(grid) * len(grid[0])
	for r := 0; r < len(grid); r++ {
		for c := 0; c < len(grid[0]); c++ {
			if grid[r][c] == 'a' {
				start := coord12{row: r, col: c}
				path := AStarSolve[coord12](grid, start, goal)
				if len(path) > 0 && len(path)-1 < shortest_path {
					shortest_path = len(path) - 1
				}
			}
		}
	}
	result := shortest_path
	return result
}

func (t *AOC) Day12(log Log) {
	answer1 := part12a(log)
	fmt.Printf("answer1 := '%v'\n", answer1) // 425
	answer2 := part12b(log)
	fmt.Printf("answer2 := '%v'\n", answer2) // 418
}
