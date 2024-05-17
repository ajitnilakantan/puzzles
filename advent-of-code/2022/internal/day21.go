package internal

import (
	_ "embed"
	"fmt"
	"math"

	_ "github.com/stretchr/testify/assert"
)

var (
	//go:embed input/input21.txt
	input21 string
)

func read21_data(lines []string) (dict map[string][]string) {
	dict = make(map[string][]string)
	for _, line := range lines {
		tokens := split_line_regex(line, "\x20")
		dict[tokens[0][0:len(tokens[0])-1]] = tokens[1:]
	}
	return dict
}

func value21(dict map[string][]string, node string) int {
	node_val := dict[node]
	if len(node_val) == 1 {
		return my_atoi(node_val[0])
	} else {
		lhs := value21(dict, node_val[0])
		op := node_val[1]
		rhs := value21(dict, node_val[2])
		switch op {
		case "+":
			return lhs + rhs
		case "-":
			return lhs - rhs
		case "*":
			return lhs * rhs
		case "/":
			return lhs / rhs
		default:
			panic(fmt.Sprintf("unexpected: op = '%v'", op))
		}
	}
}

func bisect21_solve(dict map[string][]string) int {
	abs := func(v int) int {
		if v < 0 {
			return -v
		} else {
			return v
		}
	}

	// Find out which side is fixed
	dict["humn"] = []string{"0"}
	l1 := value21(dict, dict["root"][0])
	r1 := value21(dict, dict["root"][2])
	dict["humn"] = []string{"1024"}
	l2 := value21(dict, dict["root"][0])
	// r2 := value21(dict, dict["root"][2])

	var side int
	var fixed int
	if l1 == l2 {
		// lhs has a constant value
		fixed = l1
		side = 2 // rhs
	} else {
		// rhs has a constant value
		fixed = r1
		side = 0 // lhs
	}

	// Initial guess of interval for search. Scale down to avoid overflow
	left := math.MinInt64 / (1024 * 1024)
	right := math.MaxInt64 / (1024 * 1024)

	for right-left > 0 {
		left_third := left + (right-left)/3
		right_third := right - (right-left)/3

		dict["humn"] = []string{fmt.Sprintf("%v", left_third)}
		f_left_third := abs(value21(dict, dict["root"][side]) - fixed)
		dict["humn"] = []string{fmt.Sprintf("%v", right_third)}
		f_right_third := abs(value21(dict, dict["root"][side]) - fixed)

		if f_left_third > f_right_third {
			// Advance left to first third
			if left == left_third {
				left++
			} else {
				left = left_third
			}
		} else if f_left_third < f_right_third {
			// Move back right to second third
			if right == right_third {
				right--
			} else {
				right = right_third
			}
		} else {
			// Between first and second thirds
			if left == left_third {
				left++
			} else {
				left = left_third
			}
			if right == right_third {
				right--
			} else {
				right = right_third
			}
		}
	}

	return (left + right) / 2
}

func part21a(log Log) int {
	lines := split_lines(input21)
	dict := read21_data(lines)
	result := value21(dict, "root")
	return result
}

func part21b(log Log) int {
	lines := split_lines(input21)
	dict := read21_data(lines)
	result := bisect21_solve(dict)
	return result
}

func (t *AOC) Day21(log Log) {
	answer1 := part21a(log)
	fmt.Printf("answer1 := '%v'\n", answer1) // 379578518396784
	answer2 := part21b(log)
	fmt.Printf("answer2 := '%v'\n", answer2) // 3353687996514
}
