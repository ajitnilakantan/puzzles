package internal

import (
	"testing"

	"github.com/stretchr/testify/assert"
)

func TestDay14(t *testing.T) {
	data :=
		`498,4 -> 498,6 -> 496,6
		503,4 -> 502,4 -> 502,9 -> 494,9`

	segment := interval{from: 10, to: 20}
	wall := interval{from: 10, to: 20}
	intersect, is_changed := split14_segment(segment, wall)
	assert.Equal(t, []interval{}, intersect)
	assert.Equal(t, true, is_changed)

	wall = interval{from: 10, to: 10}
	intersect, is_changed = split14_segment(segment, wall)
	assert.Equal(t, []interval{{from: 11, to: 20}}, intersect)
	assert.Equal(t, true, is_changed)

	wall = interval{from: 20, to: 30}
	intersect, is_changed = split14_segment(segment, wall)
	assert.Equal(t, []interval{{from: 10, to: 19}}, intersect)
	assert.Equal(t, true, is_changed)

	wall = interval{from: 20, to: 20}
	intersect, is_changed = split14_segment(segment, wall)
	assert.Equal(t, []interval{{from: 10, to: 19}}, intersect)
	assert.Equal(t, true, is_changed)

	wall = interval{from: 19, to: 19}
	intersect, is_changed = split14_segment(segment, wall)
	assert.Equal(t, []interval{{from: 10, to: 18}, {from: 20, to: 20}}, intersect)
	assert.Equal(t, true, is_changed)

	wall = interval{from: 19, to: 22}
	intersect, is_changed = split14_segment(segment, wall)
	assert.Equal(t, []interval{{from: 10, to: 18}}, intersect)
	assert.Equal(t, true, is_changed)

	//
	lines := split_lines(data)
	hsegments, vsegments, minRow, maxRow, minCol, maxCol := read14_segments(lines)
	//fmt.Printf("h=%+v v=%+v   min,maxRow=%v, %v  min,maxCol=%v, %v\n", hsegments, vsegments, minRow, maxRow, minCol, maxCol)
	columns := get14_columns(hsegments, vsegments, minRow, maxRow, minCol, maxCol)
	//fmt.Printf("columns=%+v\n", columns)
	count := 0
	for drop14_ball(0, 500, columns, minCol, maxCol, maxRow) {
		count++
	}
	// fmt.Printf("count = %v\n", count)
	assert.Equal(t, 24, count)

	// part b
	lines = split_lines(data)
	hsegments, vsegments, minRow, maxRow, minCol, maxCol = read14_segments(lines)

	// Add floor
	extend := (maxCol - minCol) * (maxCol - minCol)
	hsegments = append(hsegments, hsegment{row: maxRow + 2, fromCol: minCol - extend, toCol: maxCol + extend})
	maxRow += 2
	minCol -= extend
	maxCol += extend
	columns = get14_columns(hsegments, vsegments, minRow, maxRow, minCol, maxCol)
	count = 0
	for drop14_ball(0, 500, columns, minCol, maxCol, maxRow) {
		count++
	}
	// fmt.Printf("Part b count = %v\n", count)
	assert.Equal(t, 93, count)

}
