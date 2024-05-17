package internal

import (
	"testing"

	"github.com/stretchr/testify/assert"
)

func TestDay08(t *testing.T) {
	data :=
		`30373
		25512
		65332
		33549
		35390`
	grid := read08_grid(split_lines(data))
	// t.Logf("grid = '%+v' grid[1][3]='%v'\n", grid, grid[1][3])
	assert.Equal(t, 1, grid[1][3])

	count := visible08_trees(grid)
	assert.Equal(t, 21, count)

	score := visible08_score(grid, 1, 2)
	assert.Equal(t, 5, grid[1][2])
	assert.Equal(t, 4, score)

	score = visible08_score(grid, 3, 2)
	assert.Equal(t, 5, grid[3][2])
	assert.Equal(t, 8, score)

	best_score := visible08_best_score(grid)
	assert.Equal(t, 8, best_score)
}
