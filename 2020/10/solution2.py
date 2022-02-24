# with open("Input.txt") as f:
#    grps = [x.strip().split() for x in f.read().split("\n\n")]

import collections
import numpy as np

with open("Input.txt", "r") as fp:
    lines = fp.readlines()

# remove whitespace characters like `\n` at the end of each line
lines = [x.strip() for x in lines]
nums = [int(x) for x in lines]

device_joltage = max(nums) + 3
nums.append(device_joltage)
nums.append(0)

nums = sorted(nums)


def dfs(u, t, G):
    # See: https://cs.stackexchange.com/questions/3078/algorithm-that-finds-the-number-of-simple-paths-from-s-to-t-in-g  # noqa
    global u_npaths
    if u == t:
        return 1
    else:
        # if not u.npaths:
        if not u_npaths[u]:
            # assume sum returns 0 if u has no children
            # u_npaths[u] = sum(dfs(c, t, G) for c in u.children)
            u_npaths[u] = sum(dfs(c, t, G) for c in G.get(u, []))
        return u_npaths[u]

G = {}
seen = set()
for i in range(len(nums)):
    to = []
    for j in range(i+1, len(nums)):
        if nums[j] - nums[i] <= 3:
            to.append(j)
        else:
            break
    G[i] = to

print(f"G = {G} 0 to {len(nums)-1}")
# ret = DFS(0, len(nums)-2, G, seen) # returns 2


u_npaths = [0 for _ in range(len(nums))]
ret = dfs(0, len(nums)-1, G)
print(ret)


def count_paths2(nums):
    count = [0 for _ in range(len(nums))]
    count[0] = 1
    for n in range(1, len(nums)):
        for i in range(0, n):
            if nums[n] - nums[i] <= 3:
                count[n] += count[i]
    print(nums)
    print(count)
    return count[-1]


ret = count_paths2(nums)
print(f"count_paths2 = {ret}")

def adjacency_matrix(nums):
    # https://www.reddit.com/r/adventofcode/comments/kb7qt0/2020_day_10_part_2_python_number_of_paths_by/
    n_lines = len(nums)
    f = lambda i, j: j > i and nums[j] - nums[i] <= 3
    m = np.fromfunction(np.vectorize(f), (n_lines, n_lines), dtype=int).astype(int)
    aux = np.identity(n_lines)

    sol_part_2 = 0
    for _ in range(n_lines):
        aux = aux @ m
        sol_part_2 += aux[0, n_lines - 1]
    return sol_part_2

def adjacency_matrix_optimized(nums):
    n = len(nums)
    f = lambda i, j : j > i and nums[j] - nums[i] <= 3
    m = np.fromfunction(np.vectorize(f), (n, n), dtype=np.int64).astype(np.int64)
    m[n-1, n-1] = 1

    aux = np.linalg.matrix_power(m, len(nums))

    ans = aux[0, len(nums) - 1]
    return ans


ret = adjacency_matrix_optimized(nums)
print(f"adjacency_matrix_optimized = {ret}")

ret = adjacency_matrix(nums)
print(f"adjacency_matrix = {ret}")
