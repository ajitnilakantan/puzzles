package internal

import (
	_ "embed"
	"fmt"
	"math"

	_ "github.com/stretchr/testify/assert"
)

var (
	//go:embed input/input23.txt
	input23 string
)

type rowcol23 struct {
	row int
	col int
}

func (rc *rowcol23) plus(pos *rowcol23) rowcol23 {
	return rowcol23{row: rc.row + pos.row, col: rc.col + pos.col}
}

type grid23 = Set[rowcol23]

func read23_grid(lines []string) grid23 {
	grid := make(grid23)
	for row, line := range lines {
		for col, val := range line {
			if val == '#' {
				grid.Add(rowcol23{row: row, col: col})
			}
		}
	}
	return grid
}

func make23_moves(grid grid23, num int) (grid23, int, int) {
	rounds := 0
	moves := [][]rowcol23{
		// N NE NW
		{{-1, 0}, {-1, +1}, {-1, -1}},
		// S SE SW
		{{+1, 0}, {+1, +1}, {+1, -1}},
		// W NW SW
		{{0, -1}, {-1, -1}, {+1, -1}},
		// E NE SE
		{{0, +1}, {-1, +1}, {+1, +1}},
	}
	neighbours_empty := func(pos rowcol23, grid grid23) bool {
		for r := -1; r <= 1; r++ {
			for c := -1; c <= 1; c++ {
				if r == 0 && c == 0 {
					continue
				}
				if grid.Contains(pos.plus(&rowcol23{row: r, col: c})) {
					return false
				}
			}
		}
		return true
	}
	get_count := func(grid grid23) int {
		count := 0
		min, max := get23_bounds(grid)
		for r := min.row; r <= max.row; r++ {
			for c := min.col; c <= max.col; c++ {
				if !grid.Contains(rowcol23{row: r, col: c}) {
					count++
				}
			}
		}
		return count
	}

	for i := 0; i < num; i++ {
		tentative_move := make(map[rowcol23]rowcol23)
		move_count := make(map[rowcol23]int)
		for k := range grid {
			// Check immediate neighbours. If all empty, don't move
			if neighbours_empty(k, grid) {
				tentative_move[k] = k
				move_count[k] = move_count[k] + 1
				continue
			}

			found := false
			// Process possible moves
			for _, m := range moves {
				if !grid.Contains(k.plus(&m[0])) && !grid.Contains(k.plus(&m[1])) && !grid.Contains(k.plus(&m[2])) {
					found = true
					tentative_move[k] = k.plus(&m[0])
					move_count[k.plus(&m[0])] = move_count[k.plus(&m[0])] + 1
					break
				}
			}
			if !found {
				tentative_move[k] = k
				move_count[k] = move_count[k] + 1
			}
		}

		// Update new grid
		dirty := false
		new_grid := make(grid23)
		for k, v := range tentative_move {
			if k != v {
				dirty = true
			}
			if move_count[v] <= 1 {
				new_grid.Add(v)
			} else {
				new_grid.Add(k)
			}
		}
		// Check if there was no change
		if !dirty {
			return grid, get_count(grid), rounds
		}

		if len(grid) != len(new_grid) {
			panic(fmt.Sprintf("unexpected len(grid)=%v != len(new_grid)=%v", len(grid), len(new_grid)))
		}
		grid = new_grid

		// Shuffle moves
		moves = append(moves[1:], moves[0])
		rounds++
	}

	return grid, get_count(grid), rounds
}

func get23_bounds(grid grid23) (min, max rowcol23) {
	min.row = math.MaxInt32
	min.col = math.MaxInt32
	max.row = math.MinInt32
	max.col = math.MinInt32
	for k := range grid {
		if min.row > k.row {
			min.row = k.row
		}
		if min.col > k.col {
			min.col = k.col
		}
		if max.row < k.row {
			max.row = k.row
		}
		if max.col < k.col {
			max.col = k.col
		}
	}
	return
}

func debug_print23_grid(grid grid23) {
	min, max := get23_bounds(grid)
	for r := min.row; r <= max.row; r++ {
		for c := min.col; c <= max.col; c++ {
			if grid.Contains(rowcol23{row: r, col: c}) {
				fmt.Printf("#")
			} else {
				fmt.Printf(".")
			}
		}
		fmt.Printf("\n")
	}
	fmt.Printf("\n")
}

func part23a(log Log) int {
	lines := split_lines(input23)
	grid := read23_grid(lines)
	_, count, _ := make23_moves(grid, 10)
	result := count

	return result
}

func part23b(log Log) int {
	lines := split_lines(input23)
	grid := read23_grid(lines)
	_, _, rounds := make23_moves(grid, math.MaxInt16)
	result := rounds + 1

	return result
}

func (t *AOC) Day23(log Log) {
	answer1 := part23a(log)
	fmt.Printf("answer1 := '%v'\n", answer1) // 4000
	answer2 := part23b(log)
	fmt.Printf("answer2 := '%v'\n", answer2) // 1040
}
