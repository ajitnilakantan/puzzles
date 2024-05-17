package internal

import (
	"testing"

	"github.com/stretchr/testify/assert"
)

func TestDay11(t *testing.T) {
	data :=
		`Monkey 0:
		Starting items: 79, 98
		Operation: new = old * 19
		Test: divisible by 23
		  If true: throw to monkey 2
		  If false: throw to monkey 3
	  
	  Monkey 1:
		Starting items: 54, 65, 75, 74
		Operation: new = old + 6
		Test: divisible by 19
		  If true: throw to monkey 2
		  If false: throw to monkey 0
	  
	  Monkey 2:
		Starting items: 79, 60, 97
		Operation: new = old * old
		Test: divisible by 13
		  If true: throw to monkey 1
		  If false: throw to monkey 3
	  
	  Monkey 3:
		Starting items: 74
		Operation: new = old + 3
		Test: divisible by 17
		  If true: throw to monkey 0
		  If false: throw to monkey 1`

	lines := split_lines(data)
	chunks := split_chunks(lines)
	monkeys := read11_monkeys(chunks)
	// fmt.Printf("monkeys=\n%+v\n", monkeys)
	val := simulate11_rounds(monkeys, 20, 3)
	// fmt.Printf("monkeys after 20 rounds=%v=\n%+v\n", val, monkeys)
	assert.Equal(t, []int{10, 12, 14, 26, 34}, monkeys[0].items)
	assert.Equal(t, []int{245, 93, 53, 199, 115}, monkeys[1].items)
	assert.Equal(t, 0, len(monkeys[2].items))
	assert.Equal(t, 0, len(monkeys[3].items))
	assert.Equal(t, []int{101, 95, 7, 105}, val)
	val = Reverse(Sort(val))
	result := val[0] * val[1]
	assert.Equal(t, 10605, result)

	// part 2
	monkeys = read11_monkeys(chunks)
	val = simulate11_rounds(monkeys, 20, 0)
	assert.Equal(t, []int{99, 97, 8, 103}, val)

	monkeys = read11_monkeys(chunks)
	val = simulate11_rounds(monkeys, 10000, 0)
	assert.Equal(t, []int{52166, 47830, 1938, 52013}, val)
	val = Reverse(Sort(val))
	result = val[0] * val[1]
	assert.Equal(t, 2713310158, result)
}
