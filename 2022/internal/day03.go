package internal

import (
	_ "embed"
	"fmt"

	_ "github.com/stretchr/testify/assert"
)

var (
	//go:embed input/input03.txt
	input03 string
)

// Solve using set intersections

func get3_priority(b byte) int {
	if b >= 'a' && b <= 'z' {
		return int(b - 'a' + 1)
	} else if b >= 'A' && b <= 'Z' {
		return int(b - 'A' + 27)
	} else {
		return 0
	}
}
func process3_priority(line string) int {
	a := []byte(line[0 : len(line)/2])
	b := []byte(line[len(line)/2:])
	common := MakeSet(a).Intersect(MakeSet(b)).Members()
	sumFunc := func(cur int, next byte) int { return cur + get3_priority(next) }
	return Reduce(common, 0, sumFunc)
}

func process3_priority_triple(line1, line2, line3 string) int {
	b1 := []byte(line1)
	b2 := []byte(line2)
	b3 := []byte(line3)
	common := MakeSet(b1).Intersect(MakeSet(b2)).Intersect(MakeSet(b3)).Members()
	sumFunc := func(cur int, next byte) int { return cur + get3_priority(next) }
	return Reduce(common, 0, sumFunc)
}

func part03a(log Log) int {
	lines := split_lines(input03)
	sumFunc := func(cur int, next string) int { return cur + process3_priority(next) }
	result := Reduce(lines, 0, sumFunc)

	return result
}

func part03b(log Log) int {
	lines := split_lines(input03)
	sum := 0
	for i := 0; i < len(lines); i = i + 3 {
		sum += process3_priority_triple(lines[i], lines[i+1], lines[i+2])
	}
	result := sum
	return result
}

func (t *AOC) Day03(log Log) {
	answer1 := part03a(log)
	fmt.Printf("answer1 := '%v'\n", answer1) // 8105
	answer2 := part03b(log)
	fmt.Printf("answer2 := '%v'\n", answer2) // 2363
}
