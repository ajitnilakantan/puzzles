package internal

import (
	_ "embed"
	"fmt"

	_ "github.com/stretchr/testify/assert"
)

var (
	//go:embed input/input04.txt
	input04 string
)

func count04_contained(lines []string) int {
	count := 0
	for _, line := range lines {
		s := split_line_regex(line, "[-,]")
		a0 := my_atoi(s[0])
		a1 := my_atoi(s[1])
		b0 := my_atoi(s[2])
		b1 := my_atoi(s[3])
		if b0 >= a0 && b0 <= a1 && b1 >= a0 && b1 <= a1 || a0 >= b0 && a0 <= b1 && a1 >= b0 && a1 <= b1 {
			count += 1
		}
	}
	return count
}

func count04_overlapped(lines []string) int {
	count := 0
	for _, line := range lines {
		s := split_line_regex(line, "[-,]")
		a0 := my_atoi(s[0])
		a1 := my_atoi(s[1])
		b0 := my_atoi(s[2])
		b1 := my_atoi(s[3])
		if b0 >= a0 && b0 <= a1 || b1 >= a0 && b1 <= a1 || a0 >= b0 && a0 <= b1 || a1 >= b0 && a1 <= b1 {
			count += 1
		}
	}
	return count
}

func part04a(log Log) int {
	lines := split_lines(input04)
	result := count04_contained(lines)

	return result
}

func part04b(log Log) int {
	lines := split_lines(input04)
	result := count04_overlapped(lines)
	return result
}

func (t *AOC) Day04(log Log) {
	answer1 := part04a(log)
	fmt.Printf("answer1 := '%v'\n", answer1) // 441
	answer2 := part04b(log)
	fmt.Printf("answer2 := '%v'\n", answer2) // 861
}
