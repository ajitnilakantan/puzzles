package internal

import (
	_ "embed"
	"fmt"

	_ "github.com/stretchr/testify/assert"
)

var (
	//go:embed input/input05.txt
	input05 string
)

// Format stacks as readable string
//
//lint:ignore U1000 Ignore unused function temporarily for debugging
func stacks_tostring05(stacks [][]byte) string {
	ret := ""
	for _, s := range stacks {
		ret = ret + fmt.Sprintf(" '%v'", string(s))
	}
	return ret
}

// Return list of stacks.  Stacks are slices from [bottom..top]
func read05_stacks(lines []string) [][]byte {
	num_stacks := (len(lines[0]) + 1) / 4
	stacks := make([][]byte, num_stacks)
	for i := 0; i < len(lines)-1; i++ {
		// Last line is the count -- can ignore
		line := lines[i]
		for j := 0; j < num_stacks; j++ {
			var c byte = line[4*j+1]
			if c != '\x20' {
				stacks[j] = append([]byte{c}, stacks[j]...)
			}
		}
	}
	return stacks
}

func read_move05(line string) (num, from, to int) {
	// The first token is blank because the line starts with " move ".  Strip it.
	tokens := split_line_regex(line, "(\\s*move\\s*|\\s*from\\s*|\\s*to\\s*)")[1:]

	num = my_atoi(tokens[0])
	from = my_atoi(tokens[1])
	to = my_atoi(tokens[2])
	return
}

func perform_move05(stacks [][]byte, num int, from int, to int, one_at_a_time bool) {
	if one_at_a_time {
		for i := 0; i < num; i++ {
			// from and t are 1-index, so subtract 1
			top := stacks[from-1][len(stacks[from-1])-1]               // top element
			stacks[from-1] = stacks[from-1][0 : len(stacks[from-1])-1] // pop
			stacks[to-1] = append(stacks[to-1], top)
		}
	} else {
		top := stacks[from-1][len(stacks[from-1])-num:]              // top element
		stacks[from-1] = stacks[from-1][0 : len(stacks[from-1])-num] // pop
		stacks[to-1] = append(stacks[to-1], top...)
	}

}

func perform_all_moves05(stacks [][]byte, lines []string, one_at_a_time bool) string {
	for _, line := range lines {
		num, from, to := read_move05(line)
		perform_move05(stacks, num, from, to, one_at_a_time)
	}
	sumFunc := func(cur string, next []byte) string { return cur + string([]byte{next[len(next)-1]}) }
	result := Reduce(stacks, "", sumFunc)
	return result
}

func part05a(log Log) string {
	lines := split_lines(input05, false)
	chunks := split_chunks(lines)
	stacks := read05_stacks(chunks[0])
	result := perform_all_moves05(stacks, chunks[1], true)

	return result
}

func part05b(log Log) string {
	lines := split_lines(input05, false)
	chunks := split_chunks(lines)
	stacks := read05_stacks(chunks[0])
	result := perform_all_moves05(stacks, chunks[1], false)

	return result
}

func (t *AOC) Day05(log Log) {
	answer1 := part05a(log)
	fmt.Printf("answer1 := '%v'\n", answer1) // SVFDLGLWV
	answer2 := part05b(log)
	fmt.Printf("answer2 := '%v'\n", answer2) // DCVTCVPCL
}
