package internal

import (
	_ "embed"
	"fmt"

	_ "github.com/stretchr/testify/assert"
)

var (
	//go:embed input/input02.txt
	input02 string
)

// A,B,C = Rock/Paper/Scissors
var rps_val map[byte]int = map[byte]int{'A': 1, 'B': 2, 'C': 3}

func score_rps(a byte, b byte) int {
	if a == b {
		// Tie
		return 3 + rps_val[b]
	} else if a == 'A' && b == 'C' || a == 'B' && b == 'A' || a == 'C' && b == 'B' {
		// Lose
		return 0 + rps_val[b]
	} else {
		// Win
		return 6 + rps_val[b]
	}
}

func score_rps_game1(lines []string, xyz_to_abc map[byte]byte) int {
	score := 0
	for _, line := range lines {
		var a byte = line[0]
		var b byte = line[2]
		score += score_rps(a, xyz_to_abc[b])
	}
	return score
}

func get_rps_op(opt byte, a byte) byte {
	if opt == 'X' {
		// lose
		lose := map[byte]byte{'A': 'C', 'B': 'A', 'C': 'B'}
		return lose[a]
	} else if opt == 'Y' {
		// draw
		return a
	} else {
		// win
		win := map[byte]byte{'A': 'B', 'B': 'C', 'C': 'A'}
		return win[a]
	}
}

func score_rps_game2(lines []string) int {
	score := 0
	for _, line := range lines {
		var a byte = line[0]
		var b byte = line[2]
		score += score_rps(a, get_rps_op(b, a))
	}
	return score
}

func part02a(log Log) int {
	xyz_to_abc := map[byte]byte{'X': 'A', 'Y': 'B', 'Z': 'C'}
	lines := split_lines(input02)

	return score_rps_game1(lines, xyz_to_abc)
}

func part02b(log Log) int {
	lines := split_lines(input02)

	return score_rps_game2(lines)
}

func (t *AOC) Day02(log Log) {
	log.Info("Day02\n")
	answer1 := part02a(log)
	fmt.Printf("answer1 := '%v'\n", answer1) // 10404
	answer2 := part02b(log)
	fmt.Printf("answer2 := '%v'\n", answer2) // 10334
}
