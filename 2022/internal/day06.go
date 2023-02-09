package internal

import (
	_ "embed"
	"fmt"

	_ "github.com/stretchr/testify/assert"
)

var (
	//go:embed input/input06.txt
	input06 string
)

func count06_header_offset(line string, sequence_length int) int {
	data := []byte(line)
	for i := 0; i < len(data)-sequence_length; i++ {
		s := MakeSet(data[i : i+sequence_length])
		if len(s) == sequence_length {
			// Add sequence_length for the sequence length
			return i + sequence_length
		}
	}

	return -1
}

func part06a(log Log) int {
	lines := split_lines(input06)
	result := count06_header_offset(lines[0], 4)

	return result
}

func part06b(log Log) int {
	lines := split_lines(input06)
	result := count06_header_offset(lines[0], 14)
	return result
}

func (t *AOC) Day06(log Log) {
	answer1 := part06a(log)
	fmt.Printf("answer1 := '%v'\n", answer1) // 1855
	answer2 := part06b(log)
	fmt.Printf("answer2 := '%v'\n", answer2) // 3256
}
