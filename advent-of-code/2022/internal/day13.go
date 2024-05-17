package internal

import (
	_ "embed"
	"fmt"
	"sort"
	"strings"

	_ "github.com/stretchr/testify/assert"
)

var (
	//go:embed input/input13.txt
	input13 string
)

type CompareResult string

const (
	Continue     CompareResult = "continue"
	LeftSmaller  CompareResult = "leftSmaller"
	RightSmaller CompareResult = "rightSmaller"
)

func split13_tokens(line string) []string {
	result := make([]string, 0)
	// skip enclosing [] characters
	start, end := 1, 1
	for start < len(line)-1 {
		end = start
		if line[start] >= '0' && line[start] <= '9' {
			for line[end] >= '0' && line[end] <= '9' {
				end++
			}
			result = append(result, line[start:end])
			if line[end] == ',' {
				end++
			}
			start = end
		} else {
			if line[start] != '[' {
				panic(fmt.Sprintf("Cannot parse line '%v' start=%v end=%v", line, start, end))
			}
			end = start + 1
			bracket_count := 1
			for bracket_count > 0 {
				if line[end] == '[' {
					bracket_count++
				} else if line[end] == ']' {
					bracket_count--
				}
				end++
			}
			result = append(result, line[start:end])
			if line[end] == ',' {
				end++
			}
			start = end
		}
	}
	return result
}
func compare13_tokens(left string, right string) CompareResult {
	is_primitive := func(token string) bool { return !strings.HasPrefix(token, "[") }
	if is_primitive(left) && is_primitive(right) {
		if my_atoi(left) == my_atoi(right) {
			return Continue
		} else if my_atoi(left) < my_atoi(right) {
			return LeftSmaller
		} else {
			return RightSmaller
		}
	} else if is_primitive(left) && !is_primitive(right) {
		return compare13_tokens("["+left+"]", right)
	} else if !is_primitive(left) && is_primitive(right) {
		return compare13_tokens(left, "["+right+"]")
	} else {
		left_tokens := split13_tokens(left)
		right_tokens := split13_tokens(right)
		index := 0
		for index = 0; index < len(left_tokens) && index < len(right_tokens); index++ {
			ret := compare13_tokens(left_tokens[index], right_tokens[index])
			if ret == Continue {
				continue
			} else {
				return ret
			}
		}
		if len(left_tokens) == len(right_tokens) {
			return Continue
		} else if len(left_tokens) < len(right_tokens) {
			return LeftSmaller
		} else {
			return RightSmaller
		}
	}
}

func part13a(log Log) int {
	lines := split_lines(input13)
	chunks := split_chunks(lines)
	result := 0
	for index, pair := range chunks {
		if compare13_tokens(pair[0], pair[1]) == LeftSmaller {
			result += index + 1
		}
	}

	return result
}

func part13b(log Log) int {
	lines := split_lines(input13)
	all_lines := make([]string, 0, len(lines))
	for _, line := range lines {
		if line != "" {
			all_lines = append(all_lines, line)
		}
	}
	all_lines = append(all_lines, "[[2]]")
	all_lines = append(all_lines, "[[6]]")
	sort.Slice(all_lines, func(i, j int) bool {
		return compare13_tokens(all_lines[i], all_lines[j]) == LeftSmaller
	})

	result := (IndexOf(all_lines, "[[2]]") + 1) * (IndexOf(all_lines, "[[6]]") + 1)

	return result
}

func (t *AOC) Day13(log Log) {
	answer1 := part13a(log)
	fmt.Printf("answer1 := '%v'\n", answer1) // 6656
	answer2 := part13b(log)
	fmt.Printf("answer2 := '%v'\n", answer2) // 19716
}
