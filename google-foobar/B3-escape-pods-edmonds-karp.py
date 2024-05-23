"""
Searching for "network flow algorithms" leads to https://en.wikipedia.org/wiki/Network_flow_problem
which leads to https://en.wikipedia.org/wiki/Edmonds%E2%80%93Karp_algorithm which has a description
of the problem as well as a Python implementation of the solution in the
linked article: https://en.wikipedia.org/wiki/Ford%E2%80%93Fulkerson_algorithm
"""

### BEGIN CODE FROM WIKIPEDIA #################################################
# This code is taken from the Python implementation in: 
#   https://en.wikipedia.org/wiki/Ford%E2%80%93Fulkerson_algorithm#Python_implementation_of_Edmonds%E2%80%93Karp_algorithm 
###############################################################################
import collections
# This class represents a directed graph using adjacency matrix representation
class Graph:
  
    def __init__(self,graph):
        self.graph = graph  # residual graph
        self.ROW = len(graph)
  
    def bfs(self, s, t, parent):
        '''Returns true if there is a path from source 's' to sink 't' in
        residual graph. Also fills parent[] to store the path '''

        # Mark all the vertices as not visited
        visited = [False] * (self.ROW)
         
        # Create a queue for BFS
        queue = collections.deque()
         
        # Mark the source node as visited and enqueue it
        queue.append(s)
        visited[s] = True
         
        # Standard BFS loop
        while queue:
            u = queue.popleft()
         
            # Get all adjacent vertices's of the dequeued vertex u
            # If an adjacent has not been visited, then mark it
            # visited and enqueue it
            for ind, val in enumerate(self.graph[u]):
                if (visited[ind] == False) and (val > 0):
                    queue.append(ind)
                    visited[ind] = True
                    parent[ind] = u
 
        # If we reached sink in BFS starting from source, then return
        # true, else false
        return visited[t]
             
    # Returns the maximum flow from s to t in the given graph
    def edmonds_karp(self, source, sink):
 
        # This array is filled by BFS and to store path
        parent = [-1] * (self.ROW)
 
        max_flow = 0 # There is no flow initially
 
        # Augment the flow while there is path from source to sink
        while self.bfs (source, sink, parent):
 
            # Find minimum residual capacity of the edges along the
            # path filled by BFS. Or we can say find the maximum flow
            # through the path found.
            path_flow = float("Inf")
            s = sink
            while s != source:
                path_flow = min(path_flow, self.graph[parent[s]][s])
                s = parent[s]
 
            # Add path flow to overall flow
            max_flow += path_flow
 
            # update residual capacities of the edges and reverse edges
            # along the path
            v = sink
            while v !=  source:
                u = parent[v]
                self.graph[u][v] -= path_flow
                self.graph[v][u] += path_flow
                v = parent[v]
 
        return max_flow

### END CODE FROM WIKIPEDIA ###################################################
  


def solution(entrances, exits, path):
    """
    Solve using the Edmonds Karp algorithm.  The algorithm given assumes a single source and a single sink
    so we add two dummy rooms.
    """
    # Add dummy rooms as a single source and as a single sink
    num_rooms = len(path)
    assert(num_rooms == len(path[0]))
    # Extend by 2 (source + sink rooms)
    for p in path:
        p.append(0)
        p.append(0)
    path.append([0]*(num_rooms + 2))
    path.append([0]*(num_rooms + 2))

    # Create paths from the single source and sink to the provided entrances, exits
    CAPACITY = 2000000
    source = num_rooms
    sink = num_rooms+1
    for r in entrances:
        path[source][r] = CAPACITY
    for r in exits:
        path[r][sink] = CAPACITY

    g = Graph(path)
    ret = g.edmonds_karp(source, sink)
    return ret

#assert(6 == solution([0], [3], [[0, 7, 0, 0], [0, 0, 6, 0], [0, 0, 0, 8], [9, 0, 0, 0]]))
#assert(16 == solution([0, 1], [4, 5], [[0, 0, 4, 6, 0, 0], [0, 0, 5, 2, 0, 0], [0, 0, 0, 0, 4, 4], [0, 0, 0, 0, 6, 6], [0, 0, 0, 0, 0, 0], [0, 0, 0, 0, 0, 0]]))

