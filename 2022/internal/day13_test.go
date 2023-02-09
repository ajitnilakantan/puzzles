package internal

import (
	"sort"
	"testing"

	"github.com/stretchr/testify/assert"
)

func TestDay13(t *testing.T) {
	data :=
		`[1,1,3,1,1]
		[1,1,5,1,1]
		
		[[1],[2,3,4]]
		[[1],4]
		
		[9]
		[[8,7,6]]
		
		[[4,4],4,4]
		[[4,4],4,4,4]
		
		[7,7,7,7]
		[7,7,7]
		
		[]
		[3]
		
		[[[]]]
		[[]]
		
		[1,[2,[3,[4,[5,6,7]]]],8,9]
		[1,[2,[3,[4,[5,6,0]]]],8,9]`

	line := "[1,1,3,1,1]"
	tokens := split13_tokens(line)
	assert.Equal(t, []string{"1", "1", "3", "1", "1"}, tokens)
	//fmt.Printf("line '%v' -> len=%v : '%+v'\n", line, len(tokens), tokens)

	line = "[[1],[2,3,4]]"
	tokens = split13_tokens(line)
	assert.Equal(t, []string{"[1]", "[2,3,4]"}, tokens)
	//fmt.Printf("line '%v' -> len=%v : '%+v'\n", line, len(tokens), tokens)

	line = "[1,[2,[3,[4,[5,6,7]]]],8,9]"
	tokens = split13_tokens(line)
	assert.Equal(t, []string{"1", "[2,[3,[4,[5,6,7]]]]", "8", "9"}, tokens)
	//fmt.Printf("line '%v' -> len=%v : '%+v'\n", line, len(tokens), tokens)

	var line1, line2 string
	line1 = "[1,1,3,1,1]"
	line2 = "[1,1,5,1,1]"
	assert.Equal(t, LeftSmaller, compare13_tokens(line1, line2))

	line1 = "[[1],[2,3,4]]"
	line2 = "[[1],4]"
	assert.Equal(t, LeftSmaller, compare13_tokens(line1, line2))

	line1 = "[9]"
	line2 = "[[8,7,6]]"
	assert.Equal(t, RightSmaller, compare13_tokens(line1, line2))

	line1 = "[[4,4],4,4]"
	line2 = "[[4,4],4,4,4]"
	assert.Equal(t, LeftSmaller, compare13_tokens(line1, line2))

	line1 = "[7,7,7,7]"
	line2 = "[7,7,7]"
	assert.Equal(t, RightSmaller, compare13_tokens(line1, line2))

	line1 = "[]"
	line2 = "[3]"
	assert.Equal(t, LeftSmaller, compare13_tokens(line1, line2))

	line1 = "[[[]]]"
	line2 = "[[]]"
	assert.Equal(t, RightSmaller, compare13_tokens(line1, line2))

	line1 = "[1,[2,[3,[4,[5,6,7]]]],8,9]"
	line2 = "[1,[2,[3,[4,[5,6,0]]]],8,9]"
	assert.Equal(t, RightSmaller, compare13_tokens(line1, line2))

	lines := split_lines(data)
	chunks := split_chunks(lines)
	result := 0
	for index, pair := range chunks {
		if compare13_tokens(pair[0], pair[1]) == LeftSmaller {
			result += index + 1
		}
	}
	assert.Equal(t, 13, result)

	// part b
	all_lines := make([]string, 0, len(lines))
	for _, line := range lines {
		if line != "" {
			all_lines = append(all_lines, line)
		}
	}
	all_lines = append(all_lines, "[[2]]")
	all_lines = append(all_lines, "[[6]]")
	sort.Slice(all_lines, func(i, j int) bool {
		return compare13_tokens(all_lines[i], all_lines[j]) == LeftSmaller
	})

	// fmt.Printf("%v\n", all_lines)
	result = (IndexOf(all_lines, "[[2]]") + 1) * (IndexOf(all_lines, "[[6]]") + 1)
	assert.Equal(t, 140, result)
}
