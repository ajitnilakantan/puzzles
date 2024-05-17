package internal

import (
	"testing"

	"github.com/stretchr/testify/assert"
)

func TestSet(t *testing.T) {
	a := []string{"a", "b", "c", "d", "e"}
	b := []string{"b", "e", "x", "y", "z"}

	sa := MakeSet(a)
	sb := MakeSet(b)

	t.Logf("sa = '%v'", &sa)
	t.Logf("sb = '%v'", sb)

	u := sa.Union(sb)
	i := sa.Intersect(sb)

	t.Logf("u = '%v'", u)
	t.Logf("i = '%v'", i)

	assert.True(t, u.Equals(MakeSet([]string{"a", "b", "c", "d", "e", "x", "y", "z"})))
	assert.True(t, i.Equals(MakeSet([]string{"b", "e"})))
}
