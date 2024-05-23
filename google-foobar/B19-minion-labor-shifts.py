from collections import Counter

def solution(data, n):
    ret = []
    counter = Counter()
    for d in data:
        counter[d] += 1
    for d in data:
        if counter[d] <= n:
            ret.append(d)
    return ret


assert(solution([1, 2, 3], 0) == [])
assert(solution([1, 2, 2, 3, 3, 3, 4, 5, 5], 1) == [1, 4])
