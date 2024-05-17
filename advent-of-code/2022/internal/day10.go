package internal

import (
	_ "embed"
	"fmt"

	_ "github.com/stretchr/testify/assert"
)

var (
	//go:embed input/input10.txt
	input10 string
)

func process10_instructions(lines []string) []int {
	x := make([]int, 500)
	curCycle := 1
	x[curCycle-1] = 1
	for lineno, line := range lines {
		tokens := split_line_regex(line, "\x20")
		if tokens[0] == "noop" {
			curCycle++
			x[curCycle-1] = x[curCycle-2]
		} else if tokens[0] == "addx" {
			val := my_atoi(tokens[1])
			curCycle++
			x[curCycle-1] = x[curCycle-2]
			curCycle++
			x[curCycle-1] = x[curCycle-2] + val
		} else {
			panic(fmt.Sprintf("Unknown token '%v' lineno=%v line='%v'", tokens[0], lineno, line))
		}
	}
	return x
}

func process10_crt(lines []string) string {
	x := process10_instructions(lines)
	sprites := make([]byte, 240)
	pixel := 0
	abs := func(v int) int {
		if v < 0 {
			return -v
		}
		return v
	}
	for pixel < 240 {
		sprite_pos := x[pixel]
		if abs(sprite_pos-(pixel%40)) <= 1 {
			sprites[pixel] = '#'
		} else {
			sprites[pixel] = '.'
		}
		pixel++
	}
	output := fmt.Sprintf("%v\n%v\n%v\n%v\n%v\n%v", string(sprites[0:40]), string(sprites[40:80]), string(sprites[80:120]), string(sprites[120:160]), string(sprites[160:200]), string(sprites[200:240]))
	return output
}
func part10a(log Log) int {
	lines := split_lines(input10)
	x := process10_instructions(lines)
	steps := []int{20, 60, 100, 140, 180, 220}
	sumFunc := func(cur int, next int) int { return cur + x[next-1]*next }
	result := Reduce(steps, 0, sumFunc)

	return result
}

func part10b(log Log) string {
	lines := split_lines(input10)
	result := process10_crt(lines)
	return result
}

func (t *AOC) Day10(log Log) {
	answer1 := part10a(log)
	fmt.Printf("answer1 := '%v'\n", answer1) // 16060
	answer2 := part10b(log)
	fmt.Printf("answer2 := \n%v\n", answer2) // BACEKLHF
}
