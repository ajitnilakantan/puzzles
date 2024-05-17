package internal

import (
	_ "embed"
	"fmt"

	_ "github.com/stretchr/testify/assert"
	"golang.org/x/exp/slices"
)

var (
	//go:embed input/input18.txt
	input18 string
)

type xyz18 struct{ x, y, z float64 }

// The centres of the faces of a unit cube from 0,0,0 to 1,1,1
var face18_centres []xyz18 = []xyz18{
	// see https://www.mathworks.com/help/matlab/visualize/multifaceted-patches.html
	{0.5, 0.0, 0.5},
	{1.0, 0.5, 0.5},
	{0.5, 1.0, 0.5},
	{0.0, 0.5, 0.5},
	{0.5, 0.5, 0.0},
	{0.5, 0.5, 1.0},
}

// The normals corresponding to each of the faces of a unit cube from 0,0,0 to 1,1,1
var face18_normals []xyz18 = []xyz18{
	// see https://www.mathworks.com/help/matlab/visualize/multifaceted-patches.html
	{0.0, -1.0, 0.0},
	{1.0, 0.0, 0.0},
	{0.0, 1.0, 0.0},
	{-1.0, 0.0, 0.0},
	{0.0, 0.0, -1.0},
	{0.0, 0.0, 1.0},
}

// Return list of faces and their normals
func get18_open_faces(lines []string) ([]xyz18, map[xyz18]xyz18, map[xyz18]int, map[xyz18]xyz18) {
	// The unit cubes provided in the input. Value of lower corner
	cubes := make([]xyz18, 0)
	// Map of faces to cube.  Is unique for boundary faces.
	cubes_map := make(map[xyz18]xyz18) // map face -> cube
	// All faces
	faces := make(map[xyz18]int) // map of faces + count.  Count is 1 for boundary faces
	// All corresponding normals
	normals := make(map[xyz18]xyz18) // map face to normal

	// Add all faces
	for _, line := range lines {
		tokens := Map(split_line_regex(line, ","), my_atoi)
		x, y, z := float64(tokens[0]), float64(tokens[1]), float64(tokens[2])
		cubes = append(cubes, xyz18{x: x, y: y, z: z})

		for face_index, f := range face18_centres {
			// Add centres of each face
			centre := xyz18{x: x + f.x, y: y + f.y, z: z + f.z}
			if count, ok := faces[centre]; ok {
				faces[centre] = count + 1
			} else {
				faces[centre] = 1
			}
			// Add face normals
			normals[centre] = xyz18{x: face18_normals[face_index].x, y: face18_normals[face_index].y, z: face18_normals[face_index].z}
			// Add corresponding cube
			cubes_map[centre] = xyz18{x: x, y: y, z: z}
		}
	}

	return cubes, cubes_map, faces, normals
}

func count18_open_faces(lines []string) int {
	_, _, faces, _ := get18_open_faces(lines)
	// Return count of non-adjoining faces
	count := 0
	for _, v := range faces {
		if v == 1 {
			count++
		}
	}

	return count
}

func generate18_Grid(x int, y int, z int) [][][]byte {
	tiles := make([][][]byte, x)

	for i := range tiles {
		tiles[i] = make([][]byte, y)
		for j := range tiles[i] {
			tiles[i][j] = make([]byte, z)
		}
	}

	return tiles
}

func flood18_fill(grid [][][]byte, cubes []xyz18, seed xyz18, gridMin xyz18, gridMax xyz18, outside bool, fillColor byte) {
	queue := make([]xyz18, 0)
	queue = append(queue, seed)
	for len(queue) > 0 {
		top := queue[0]
		queue = queue[1:]
		if top.x < gridMin.x || top.x > gridMax.x || top.y < gridMin.y || top.y > gridMax.y || top.z < gridMin.z || top.z > gridMax.z {
			continue
		}
		topx, topy, topz := int(top.x-gridMin.x), int(top.y-gridMin.y), int(top.z-gridMin.z)
		if grid[topx][topy][topz] != 0 {
			// already visited
			continue
		}
		cube_num := slices.IndexFunc(cubes, func(v xyz18) bool { return v == top })
		var hit bool
		if outside && cube_num == -1 {
			hit = true
		} else if !outside && cube_num != -1 {
			hit = true
		} else {
			hit = false
		}
		if hit {
			topx, topy, topz := int(top.x-gridMin.x), int(top.y-gridMin.y), int(top.z-gridMin.z)
			grid[topx][topy][topz] = fillColor
			neighbors := []xyz18{{top.x - 1, top.y, top.z}, {top.x + 1, top.y, top.z}, {top.x, top.y - 1, top.z}, {top.x, top.y + 1, top.z}, {top.x, top.y, top.z - 1}, {top.x, top.y, top.z + 1}}
			queue = append(queue, neighbors...)
		}
	}
}

func count18_outside_faces(lines []string) int {
	// cubes, faces, normals := get18_open_faces(lines)
	cubes, cube_map, faces, normals := get18_open_faces(lines)

	// flood fill outside region / cubes. This will leave behind the "holes" unfilled
	minFunc := func(a, b float64) float64 {
		if a < b {
			return a
		}
		return b
	}
	maxFunc := func(a, b float64) float64 {
		if a > b {
			return a
		}
		return b
	}
	min := cubes[0]
	max := cubes[0]
	for _, c := range cubes {
		min.x = minFunc(min.x, c.x)
		min.y = minFunc(min.y, c.y)
		min.z = minFunc(min.z, c.z)
		max.x = maxFunc(max.x, c.x)
		max.y = maxFunc(max.y, c.y)
		max.z = maxFunc(max.z, c.z)
	}

	// Grid to flood fill
	gridMin := xyz18{min.x - 1, min.y - 1, min.z - 1}
	gridMax := xyz18{max.x + 1, max.y + 1, max.z + 1}
	grid := generate18_Grid(int(gridMax.x-gridMin.x+1), int(gridMax.y-gridMin.y+1), int(gridMax.z-gridMin.z+1))
	// Flood fill the "outside"
	flood18_fill(grid, cubes, gridMin, gridMin, gridMax, true, 2)
	// Flood fill the "cubes"

	flood18_fill(grid, cubes, cubes[0], gridMin, gridMax, false, 1)
	// The "holes" should be left uncolored at zero

	// Now loop through all the open faces.  Their normals should point to the "outside" which is colored "2"
	result := 0
	for face, count := range faces {
		if count != 1 {
			continue
		}
		cube := cube_map[face]
		normal := normals[face]
		gridx, gridy, gridz := int(cube.x+normal.x-gridMin.x), int(cube.y+normal.y-gridMin.y), int(cube.z+normal.z-gridMin.z)
		if grid[gridx][gridy][gridz] != 0 {
			result++
		}
	}

	return result
}

func part18a(log Log) int {
	lines := split_lines(input18)
	result := count18_open_faces(lines)

	return result
}

func part18b(log Log) int {
	lines := split_lines(input18)
	result := count18_outside_faces(lines)

	return result
}

func (t *AOC) Day18(log Log) {
	answer1 := part18a(log)
	fmt.Printf("answer1 := '%v'\n", answer1) // 4460
	answer2 := part18b(log)
	fmt.Printf("answer2 := '%v'\n", answer2) // 2498
}
