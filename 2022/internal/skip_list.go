package internal

import (
	"fmt"
	"math/rand"
	"reflect"
	"strings"

	"golang.org/x/exp/constraints"
)

// See https://ee.usc.edu/~redekopp/cs104/slides/L23_SkipLists.pdf
//     https://drum.lib.umd.edu/bitstream/handle/1903/544/CS-TR-2286.1.pdf
//     https://alexdremov.me/skip-list-indexation-and-kth-maximum
//     https://code.activestate.com/recipes/576930/
//     https://www.openmymind.net/Fast-Large-Offsets-With-An-Indexed-Skiplist/
//     https://en.wikipedia.org/wiki/Order_statistic_tree

type skipNode[K constraints.Ordered, V any] struct {
	// Record
	key   K
	value V
	// "distance" to the next node. At the bottom level, it
	// is 1.  In the "fast lanes" it is >= 1
	width []int
	// Next pointer
	next []*skipNode[K, V]
}

func newSkipNode[K constraints.Ordered, V any](key K, value V, level int) *skipNode[K, V] {
	// The 0th level are the actual records at the bottom of the skiplist.
	// So we allocate level+1 sized arrays.
	return &skipNode[K, V]{
		key:   key,
		value: value,
		width: make([]int, level+1),
		next:  make([]*skipNode[K, V], level+1),
	}
}

type SkipList[K constraints.Ordered, V any] struct {
	head      *skipNode[K, V]
	level     int
	max_level int
}

func MakeSkipList[K constraints.Ordered, V any]() *SkipList[K, V] {
	return &SkipList[K, V]{
		head:      newSkipNode(*new(K), *new(V), 0),
		level:     -1,
		max_level: 15, // TODO: should be ~log_2(expected # nodes)
	}
}

func (s *SkipList[K, V]) Find(key K) (int, V, bool) {
	// Search for the key
	index := 0
	node := s.head
	for i := s.level; i >= 0; i-- {
		for {
			if node.next[i] == nil || node.next[i].key > key {
				// move to the next level
				break
			} else if node.next[i].key == key {
				// found
				index += node.next[i].width[i] - 1 // subtract 1 because 1st width starts at 1
				return index, node.next[i].value, true
			} else {
				// move right
				index += node.next[i].width[i]
				node = node.next[i]
			}
		}
	}
	return -1, *new(V), false
}

func (s *SkipList[K, V]) get_random_level() int {
	level := 0
	for rand.Int31()%2 == 0 && level < s.max_level {
		level += 1
	}
	return level
}

func get_width[K constraints.Ordered, V any](node *skipNode[K, V], level int, key K) int {
	width := 0
	for ; node != nil; node = node.next[level] {
		if node.key > key {
			break
		}
		width += node.width[level]
	}
	return width
}

// Insert key/value into the skiplist
func (s *SkipList[K, V]) Insert(key K, value V) {
	// Choose a new random level for the new node
	newLevel := s.get_random_level()
	if newLevel > s.level {
		// expand the head's levels
		s.head.next = append(s.head.next, make([]*skipNode[K, V], newLevel-s.level)...)
		s.head.width = append(s.head.width, make([]int, newLevel-s.level)...)
		s.level = newLevel
	}
	// create a new node wth a random new level
	newNode := newSkipNode(key, value, newLevel)

	// for each level, keep a pointer to the new node
	// keep track of the chain of nodes to the new node so we can update "width"
	current := s.head
	for level := s.level; level >= 0; level-- {
		for ; current.next[level] != nil; current = current.next[level] {
			// move right. stop before the next one is greater
			if current.next[level].key > key {
				break
			}
		}
		if level > newLevel {
			// Increase all the levels above this one by 1
			if current.next[level] != nil {
				current.next[level].width[level]++
			}
			continue
		} else if level == 0 {
			// At level==0, i.e. the bottom level, all widths are 1
			newNode.width[level] = 1
		} else {
			// Calculate the level of the node
			width := get_width(current.next[level-1], level-1, key)
			for i := level; i <= newLevel; i++ {
				newNode.width[i] += width
			}
			newNode.width[level] += 1
		}
		// Insert the new node
		newNode.next[level] = current.next[level]
		current.next[level] = newNode
	}

	// Change the width of subsequent nodes
	for i := 1; i <= newLevel; i++ {
		if next := newNode.next[i]; next != nil {
			next.width[i] -= newNode.width[i] - 1
		}
	}
}

func (s *SkipList[K, V]) Delete(key K) bool {
	found := false
	node := s.head
	chain := make([]*skipNode[K, V], s.level+1)
	var foundNode *skipNode[K, V]
	for i := s.level; i >= 0; i-- {
		for {
			if node.next[i] == nil || node.next[i].key > key {
				// go down to next level
				chain[i] = node.next[i]
				break
			} else if node.next[i].key == key {
				if foundNode == nil {
					foundNode = node.next[i]
				}
				// chain[i] = node.next[i]
				node.next[i] = node.next[i].next[i]
				found = true
			} else {
				node = node.next[i]
				chain[i] = node
			}
		}
	}
	// Update the widths.  Decrement higher level nodes by 1 (because of deleted node).
	// Increase next nodes by (width-1) (subract 1 because node itself is deleted).
	if found {
		for i := s.level; i >= len(foundNode.next); i-- {
			if chain[i] != nil {
				chain[i].width[i] -= 1
			}
		}
		for i := len(foundNode.next) - 1; i >= 0; i-- {
			if foundNode.next[i] != nil {
				foundNode.next[i].width[i] += (foundNode.width[i] - 1)
			}
		}
	}

	return found
}

func (s *SkipList[K, V]) GetAt(index int) (K, V, bool) {
	debug_index := index
	node := s.head
	index += 1

	for level := s.level; level >= 0; level-- {

		for node.next[level] != nil && node.next[level].width[level] <= index {
			// move right. stop before the next one is greater
			index -= node.next[level].width[level]
			node = node.next[level]
			if index == 0 {
				break
			}
		}
		if index == 0 {
			break
		}
	}
	if index == 0 {
		s.debug_sanity_check_getat(debug_index, node.key, node.value, true)
		return node.key, node.value, true
	}
	s.debug_sanity_check_getat(debug_index, *new(K), *new(V), false)
	return *new(K), *new(V), false
}

func (s *SkipList[K, V]) Keys() []K {
	keys := make([]K, 0)
	level := 0
	node := s.head
	for node.next[level] != nil {
		keys = append(keys, node.next[level].key)
		node = node.next[level]
	}
	return keys
}

func (s *SkipList[K, V]) Values() []V {
	values := make([]V, 0)
	level := 0
	node := s.head
	for node.next[level] != nil {
		values = append(values, node.next[level].value)
		node = node.next[level]
	}
	return values
}

func (s *SkipList[K, V]) debug_print() {
	fmt.Printf("\n")

	for level := s.level; level >= 0; level-- {
		if s.head.next[level] == nil {
			continue
		}
		fmt.Printf("%2.2v: ", level)
		for node := s.head.next[level]; node != nil; node = node.next[level] {
			width := node.width[level]
			fmt.Print("--", strings.Repeat("------", width-1), node.key, "(", width, ")")
		}
		fmt.Println("")
	}
	fmt.Println("")
}

// Run a sanity check on GetAt -- match with the slow linear search
func (s *SkipList[K, V]) debug_sanity_check_getat(index int, key K, value V, found bool) {
	count := 0
	var node *skipNode[K, V]
	for node = s.head.next[0]; node != nil && count < index; node = node.next[0] {
		count++
	}
	if node == nil && !found {
		return
	}
	if node == nil {
		panic("GETAT")
	} else {
		nv := fmt.Sprintf("%v", node.value)
		v := fmt.Sprintf("%v", value)
		if node.key != key || nv != v {
			panic("GetAT")
		}
	}
}

// Run a sanity check on the skip list
func (s *SkipList[K, V]) debug_validate() bool {
	keys := s.Keys()
	sortedKeys := Sort(keys)
	if !reflect.DeepEqual(keys, sortedKeys) {
		// Keys not sorted
		fmt.Printf("debug_validate: not sorted\n k:=%+v\n s:=%+v\n", keys, sortedKeys)
		return false
	}
	for i := 0; i < len(keys); i++ {
		k, v, found := s.GetAt(i)
		if !found || k != keys[i] {
			fmt.Printf("debug_validate: not consistent\n expect key=%+v got k=%+v v=%+v index=%+v\n", keys[i], k, v, i)
			return false
		}
	}
	return true
}
