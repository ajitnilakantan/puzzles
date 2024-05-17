package internal

import (
	"testing"

	"github.com/stretchr/testify/assert"
)

func TestDay16(t *testing.T) {
	data :=
		`Valve AA has flow rate=0; tunnels lead to valves DD, II, BB
		Valve BB has flow rate=13; tunnels lead to valves CC, AA
		Valve CC has flow rate=2; tunnels lead to valves DD, BB
		Valve DD has flow rate=20; tunnels lead to valves CC, AA, EE
		Valve EE has flow rate=3; tunnels lead to valves FF, DD
		Valve FF has flow rate=0; tunnels lead to valves EE, GG
		Valve GG has flow rate=0; tunnels lead to valves FF, HH
		Valve HH has flow rate=22; tunnel leads to valve GG
		Valve II has flow rate=0; tunnels lead to valves AA, JJ
		Valve JJ has flow rate=21; tunnel leads to valve II`
	assert.NotEqual(t, 2, len(split_lines(data)))
	lines := split_lines(data)
	// Skip first blank
	tokens := split_line_regex(lines[0], "Valve\x20|\x20has\x20flow\x20rate=|;\x20tunnel.*valve[s]?\x20")[1:]
	nodename := tokens[0]
	nodeval := my_atoi(tokens[1])
	successors := split_line_regex(tokens[2], "[\x20,]+")
	assert.Equal(t, "AA", nodename)
	assert.Equal(t, 0, nodeval)
	assert.Equal(t, []string{"DD", "II", "BB"}, successors)
	//fmt.Printf("tokens = %v = '%v' -> '%v' '%v' '%v' (len=%v)\n", len(tokens), tokens, nodename, nodeval, successors, len(successors))

	graph := read16_graph(lines)
	//fmt.Printf("Graph=\n%+v\n", graph)
	result := bfs16(graph, "AA", 30)
	assert.Equal(t, 1651, result)

	// part b
	graph = read16_graph(lines)
	//fmt.Printf("Graph=\n%+v\n", graph)
	result = dfs16(graph, "AA", 26)
	assert.Equal(t, 1707, result)

}
