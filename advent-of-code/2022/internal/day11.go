package internal

import (
	_ "embed"
	"fmt"
	"reflect"
	"strings"

	_ "github.com/stretchr/testify/assert"
)

var (
	//go:embed input/input11.txt
	input11 string
)

type monkey11 struct {
	items     []int
	operation operation11func
	divby     int
	iftrue    int
	iffalse   int
}

type operation11func func(int) int

type operation_type int

const (
	multiply11 operation_type = iota
	addition11
)

func read11_monkeys(chunks [][]string) []monkey11 {
	getoperation := func(op operation_type, val int) operation11func {
		if op == multiply11 {
			return func(x int) int {
				if val == 0 {
					return x * x
				}
				return x * val
			}
		} else {
			return func(x int) int { return x + val }
		}
	}

	monkeys := make([]monkey11, 0)
	for _, lines := range chunks {
		monkey := monkey11{}
		assertTokens := func(line string, tokens []string, vals ...string) {
			if !reflect.DeepEqual(tokens[0:len(vals)], vals) {
				panic(fmt.Sprintf("bad line: '%v' no match to %v", line, vals))
			}
		}
		// header
		if !strings.HasPrefix(lines[0], "Monkey") {
			panic(fmt.Sprintf("bad line0: '%v'", lines[0]))
		}
		// starting items
		tokens := split_line_regex(lines[1], "[\\s,]+")
		assertTokens(lines[1], tokens, "Starting", "items:")
		for i := 2; i < len(tokens); i++ {
			monkey.items = append(monkey.items, my_atoi(tokens[i]))
		}
		// operation
		tokens = split_line_regex(lines[2], "[\\s]+")
		assertTokens(lines[2], tokens, "Operation:", "new", "=")
		if tokens[4] == "*" {
			val := 0
			if tokens[5] != "old" {
				val = my_atoi(tokens[5])
			}
			monkey.operation = getoperation(multiply11, val)
		} else {
			val := my_atoi(tokens[5])
			monkey.operation = getoperation(addition11, val)
		}
		// test div by
		tokens = split_line_regex(lines[3], "[\\s]+")
		assertTokens(lines[3], tokens, "Test:", "divisible", "by")
		monkey.divby = my_atoi(tokens[3])
		// if true
		tokens = split_line_regex(lines[4], "[\\s]+")
		assertTokens(lines[4], tokens, "If", "true:", "throw", "to", "monkey")
		monkey.iftrue = my_atoi(tokens[5])
		// if false
		tokens = split_line_regex(lines[5], "[\\s]+")
		assertTokens(lines[5], tokens, "If", "false:", "throw", "to", "monkey")
		monkey.iffalse = my_atoi(tokens[5])

		// Add to list
		monkeys = append(monkeys, monkey)
	}

	return monkeys
}

func simulate11_rounds(monkeys []monkey11, numRounds int, worryFactor int) []int {
	inspection_count := make([]int, len(monkeys))
	multFunction := func(cur int, next monkey11) int { return cur * next.divby }
	modulo := Reduce(monkeys, 1, multFunction)
	for round := 0; round < numRounds; round++ {
		for monkeyNum, monkey := range monkeys {
			for _, item := range monkey.items {
				val := monkey.operation(item)
				if worryFactor == 0 {
					val = val % modulo
				} else {
					val /= worryFactor
				}
				if val%monkey.divby == 0 {
					monkeys[monkey.iftrue].items = append(monkeys[monkey.iftrue].items, val)
				} else {
					monkeys[monkey.iffalse].items = append(monkeys[monkey.iffalse].items, val)
				}
				inspection_count[monkeyNum]++
			}
			// Clear
			monkeys[monkeyNum].items = make([]int, 0)
		}
	}
	return inspection_count
}

func part11a(log Log) int {
	lines := split_lines(input11)
	chunks := split_chunks(lines)
	monkeys := read11_monkeys(chunks)
	val := simulate11_rounds(monkeys, 20, 3)
	val = Reverse(Sort(val))
	result := val[0] * val[1]

	return result
}

func part11b(log Log) int {
	lines := split_lines(input11)
	chunks := split_chunks(lines)
	monkeys := read11_monkeys(chunks)
	val := simulate11_rounds(monkeys, 10000, 0)
	val = Reverse(Sort(val))
	result := val[0] * val[1]
	return result
}

func (t *AOC) Day11(log Log) {
	answer1 := part11a(log)
	fmt.Printf("answer1 := '%v'\n", answer1) // 78678
	answer2 := part11b(log)
	fmt.Printf("answer2 := '%v'\n", answer2) // 15333249714
}
