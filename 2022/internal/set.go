package internal

import (
	"fmt"
	"reflect"
)

//	type allowedHashTypes interface {
//		~string | ~int | ~uint | ~int64 | ~uint64 | ~int32 | ~uint32 | ~int16 | ~uint16 | ~int8 | ~uint8
//	}
type Set[T comparable] map[T]struct{}

var setMarker = struct{}{}

func MakeSet[T comparable](s []T) Set[T] {
	ret := make(Set[T])
	for _, v := range s {
		ret.Add(v)
	}
	return ret
}
func (s *Set[T]) Add(v T) {
	// Use pointer receivers because we modify the slice's length,
	// not just its contents.
	(*s)[v] = setMarker
}

func (s *Set[T]) Remove(el T) {
	// Use pointer receivers because we modify the slice's length,
	// not just its contents.
	delete(*s, el)
}

func (s Set[T]) Members() (o []T) {
	for k := range s {
		o = append(o, k)
	}
	return o
}

func (s Set[T]) Contains(el T) bool {
	_, found := s[el]
	return found
}

func (s Set[T]) Equals(t Set[T]) bool {
	return reflect.DeepEqual(s, t)
}

func (s Set[T]) Intersect(t Set[T]) Set[T] {
	intersect := []T{}
	for k := range t {
		if s.Contains(k) {
			intersect = append(intersect, k)
		}
	}
	return MakeSet(intersect)
}

func (s Set[T]) Subtract(t Set[T]) Set[T] {
	result := s.Clone()
	for k := range t {
		if result.Contains(k) {
			result.Remove(k)
		}
	}
	return result
}
func (s Set[T]) Clone() Set[T] {
	result := map[T]struct{}{}
	for k := range s {
		result[k] = setMarker
	}
	return result
}
func (s Set[T]) Union(t Set[T]) Set[T] {
	result := s.Clone()

	for k := range t {
		result.Add(k)
	}
	return result
}

func (s Set[T]) String() string {
	return fmt.Sprintf("%v", s.Members())
}
