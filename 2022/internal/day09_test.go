package internal

import (
	"testing"

	"github.com/stretchr/testify/assert"
)

func TestDay09(t *testing.T) {
	data :=
		`R 4
		U 4
		L 3
		D 1
		R 4
		D 1
		L 5
		R 2`
	tail := coord09{1, 1}
	head := coord09{1, 2}
	tail = move09_tail(tail, head)
	assert.Equal(t, coord09{1, 1}, tail)
	head = coord09{1, 3}
	tail = move09_tail(tail, head)
	assert.Equal(t, coord09{1, 2}, tail)

	tail = coord09{3, 1}
	head = coord09{2, 2}
	tail = move09_tail(tail, head)
	assert.Equal(t, coord09{3, 1}, tail)
	head = coord09{1, 2}
	tail = move09_tail(tail, head)
	assert.Equal(t, coord09{2, 2}, tail)

	lines := split_lines(data)
	count := count09_moves(lines)
	assert.Equal(t, 13, count)

	data = `R 5
	U 8
	L 8
	D 3
	R 17
	D 10
	L 25
	U 20`
	lines = split_lines(data)
	count = count09_rope_moves(lines)
	assert.Equal(t, 36, count)
}
