package internal

import (
	"fmt"
	"regexp"
	"strconv"
	"strings"
)

// Return [][]T addressed by v[row][column] where row=0..height-1, col=0..width-1
func MakeMatrix2D[T any](height int, width int) [][]T {
	matrix := make([][]T, height)
	rows := make([]T, width*height)
	for row, startRow := 0, 0; row < height; row, startRow = row+1, startRow+width {
		endRow := startRow + width
		matrix[row] = rows[startRow:endRow:endRow]
	}
	return matrix
}

// Split string on supplied regex
func split_line_regex(input string, regex string) []string {
	re := regexp.MustCompile(regex)
	split := re.Split(input, -1)
	tokens := []string{}
	tokens = append(tokens, split...)
	return tokens
}

// Separate string by newline, optionally (default true), trimming strings
func split_lines(input string, trim ...bool) []string {
	lines := strings.Split(input, "\n")
	if len(trim) == 0 || len(trim) > 0 && trim[0] {
		// Trim whitespace
		lines = Map(lines, func(item string) string { return strings.TrimSpace(item) })
	}
	return lines
}

// Return "chunks" lines -- i.e. lines of text separated by blank lines
func split_chunks(input []string) [][]string {
	ret := [][]string{}
	chunk := []string{}
	for _, v := range input {
		if len(v) == 0 || v == "\n" || v == "\r" || v == "\n\r" || v == "\r\n" {
			if len(chunk) > 0 {
				ret = append(ret, chunk)
			}
			chunk = []string{}
		} else {
			chunk = append(chunk, v)
		}
	}
	if len(chunk) > 0 {
		ret = append(ret, chunk)
	}
	return ret
}

func my_atoi(input string) int {
	ret, err := strconv.Atoi(input)
	if err == nil {
		return ret
	}

	panic(fmt.Sprintf("Unknown text '%v'", input))
	// return 0
}
