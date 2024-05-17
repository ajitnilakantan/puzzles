package internal

import (
	"testing"

	"github.com/stretchr/testify/assert"
)

var log = GetLogger()

func TestDay01a(t *testing.T) {
	data :=
		`1000
2000
3000

4000

5000
6000

7000
8000
9000

10000`

	lines := split_lines(data)
	//fmt.Printf("Data = '%v'\n", lines)
	//t.Logf("data = '%v'\n", data)
	chunks := split_chunks(lines)
	//t.Logf("chunks = '%+v'\n", chunks)

	calories := Map(chunks, func(items []string) []int {
		return Map(items, func(item string) int { return my_atoi(item) })
	})

	t.Logf("calories = '%+v'\n", calories)
	sumFunc := func(cur int, next int) int { return cur + next }
	totalCalories := Map(calories, func(item []int) int { return Reduce(item, 0, sumFunc) })
	t.Logf("totalCalories = '%+v'\n", totalCalories)
	_, _, _, maxcalories, err := MinMaxSlice(totalCalories)
	assert.NoError(t, err)
	assert.Equal(t, 24000, maxcalories)
	log.Info("Done\n")
}
