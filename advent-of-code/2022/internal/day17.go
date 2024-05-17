package internal

import (
	_ "embed"
	"fmt"
	"reflect"

	_ "github.com/stretchr/testify/assert"
)

var (
	//go:embed input/input17.txt
	input17 string
)

type shape17 struct {
	// height and width of the sprite
	height, width int
	// bits of sprite counting top down, left to right
	bitmask []int
}

var shapes17 []shape17 = []shape17{
	// ####
	{height: 1, width: 4, bitmask: []int{0b111_100_000}},

	// .#.
	// ###
	// .#.
	{height: 3, width: 3, bitmask: []int{0b010_000_000, 0b111_000_000, 0b010_000_000}},

	// ..#
	// ..#
	// ###
	{height: 3, width: 3, bitmask: []int{0b001_000_000, 0b001_000_000, 0b111_000_000}},

	// #
	// #
	// #
	// #
	{height: 4, width: 1, bitmask: []int{0b100_000_000, 0b100_000_000, 0b100_000_000, 0b100_000_000}},

	// ##
	// ##
	{height: 2, width: 2, bitmask: []int{0b110_000_000, 0b110_000_000}},
}

/*
	4 |..@@@@.|
	3 |.......|
	2 |.......|
	1 |.......|
	0 +-------+
	  012345678

Store the grid as an array of ints bottom up
Bit 0 is always 1 (left wall) and bit 8 is
always 1 (right wall)
*/
type grid17 []int

// const grid17_width int = 9 // width of the grid, including the walls
const grid_height_increment = 1024 // increase the height of the grid in this increment

func extend17_grid(grid grid17) grid17 {
	oldlen := len(grid)
	grid = append(grid, make([]int, grid_height_increment)...)
	newlen := len(grid)
	for i := oldlen; i < newlen; i++ {
		grid[i] = 0b100_000_001 // walls
	}
	return grid
}

func get17_grid() grid17 {
	grid := make(grid17, 1)
	grid[0] = 0b111_111_111    // floor
	grid = extend17_grid(grid) // adds the walls
	return grid
}

func shape17_hits_grid(grid *grid17, shape shape17, posy int, posx int) bool {
	if posy >= len(*grid) {
		*grid = extend17_grid(*grid)
	}
	for row := 0; row < shape.height; row++ {
		if (*grid)[posy-row]&(shape.bitmask[row]>>posx) != 0 {
			return true
		}
	}
	return false
}

// Add the shape to the grid at the specified position. Update the top height and return it.
func add17_shape(grid grid17, shape shape17, posy int, posx int, current_top *int) {
	for row := 0; row < shape.height; row++ {
		grid[posy-row] |= (shape.bitmask[row] >> posx)
	}
	if posy > *current_top {
		*current_top = posy
	}
}

//lint:ignore U1000 Ignore unused function temporarily for debugging
func print17_grid(grid grid17, posy int, current_top int) {
	fmt.Printf("---y:= %v top:= %v\n", posy, current_top)
	for i := current_top; i >= 0; i-- {
		fmt.Printf("%09b\n", grid[i])
	}
	fmt.Printf("\n")
}

func process17_moves(grid grid17, directions string, num_moves int) int {
	current_top := 0            // floor
	current_top_skip_ahead := 0 // if we detect a repeating pattern, we can skip ahead
	current_shape := 0
	current_direction := 0

	cycle_count := 0                       // look for cycles
	cycles_to_check := 2 * (2 * 3 * 4 * 5) // look for cycles of upto 5 in size
	cycles_to_check = 20
	cycle_current_top := make([]int, cycles_to_check)
	cycle_move := make([]int, cycles_to_check)

	for move := 0; move < num_moves; move++ {
		shape := shapes17[current_shape]
		current_shape = (current_shape + 1) % len(shapes17)
		posx := 3                              // always start 3 units away from the left wall
		posy := current_top + 3 + shape.height // always start 3 above the current top postion
		// print17_grid(grid, posy, current_top)

		for {
			var direction int
			if directions[current_direction] == '<' {
				direction = -1
			} else {
				direction = 1
			}

			if current_direction == 0 && current_shape == 0 {
				if cycle_count < cycles_to_check {
					cycle_move[cycle_count] = move
					cycle_current_top[cycle_count] = current_top
				}
				cycle_count++
			}

			current_direction = (current_direction + 1) % len(directions)

			if current_direction == 0 && current_shape == 0 && cycle_count == cycles_to_check-1 {
				// Get the deltas in the move
				cycle_length_deltas := make([]int, len(cycle_move))
				for i := 1; i < len(cycle_move)-2; i++ {
					cycle_length_deltas[i] = cycle_move[i] - cycle_move[i-1]
				}
				found_match := 0
				// Look for looping of [2..5] in size
				for c := 2; c <= 5; c++ {
					found_match = c
					for cn := c; cn < cycles_to_check-1-c; cn += c {
						if !reflect.DeepEqual(cycle_length_deltas[c:2*c], cycle_length_deltas[cn:cn+c]) {
							found_match = 0
							break
						}
					}
					if found_match != 0 {
						break
					}
				}
				if found_match != 0 {
					// Advance
					sumFunc := func(cur int, next int) int { return cur + next }
					delta_move := Reduce(cycle_length_deltas[found_match:2*found_match], 0, sumFunc)
					delta_current_top := cycle_current_top[2*found_match] - cycle_current_top[found_match]

					delta := ((num_moves - move) / delta_move)
					num_moves -= delta * delta_move
					current_top_skip_ahead = delta * delta_current_top
				}
			}

			// wind blows
			if !shape17_hits_grid(&grid, shape, posy, posx+direction) {
				posx = posx + direction
			}
			// move down
			if !shape17_hits_grid(&grid, shape, posy-1, posx) {
				posy -= 1

			} else {
				// hit something. stop
				add17_shape(grid, shape, posy, posx, &current_top)
				break
			}
		}

	}

	return current_top + current_top_skip_ahead
}

// Return array of negative (<move left) and positive (>move right) of consecutive moves
func read17_data(line string) string {
	return line
}
func part17a(log Log) int {
	lines := split_lines(input17)
	directions := read17_data(lines[0])
	grid := get17_grid()
	result := process17_moves(grid, directions, 2022)
	return result
}

func part17b(log Log) int {
	lines := split_lines(input17)
	directions := read17_data(lines[0])
	grid := get17_grid()
	result := process17_moves(grid, directions, 1000000000000)
	return result
}

func (t *AOC) Day17(log Log) {
	answer1 := part17a(log)
	fmt.Printf("answer1 := '%v'\n", answer1) // 3202
	answer2 := part17b(log)
	fmt.Printf("answer2 := '%v'\n", answer2) // 1591977077352
}
