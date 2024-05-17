package internal

import (
	_ "embed"
	"fmt"

	_ "github.com/stretchr/testify/assert"
)

var (
	//go:embed input/input02.txt
	inputNN string
)

func partNNa(log Log) int {
	lines := split_lines(inputNN)
	result := len(lines)

	return result
}

func partNNb(log Log) int {
	lines := split_lines(inputNN)
	result := len(lines)
	return result
}

func (t *AOC) DayNN(log Log) {
	answer1 := partNNa(log)
	fmt.Printf("answer1 := '%v'\n", answer1) //
	answer2 := partNNb(log)
	fmt.Printf("answer2 := '%v'\n", answer2) //
}
