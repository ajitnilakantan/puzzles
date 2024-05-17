package internal

import (
	"strings"
	"testing"

	"github.com/stretchr/testify/assert"
)

func TestDay19(t *testing.T) {
	data :=
		`Blueprint 1:
		Each ore robot costs 4 ore.
		Each clay robot costs 2 ore.
		Each obsidian robot costs 3 ore and 14 clay.
		Each geode robot costs 2 ore and 7 obsidian.
	  
	  Blueprint 2:
		Each ore robot costs 2 ore.
		Each clay robot costs 3 ore.
		Each obsidian robot costs 3 ore and 8 clay.
		Each geode robot costs 3 ore and 12 obsidian.`

	chunks := split_chunks(split_lines(data))
	lines := Map(chunks, func(x []string) string { return strings.Join(x, " ") })
	// fmt.Printf("len = %v '%v'  '%v'\n", len(lines), lines[0], lines[1])
	blueprints := read19_blueprints(lines)
	// fmt.Printf("BP = %+v\n", blueprints[0])

	num_geodes := run19_blueprint(blueprints[0], 24)
	assert.Equal(t, 9, num_geodes)

	// sum up "quality levels"
	result := 0
	for index, blueprint := range blueprints {
		result += (index + 1) * run19_blueprint(blueprint, 24)
	}
	assert.Equal(t, 33, result)

	// These tests takes a long time to run
	// part b
	// num_geodes = run19_blueprint(blueprints[0], 32)
	// assert.Equal(t, 56, num_geodes)
	// num_geodes = run19_blueprint(blueprints[1], 32)
	// assert.Equal(t, 62, num_geodes)
}
