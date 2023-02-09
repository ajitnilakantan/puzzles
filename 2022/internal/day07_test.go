package internal

import (
	"testing"

	"github.com/stretchr/testify/assert"
)

func TestDay07(t *testing.T) {
	data :=
		`$ cd /
		$ ls
		dir a
		14848514 b.txt
		8504156 c.dat
		dir d
		$ cd a
		$ ls
		dir e
		29116 f
		2557 g
		62596 h.lst
		$ cd e
		$ ls
		584 i
		$ cd ..
		$ cd ..
		$ cd d
		$ ls
		4060174 j
		8033020 d.log
		5626152 d.ext
		7214296 k`
	fs := make(filesystem07)
	lines := split_lines(data)
	process07_commands(fs, lines)
	sum := get07_get_small_directories(fs, 100000)
	assert.Equal(t, 95437, sum)
	to_delete := get07_directory_to_delete(fs)
	assert.Equal(t, 24933642, to_delete)
}
