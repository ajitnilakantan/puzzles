"""
Start at position [0,0] and go to [height-1][width-1] of the grid.
At the destination, want to minimize amount of food left, but still have >=0
"""

###############################################################################
## BEGIN A-STAR SEARCH
##
# See: https://code.activestate.com/recipes/578919-python-a-pathfinding-with-binary-heap/
# Author: Christian Careaga (christian.careaga7@gmail.com)
# A* Pathfinding in Python (2.7)
# Please give credit if used

from heapq import *


def heuristic(a, b, grid):
    # Use the Manhattan distance
    #return abs(b[0] - a[0]) + abs(b[1] - a[1])
    return -grid[b[0]][b[1]]

def astar(start, goal, food, grid):
    """ move from (0,0) to (posx, posy) """

    board_height = len(grid)
    board_width = len(grid[0])
    neighbors = [(1,0),(0,1)]

    close_set = set()
    came_from = {}
    gscore = {start:0}
    fscore = {start:heuristic(start, goal, grid)}
    fscore = {start:0}
    oheap = []

    heappush(oheap, (fscore[start], start))

    while oheap:

        current = heappop(oheap)[1]

        if current == goal:
            data = []
            while current in came_from:
                data.append(current)
                current = came_from[current]
            data.append(start)
            data.reverse()  # put in start..goal order
            return data
            #return len(data)-1 #data

        close_set.add(current)
        for i, j in neighbors:
            neighbor = current[0] + i, current[1] + j            
            # Check bounds
            if not 0 <= neighbor[0] < board_height or not 0 <= neighbor[1] < board_width:
                continue

            tentative_g_score = gscore[current] + heuristic(current, neighbor, grid)

            if neighbor in close_set and tentative_g_score >= gscore.get(neighbor, 0):
                continue
            if abs(tentative_g_score) > food:
                continue

            if tentative_g_score < gscore.get(neighbor, 0) or neighbor not in [i[1]for i in oheap]:
                came_from[neighbor] = current
                gscore[neighbor] = tentative_g_score
                fscore[neighbor] = tentative_g_score + heuristic(neighbor, goal, grid)
                heappush(oheap, (fscore[neighbor], neighbor))

    return None
##
## END A-STAR SEARCH
###############################################################################

def solution(food, grid):
    start = (0,0)
    goal = (len(grid)-1, len(grid[0])-1)
    result = astar(start, goal, food, grid)
    if result == None:
        return -1
    else:
        total = 0
        for cell in result:
            total += grid[cell[0]][cell[1]]

    return food - total

assert(solution(food = 7, grid = [[0, 2, 5], [1, 1, 3], [2, 1, 1]]) == 0)
assert(solution(food = 12, grid = [[0, 2, 5], [1, 1, 3], [2, 1, 1]]) == 1)
