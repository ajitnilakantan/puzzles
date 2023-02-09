package internal

import (
	"testing"

	"github.com/stretchr/testify/assert"
)

func TestDay21(t *testing.T) {
	data :=
		`root: pppw + sjmn
		dbpl: 5
		cczh: sllz + lgvd
		zczc: 2
		ptdq: humn - dvpt
		dvpt: 3
		lfqf: 4
		humn: 5
		ljgn: 2
		sjmn: drzm * dbpl
		sllz: 4
		pppw: cczh / lfqf
		lgvd: ljgn * ptdq
		drzm: hmdt - zczc
		hmdt: 32`
	lines := split_lines(data)
	dict := read21_data(lines)
	// fmt.Printf("%+v\n", dict)
	result := value21(dict, "root")
	assert.Equal(t, 152, result)

	result = bisect21_solve(dict)
	assert.Equal(t, 301, result)

	//dict["humn"] = []string{"0"}
	//fmt.Printf("root(humn=%v) = %v %v %v\n", dict["humn"][0], value21(dict, dict["root"][0]), dict["root"][1], value21(dict, dict["root"][2]))

	//dict["humn"] = []string{"301"}
	//fmt.Printf("root(humn=%v) = %v %v %v\n", dict["humn"][0], value21(dict, dict["root"][0]), dict["root"][1], value21(dict, dict["root"][2]))

	//dict["humn"] = []string{"30031"}
	//fmt.Printf("root(humn=%v) = %v %v %v\n", dict["humn"][0], value21(dict, dict["root"][0]), dict["root"][1], value21(dict, dict["root"][2]))
}
