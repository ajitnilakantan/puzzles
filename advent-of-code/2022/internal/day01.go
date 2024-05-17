package internal

import (
	_ "embed"
	"fmt"

	"github.com/stretchr/testify/assert"
)

var (
	//go:embed input/input01.txt
	input01 string
)

// Simple summation

func get_calories(log Log, input string) []int {
	lines := split_lines(input)
	chunks := split_chunks(lines)
	calories := Map(chunks, func(items []string) []int {
		return Map(items, func(item string) int { return my_atoi(item) })
	})

	sumFunc := func(cur int, next int) int { return cur + next }
	totalCalories := Map(calories, func(item []int) int { return Reduce(item, 0, sumFunc) })
	return totalCalories
}
func part01a(log Log) int {

	totalCalories := get_calories(log, input01)
	_, _, _, maxcalories, _ := MinMaxSlice(totalCalories)
	assert.Equal(log, maxcalories, 72017)
	return maxcalories
}

func part01b(log Log) int {
	totalCalories := get_calories(log, input01)
	totalCalories = Sort(totalCalories)
	assert.GreaterOrEqual(log, len(totalCalories), 3)
	top3 := totalCalories[len(totalCalories)-1] + totalCalories[len(totalCalories)-2] + totalCalories[len(totalCalories)-3]
	assert.Equal(log, top3, 212520)
	return top3
}

func (t *AOC) Day01(log Log) {
	log.Info("Day01\n")
	answer1 := part01a(log)
	fmt.Printf("answer1 := '%v'\n", answer1)
	answer2 := part01b(log)
	fmt.Printf("answer2 := '%v'\n", answer2)
}
