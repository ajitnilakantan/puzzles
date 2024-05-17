package internal

import (
	"fmt"
	"testing"

	"github.com/stretchr/testify/assert"
)

func TestSkipList(t *testing.T) {
	sl := MakeSkipList[int, int]()

	// Test insert
	for i := 0; i < 10; i++ {
		sl.Insert(i, i)
	}
	for i := -10; i < 20; i++ {
		index, v, found := sl.Find(i)
		if i >= 0 && i < 10 {
			assert.Equal(t, i, index)
			assert.Equal(t, i, v)
			assert.Equal(t, true, found)
		} else {
			assert.Equal(t, -1, index)
			assert.Equal(t, 0, v)
			assert.Equal(t, false, found)
		}
	}
	sl.debug_print()

	key, value, found := sl.GetAt(2)
	assert.Equal(t, 2, key)
	assert.Equal(t, 2, value)
	assert.Equal(t, true, found)

	key, value, found = sl.GetAt(7)
	assert.Equal(t, 7, key)
	assert.Equal(t, 7, value)
	assert.Equal(t, true, found)

	_, _, found = sl.GetAt(11)
	assert.Equal(t, false, found)

	// Test delete
	sl.Delete(1)
	sl.Delete(15)
	sl.Delete(7)
	// new key indices after delete
	indices := map[int]int{0: 0, 2: 1, 3: 2, 4: 3, 5: 4, 6: 5, 8: 6, 9: 7}
	for i := 0; i < 10; i++ {
		index, v, found := sl.Find(i)
		if i != 1 && i != 7 {
			assert.Equal(t, indices[i], index)
			assert.Equal(t, i, v)
			assert.Equal(t, true, found)
		} else {
			assert.Equal(t, -1, index)
			assert.Equal(t, 0, v)
			assert.Equal(t, false, found)
		}
	}

	sl.debug_print()

	// Keys/Values after delete
	assert.Equal(t, []int{0, 2, 3, 4, 5, 6, 8, 9}, sl.Keys())
	assert.Equal(t, []int{0, 2, 3, 4, 5, 6, 8, 9}, sl.Values())
	// fmt.Printf("Keys = %+v\nValues = %+v\n", sl.Keys(), sl.Values())

	sl.Delete(8)
	assert.Equal(t, []int{0, 2, 3, 4, 5, 6, 9}, sl.Keys())
	assert.Equal(t, []int{0, 2, 3, 4, 5, 6, 9}, sl.Values())
	sl.debug_print()

	sl.Delete(0)
	assert.Equal(t, []int{2, 3, 4, 5, 6, 9}, sl.Keys())
	assert.Equal(t, []int{2, 3, 4, 5, 6, 9}, sl.Values())
	sl.debug_print()

	sl.Insert(0, 0)
	sl.debug_print()

	sl.Insert(111, 111)
	sl.debug_print()

	sl.Insert(77, 77)
	sl.debug_print()
	fmt.Printf("Keys = %+v\nValues = %+v\n", sl.Keys(), sl.Values())
	keys := sl.Keys()
	values := sl.Values()
	for i := 0; i < len(values); i++ {
		key, value, found := sl.GetAt(i)
		assert.Equal(t, keys[i], key)
		assert.Equal(t, values[i], value)
		assert.Equal(t, true, found)
	}

	assert.True(t, sl.debug_validate())
}
