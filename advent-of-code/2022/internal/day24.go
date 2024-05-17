package internal

import (
	_ "embed"
	"fmt"
	"math"

	_ "github.com/stretchr/testify/assert"
)

var (
	//go:embed input/input24.txt
	input24 string
)

type coord24 struct {
	row, col int
	time     int
}
type grid24 struct {
	height, width int
	cell          [][]byte
}

// impl Graph
func (g *grid24) get_successors(node coord24) []coord24 {
	mod := func(val, modulo int) int { return (((val + modulo) % modulo) + modulo) % modulo }
	get_at := func(node coord24, grid *grid24) int {
		count := 0
		// look left for '>'
		if g.cell[node.row][mod(node.col-1-node.time, grid.width-2)+1] == '>' {
			count++
		}
		// look right for '<'
		if g.cell[node.row][mod(node.col-1+node.time, grid.width-2)+1] == '<' {
			count++
		}
		// look up for 'v'
		if g.cell[mod(node.row-1-node.time, grid.height-2)+1][node.col] == 'v' {
			count++
		}
		// look down for '^'
		if g.cell[mod(node.row-1+node.time, grid.height-2)+1][node.col] == '^' {
			count++
		}

		return count
	}

	// look at the 5 neighbours (up, down, left, right, current) around the
	// current position at time=time+1
	dirs := [][]int{{-1, 0}, {1, 0}, {0, -1}, {0, 1}, {0, 0}}
	neighbors := []coord24{}
	for _, d := range dirs {
		neighbor := coord24{row: node.row + d[0], col: node.col + d[1], time: node.time + 1}
		// if node.row != 0 && neighbor.row == 0 {
		// 	continue
		// }
		if neighbor.row < 0 || neighbor.row > g.height-1 || neighbor.col < 0 || neighbor.col > g.width-1 || g.cell[neighbor.row][neighbor.col] == '#' {
			continue
		}
		if get_at(neighbor, g) == 0 {
			neighbors = append(neighbors, neighbor)
		}
	}
	return neighbors
}

// impl GraphWithWeights
func (g *grid24) get_dist_between(node coord24, neighbour coord24) float64 {
	if neighbour.time != -1 && neighbour.time-node.time != 1 {
		panic("unexpected: time difference should be 1")
	}
	if neighbour.time == -1 && node.row == neighbour.row && node.col == neighbour.col {
		return 0
	}
	return math.Abs(float64(node.row-neighbour.row)) + math.Abs(float64(node.col-neighbour.col)) + 1
}

// impl GraphWithHeuristic
func (g *grid24) get_heuristic(node coord24, neighbour coord24) float64 {
	return math.Abs(float64(node.row-neighbour.row)) + math.Abs(float64(node.col-neighbour.col))
}

func read24_grid(lines []string) grid24 {
	grid := grid24{height: len(lines), width: len(lines[0])}
	grid.cell = MakeMatrix2D[byte](grid.height, grid.width)
	for r := 0; r < grid.height; r++ {
		for c := 0; c < grid.width; c++ {
			grid.cell[r][c] = lines[r][c]
		}
	}
	return grid
}

func part24a(log Log) int {
	lines := split_lines(input24)
	grid := read24_grid(lines)
	start := coord24{row: 0, col: 1, time: 0}
	goal := coord24{row: grid.height - 1, col: grid.width - 2, time: -1}
	path := AStarSolve[coord24](&grid, start, goal)
	//path := DijkstraSolve[coord24](&grid, start, goal)
	result := len(path) - 1

	return result
}

func part24b(log Log) int {
	lines := split_lines(input24)
	grid := read24_grid(lines)
	start := coord24{row: 0, col: 1, time: 0}
	goal := coord24{row: grid.height - 1, col: grid.width - 2, time: -1}
	path := AStarSolve[coord24](&grid, start, goal)
	time := len(path) - 1
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

	result := time
	return result
}

func (t *AOC) Day24(log Log) {
	answer1 := part24a(log)
	fmt.Printf("answer1 := '%v'\n", answer1) // 225
	answer2 := part24b(log)
	fmt.Printf("answer2 := '%v'\n", answer2) // 711
}
