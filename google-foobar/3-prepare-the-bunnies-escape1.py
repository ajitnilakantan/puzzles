# Modify simple recursive (with backtracking) maze solver
# https://www.techiedelight.com/find-shortest-path-in-maze/
# Added the num_broken_walls variable to the check

# Check if it is possible to go to (x, y) from current position. The
# function returns false if the cell has value 0 or already visited
def isSafe(mat, M, N, visited, x, y, num_broken_walls):
    if not isValid(x, y, M, N):
        return (False, num_broken_walls)
    elif visited[x][y]:
        return (False, num_broken_walls)
    elif mat[x][y] == 0:
        return (True, num_broken_walls)
    elif mat[x][y] == 1 and num_broken_walls < 1:
        num_broken_walls += 1
        return (True, num_broken_walls)
    else:
        return (False, num_broken_walls)
    #return not (mat[x][y] == 1 or visited[x][y])


# if not a valid position, return false
def isValid(x, y, M, N):
    return M > x >= 0 and N > y >= 0


# Find Shortest Possible Route in a Matrix mat from source cell (0, 0)
# to destination cell (x, y)

# 'min_dist' stores length of longest path from source to destination
# found so far and 'dist' maintains length of path from source cell to
# the current cell (i, j)

def findShortestPath(mat, visited, i, j, x, y, min_dist, dist, num_broken_walls):

    M = len(mat)
    N = len(mat[0])

    # if destination is found, update min_dist
    if i == x and j == y:
        return min(dist, min_dist)

    # set (i, j) cell as visited
    visited[i][j] = 1

    # go to bottom cell
    (is_safe, num_broken_walls) = isSafe(mat, M, N, visited, i + 1, j, num_broken_walls)
    if is_safe:
        min_dist = findShortestPath(mat, visited, i + 1, j, x, y, min_dist, dist + 1, num_broken_walls)

    # go to right cell
    (is_safe, num_broken_walls) = isSafe(mat, M, N, visited, i, j + 1, num_broken_walls)
    if is_safe:
        min_dist = findShortestPath(mat, visited, i, j + 1, x, y, min_dist, dist + 1, num_broken_walls)

    # go to top cell
    (is_safe, num_broken_walls) = isSafe(mat, M, N, visited, i - 1, j, num_broken_walls)
    if is_safe:
        min_dist = findShortestPath(mat, visited, i - 1, j, x, y, min_dist, dist + 1, num_broken_walls)

    # go to left cell
    (is_safe, num_broken_walls) = isSafe(mat, M, N, visited, i, j - 1, num_broken_walls)
    if is_safe:
        min_dist = findShortestPath(mat, visited, i, j - 1, x, y, min_dist, dist + 1, num_broken_walls)

    # Backtrack - Remove (i, j) from visited matrix
    visited[i][j] = 0

    return min_dist


def print_map(map, path=None):
    for m in range(0, len(map)):
        for n in range(0, len(map[0])):
            if path and (m, n) in path:
                print(f"*", end='')
            else:
                print(f"{map[m][n]}", end='')
        print("")
if __name__ == '__main__':

    mat = [
        [1, 1, 1, 1, 1, 0, 0, 1, 1, 1],
        [0, 1, 1, 1, 1, 1, 0, 1, 0, 1],
        [0, 0, 1, 0, 1, 1, 1, 0, 0, 1],
        [1, 0, 1, 1, 1, 0, 1, 1, 0, 1],
        [0, 0, 0, 1, 0, 0, 0, 1, 0, 1],
        [1, 0, 1, 1, 1, 0, 0, 1, 1, 0],
        [0, 0, 0, 0, 1, 0, 0, 1, 0, 1],
        [0, 1, 1, 1, 1, 1, 1, 1, 0, 0],
        [1, 1, 1, 1, 1, 0, 0, 1, 1, 1],
        [0, 0, 1, 0, 0, 1, 1, 0, 0, 1]
    ]

    M = N = 10

    mat = [[0, 1, 1, 0], [0, 0, 0, 1], [1, 1, 0, 0], [0,0,0,0], [1, 1, 1, 0]]
    mat = [[0, 0, 0, 0, 0, 0], [1, 1, 1, 1, 1, 0], [0, 0, 0, 0, 0, 0], [0, 1, 1, 1, 1, 1], [0, 1, 1, 1, 1, 1], [0, 0, 0, 0, 0, 0]]
    mat = [[0, 0, 0, 0, 0, 0], [1, 1, 1, 1, 1, 0], [1, 1, 1, 0, 0, 0], [0, 0, 0, 0, 1, 1], [1, 1, 1, 1, 1, 1], [0, 0, 0, 0, 0, 0]]
    M = len(mat)
    N = len(mat[0])
    print(f"M={M}, N={N}")

    # construct a matrix to keep track of visited cells
    visited = [[0 for x in range(N)] for y in range(M)]

    #min_dist = findShortestPath(mat, visited, 0, 0, 7, 5, float('inf'), 0)
    min_dist = findShortestPath(mat, visited, 0, 0, M-1, N-1, float('inf'), 0, num_broken_walls=0)

    if min_dist != float('inf'):
        print_map(mat)
        print("The shortest path from source to destination has length", min_dist)
        print(visited)
        print_map(visited)
    else:
        print("Destination can't be reached from source")
        print_map(mat)

def solution(map):
    M = len(map)
    N = len(map[0])
    # construct a matrix to keep track of visited cells
    visited = [[0 for x in range(N)] for y in range(M)]
    min_dist = findShortestPath(map, visited, 0, 0, M-1, N-1, float('inf'), 0, num_broken_walls=0)

    return min_dist + 1 if min_dist != float('inf') else -1
