package internal

import (
	"fmt"
	"testing"

	"github.com/stretchr/testify/assert"
)

func TestDay10(t *testing.T) {
	data :=
		`noop
		addx 3
		addx -5`
	lines := split_lines(data)
	x := process10_instructions(lines)
	assert.Equal(t, []int{1, 1, 1, 4, 4, -1}, x[0:6])
	//t.Logf("x='%v'\n", x)

	data = `addx 15
	addx -11
	addx 6
	addx -3
	addx 5
	addx -1
	addx -8
	addx 13
	addx 4
	noop
	addx -1
	addx 5
	addx -1
	addx 5
	addx -1
	addx 5
	addx -1
	addx 5
	addx -1
	addx -35
	addx 1
	addx 24
	addx -19
	addx 1
	addx 16
	addx -11
	noop
	noop
	addx 21
	addx -15
	noop
	noop
	addx -3
	addx 9
	addx 1
	addx -3
	addx 8
	addx 1
	addx 5
	noop
	noop
	noop
	noop
	noop
	addx -36
	noop
	addx 1
	addx 7
	noop
	noop
	noop
	addx 2
	addx 6
	noop
	noop
	noop
	noop
	noop
	addx 1
	noop
	noop
	addx 7
	addx 1
	noop
	addx -13
	addx 13
	addx 7
	noop
	addx 1
	addx -33
	noop
	noop
	noop
	addx 2
	noop
	noop
	noop
	addx 8
	noop
	addx -1
	addx 2
	addx 1
	noop
	addx 17
	addx -9
	addx 1
	addx 1
	addx -3
	addx 11
	noop
	noop
	addx 1
	noop
	addx 1
	noop
	noop
	addx -13
	addx -19
	addx 1
	addx 3
	addx 26
	addx -30
	addx 12
	addx -1
	addx 3
	addx 1
	noop
	noop
	noop
	addx -9
	addx 18
	addx 1
	addx 2
	noop
	noop
	addx 9
	noop
	noop
	noop
	addx -1
	addx 2
	addx -37
	addx 1
	addx 3
	noop
	addx 15
	addx -21
	addx 22
	addx -6
	addx 1
	noop
	addx 2
	addx 1
	noop
	addx -10
	noop
	noop
	addx 20
	addx 1
	addx 2
	addx 2
	addx -6
	addx -11
	noop
	noop
	noop`

	lines = split_lines(data)
	x = process10_instructions(lines)
	steps := []int{20, 60, 100, 140, 180, 220}
	sumFunc := func(cur int, next int) int { return cur + x[next-1]*next }
	sum := Reduce(steps, 0, sumFunc)
	assert.Equal(t, 13140, sum)

	output := process10_crt(lines)
	fmt.Printf("Output=\n%v\n", output)
}
