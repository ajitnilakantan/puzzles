def solution(x, y):
    x = sorted(x)
    y = sorted(y)
    if len(x) > len(y):
        x,y = y,x
    for index,n in enumerate(x):
        if n != y[index]:
            return y[index]
    return y[-1]

assert(solution([13, 5, 6, 2, 5], [5, 2, 5, 13]) == 6)
assert(solution([14, 27, 1, 4, 2, 50, 3, 1], [2, 4, -4, 3, 1, 1, 14, 27, 50]) == -4)
