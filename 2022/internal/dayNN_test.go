package internal

import (
	"testing"

	"github.com/stretchr/testify/assert"
)

func TestDayNN(t *testing.T) {
	data :=
		`abc
		xyz`
	assert.Equal(t, 2, len(split_lines(data)))
}
