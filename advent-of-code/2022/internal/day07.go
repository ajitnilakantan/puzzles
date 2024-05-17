package internal

import (
	_ "embed"
	"fmt"
	"strings"

	_ "github.com/stretchr/testify/assert"
)

var (
	//go:embed input/input07.txt
	input07 string
)

type file07 struct {
	name  string
	size  int
	isdir bool
}

// Map directory name to list of containing files
type filesystem07 map[string][]file07

func process07_commands(fs filesystem07, lines []string) int {
	curline := 0
	cwd := []string{"/"}
	for curline < len(lines) {
		line := lines[curline]
		if len(line) == 0 || line[0] != '$' {
			panic(fmt.Sprintf("Error parsing line %v = '%v'", curline, line))
		}
		tokens := split_line_regex(line, "\x20")
		if tokens[1] == "cd" {
			if tokens[2] == ".." {
				// Up
				if len(cwd) > 1 {
					cwd = cwd[0 : len(cwd)-1]
				}
			} else if tokens[2] == "/" {
				// goto root
				cwd = []string{"/"}
			} else if strings.Contains(tokens[2], "/") {
				panic(fmt.Sprintf("TODO: Handle cd with slashes '%v'", tokens[2]))
			} else {
				cwd = append(cwd, tokens[2])
			}
		} else if tokens[1] == "ls" {
			path_contents := []file07{}
			for curline < len(lines)-1 && !strings.HasPrefix(lines[curline+1], "$") {
				curline++
				line = lines[curline]
				tokens = split_line_regex(line, "\x20")
				if tokens[0] == "dir" {
					// directory entry
					path_contents = append(path_contents, file07{name: tokens[1], size: 0, isdir: true})
				} else {
					path_contents = append(path_contents, file07{name: tokens[1], size: my_atoi(tokens[0]), isdir: false})

				}
			}
			cwdpath := "/" + strings.Join(cwd[1:], "/") // convert to string
			// fmt.Printf("'%v' -> '%v'\n", cwd, cwdpath)
			fs[cwdpath] = path_contents
		} else {
			panic(fmt.Sprintf("Unknown token '%v' for line %v='%v'\n", tokens[1], curline, line))
		}
		curline++
	}
	// fmt.Printf("fs='%+v'\n", fs)
	return 0
}

func calc07_dirsize(fs filesystem07, dir string) int {
	val, ok := fs[dir]
	size := 0
	if !ok {
		// not found
		return 0
	} else {
		for _, entry := range val {
			if entry.isdir {
				var subdir string
				if dir == "/" {
					subdir = dir + entry.name
				} else {
					subdir = dir + "/" + entry.name
				}
				size += calc07_dirsize(fs, subdir)
			} else {
				size += entry.size
			}
		}
	}

	return size
}

func get07_get_small_directories(fs filesystem07, limit int) int {
	sum := 0
	for k := range fs {
		size := calc07_dirsize(fs, k)
		if size <= limit {
			sum += size
		}
	}
	return sum
}

func get07_directory_to_delete(fs filesystem07) int {
	limit := 70000000
	required := 30000000
	used := calc07_dirsize(fs, "/")
	delta := required - (limit - used)

	result := limit
	for k := range fs {
		size := calc07_dirsize(fs, k)
		if size >= delta && size < result {
			result = size
		}
	}
	return result
}
func part07a(log Log) int {
	lines := split_lines(input07)
	fs := make(filesystem07)
	process07_commands(fs, lines)
	result := get07_get_small_directories(fs, 100000)

	return result
}

func part07b(log Log) int {
	lines := split_lines(input07)
	fs := make(filesystem07)
	process07_commands(fs, lines)
	result := get07_directory_to_delete(fs)
	return result
}

func (t *AOC) Day07(log Log) {
	answer1 := part07a(log)
	fmt.Printf("answer1 := '%v'\n", answer1) // 1118405
	answer2 := part07b(log)
	fmt.Printf("answer2 := '%v'\n", answer2) // 12545514
}
