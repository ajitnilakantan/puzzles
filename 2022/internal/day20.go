package internal

import (
	_ "embed"
	"fmt"

	_ "github.com/stretchr/testify/assert"
	"golang.org/x/exp/slices"
)

var (
	//go:embed input/input20.txt
	input20 string
)

type shuffle20_state struct {
	// indexed values
	data []int
	// Create skiplist
	sl *SkipList[float64, int]
	// Reverse index to key map
	keys []float64
}

func read20_input(lines []string, decrypt_key int) shuffle20_state {
	state := shuffle20_state{}
	// indexed values
	state.data = Map(lines, func(item string) int { return decrypt_key * my_atoi(item) })
	// Create skiplist
	state.sl = MakeSkipList[float64, int]()
	// Reverse index to key map
	state.keys = make([]float64, len(state.data))

	for i := 0; i < len(state.data); i++ {
		state.sl.Insert(float64(i), state.data[i])
		state.keys[i] = float64(i)
	}

	return state
}

func shuffle20_data(state shuffle20_state, i int) {
	val := state.data[i]
	if val == 0 {
		// nothing to do
		return
	}
	// Process repeated values in order, right to left, as they are currently ordered
	oldKey := state.keys[i]
	var newKey float64
	index, _val, _ := state.sl.Find(oldKey)
	if val != _val {
		panic(fmt.Sprintf("unexpected: val=%v != _val=%v", val, _val))
	}
	offset := val
	newIndex := (((index + offset + len(state.data) - 1) % (len(state.data) - 1)) + len(state.data) - 1) % (len(state.data) - 1)
	if newIndex < 0 {
		panic(fmt.Sprintf("unexpected: newIndex negative=%v", newIndex))
	}

	//fmt.Printf(" index %v -> newIndex %v ", index, newIndex)
	state.sl.Delete(oldKey)
	var key0, key1 float64
	key0, _, _ = state.sl.GetAt(newIndex)
	if newIndex == 0 {
		key1 = key0 - 1.0
	} else {
		key1, _, _ = state.sl.GetAt(newIndex - 1)
	}
	newKey = (key0 + key1) / 2.0
	state.sl.Insert(newKey, val)
	state.keys[i] = newKey
	// fmt.Printf("%v v=%v k=%v\n", i, sl.Values(), sl.Keys())
}

func solve20a(lines []string, decrypt_key int, rounds int) int {
	state := read20_input(lines, decrypt_key)

	// Loop through and perform the shuffle
	// fmt.Printf("%v v=%v k=%v\n", 0, sl.Values(), sl.Keys())
	for r := 0; r < rounds; r++ {
		for i := 0; i < len(state.data); i++ {
			shuffle20_data(state, i)
		}
	}

	zeroPosition := slices.Index(state.data, 0)
	if zeroPosition == -1 {
		panic(fmt.Sprintf("unexpected: cannot find 0: %v", zeroPosition))
	}
	zeroIndex, _, _ := state.sl.Find(state.keys[zeroPosition])
	if zeroIndex < 0 {
		panic(fmt.Sprintf("unexpected: cannot find 0index: %v", zeroIndex))
	}
	_, v1000, _ := state.sl.GetAt((1000 + zeroIndex) % len(state.data))
	_, v2000, _ := state.sl.GetAt((2000 + zeroIndex) % len(state.data))
	_, v3000, _ := state.sl.GetAt((3000 + zeroIndex) % len(state.data))
	return v1000 + v2000 + v3000
}
func part20a(log Log) int {
	lines := split_lines(input20)
	result := solve20a(lines, 1, 1)

	return result
}

func part20b(log Log) int {
	lines := split_lines(input20)
	result := solve20a(lines, 811589153, 10)

	return result
}

func (t *AOC) Day20(log Log) {
	answer1 := part20a(log)
	fmt.Printf("answer1 := '%v'\n", answer1) // 11073
	answer2 := part20b(log)
	fmt.Printf("answer2 := '%v'\n", answer2) // 11102539613040
}
