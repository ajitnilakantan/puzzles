package internal

import (
	"container/heap"
)

type Graph[N comparable] interface {
	get_successors(node N) []N
}

type GraphWithWeights[N comparable] interface {
	Graph[N]
	// Get distance between two adjacent nodes
	get_dist_between(node N, neighbour N) float64
}

type GraphWithHeuristic[N comparable] interface {
	GraphWithWeights[N]
	// Estimated movement cost from the current position to the goal.
	// This should be an underestimate for the shortest path solution.
	get_heuristic(node N, goal N) float64
}

type PriorityItem[T any] struct {
	priority float64
	item     T
}
type MinPriorityQueue[T comparable] []PriorityItem[T]

// Implement container/heap Interface
func (h MinPriorityQueue[T]) Len() int           { return len(h) }
func (h MinPriorityQueue[T]) Less(i, j int) bool { return h[i].priority < h[j].priority }
func (h MinPriorityQueue[T]) Swap(i, j int)      { h[i], h[j] = h[j], h[i] }

func (h *MinPriorityQueue[T]) Push(item any) {
	// Push and Pop use pointer receivers because they modify the slice's length,
	// not just its contents.
	val, _ := item.(PriorityItem[T])
	*h = append(*h, val)
}
func (h *MinPriorityQueue[T]) Pop() any {
	// Push and Pop use pointer receivers because they modify the slice's length,
	// not just its contents.
	old := *h
	n := len(old)
	x := old[n-1]
	*h = old[0 : n-1]
	return x
}

// Helper
func (h MinPriorityQueue[T]) Contains(val T) bool {
	for _, item := range h {
		if item.item == val {
			return true
		}
	}
	return false
}

type node_and_path[N comparable] struct {
	node N
	path []N
}

func DijkstraSolve[N comparable](graph GraphWithWeights[N], start N, goal N) []N {
	// The set of discovered nodes that may need to be (re-)expanded.
	// Initially, only the start node is known.
	// Pop heap from low to high
	frontier := make(MinPriorityQueue[N], 0)
	heap.Init(&frontier)
	heap.Push(&frontier, PriorityItem[N]{0, start})

	// For node n, cameFrom[n] is the node immediately preceding it
	// on the cheapest path from start to n currently known.
	came_from := map[N]N{}

	// Actual movement cost to each position from the start position
	cost_so_far := map[N]float64{start: 0}

	for len(frontier) > 0 {
		current := heap.Pop(&frontier).(PriorityItem[N]).item

		if current == goal || graph.get_dist_between(current, goal) == 0 {
			// Found path
			path := []N{}
			for from, ok := came_from[current]; ok; from, ok = came_from[current] {
				path = append(path, current)
				current = from
			}
			path = append(path, start)
			path = Reverse(path)
			return path
		}

		neighbors := graph.get_successors(current)
		for _, neighbor := range neighbors {
			// The distance from start to a neighbor
			new_cost := cost_so_far[current] + graph.get_dist_between(current, neighbor)
			var neighbor_in_cost_sofar bool
			_, neighbor_in_cost_sofar = cost_so_far[neighbor]
			if !neighbor_in_cost_sofar || new_cost < cost_so_far[neighbor] {
				// This path is the best until now. Record it!
				cost_so_far[neighbor] = new_cost
				priority := new_cost
				heap.Push(&frontier, PriorityItem[N]{priority, neighbor})
				came_from[neighbor] = current
			}
		}
	}

	return []N{}
}

func AStarSolve[N comparable](graph GraphWithHeuristic[N], start N, goal N) []N {
	// The set of discovered nodes that may need to be (re-)expanded.
	// Initially, only the start node is known.
	// Pop heap from low to high
	frontier := make(MinPriorityQueue[N], 0)
	heap.Init(&frontier)
	heap.Push(&frontier, PriorityItem[N]{0, start})

	// For node n, cameFrom[n] is the node immediately preceding it
	// on the cheapest path from start to n currently known.
	came_from := map[N]N{}

	// Actual movement cost to each position from the start position
	cost_so_far := map[N]float64{start: 0}

	for len(frontier) > 0 {
		current := heap.Pop(&frontier).(PriorityItem[N]).item

		if current == goal || graph.get_dist_between(current, goal) == 0 {
			// Found path
			path := []N{}
			for from, ok := came_from[current]; ok; from, ok = came_from[current] {
				path = append(path, current)
				current = from
			}
			path = append(path, start)
			path = Reverse(path)
			return path
		}

		neighbors := graph.get_successors(current)
		for _, neighbor := range neighbors {
			// The distance from start to a neighbor
			new_cost := cost_so_far[current] + graph.get_dist_between(current, neighbor)
			var neighbor_in_cost_sofar bool
			_, neighbor_in_cost_sofar = cost_so_far[neighbor]
			if !neighbor_in_cost_sofar || new_cost < cost_so_far[neighbor] {
				// This path is the best until now. Record it!
				cost_so_far[neighbor] = new_cost
				priority := new_cost + graph.get_heuristic(neighbor, goal)
				heap.Push(&frontier, PriorityItem[N]{priority, neighbor})
				came_from[neighbor] = current
			}
		}
	}

	return []N{}
}

func BFS[N comparable](graph Graph[N], start N, goal N) []N {

	queue := []node_and_path[N]{{start, []N{start}}}
	for len(queue) > 0 {
		// pop first element
		vertex, path := queue[0].node, queue[0].path
		queue = queue[1:]
		// get neighbours, excluding nodes already in path
		neighbours := MakeSet(graph.get_successors(vertex)).Subtract(MakeSet(path))
		for next := range neighbours {
			newPath := append(path, next)
			if next == goal {
				// can yield here to get all paths
				return newPath
			} else {
				queue = append(queue, node_and_path[N]{next, newPath})
			}
		}
	}

	return nil
}

/*
// Compute Tarjan connectivity. Return set of strongly connected nodes
// See: https://en.m.wikipedia.org/wiki/Tarjan%27s_strongly_connected_components_algorithm
func Tarjan[N comparable](graph Graph[N], vertex Set[N]) Set[N] {
	strongconnect := func()
	index := 0
    S := empty stack
    for each v in V do
        if v.index is undefined then
            strongconnect(v)
}
*/
