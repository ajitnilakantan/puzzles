package internal

import (
	"testing"

	"github.com/stretchr/testify/assert"
)

func TestDay18(t *testing.T) {
	data :=
		`2,2,2
		1,2,2
		3,2,2
		2,1,2
		2,3,2
		2,2,1
		2,2,3
		2,2,4
		2,2,6
		1,2,5
		3,2,5
		2,1,5
		2,3,5`
	lines := split_lines(data)
	result := count18_open_faces(lines)
	assert.Equal(t, 64, result)

	lines = split_lines(data)
	result = count18_outside_faces(lines)
	assert.Equal(t, 58, result)

}
