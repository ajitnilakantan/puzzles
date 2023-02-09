package internal

import (
	_ "embed"
	"fmt"

	_ "github.com/stretchr/testify/assert"
)

var (
	//go:embed input/input09.txt
	input09 string
)

type coord09 struct{ r, c int }

func move09_tail(t coord09, h coord09) coord09 {
	abs := func(v int) int {
		if v < 0 {
			return -v
		}
		return v
	}
	sign := func(v int) int {
		if v < 0 {
			return -1
		} else {
			return 1
		}
	}
	if abs(t.r-h.r) <= 1 && abs(t.c-h.c) <= 1 {
		return t
	}
	if t.r == h.r {
		t.c += sign(h.c - t.c)
	} else if t.c == h.c {
		t.r += sign(h.r - t.r)
	} else {
		t.c += sign(h.c - t.c)
		t.r += sign(h.r - t.r)
	}
	return t
}

func get09_direction(dir string) (dx, dy int) {
	dx, dy = 0, 0
	if dir == "U" {
		dy = -1
	}
	if dir == "D" {
		dy = 1
	}
	if dir == "L" {
		dx = -1
	}
	if dir == "R" {
		dx = 1
	}
	return
}

func count09_moves(lines []string) int {
	head, tail := coord09{0, 0}, coord09{0, 0}
	tails := make(Set[coord09])
	tails.Add(tail)
	for _, line := range lines {
		tokens := split_line_regex(line, "\\s+")
		dir := tokens[0]
		num := my_atoi(tokens[1])
		dx, dy := get09_direction(dir)
		for i := 0; i < num; i++ {
			head = coord09{r: head.r + dy, c: head.c + dx}
			tail = move09_tail(tail, head)
			tails.Add(tail)
		}
	}
	return len(tails)
}

func count09_rope_moves(lines []string) int {
	ropelen := 10
	rope := make([]coord09, ropelen)
	head := &rope[ropelen-1]
	tail := &rope[0]

	tails := make(Set[coord09])
	tails.Add(*tail)

	for _, line := range lines {
		tokens := split_line_regex(line, "\\s+")
		dir := tokens[0]
		num := my_atoi(tokens[1])
		dx, dy := get09_direction(dir)
		for i := 0; i < num; i++ {
			head.r, head.c = head.r+dy, head.c+dx
			for i := ropelen - 2; i >= 0; i-- {
				newpos := move09_tail(rope[i], rope[i+1])
				rope[i].r, rope[i].c = newpos.r, newpos.c
			}

			tails.Add(*tail)
		}
	}
	// fmt.Printf("rop='%+v' h=%v t=%v\n", rope, head, tail)
	return len(tails)
}

func part09a(log Log) int {
	lines := split_lines(input09)
	result := count09_moves(lines)

	return result
}

func part09b(log Log) int {
	lines := split_lines(input09)
	result := count09_rope_moves(lines)
	return result
}

func (t *AOC) Day09(log Log) {
	answer1 := part09a(log)
	fmt.Printf("answer1 := '%v'\n", answer1) // 5858
	answer2 := part09b(log)
	fmt.Printf("answer2 := '%v'\n", answer2) //
}
