"""
This can be easily solved by A-star,  but since the board is so small and there are
a lot of symmetries we can be lazy and pre-compute all the solutions in advance and have an O(1)
lookup of the solution.
"""

###############################################################################
## BEGIN A-STAR SEARCH
##
# See: https://code.activestate.com/recipes/578919-python-a-pathfinding-with-binary-heap/
# Author: Christian Careaga (christian.careaga7@gmail.com)
# A* Pathfinding in Python (2.7)
# Please give credit if used

from heapq import *


def heuristic(a, b):
    # Use the Manhattan distance
    return abs(b[0] - a[0]) + abs(b[1] - a[1])

def astar(start, goal):
    """ Knight's move from start to goal. start and goal are tuples (0,0) and (posx, posy) """

    board_height = 8
    board_width = 8
    neighbors = [(1,-2),(1,2),(-1,2),(-1,-2), (2,-1), (2,1), (-2,-1), (-2,1)]

    close_set = set()
    came_from = {}
    gscore = {start:0}
    fscore = {start:heuristic(start, goal)}
    oheap = []

    heappush(oheap, (fscore[start], start))

    while oheap:

        current = heappop(oheap)[1]

        if current == goal:
            data = [start]
            while current in came_from:
                data.append(current)
                current = came_from[current]
            return len(data)-1 #data

        close_set.add(current)
        for i, j in neighbors:
            neighbor = current[0] + i, current[1] + j            
            tentative_g_score = gscore[current] + heuristic(current, neighbor)
            # Check bounds
            if not 0 <= neighbor[0] < board_height or not 0 <= neighbor[1] < board_width:
                continue

            if neighbor in close_set and tentative_g_score >= gscore.get(neighbor, 0):
                continue

            if tentative_g_score < gscore.get(neighbor, 0) or neighbor not in [i[1]for i in oheap]:
                came_from[neighbor] = current
                gscore[neighbor] = tentative_g_score
                fscore[neighbor] = tentative_g_score + heuristic(neighbor, goal)
                heappush(oheap, (fscore[neighbor], neighbor))

    return 0 # None
##
## END A-STAR SEARCH
###############################################################################

def pre_compute_all():
    precomputed = {}
    coord0 = (0, 0)
    for dest in range(0, 64):
        coord1 = (dest%8, dest//8)
        delta_x = abs(coord1[0] - coord0[0])
        delta_y = abs(coord1[1] - coord0[1])
        if (delta_x > delta_y):
            delta_x, delta_y = delta_y, delta_x
        if precomputed.get((delta_x, delta_y)):
            continue
        sol = astar(coord0, coord1)
        precomputed[(delta_x, delta_y)] = sol
    print("precomputed_results = {}".format(precomputed))

pre_compute_all()

precomputed_results = {(0, 0): 0, (0, 1): 3, (0, 2): 2, (0, 3): 3, (0, 4): 2, (0, 5): 3, (0, 6): 4, (0, 7): 5, (1, 1): 4, (1, 2): 1, (1, 3): 2, (1, 4): 3, (1, 5): 4, (1, 6): 3, (1, 7): 4, (2, 2): 4, (2, 3): 3, (2, 4): 2, (2, 5): 3, (2, 6): 4, (2, 7): 5, (3, 3): 2, (3, 4): 3, (3, 5): 4, (3, 6): 3, (3, 7): 4, (4, 4): 4, (4, 5): 3, (4, 6): 4, (4, 7): 5, (5, 5): 4, (5, 6): 5, (5, 7): 4, (6, 6): 4, (6, 7): 5, (7, 7): 6}

def solution(src, dest):
    coord0 = (src%8, src//8)
    coord1 = (dest%8, dest//8)
    delta_x = abs(coord1[0] - coord0[0])
    delta_y = abs(coord1[1] - coord0[1])
    if (delta_x > delta_y):
        delta_x, delta_y = delta_y, delta_x
    # sol = astar(coord0, coord1)
    # Use the precomputed lookup table created by "pre_compute_all()"
    sol = precomputed_results[(delta_x, delta_y)]
    return sol

assert(solution(0, 1) == 3)
assert(solution(19, 36) == 1)
