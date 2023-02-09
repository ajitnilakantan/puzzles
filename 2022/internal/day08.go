package internal

import (
	_ "embed"
	"fmt"

	_ "github.com/stretchr/testify/assert"
)

var (
	//go:embed input/input08.txt
	input08 string
)

func read08_grid(lines []string) [][]int {
	width := len(lines[0])
	height := len(lines)
	grid := MakeMatrix2D[int](height, width)
	for row, line := range lines {
		for col := 0; col < width; col++ {
			grid[row][col] = int(line[col] - '0')
		}
	}
	return grid
}

func visible08_trees(grid [][]int) int {
	width := len(grid[0])
	height := len(grid)
	type coord struct{ row, col int }
	visible_trees := make(Set[coord])

	// By row
	for r := 0; r < height; r++ {
		maxleft, maxright := -1, -1
		for cleft, cright := 0, width-1; cleft < width-1; cleft, cright = cleft+1, cright-1 {
			if maxleft < grid[r][cleft] {
				maxleft = grid[r][cleft]
				visible_trees.Add(coord{r, cleft})
			}
			if maxright < grid[r][cright] {
				maxright = grid[r][cright]
				visible_trees.Add(coord{r, cright})
			}

		}
	}
	// By column
	for c := 0; c < width; c++ {
		maxtop, maxbottom := -1, -1
		for rtop, rbottom := 0, height-1; rtop < height-1; rtop, rbottom = rtop+1, rbottom-1 {
			if maxtop < grid[rtop][c] {
				maxtop = grid[rtop][c]
				visible_trees.Add(coord{rtop, c})
			}
			if maxbottom < grid[rbottom][c] {
				maxbottom = grid[rbottom][c]
				visible_trees.Add(coord{rbottom, c})
			}

		}
	}
	return len(visible_trees)
}

func visible08_score(grid [][]int, row int, col int) int {
	width := len(grid[0])
	height := len(grid)
	g := grid[row][col]
	// up
	var count int
	score := 1
	count = 0
	for x := row - 1; x >= 0; x-- {
		if grid[x][col] >= g {
			count++
			break
		}
		count++
	}
	score *= count

	// down
	count = 0
	for x := row + 1; x < height; x++ {
		if grid[x][col] >= g {
			count++
			break
		}
		count++
	}
	score *= count

	// left
	count = 0
	for x := col - 1; x >= 0; x-- {
		if grid[row][x] >= g {
			count++
			break
		}
		count++
	}
	score *= count

	// right
	count = 0
	for x := col + 1; x < width; x++ {
		if grid[row][x] >= g {
			count++
			break
		}
		count++
	}
	score *= count

	return score
}

func visible08_best_score(grid [][]int) int {
	width := len(grid[0])
	height := len(grid)
	best := 0
	for r := 0; r < height; r++ {
		for c := 0; c < width; c++ {
			if score := visible08_score(grid, r, c); best < score {
				best = score
			}
		}
	}
	return best
}

func part08a(log Log) int {
	lines := split_lines(input08)
	grid := read08_grid(lines)
	result := visible08_trees(grid)

	return result
}

func part08b(log Log) int {
	lines := split_lines(input08)
	grid := read08_grid(lines)
	result := visible08_best_score(grid)
	return result
}

func (t *AOC) Day08(log Log) {
	answer1 := part08a(log)
	fmt.Printf("answer1 := '%v'\n", answer1) // 1798
	answer2 := part08b(log)
	fmt.Printf("answer2 := '%v'\n", answer2) // 259308
}
