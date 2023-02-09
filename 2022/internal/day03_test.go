package internal

import (
	"testing"

	"github.com/stretchr/testify/assert"
)

func TestDay03(t *testing.T) {
	data :=
		`vJrwpWtwJgWrhcsFMMfFFhFp
		jqHRNqRjqzjGDLGLrsFMfFZSrLrFZsSL
		PmmdzqPrVvPwwTWBwg
		wMqvLMZHhHMvwLHjbvcjnnSBnvTQFn
		ttgJtRGJQctTZtZT
		CrZsJsPPZsGzwwsLwLmpwMDw`

	lines := split_lines(data)
	sumFunc := func(cur int, next string) int { return cur + process3_priority(next) }
	result := Reduce(lines, 0, sumFunc)

	assert.Equal(t, result, 157)

	sum := 0
	for i := 0; i < len(lines); i = i + 3 {
		sum += process3_priority_triple(lines[i], lines[i+1], lines[i+2])
	}
	assert.Equal(t, 70, sum)
}
