package internal

import (
	_ "embed"
	"fmt"
	"sort"

	_ "github.com/stretchr/testify/assert"
)

var (
	//go:embed input/input15.txt
	input15 string
)

// Position of sensor + beacon + distance from sensor to beacon
type sensor15 struct {
	posy, posx       int
	beacony, beaconx int
	size             int
}

// Inclusive segment [from, to]
type segment15 struct {
	from, to int
}

// Return the size of the intersection of a and b
func intersect15_size(a, b segment15) int {
	if b.from >= a.from && b.from <= a.to && b.to >= a.from && b.to <= a.to {
		// b is enclosed in a
		return b.to - b.from + 1
	} else if a.from >= b.from && a.from <= b.to && a.to >= b.from && a.to <= b.to {
		// a is enclosed in b
		return a.to - a.from + 1
	} else if a.from <= b.from && a.to >= b.from && a.to <= b.to {
		// a intersects b from the left
		return a.to - b.from + 1
	} else if a.from >= b.from && a.from <= b.to && a.to >= b.to {
		// a intersects b from the right
		return b.to - a.from + 1
	} else {
		return 0
	}
}

func make15_union(union *[]segment15, segment segment15) []segment15 {
	dirty := true

	for dirty {
		dirty = false
		for i := range *union {
			if intersect15_size((*union)[i], segment) != 0 {
				dirty = true
				if segment.from > (*union)[i].from {
					segment.from = (*union)[i].from
				}
				if segment.to < (*union)[i].to {
					segment.to = (*union)[i].to
				}
				*union = append((*union)[0:i], (*union)[i+1:]...)
				break
			}
		}
		if !dirty {
			*union = append(*union, segment)
		}
	}

	return *union
}
func get15_segments(row int, sensors []sensor15) []segment15 {
	abs := func(v int) int {
		if v < 0 {
			return -v
		}
		return v
	}
	segments := make([]segment15, 0)
	for _, sensor := range sensors {
		if dist := abs(sensor.posy - row); dist <= sensor.size {
			s := segment15{from: sensor.posx - sensor.size + dist, to: sensor.posx + sensor.size - dist}
			if s.from > s.to {
				s.from, s.to = s.to, s.from
			}
			segments = append(segments, s)
		}
	}

	return segments
}

// Return the size of the covered area at the specified row.
// Get the union of all segments
func get15_segments_measure(row int, sensors []sensor15) int {
	type pos struct{ x, y int }
	segments := get15_segments(row, sensors)

	union := make([]segment15, 0)
	for _, s := range segments {
		union = make15_union(&union, s)
	}

	measure := 0
	for _, s := range union {
		measure += s.to - s.from + 1
	}

	// remove count of sensorts and beacons on the row as well
	beacons_on_row := make(Set[pos])
	for _, s := range sensors {
		if s.beacony == row {
			for _, seg := range segments {
				if s.beaconx >= seg.from && s.beaconx <= seg.to {
					beacons_on_row.Add(pos{x: s.beaconx, y: s.beacony})
					break
				}
			}
		}
		if s.posy == row {
			for _, seg := range segments {
				if s.posx >= seg.from && s.posx <= seg.to {
					beacons_on_row.Add(pos{x: s.posx, y: s.posy})
					break
				}
			}
		}
	}

	return measure - len(beacons_on_row)
}

func no_sensors_at(sensors []sensor15, row int, col int) bool {
	for _, s := range sensors {
		if s.beaconx == col && s.beacony == row || s.posx == col && s.posy == row {
			return false
		}
	}
	return true
}

// Return row,col of empty space
func get15_empty_space(sensors []sensor15, maxVal int) (int, int) {
	for row := 0; row < maxVal; row++ {
		segments := get15_segments(row, sensors)
		union := make([]segment15, 0)
		for _, s := range segments {
			union = make15_union(&union, s)
		}
		sort.Slice(union, func(i, j int) bool {
			return union[i].from < union[j].from
		})
		for i := range union {
			if i == 0 && union[i].from > 0 {
				return row, 0
			} else if i == len(union)-1 && union[i].to < maxVal {
				return row, union[i].to + 1
			} else if i < len(union)-1 && union[i].to+1 < union[i+1].to && no_sensors_at(sensors, row, union[i].to+1) {
				return row, union[i].to + 1
			}
		}
		//fmt.Printf("row %v u=%v\n", row, union)
	}
	return 0, 0
}

func read15_data(lines []string) []sensor15 {
	abs := func(v int) int {
		if v < 0 {
			return -v
		}
		return v
	}
	sensors := make([]sensor15, 0)
	for _, line := range lines {
		tokens := split_line_regex(line, "[A-Za-z=,\x20\\:]+")[1:]
		s := sensor15{posx: my_atoi(tokens[0]), posy: my_atoi(tokens[1]), beaconx: my_atoi(tokens[2]), beacony: my_atoi(tokens[3])}
		s.size = abs(s.posx-s.beaconx) + abs(s.posy-s.beacony)
		sensors = append(sensors, s)
	}
	return sensors
}
func part15a(log Log) int {
	lines := split_lines(input15)
	sensors := read15_data(lines)
	result := get15_segments_measure(2000000, sensors)

	return result
}

func part15b(log Log) int {
	lines := split_lines(input15)
	sensors := read15_data(lines)
	emptyRow, emptyCol := get15_empty_space(sensors, 4000000)
	result := 4000000*emptyCol + emptyRow
	return result
}

func (t *AOC) Day15(log Log) {
	answer1 := part15a(log)
	fmt.Printf("answer1 := '%v'\n", answer1) // 4560025
	answer2 := part15b(log)
	fmt.Printf("answer2 := '%v'\n", answer2) // 12480406634249
}
