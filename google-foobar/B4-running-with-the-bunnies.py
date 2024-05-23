class Node:
    """
        A node class for Pathfinding
    """
    def __init__(self, id):
        self.id = id
        # Array of tuple (Node, cost)
        self.children=[]
    def __eq__(self, other):
        return self.id == other.id
    def __str__(self):
        """Gives a short string representation of the variable."""
        return "Node({self.id}, children={[(c[0].id, c[1]) for c in self.children]})".format(self.id,[(c[0].id, c[1]) for c in self.children]);
    def __repr__(self):
        """Gives a precise string representation of the variable."""
        return "Node({self.id}, children={[(c[0].id, c[1]) for c in self.children]})".format(self.id,[(c[0].id, c[1]) for c in self.children]);

    def bunny_id(self):
        return self.id - 1
    def add_child(self, child):
        """ child is a tuple (Node, edge_cost) """
        self.children.append(child)

def create_graph(times):
    """ Convert the supplied 2D list into a graph of Nodes. Return first, last and list of all nodes """
    nodes = [Node(i) for i in range(len(times))]
    for node_index, node in enumerate(times):
        for edge_index, edge_cost in enumerate(node):
            if node_index == edge_index:
                # Don't loop back to yourself
                continue
            nodes[node_index].add_child( (nodes[edge_index], edge_cost) )
            

    # Return (start_node, end_node)
    return (nodes[0], nodes[len(times)-1], nodes)

def bfs_traverse_graph(start_node, end_node, times_limit, all_nodes):
    """ BFS traversal of nodes. Accumulate all solutions. Return the best one. """
    # Accumulate found solutions
    solutions = [] # set of sets

    # Keep track of the state of visited nodes: node#, times_limit, bunnies_collected.
    # If we re-visit a node in the same state, we are looping
    visited = [ {} for n in range(0, len(all_nodes)) ]

    # List of list of tuples ( (node, bunnies_collected_set, current_search_path_str, time_so_far) )
    # Each sublist in the list is a path, starting at start_node
    queue = [ [(start_node, set(), "0 ", times_limit)] ]

    done = False
    while queue and not done:
        # pop the first path from the queue
        path = queue.pop(0)
        # get the last node from the path
        node = path[-1]
        node_node = node[0] # Node
        node_bunnies_collected = node[1] # set(int)
        node_current_search_path_str = node[2] # string
        node_times_limit = node[3] # int

        # Add to visited
        hashkey = node_times_limit * 1000 + len(node_bunnies_collected)
        if not visited[node_node.id].get(hashkey):
            visited[node_node.id][hashkey] = [node_bunnies_collected]
        else:
            visited[node_node.id][hashkey].append(node_bunnies_collected)

        for child in node_node.children:
            child_node = child[0] # Node
            child_edge_cost = child[1] # int
            child_bunnies_collected = node_bunnies_collected # copy initial value from parent
            child_current_search_path_str = node_current_search_path_str + str(child_node.id) + " "
            child_times_limit = node_times_limit - child_edge_cost

            # Check if already visited under similar circumstances
            hashkey = child_times_limit * 1000 + len(child_bunnies_collected)
            if not visited[child_node.id].get(hashkey):
                visited[child_node.id][hashkey] = [child_bunnies_collected]
            elif child_bunnies_collected in visited[child_node.id][hashkey]:
                # Already visited
                continue
            else:
                visited[child_node.id][hashkey].append(child_bunnies_collected)

            # You can't go from negative to negative.  And also, cannot go to end_node with a negative. Can go from negative to >= 0
            if not (node_times_limit < 0 and child_times_limit < 0) and not (child_times_limit < 0 and child_node == end_node):
                if child_node != start_node and child_node != end_node and not child_node.bunny_id() in child_bunnies_collected:
                    child_bunnies_collected = set(child_bunnies_collected)
                    child_bunnies_collected.add(child_node.bunny_id())
                if child_node == end_node:
                    solutions.append(child_bunnies_collected)
                    if len(child_bunnies_collected) == len(all_nodes)-2:
                        done = True
                        break

                # Create new path
                new_path = list(path)
                new_path.append( (child_node, child_bunnies_collected, child_current_search_path_str, child_times_limit) )
                queue.append(new_path)

    solutions = sorted(solutions, key = lambda i: (-len(i), list(i)))
    result = list(solutions[0])
    #print(f"ZZZ got result {result}")
    return result

def solution(times, times_limit):
    """ times is an NxN matrix giving the costs to go from node_rownumber to node_columnnumber.
        times_limit should not be exceeded before reaching the goal """
    start_node, end_node, all_nodes = create_graph(times)
    return bfs_traverse_graph(start_node, end_node, times_limit, all_nodes)


assert([1, 2] == solution([[0, 2, 2, 2, -1], [9, 0, 2, 2, -1], [9, 3, 0, 2, -1], [9, 3, 2, 0, -1], [9, 3, 2, 2, 0]], 1))
assert([0, 1] == solution([[0, 1, 1, 1, 1], [1, 0, 1, 1, 1], [1, 1, 0, 1, 1], [1, 1, 1, 0, 1], [1, 1, 1, 1, 0]], 3) )
