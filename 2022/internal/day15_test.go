package internal

import (
	"testing"

	"github.com/stretchr/testify/assert"
)

func TestDay15(t *testing.T) {
	data :=
		`Sensor at x=2, y=18: closest beacon is at x=-2, y=15
		Sensor at x=9, y=16: closest beacon is at x=10, y=16
		Sensor at x=13, y=2: closest beacon is at x=15, y=3
		Sensor at x=12, y=14: closest beacon is at x=10, y=16
		Sensor at x=10, y=20: closest beacon is at x=10, y=16
		Sensor at x=14, y=17: closest beacon is at x=10, y=16
		Sensor at x=8, y=7: closest beacon is at x=2, y=10
		Sensor at x=2, y=0: closest beacon is at x=2, y=10
		Sensor at x=0, y=11: closest beacon is at x=2, y=10
		Sensor at x=20, y=14: closest beacon is at x=25, y=17
		Sensor at x=17, y=20: closest beacon is at x=21, y=22
		Sensor at x=16, y=7: closest beacon is at x=15, y=3
		Sensor at x=14, y=3: closest beacon is at x=15, y=3
		Sensor at x=20, y=1: closest beacon is at x=15, y=3`
	var a, b segment15
	a = segment15{from: 10, to: 20}

	b = segment15{from: 10, to: 20}
	assert.Equal(t, 11, intersect15_size(a, b))

	b = segment15{from: 10, to: 30}
	assert.Equal(t, 11, intersect15_size(a, b))

	b = segment15{from: -10, to: 20}
	assert.Equal(t, 11, intersect15_size(a, b))

	b = segment15{from: -10, to: 15}
	assert.Equal(t, 6, intersect15_size(a, b))

	b = segment15{from: 15, to: 30}
	assert.Equal(t, 6, intersect15_size(a, b))

	b = segment15{from: 15, to: 15}
	assert.Equal(t, 1, intersect15_size(a, b))

	lines := split_lines(data)
	// Skip the first blank
	// tokens := split_line_regex(lines[0], "[A-Za-z=,\x20\\:]+")[1:]
	// fmt.Printf("con = %v = '%+v'\n", len(tokens), tokens)
	sensors := read15_data(lines)
	result := get15_segments_measure(10, sensors) // count row 10
	assert.Equal(t, 26, result)

	emptyRow, emptyCol := get15_empty_space(sensors, 20)
	assert.Equal(t, 11, emptyRow)
	assert.Equal(t, 14, emptyCol)
	result = 4000000*emptyCol + emptyRow
	assert.Equal(t, 56000011, result)
}
