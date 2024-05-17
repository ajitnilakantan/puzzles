package internal

import (
	_ "embed"
	"fmt"

	_ "github.com/stretchr/testify/assert"
)

var (
	//go:embed input/input14.txt
	input14 string
)

type hsegment struct {
	row            int
	fromCol, toCol int // inclusive interval
}
type vsegment struct {
	col            int
	fromRow, toRow int // inclusive interval
}

// Read the data- return horizontal and vertical segments + bounds of segments
func read14_segments(lines []string) ([]hsegment, []vsegment, int, int, int, int) {
	hsegments := make([]hsegment, 0)
	vsegments := make([]vsegment, 0)
	minRow, maxRow := 99999, -1
	minCol, maxCol := 99999, -1

	// Read the line segments
	for _, line := range lines {
		tokens := Map(split_line_regex(line, "[\x20\\->,]+"), my_atoi)
		fromRow, fromCol := 0, 0
		toRow, toCol := 0, 0

		minVal := func(a, b int) int {
			if a < b {
				return a
			}
			return b
		}
		maxVal := func(a, b int) int {
			if a > b {
				return a
			}
			return b
		}
		for index := 0; index < len(tokens); index += 2 {
			toRow, toCol = tokens[index+1], tokens[index]
			minCol = minVal(minCol, toCol)
			maxCol = maxVal(maxCol, toCol)
			minRow = minVal(minRow, toRow)
			maxRow = maxVal(maxRow, toRow)
			if index == 0 {
				fromRow, fromCol = toRow, toCol
				continue
			}
			if toRow == fromRow {
				f := minVal(fromCol, toCol)
				t := maxVal(fromCol, toCol)
				hsegments = append(hsegments, hsegment{row: fromRow, fromCol: f, toCol: t})
			} else if toCol == fromCol {
				f := minVal(fromRow, toRow)
				t := maxVal(fromRow, toRow)
				vsegments = append(vsegments, vsegment{col: fromCol, fromRow: f, toRow: t})
			}
			fromRow, fromCol = toRow, toCol
		}
	}

	/* 	// Sort the segments by height
	   	sort.Slice(hsegments, func(i, j int) bool {
	   		return hsegments[i].row < hsegments[j].row
	   	})
	   	sort.Slice(vsegments, func(i, j int) bool {
	   		return vsegments[i].fromRow < vsegments[j].toRow
	   	}) */
	return hsegments, vsegments, minRow, maxRow, minCol, maxCol
}

// Inclusinve interval [from, to]
type interval struct {
	from, to int
}

// Split an interval -- return split interval(s) + bool if something did happen
func split14_segment(segment interval, wall interval) ([]interval, bool) {
	if wall.to < wall.from {
		// degenerate wall
		return []interval{segment}, false
	} else if wall.from > segment.to || wall.to < segment.from {
		// No intersection
		return []interval{segment}, false
	} else if wall.from <= segment.from && wall.to >= segment.to {
		// Segment fully covered
		return make([]interval, 0), true
	} else {
		top := interval{from: segment.from, to: wall.from - 1}
		bottom := interval{from: wall.to + 1, to: segment.to}
		ret := make([]interval, 0)
		if top.to >= top.from {
			ret = append(ret, top)
		}
		if bottom.to >= bottom.from {
			ret = append(ret, bottom)
		}
		return ret, true
	}
}

// For each column, create a list of empty spaces [start, end)
func split14_column(col int, hsegments []hsegment, vsegments []vsegment, minRow int, maxRow int) Set[interval] {
	ret := make(Set[interval])
	ret.Add(interval{from: 0, to: maxRow + 10})

	dirty := true
	for dirty {
		dirty = false
		for segment := range ret {
			for _, h := range hsegments {
				if col >= h.fromCol && col <= h.toCol && segment.from <= h.row && segment.to >= h.row {
					dirty = true
					ret.Remove(segment)
					split_segments, _ := split14_segment(segment, interval{from: h.row, to: h.row})
					for _, s := range split_segments {
						ret.Add(s)
					}
				}
			}
			for _, v := range vsegments {
				if col == v.col {
					split_segments, is_changed := split14_segment(segment, interval{from: v.fromRow, to: v.toRow})
					if is_changed {
						dirty = true
						ret.Remove(segment)
						for _, s := range split_segments {
							ret.Add(s)
						}
					}
				}
			}
		}
	}
	return ret
}

// Convert the data- (horizontal and vertical segments + bounds of segments) to columns
// of free space
func get14_columns(hsegments []hsegment, vsegments []vsegment, minRow, maxRow, minCol, maxCol int) map[int]Set[interval] {
	ret := make(map[int]Set[interval])
	for col := minCol; col <= maxCol; col++ {
		ret[col] = split14_column(col, hsegments, vsegments, minRow, maxRow)
	}
	// Add empty columns before min/max
	ret[minCol-1] = MakeSet([]interval{{from: 0, to: maxRow + 10}})
	ret[maxCol+1] = MakeSet([]interval{{from: 0, to: maxRow + 10}})
	return ret
}

// Get the interval corresponding to row,col.  Return false if not found.
func get14_interval(row int, col int, columns map[int]Set[interval]) (interval, bool) {
	for segment := range columns[col] {
		if row >= segment.from && row <= segment.to {
			return segment, true
		}
	}
	return interval{}, false
}

// Drop a ball at row,col.  Return true if ball falls within bounds, otherwise false
// Update the map of free segments for each column
func drop14_ball(row int, col int, columns map[int]Set[interval], minCol int, maxCol int, maxRow int) bool {
	segment, found := get14_interval(row, col, columns)
	if !found {
		// No place
		return false
	}

	row = segment.to // Place on bottom

	if col < minCol || col > maxCol || row > maxRow {
		// Out of bounds
		return false
	}

	if _, found = get14_interval(row+1, col-1, columns); found {
		// Look left
		return drop14_ball(row+1, col-1, columns, minCol, maxCol, maxRow)
	} else if _, found = get14_interval(row+1, col+1, columns); found {
		// Look right
		return drop14_ball(row+1, col+1, columns, minCol, maxCol, maxRow)
	} else {
		// Drop in current row.
		segments := columns[col]
		segments.Remove(segment)
		segment.to--
		if segment.to >= segment.from {
			segments.Add(segment)
		}

		columns[col] = segments
		return true
	}
}

func part14a(log Log) int {
	lines := split_lines(input14)

	hsegments, vsegments, minRow, maxRow, minCol, maxCol := read14_segments(lines)
	columns := get14_columns(hsegments, vsegments, minRow, maxRow, minCol, maxCol)

	count := 0
	for drop14_ball(0, 500, columns, minCol, maxCol, maxRow) {
		count++
	}
	result := count

	return result
}

func part14b(log Log) int {
	lines := split_lines(input14)

	hsegments, vsegments, minRow, maxRow, minCol, maxCol := read14_segments(lines)

	// Add floor
	extend := (maxCol - minCol) * (maxCol - minCol)
	hsegments = append(hsegments, hsegment{row: maxRow + 2, fromCol: minCol - extend, toCol: maxCol + extend})
	maxRow += 2
	minCol -= extend
	maxCol += extend

	columns := get14_columns(hsegments, vsegments, minRow, maxRow, minCol, maxCol)

	count := 0
	for drop14_ball(0, 500, columns, minCol, maxCol, maxRow) {
		count++
	}
	result := count
	return result
}

func (t *AOC) Day14(log Log) {
	answer1 := part14a(log)
	fmt.Printf("answer1 := '%v'\n", answer1) // 1003
	answer2 := part14b(log)
	fmt.Printf("answer2 := '%v'\n", answer2) // 25771
}
