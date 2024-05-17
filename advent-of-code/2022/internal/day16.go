package internal

import (
	_ "embed"
	"fmt"
	"strings"

	_ "github.com/stretchr/testify/assert"
	"golang.org/x/exp/maps"
)

var (
	//go:embed input/input16.txt
	input16 string
)

type node16 struct {
	name         string
	successors   []string
	pressureFlow int
	open         bool
}

type graph16 map[string]node16

func read16_graph(lines []string) graph16 {
	graph := make(graph16)
	for _, line := range lines {
		tokens := split_line_regex(line, "Valve\x20|\x20has\x20flow\x20rate=|;\x20tunnel.*valve[s]?\x20")[1:]
		nodename := tokens[0]
		pressure := my_atoi(tokens[1])
		successors := split_line_regex(tokens[2], "[\x20,]+")
		graph[nodename] = node16{name: nodename, successors: successors, pressureFlow: pressure, open: false}
	}
	return graph
}

func bfs16(graph graph16, start string, maxSteps int) int {
	type vertex struct {
		name             string
		openedValves     Set[string]
		pressureReleased int
		timeElapsed      int
	}
	memKey := func(v vertex) string {
		return v.name + "-" + strings.Join(Sort(v.openedValves.Members()), "_")
	}
	type memVal struct {
		pressureReleased int
		timeElapsed      int
	}
	history := make(map[string]memVal)

	// number of valves with non-zero flow
	countFunc := func(cur int, next string) int {
		if graph[next].pressureFlow > 0 {
			return cur + 1
		} else {
			return cur
		}
	}
	numValves := Reduce(maps.Keys(graph), 0, countFunc)

	// add start node
	queue := []vertex{{name: start, openedValves: make(Set[string]), pressureReleased: 0, timeElapsed: 0}}

	maxReleased := 0

	for len(queue) > 0 {
		// pop first element
		vertex := queue[0]
		queue = queue[1:]

		// get neighbours. Graph has cycles so nodes can repeat. Can open a valve only once.
		neighbours := graph[vertex.name].successors
		if !vertex.openedValves.Contains(vertex.name) && graph[vertex.name].pressureFlow > 0 {
			neighbours = append(neighbours, "OPENVALVE")
		}
		for _, next := range neighbours {
			new_vertex_name := next
			new_vertex := vertex
			// Deep copy
			new_vertex.openedValves = vertex.openedValves.Clone()

			if new_vertex_name == "OPENVALVE" {
				// Stay on the node, and open the valve
				new_vertex_name = vertex.name
				new_vertex.name = new_vertex_name
				new_vertex.openedValves.Add(new_vertex_name)
				new_vertex.timeElapsed += 1
				new_vertex.pressureReleased += (maxSteps - new_vertex.timeElapsed) * graph[new_vertex_name].pressureFlow
			} else {
				new_vertex.name = new_vertex_name
				new_vertex.timeElapsed += 1
			}

			h, ok := history[memKey(new_vertex)]
			if ok && new_vertex.timeElapsed >= h.timeElapsed && new_vertex.pressureReleased <= h.pressureReleased {
				// Better result achieved.  Prune search
				continue
			}
			history[memKey(new_vertex)] = memVal{pressureReleased: new_vertex.pressureReleased, timeElapsed: new_vertex.timeElapsed}

			if maxReleased < new_vertex.pressureReleased {
				maxReleased = new_vertex.pressureReleased
			}

			if len(new_vertex.openedValves) >= numValves {
				// No point in continuing search. All valves opened
				continue
			}

			if new_vertex.timeElapsed >= maxSteps {
				// can yield here to get all paths
			} else {
				queue = append(queue, new_vertex)
			}
		}
	}

	return maxReleased
}

func dfs16(graph graph16, start string, maxSteps int) int {
	type vertex struct {
		name1            string
		name2            string
		openedValves     Set[string]
		pressureReleased int
		unopenedFlow     int // sum of unopened valves
		timeElapsed      int
	}
	memKey := func(v vertex) string {
		name := strings.Join(Sort([]string{v.name1, v.name2}), "")
		return name + "-" + strings.Join(Sort(v.openedValves.Members()), "_")
	}
	type memVal struct {
		pressureReleased int
		timeElapsed      int
	}
	history := make(map[string]memVal)

	// number of valves with non-zero flow
	countFunc := func(cur int, next string) int {
		if graph[next].pressureFlow > 0 {
			return cur + 1
		} else {
			return cur
		}
	}
	numValves := Reduce(maps.Keys(graph), 0, countFunc)

	// sum of all possible flows
	sumFunc := func(cur int, next string) int {
		return cur + graph[next].pressureFlow
	}
	totalFlow := Reduce(maps.Keys(graph), 0, sumFunc)

	// Add start pair
	queue := []vertex{{name1: start, name2: start, openedValves: make(Set[string]), pressureReleased: 0, unopenedFlow: totalFlow, timeElapsed: 0}}

	maxReleased := 0

	for len(queue) > 0 {
		// pop pair of elements from end (LIFO)
		vertex := queue[len(queue)-1]
		queue = queue[0 : len(queue)-1]

		// get neighbours. Graph has cycles so nodes can repeat. Can open a valve only once.
		neighbours1 := graph[vertex.name1].successors
		if !vertex.openedValves.Contains(vertex.name1) && graph[vertex.name1].pressureFlow > 0 {
			neighbours1 = append(neighbours1, "OPENVALVE")
		}
		neighbours2 := graph[vertex.name2].successors
		if !vertex.openedValves.Contains(vertex.name2) && graph[vertex.name2].pressureFlow > 0 {
			neighbours2 = append(neighbours2, "OPENVALVE")
		}
		// take the cartesian product
		neighbours := make([][]string, 0)
		for _, n1 := range neighbours1 {
			for _, n2 := range neighbours2 {
				neighbours = append(neighbours, []string{n1, n2})
			}
		}

		for _, next := range neighbours {
			new_vertex_name1, new_vertex_name2 := next[0], next[1]
			new_vertex := vertex
			// Deep copy
			new_vertex.openedValves = vertex.openedValves.Clone()

			if new_vertex_name1 == "OPENVALVE" && new_vertex_name2 == "OPENVALVE" && vertex.name1 == vertex.name2 {
				// Both cannot be opened in the same step
				continue
			}
			if new_vertex_name1 == "OPENVALVE" {
				// Stay on the node, and open the valve
				new_vertex_name1 = vertex.name1
				new_vertex.name1 = new_vertex_name1
				new_vertex.openedValves.Add(new_vertex_name1)
				new_vertex.pressureReleased += (maxSteps - new_vertex.timeElapsed - 1) * graph[new_vertex_name1].pressureFlow
				new_vertex.unopenedFlow -= graph[new_vertex_name1].pressureFlow
			} else {
				new_vertex.name1 = new_vertex_name1
			}

			if new_vertex_name2 == "OPENVALVE" {
				// Stay on the node, and open the valve
				new_vertex_name2 = vertex.name2
				new_vertex.name2 = new_vertex_name2
				new_vertex.openedValves.Add(new_vertex_name2)
				new_vertex.pressureReleased += (maxSteps - new_vertex.timeElapsed - 1) * graph[new_vertex_name2].pressureFlow
				new_vertex.unopenedFlow -= graph[new_vertex_name2].pressureFlow
			} else {
				new_vertex.name2 = new_vertex_name2
			}

			new_vertex.timeElapsed += 1

			// memoize results
			h, ok := history[memKey(new_vertex)]
			if ok && new_vertex.timeElapsed >= h.timeElapsed && new_vertex.pressureReleased <= h.pressureReleased {
				// Better result achieved.  Prune search
				continue
			}
			history[memKey(new_vertex)] = memVal{pressureReleased: new_vertex.pressureReleased, timeElapsed: new_vertex.timeElapsed}

			if maxReleased < new_vertex.pressureReleased {
				maxReleased = new_vertex.pressureReleased
				// fmt.Printf("Max=%v len(queue)=%v time=%v\n", maxReleased, len(queue), new_vertex.timeElapsed)
			}

			if len(new_vertex.openedValves) >= numValves {
				// No point in continuing search. All valves opened
				continue
			}

			if new_vertex.pressureReleased+(maxSteps-new_vertex.timeElapsed-1)*new_vertex.unopenedFlow < maxReleased {
				// Even opening all remaining valves now won't get past current max.
				// No point in continuing search
				continue
			}

			if new_vertex.timeElapsed >= maxSteps {
				// can yield here to get all paths
			} else {
				queue = append(queue, new_vertex)
			}
		}
	}

	return maxReleased
}

func part16a(log Log) int {
	lines := split_lines(input16)

	graph := read16_graph(lines)
	result := bfs16(graph, "AA", 30)

	return result
}

func part16b(log Log) int {
	lines := split_lines(input16)

	graph := read16_graph(lines)
	result := dfs16(graph, "AA", 26) // bfs takes too much memory

	return result
}

func (t *AOC) Day16(log Log) {
	answer1 := part16a(log)
	fmt.Printf("answer1 := '%v'\n", answer1) // 1862
	answer2 := part16b(log)
	fmt.Printf("answer2 := '%v'\n", answer2) // 2422
}
