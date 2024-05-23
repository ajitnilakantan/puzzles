"""
Keep accumulating numbers from the given list.
If we exceed the target 't', pop from the head until we are <= 't'
Each time we push, we increment the tail index; on each pop, increment head index.
"""
def solution(l, t):
    index_head = 0
    index_tail = 0
    running_sum = 0
    sums = []
    while True:
        while running_sum < t and index_tail < len(l):
            sums.append(l[index_tail])
            running_sum += l[index_tail]
            index_tail += 1
        while running_sum > t:
            val = sums.pop(0)
            running_sum -= val
            index_head += 1
        if running_sum == t:
            return [index_head, index_tail-1]
        if index_tail >= len(l):
            return [-1, -1]
    return [-1, -1]


assert(solution([1, 2, 3, 4], 15) == [-1, -1])
assert(solution([4, 3, 10, 2, 8], 12) == [2, 3])
assert(solution([1, 2, 9, 10, 5, 5, 0], 10) == [3, 3])
assert(solution([1, 2, 9, 10, 5, 5, 30], 30) == [6, 6])
