package internal

import (
	"testing"

	"github.com/stretchr/testify/assert"
)

func TestDay06(t *testing.T) {
	var data string
	data = "mjqjpqmgbljsphdztnvjfqwrcgsmlb"
	assert.Equal(t, 7, count06_header_offset(data, 4))

	data = "bvwbjplbgvbhsrlpgdmjqwftvncz"
	assert.Equal(t, 5, count06_header_offset(data, 4))

	data = "nppdvjthqldpwncqszvftbrmjlhg"
	assert.Equal(t, 6, count06_header_offset(data, 4))

	data = "nznrnfrfntjfmvfwmzdfjlvtqnbhcprsg"
	assert.Equal(t, 10, count06_header_offset(data, 4))

	data = "zcfzfwzzqfrljwzlrfnpqdbhtmscgvjw"
	assert.Equal(t, 11, count06_header_offset(data, 4))

	data = "mjqjpqmgbljsphdztnvjfqwrcgsmlb"
	assert.Equal(t, 19, count06_header_offset(data, 14))
	data = "bvwbjplbgvbhsrlpgdmjqwftvncz"
	assert.Equal(t, 23, count06_header_offset(data, 14))

	data = "nppdvjthqldpwncqszvftbrmjlhg"
	assert.Equal(t, 23, count06_header_offset(data, 14))

	data = "nznrnfrfntjfmvfwmzdfjlvtqnbhcprsg"
	assert.Equal(t, 29, count06_header_offset(data, 14))

	data = "zcfzfwzzqfrljwzlrfnpqdbhtmscgvjw"
	assert.Equal(t, 26, count06_header_offset(data, 14))
}
