package internal

import (
	"testing"

	"github.com/stretchr/testify/assert"
)

func TestDay25(t *testing.T) {
	// Test snafu to decimal
	test_data := `1=-0-2     1747
			12111      906
			2=0=      198
			21       11
			2=01      201
			111       31
			20012     1257
			112       32
			1=-1=      353
			1-12      107
			12        7
			1=        3
			122       37`

	lines := split_lines(test_data)
	for _, line := range lines {
		s := split_line_regex(line, `\s+`)
		assert.Equal(t, my_atoi(s[1]), snafu_to_decimal(s[0]))
	}

	// test decimal to snafu
	d2s := `1              1
			2              2
			3             1=
			4             1-
			5             10
			6             11
			7             12
			8             2=
			9             2-
			10             20
			15            1=0
			20            1-0
			2022         1=11-2
			12345        1-0---0
			314159265  1121-1110-1=0`

	lines = split_lines(d2s)
	for _, line := range lines {
		s := split_line_regex(line, `\s+`)
		assert.Equal(t, decimal_to_snafu(my_atoi(s[0])), s[1])
	}

	// Test part 1
	data := `1=-0-2
			12111
			2=0=
			21
			2=01
			111
			20012
			112
			1=-1=
			1-12
			12
			1=
			122`
	lines = split_lines(data)
	ret := Reduce(Map(lines, snafu_to_decimal), 0, func(acc int, val int) int { return acc + val })
	assert.Equal(t, 4890, ret)
	snafu := decimal_to_snafu(ret)
	assert.Equal(t, "2=-1=0", snafu)
}
