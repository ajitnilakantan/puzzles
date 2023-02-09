package internal

import (
	"testing"

	"github.com/stretchr/testify/assert"
)

func TestDay20(t *testing.T) {
	data :=
		`1
		2
		-3
		3
		-2
		0
		4`
	lines := split_lines(data)
	result := solve20a(lines, 1, 1)
	assert.Equal(t, 3, result)

	/*
		lines = []string{"3", "-5", "0"}
		fmt.Printf("%3.3v: %+v\n", "", lines)
		for i := -5; i <= 5; i++ {
			lines = []string{"3", "-5", "0"}
			lines[1] = fmt.Sprintf("%v", i)
			state := read20_input(lines)
			fmt.Printf("%3.3v: %+v -> ", state.data[1], state.sl.Values())
			shuffle20_data(state, 1)
			fmt.Printf("%+v\n", state.sl.Values())
		}
		fmt.Printf("----\n")
	*/

	lines = split_lines(data)
	state := read20_input(lines, 1)
	for i := 0; i < len(lines); i++ {
		//fmt.Printf("%3.3v: %+v -> ", state.data[i], state.sl.Values())
		shuffle20_data(state, i)
		//fmt.Printf("%+v\n", state.sl.Values())
	}
}
