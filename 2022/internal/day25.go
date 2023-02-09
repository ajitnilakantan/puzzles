package internal

import (
	_ "embed"
	"fmt"

	_ "github.com/stretchr/testify/assert"
)

var (
	//go:embed input/input25.txt
	input25 string
)

// Calculat x**n  Assume n >= 0
func powInt(x, n int) int {
	if n < 0 {
		panic("n<0")
	}
	if n == 0 {
		return 1
	}
	if n == 1 {
		return x
	}
	y := powInt(x, n/2)
	if n%2 == 0 {
		return y * y
	}
	return x * y * y
}

var sdig map[byte]int = map[byte]int{'0': 0, '1': 1, '2': 2, '-': -1, '=': -2}

func snafu_to_decimal(snafu string) int {
	result := 0
	for i := 0; i < len(snafu); i++ {
		s := snafu[len(snafu)-1-i]
		result += sdig[s] * powInt(5, i)
	}
	return result
}

func decimal_to_snafu(n int) string {
	if n == 0 {
		return ""
	}
	switch n % 5 {
	case 0:
		return decimal_to_snafu(n/5) + "0"
	case 1:
		return decimal_to_snafu(n/5) + "1"
	case 2:
		return decimal_to_snafu(n/5) + "2"
	case 3:
		return decimal_to_snafu((n+2)/5) + "="
	case 4:
		return decimal_to_snafu((n+1)/5) + "-"
	}
	return "" // notreached
}

func part25a(log Log) string {
	lines := split_lines(input25)
	sum := Reduce(Map(lines, snafu_to_decimal), 0, func(acc int, val int) int { return acc + val })
	result := decimal_to_snafu(sum)

	return result
}

func part25b(log Log) int {
	lines := split_lines(input25)
	result := len(lines)
	return result
}

func (t *AOC) Day25(log Log) {
	answer1 := part25a(log)
	fmt.Printf("answer1 := '%v'\n", answer1) //
	answer2 := part25b(log)
	fmt.Printf("answer2 := '%v'\n", answer2) //
}
