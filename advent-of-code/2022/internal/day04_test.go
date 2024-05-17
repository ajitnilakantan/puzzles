package internal

import (
	"testing"

	"github.com/stretchr/testify/assert"
)

func TestDay04(t *testing.T) {
	data :=
		`2-4,6-8
		2-3,4-5
		5-7,7-9
		2-8,3-7
		6-6,4-6
		2-6,4-8`
	lines := split_lines(data)
	result := count04_contained(lines)
	assert.Equal(t, 2, result)
	result = count04_overlapped(lines)
	assert.Equal(t, 4, result)
}
