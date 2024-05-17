package internal

import (
	"fmt"
	"testing"

	"github.com/stretchr/testify/assert"
)

func TestDay05(t *testing.T) {
	data :=
		`    [D]    
[N] [C]    
[Z] [M] [P]
 1   2   3 

move 1 from 2 to 1
move 3 from 1 to 3
move 2 from 2 to 1
move 1 from 1 to 2`

	lines := split_lines(data, false)
	chunks := split_chunks(lines)
	for _, l := range lines {
		t.Logf("'%v'\n", l)
	}

	stacks := read05_stacks(chunks[0])
	t.Logf("Stacks=%v\n", stacks_tostring05(stacks))
	assert.Equal(t, 2, len(chunks))
	// num, from, to := read_move05(chunks[1][0])
	// fmt.Printf("line = %v n, f, t = %v %v %v\n", chunks[1][0], num, from, to)
	result := perform_all_moves05(stacks, chunks[1], true)
	fmt.Printf("result one at a time= %v\n", result)
	assert.Equal(t, "CMZ", result)

	stacks = read05_stacks(chunks[0])
	result = perform_all_moves05(stacks, chunks[1], false)
	fmt.Printf("result many at a time = %v\n", result)
	assert.Equal(t, "MCD", result)
}
