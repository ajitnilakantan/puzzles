package internal

import (
	"errors"
	"sort"

	"golang.org/x/exp/constraints"
)

func Map[T, V any](ts []T, fn func(T) V) []V {
	result := make([]V, len(ts))
	for i, t := range ts {
		result[i] = fn(t)
	}
	return result
}

//lint:ignore U1000 Ignore unused function temporarily for debugging
func Filter[T any](ts []T, keepFilter func(T) bool) []T {
	result := make([]T, 0, len(ts))
	for _, t := range ts {
		if keepFilter(t) {
			result = append(result, t)
		}
	}
	return result
}

func IndexOf[E comparable](s []E, v E) int {
	for index, vs := range s {
		if v == vs {
			return index
		}
	}
	return -1
}
func Contains[E comparable](s []E, v E) bool {
	return IndexOf(s, v) != -1
}

func Reverse[E any](s []E) []E {
	result := make([]E, 0, len(s))
	for i := len(s) - 1; i >= 0; i-- {
		result = append(result, s[i])
	}
	return result
}

func Sort[E constraints.Ordered](s []E) []E {
	result := make([]E, len(s))
	copy(result, s)
	sort.Slice(result, func(i, j int) bool {
		return result[i] < result[j]
	})
	return result
}

func GetNilFor[T any]() T {
	return *new(T)
}

// MinMaxSlice returns the minimum index and value + maximum index and value element
// of slice s or an error in case the s is nil or empty
func MinMaxSlice[E constraints.Ordered](s []E) (int, E, int, E, error) {
	if len(s) == 0 {
		return 0, GetNilFor[E](), 0, GetNilFor[E](), errors.New("cannot find the maximum of a nil or empty slice")
	}

	minindex, maxindex := 0, 0
	min, max := s[maxindex], s[maxindex]
	for k := range s {
		if s[k] < max {
			minindex = k
			min = s[minindex]
		}
		if s[k] > max {
			maxindex = k
			max = s[maxindex]
		}
	}

	return minindex, min, maxindex, max, nil
}

// Reduce
type reduceFunc[E any, F any] func(F, E) F

func Reduce[E any, F any](s []E, init F, f reduceFunc[E, F]) F {
	cur := init
	for _, v := range s {
		cur = f(cur, v)
	}
	return cur
}

// Any returns true if f returns true for any element in arr
func Any[S ~[]T, T any](items S, f func(index int, el T) bool) bool {
	for index, el := range items {
		if f(index, el) {
			return true
		}
	}
	return false
}

// All returns true if f returns true for all elements in arr
func All[S ~[]T, T any](items S, f func(index int, el T) bool) bool {
	for index, el := range items {
		if !f(index, el) {
			return false
		}
	}
	return true
}
