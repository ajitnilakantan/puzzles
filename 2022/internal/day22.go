package internal

import (
	_ "embed"
	"fmt"
	"regexp"

	_ "github.com/stretchr/testify/assert"
)

var (
	//go:embed input/input22.txt
	input22 string
)

// Directions.  Start right 0:→  1:↓  2:←  3:↑, going clockwise. [delta_row, delta_col]
var directions [4]rowcol22 = [4]rowcol22{{0, 1}, {1, 0}, {0, -1}, {-1, 0}}

/*
** There are 11 rot/reflect invariant shapes.
** See https://en.wikipedia.org/wiki/Hexomino
** Problem shapes dumped empirically using print_scaled()
** Test case:
**    A
**  BCD
**    EF
**  Mapping:
**  A-B : r:0s side:-1 c:[2s,3s] d:3  -  r:1s side:-1 c:[1s,0s] d:1
**  A-C : c:2s side:-1 r:[0s,1s] d:2  -  r:1s side:-1 c:[1s,2s] d:1
**  A-F : c:3s side:+1 r:[0s,1s] d:0  -  c:4s side:+1 r:[3s,2s] d:2
**  D-F : c:3s side:+1 r:[1s,2s] d:0  -  r:2s side:-1 c:[4s,3s] d:1
**  C-E : r:2s side:+1 c:[1s,2s] d:1  -  r:2s side:-1 c:[3s,2s] d:0
**  B-E : r:2s side:+1 c:[0s,1s] d:1  -  r:3s side:+1 c:[3s,2s] d:3
**  B-F : c:0s side:-1 r:[1s,2s] d:2  -  r:3s side:+1 c:[3s,2s] d:3
** Input data:
**   AB
**   C
**  DE
**  F
**  Mapping:
**  A-F : r:0s side:-1 c:[1s,2s] d:3  -  c:0s side:-1 c:[3s,4s] d:0
**  A-D : c:1s side:-1 r:[0s,1s] d:2  -  c:0s side:-1 r:[3s,2s] d:0
**  B-C : r:1s side:+1 c:[2s,3s] d:1  -  c:2s side:+1 r:[1s,2s] d:2
**  B-E : c:3s side:+1 r:[0s,1s] d:0  -  c:2s side:+1 r:[3s,2s] d:2
**  B-F : r:0s side:-1 r:[2s,3s] d:3  -  r:4s side:+1 r:[0s,1s] d:3
**  E-F : r:3s side:+1 c:[1s,2s] d:1  -  c:1s side:+1 r:[3s,4s] d:2
**  E-F : r:3s side:+1 c:[1s,2s] d:1  -  c:1s side:+1 r:[3s,4s] d:2
**  C-D : c:1s side:-1 r:[1s,2s] d:2  -  r:2s side:-1 c:[0s,1s] d:1
 */
type grid22 [][]byte
type rowcol22 struct{ row, col int }

type mapping22 struct {
	typ  byte
	rc   int
	side int
	fr   int
	to   int
	dir  int
}

//lint:ignore U1000 Ignore unused function temporarily for debugging
var test22_mapping [][2]mapping22 = [][2]mapping22{
	// **  A-B : r:0s side:-1 c:[2s,3s] d:3  -  r:1s side:-1 c:[1s,0s] d:1
	{{typ: 'r', rc: 0, side: -1, fr: 2, to: 3, dir: 3}, {typ: 'r', rc: 1, side: -1, fr: 1, to: 0, dir: 1}},
	// **  A-C : c:2s side:-1 r:[0s,1s] d:2  -  r:1s side:-1 c:[1s,2s] d:1
	{{typ: 'c', rc: 2, side: -1, fr: 0, to: 1, dir: 2}, {typ: 'r', rc: 1, side: -1, fr: 1, to: 2, dir: 1}},
	// **  A-F : c:3s side:+1 r:[0s,1s] d:0  -  c:4s side:+1 r:[3s,2s] d:2
	{{typ: 'c', rc: 3, side: +1, fr: 0, to: 1, dir: 0}, {typ: 'c', rc: 4, side: +1, fr: 3, to: 2, dir: 2}},
	// **  D-F : c:3s side:+1 r:[1s,2s] d:0  -  r:2s side:-1 c:[4s,3s] d:1
	{{typ: 'c', rc: 3, side: +1, fr: 1, to: 2, dir: 0}, {typ: 'r', rc: 2, side: -1, fr: 4, to: 3, dir: 1}},
	// **  C-E : r:2s side:+1 c:[1s,2s] d:1  -  c:2s side:-1 c:[3s,2s] d:0
	{{typ: 'r', rc: 2, side: +1, fr: 1, to: 2, dir: 1}, {typ: 'c', rc: 2, side: -1, fr: 3, to: 2, dir: 0}},
	// **  B-E : r:2s side:+1 c:[0s,1s] d:1  -  r:3s side:+1 c:[3s,2s] d:3
	{{typ: 'r', rc: 2, side: +1, fr: 0, to: 1, dir: 1}, {typ: 'r', rc: 3, side: +1, fr: 3, to: 2, dir: 3}},
	// **  B-F : c:0s side:-1 r:[1s,2s] d:2  -  r:3s side:+1 c:[3s,2s] d:3
	{{typ: 'c', rc: 0, side: -1, fr: 1, to: 2, dir: 2}, {typ: 'r', rc: 3, side: +1, fr: 4, to: 3, dir: 3}},
}

var prob22_mapping [][2]mapping22 = [][2]mapping22{
	// **  A-F : r:0s side:-1 c:[1s,2s] d:3  -  c:0s side:-1 r:[3s,4s] d:0
	{{typ: 'r', rc: 0, side: -1, fr: 1, to: 2, dir: 3}, {typ: 'c', rc: 0, side: -1, fr: 3, to: 4, dir: 0}},
	// **  A-D : c:1s side:-1 r:[0s,1s] d:2  -  c:0s side:-1 r:[3s,2s] d:0
	{{typ: 'c', rc: 1, side: -1, fr: 0, to: 1, dir: 2}, {typ: 'c', rc: 0, side: -1, fr: 3, to: 2, dir: 0}},
	// **  B-C : r:1s side:+1 c:[2s,3s] d:1  -  c:2s side:+1 r:[1s,2s] d:2
	{{typ: 'r', rc: 1, side: +1, fr: 2, to: 3, dir: 1}, {typ: 'c', rc: 2, side: +1, fr: 1, to: 2, dir: 2}},
	// **  B-E : c:3s side:+1 r:[0s,1s] d:0  -  c:2s side:+1 r:[3s,2s] d:2
	{{typ: 'c', rc: 3, side: +1, fr: 0, to: 1, dir: 0}, {typ: 'c', rc: 2, side: +1, fr: 3, to: 2, dir: 2}},
	// **  B-F : r:0s side:-1 r:[2s,3s] d:3  -  r:4s side:+1 r:[0s,1s] d:3
	{{typ: 'r', rc: 0, side: -1, fr: 2, to: 3, dir: 3}, {typ: 'r', rc: 4, side: +1, fr: 0, to: 1, dir: 3}},
	// **  E-F : r:3s side:+1 c:[1s,2s] d:1  -  c:1s side:+1 r:[3s,4s] d:2
	{{typ: 'r', rc: 3, side: +1, fr: 1, to: 2, dir: 1}, {typ: 'c', rc: 1, side: +1, fr: 3, to: 4, dir: 2}},
	// **  C-D : c:1s side:-1 r:[1s,2s] d:2  -  r:2s side:-1 c:[0s,1s] d:1
	{{typ: 'c', rc: 1, side: -1, fr: 1, to: 2, dir: 2}, {typ: 'r', rc: 2, side: -1, fr: 0, to: 1, dir: 1}},
}

func read22_grid(lines []string) (grid grid22, start rowcol22) {
	// The grid is not "dense". Each line can have a different size.
	// LHS is always space padded. We need to space pad RHS
	width := 0
	for _, line := range lines {
		if width < len(line) {
			width = len(line)
		}
	}
	height := len(lines)
	grid = MakeMatrix2D[byte](height, width)
	for row, line := range lines {
		for col := 0; col < len(line); col++ {
			grid[row][col] = line[col]
		}
		for col := len(line); col < width; col++ {
			grid[row][col] = '\x20'
		}
	}
	for row := 0; row < height; row++ {
		for col := 0; col < width; col++ {
			if grid[row][col] == '.' {
				start = rowcol22{row, col}
				return grid, start
			}
		}
	}
	panic("unexpected: cannot find grid start coords")
}

//lint:ignore U1000 Ignore unused function temporarily for debugging
func print_scaled(grid grid22, scale int) {
	height := len(grid)
	width := len(grid[0])
	for row := 0; row < height; row += scale {
		for col := 0; col < width; col += scale {
			if grid[row][col] == '\x20' {
				fmt.Printf("\x20")
			} else {
				fmt.Printf("X")
			}
		}
		fmt.Printf("\n")
	}
}

func play22_move(grid grid22, num int, direction int, start rowcol22) rowcol22 {
	mod := func(val, modulo int) int { return (((val + modulo) % modulo) + modulo) % modulo }
	next := func(pos rowcol22, direction int) rowcol22 {
		row := mod(pos.row+directions[direction].row, len(grid))
		col := mod(pos.col+directions[direction].col, len(grid[0]))
		return rowcol22{row: row, col: col}
	}
	pos := start
	next_pos := pos
	for num > 0 {
		next_pos = next(next_pos, direction)
		if grid[next_pos.row][next_pos.col] == '#' {
			return pos
		} else if grid[next_pos.row][next_pos.col] == '\x20' {
			continue
		} else if grid[next_pos.row][next_pos.col] == '.' {
			pos = next_pos
			num--
		} else {
			panic(fmt.Sprintf("unexpected grid character '%v' at %+v", grid[next_pos.row][next_pos.col], next_pos))
		}
	}
	return pos
}

// Move along the faces of a cube stitched together using "mapping"
func play22_move_cube(grid grid22, num int, direction int, start rowcol22, mapping [][2]mapping22, scale int) (int, rowcol22) {
	next := func(pos rowcol22, direction int) rowcol22 {
		row := pos.row + directions[direction].row
		col := pos.col + directions[direction].col
		return rowcol22{row: row, col: col}
	}
	between := func(a, x, b int) bool {
		// a <= x < b || b <= x < a
		if a > b {
			a, b = b, a
		}
		if x >= a && x < b {
			return true
		}
		return false
	}
	mapx := func(x, from0, from1, to0, to1 int) int {
		return to0 + ((to1-to0)/(from1-from0))*(x-from0)
	}
	past_border_row := func(pos rowcol22, m mapping22) bool {
		row := scale * m.rc
		if m.side == -1 {
			row -= 1
		}
		if m.typ == 'r' && row == pos.row && between(scale*m.fr, pos.col, scale*m.to) {
			return true
		}
		return false
	}
	past_border_col := func(pos rowcol22, m mapping22) bool {
		col := scale * m.rc
		if m.side == -1 {
			col -= 1
		}
		if m.typ == 'c' && col == pos.col && between(scale*m.fr, pos.row, scale*m.to) {
			return true
		}
		return false
	}

	map_pos := func(pos rowcol22, from mapping22, to mapping22, scale int) rowcol22 {
		from0 := scale * from.fr
		from1 := scale * from.to
		to0 := scale * to.fr
		to1 := scale * to.to
		// Subtract 1 because we have [closed open) intervals
		if from1 > from0 {
			from1 -= 1
		} else {
			from0 -= 1
		}
		if to1 > to0 {
			to1 -= 1
		} else {
			to0 -= 1
		}

		// Input to map
		var x int
		if from.typ == 'r' {
			x = pos.col
		} else {
			x = pos.row
		}
		// Map to output
		if to.typ == 'r' {
			if to.side == -1 {
				pos = rowcol22{row: scale * to.rc, col: mapx(x, from0, from1, to0, to1)}
			} else {
				pos = rowcol22{row: scale*to.rc - 1, col: mapx(x, from0, from1, to0, to1)}
			}
		} else {
			if to.side == -1 {
				pos = rowcol22{row: mapx(x, from0, from1, to0, to1), col: scale * to.rc}
			} else {
				pos = rowcol22{row: mapx(x, from0, from1, to0, to1), col: scale*to.rc - 1}
			}
		}
		return pos
	}

	pos := start
	next_pos := pos
	for num > 0 {
		next_pos = next(next_pos, direction)
		// check for wraparound to another face
		for _, m := range mapping {
			// {{typ: 'r', rc: 0, side: -1, fr: 1, to: 2, dir: 1}, {typ: 'c', rc: 0, side: -1, fr: 3, to: 4, dir: 0}},
			// Map from:
			if direction == m[0].dir && (past_border_row(next_pos, m[0]) || past_border_col(next_pos, m[0])) {
				// Map to:
				old_pos := next_pos
				next_pos = map_pos(next_pos, m[0], m[1], scale)
				if grid[next_pos.row][next_pos.col] == '#' {
					// backtrack 1
					next_pos = next(old_pos, (direction+2)%4)
					return direction, next_pos
				}
				direction = m[1].dir
				break
			} else if (direction+2)%4 == m[1].dir && (past_border_row(next_pos, m[1]) || past_border_col(next_pos, m[1])) {
				// Map to:
				old_pos := next_pos
				next_pos = map_pos(next_pos, m[1], m[0], scale)
				if grid[next_pos.row][next_pos.col] == '#' {
					// backtrack 1
					next_pos = next(old_pos, (direction+2)%4)
					return direction, next_pos
				}
				direction = (m[0].dir + 2) % 4
				break
			}
		}
		if grid[next_pos.row][next_pos.col] == '#' {
			return direction, pos
		} else if grid[next_pos.row][next_pos.col] == '\x20' {
			panic(fmt.Sprintf("unexpected: space at pos %v direction %v", next_pos, direction))
			// continue
		} else if grid[next_pos.row][next_pos.col] == '.' {
			pos = next_pos
			num--
		} else {
			panic(fmt.Sprintf("unexpected grid character '%v' at %+v", grid[next_pos.row][next_pos.col], next_pos))
		}
	}
	return direction, pos
}

func play22_moves(grid grid22, moves []string, start rowcol22, mapping [][2]mapping22, scale int) (rowcol22, int) {
	// Initially right
	direction := 0
	pos := start
	for _, m := range moves {
		switch m {
		case "L":
			direction = (direction + 3) % 4
		case "R":
			direction = (direction + 1) % 4
		default:
			num := my_atoi(m)
			if mapping != nil && scale != 0 {
				// fmt.Printf("from %+v dir=%v num=%v ", pos, direction, num)
				direction, pos = play22_move_cube(grid, num, direction, pos, mapping, scale)
				// fmt.Printf("to %+v dir=%v\n", pos, direction)
			} else {
				pos = play22_move(grid, num, direction, pos)
			}
		}
	}
	return pos, direction
}

func read22_moves(line string) []string {
	regex := `\d+|[LR]`
	re := regexp.MustCompile(regex)
	rs := re.FindAllString(line, -1)
	return rs
}

func part22a(log Log) int {
	chunks := split_chunks(split_lines(input22, false))
	grid, start := read22_grid(chunks[0])
	moves := read22_moves(chunks[1][0])
	pos, direction := play22_moves(grid, moves, start, nil, 0)
	result := 1000*(pos.row+1) + 4*(pos.col+1) + direction

	return result
}

func part22b(log Log) int {
	chunks := split_chunks(split_lines(input22, false))
	grid, start := read22_grid(chunks[0])
	moves := read22_moves(chunks[1][0])
	pos, direction := play22_moves(grid, moves, start, prob22_mapping, 50)
	result := 1000*(pos.row+1) + 4*(pos.col+1) + direction
	// print_scaled(grid, 50)

	return result
}

func (t *AOC) Day22(log Log) {
	answer1 := part22a(log)
	fmt.Printf("answer1 := '%v'\n", answer1) // 27436
	answer2 := part22b(log)
	fmt.Printf("answer2 := '%v'\n", answer2) // 15426
}
