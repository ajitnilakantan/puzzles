package internal

import (
	_ "embed"
	"fmt"
	"regexp"
	"runtime"
	"strings"

	_ "github.com/stretchr/testify/assert"
)

var (
	//go:embed input/input19.txt
	input19 string
)

type item_type int

const (
	first_item19 item_type = iota
	ore          item_type = 0
	clay         item_type = 1
	obsidian     item_type = 2
	geode        item_type = 3
	num_items19  item_type = 4
)

type inventory19 struct {
	items [num_items19]byte
}

type search19 struct {
	inventory19

	robots [num_items19]byte

	time byte
}

type blueprint19 struct {
	to_make [num_items19]inventory19
}

func read19_recipe(line string) inventory19 {
	// read a single line e.g.
	//   Each obsidian robot costs 3 ore and 14 clay
	//   Each clay robot costs 2 ore
	regex := `(.+?((\d+)\sore)){0,1}(.+?((\d+)\sclay)){0,1}(.+?((\d+)\sobsidian)){0,1}`
	re := regexp.MustCompile(regex)
	rs := re.FindStringSubmatch(line)
	//fmt.Printf("Recipe : '%+v'\n", rs)
	//for i, tt := range rs {
	//	fmt.Printf("  '%v' = '%+v'\n", i, tt)
	//}
	atoi := func(s string) int {
		if s == "" {
			return 0
		} else {
			return my_atoi(s)
		}
	}
	inventory := inventory19{}
	inventory.items[ore] = byte(atoi(rs[3]))
	inventory.items[clay] = byte(atoi(rs[6]))
	inventory.items[obsidian] = byte(atoi(rs[9]))
	inventory.items[geode] = 0
	return inventory
}

func read19_blueprints(lines []string) []blueprint19 {
	blueprints := make([]blueprint19, 0)
	for _, line := range lines {
		// Bluprint NN: remove leading blueprint number
		line = line[strings.Index(line, ":")+1:]
		// Break into sentences. Each for the cost of each robot
		regex := `(.*?)\.(.*?)\.(.*?)\.(.*?)\.`
		re := regexp.MustCompile(regex)
		rs := re.FindStringSubmatch(line)

		blueprint := blueprint19{}
		blueprint.to_make[ore] = read19_recipe(rs[1])
		blueprint.to_make[clay] = read19_recipe(rs[2])
		blueprint.to_make[obsidian] = read19_recipe(rs[3])
		blueprint.to_make[geode] = read19_recipe(rs[4])
		blueprints = append(blueprints, blueprint)
	}

	return blueprints
}

// func have19_enough(inven)
func get19_moves(node search19, blueprint blueprint19, max_cost inventory19) []search19 {
	have_enough_for_robot := func(item item_type, node search19, blueprint blueprint19) bool {
		for i := first_item19; i < num_items19; i++ {
			if node.items[i] < blueprint.to_make[item].items[i] {
				return false
			}
		}
		return true
	}
	purchase_robot := func(item item_type, node *search19, blueprint blueprint19) {
		for i := first_item19; i < num_items19; i++ {
			(*node).items[i] -= blueprint.to_make[item].items[i]
		}
		(*node).robots[item] += 1
	}
	mine_new_items := func(node *search19) {
		for item := first_item19; item < num_items19; item++ {
			(*node).items[item] += node.robots[item]
		}
	}

	successors := make([]search19, 0)

	for item := first_item19; item < num_items19; item++ {
		if have_enough_for_robot(item, node, blueprint) {
			new_node := node
			mine_new_items(&new_node)
			purchase_robot(item, &new_node, blueprint)
			new_node.time += 1
			successors = append(successors, new_node)
		}
	}
	// nothing purchased. just generate
	if Any(node.inventory19.items[:], func(index int, val byte) bool { return val < max_cost.items[index] && node.robots[index] > 0 }) {
		// Heuristic - don't accumulate if a robot purchase is possible
		new_node := node
		mine_new_items(&new_node)
		new_node.time += 1
		successors = append(successors, new_node)
	}

	return successors
}

func run19_blueprint(blueprint blueprint19, time_limit int) int {
	// Run a DFS
	visited := make(Set[search19]) // set of visited states /time
	// Keep track of minimum time to make "n" geodes
	time_to_geode := make([]byte, 255)
	for i := range time_to_geode {
		time_to_geode[i] = byte(time_limit) + 1
	}
	//fmt.Printf("run  bp %v to %v\n", blueprint, time_limit)

	// Calculate the max cost of any robot.  We should not accumulate more than
	// this number of items. Cuts down the search space
	max_cost := inventory19{}
	for _, item := range blueprint.to_make {
		for mineral := first_item19; mineral < num_items19; mineral++ {
			if max_cost.items[mineral] < item.items[mineral] {
				max_cost.items[mineral] = item.items[mineral]
			}
		}
	}

	max_geodes := 0
	start := search19{}
	start.robots[ore] = 1 // Start with 1 ore robot
	queue := make([]search19, 0)
	queue = append(queue, start)
	for len(queue) > 0 {
		// pop pair of elements from end (LIFO / DFS)
		node := queue[len(queue)-1]
		queue = queue[0 : len(queue)-1]

		// get neighbours
		for _, next := range get19_moves(node, blueprint, max_cost) {

			if visited.Contains(next) {
				continue
			}
			visited.Add(next)
			if next.items[geode] != node.items[geode] {
				// new geode added.
				if next.time < time_to_geode[next.items[geode]] {
					// update min time to make "n" geodes
					time_to_geode[next.items[geode]] = next.time
					//fmt.Printf("time to %v =  %v visited = %v queue=%v\n", next.items[geode], next.time, len(visited), len(queue))
					if len(visited) > 8*1024*1024 {
						visited = make(Set[search19])
						runtime.GC()
					}
				}
			}

			/*
				better_found := func(cur_time byte, cur_num_geodes byte) bool {
					for i := cur_num_geodes; i < byte(len(time_to_geode)); i++ {
						if time_to_geode[i] == byte(time_limit)+1 {
							return false
						}
						if cur_time > time_to_geode[i] {
							return true
						}
					}
					return false
				}
			*/
			if next.time > time_to_geode[next.items[geode]+1] || next.time > time_to_geode[next.items[geode]+2] || next.time > time_to_geode[next.items[geode]+3] {
				//if better_found(next.time, next.items[geode]) {
				// faster path to "n+" geodes already found
				// fmt.Printf("New geode # %v old %v time: %v timeto %v\n", next.inventory19.items[geode], node.inventory19.items[geode], next.time, time_to_geode[next.inventory19.items[geode]+1])
				continue
			}

			if max_geodes < int(next.items[geode]) {
				max_geodes = int(next.items[geode])
			}
			if next.time >= byte(time_limit) {
				// timeout
				continue
			} else {

				queue = append(queue, next)
			}
		}
	}

	return max_geodes
}

func part19a(log Log) int {
	lines := split_lines(input19)
	blueprints := read19_blueprints(lines)
	result := 0
	for index, blueprint := range blueprints {
		result += (index + 1) * run19_blueprint(blueprint, 24)
	}

	return result
}

func part19b(log Log) int {
	lines := split_lines(input19)
	blueprints := read19_blueprints(lines)[0:3]
	result := 1
	for _, blueprint := range blueprints {
		val := run19_blueprint(blueprint, 32)
		result *= val
	}

	return result
}

func (t *AOC) Day19(log Log) {
	answer1 := part19a(log)
	fmt.Printf("answer1 := '%v'\n", answer1) // 1681
	answer2 := part19b(log)
	fmt.Printf("answer2 := '%v'\n", answer2) // 5394
}
