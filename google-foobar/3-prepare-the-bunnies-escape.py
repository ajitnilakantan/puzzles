# See: https://code.activestate.com/recipes/578919-python-a-pathfinding-with-binary-heap/
# Author: Christian Careaga (christian.careaga7@gmail.com)
# A* Pathfinding in Python (2.7)
# Please give credit if used

from heapq import *


def heuristic(a, b):
    # Use the Manhattan distance
    return abs(b[0] - a[0]) + abs(b[1] - a[1])

def astar(array, start, goal):

    array_height = len(array)
    array_width = len(array[0])
    neighbors = [(0,1),(0,-1),(1,0),(-1,0)]

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
            return data

        close_set.add(current)
        for i, j in neighbors:
            neighbor = current[0] + i, current[1] + j            
            tentative_g_score = gscore[current] + heuristic(current, neighbor)
            # Check bounds
            if not 0 <= neighbor[0] < array_height or not 0 <= neighbor[1] < array_width:
                continue

            if array[neighbor[0]][neighbor[1]] == 1:
                continue

            if neighbor in close_set and tentative_g_score >= gscore.get(neighbor, 0):
                continue

            if tentative_g_score < gscore.get(neighbor, 0) or neighbor not in [i[1]for i in oheap]:
                came_from[neighbor] = current
                gscore[neighbor] = tentative_g_score
                fscore[neighbor] = tentative_g_score + heuristic(neighbor, goal)
                heappush(oheap, (fscore[neighbor], neighbor))

    return None

"""
def print_map(map, path=None):
    for m in range(0, len(map)):
        for n in range(0, len(map[0])):
            if path and (m, n) in path:
                print(f"*", end='')
            else:
                print(f"{map[m][n]}", end='')
        print("")
"""

def solution(map):
    start = (0, 0) # starting position
    end = (len(map)-1, len(map[0])-1) # ending position

    path = astar(map, start=start, goal=end)
    min_len = len(map)*len(map[0]) if path == None else len(path)
    # Brute force it
    for m in range(0, len(map)):
        for n in range(0, len(map[0])):
            if map[m][n] == 1:
                map[m][n] = 0
                path = astar(map, start=start, goal=end)
                new_len = len(map)*len(map[0]) if path == None else len(path)
                if new_len < min_len:
                    min_len = new_len
                map[m][n] = 1

    #print(f"min = {min_len}")
    #print_map(map)
    #print(f"len = {len(path)}")
    #print_map(map, path)
    #return -1 if path == None else len(path)
    return min_len

'''Here is an example of using my algo with a numpy array,
   astar(array, start, destination)
   astar function returns a list of points (shortest path)'''

"""
nmap = [
    [0,0,0,0,0,0,0,0,0,0,0,0,0,0],
    [1,1,1,1,1,1,1,1,1,1,1,1,0,1],
    [0,0,0,0,0,0,0,0,0,0,0,0,0,0],
    [1,0,1,1,1,1,1,1,1,1,1,1,1,1],
    [0,0,0,0,0,0,0,0,0,0,0,0,0,0],
    [1,1,1,1,1,1,1,1,1,1,1,1,0,1],
    [0,0,0,0,0,0,0,0,0,0,0,0,0,0],
    [1,0,1,1,1,1,1,1,1,1,1,1,1,1],
    [0,0,0,0,0,0,0,0,0,0,0,0,0,0],
    [1,1,1,1,1,1,1,1,1,1,1,1,0,1],
    [0,0,0,0,0,0,0,0,0,0,0,0,0,0]]

map = [[0, 0, 0, 0, 0, 0], [1, 1, 1, 1, 1, 0], [0, 0, 0, 0, 0, 0], [0, 1, 1, 1, 1, 1], [0, 1, 1, 1, 1, 1], [0, 0, 0, 0, 0, 0]]
map = [[0, 0, 0, 0, 0, 0], [1, 1, 1, 1, 1, 0], [0, 0, 0, 0, 0, 0], [0, 1, 1, 1, 1, 1], [0, 1, 1, 1, 1, 1], [0, 0, 0, 0, 0, 0]]
start = (0,0)
goal = (len(map)-1, len(map[0])-1)
path =  astar(map, start, goal)
print_map(map)
print(f"len = {len(path)}")
print_map(map, path)
"""
