def solution(xs):
    # Separate the negative (nonzero) and postive (nonzero) numbers.  Pick the largest even number of negative
    # an all positive numbers for the product
    positive = [x for x in xs if x >= 1]
    negative = [x for x in xs if x <= -1]
    negative.sort()
    # A lot of edge cases to handle:
    if len(negative) == 1 and len(positive) == 0:
        return '0' if 0 in xs else str(negative[0])
    if len(negative) % 2 == 1:
        # Keep an even number, so remove the smallest in absolute value
        negative.pop()
    if len(negative) == 0 and len(positive) == 0:
        return "0"
    prod = 1L
    for x in negative:
        prod *= x
    for x in positive:
        prod *= x
    return str(prod)

assert(solution([2,-3,1,0,-5]) == "30")
assert(solution([2, 0, 2, 2, 0]) == "8")
assert(solution([-2, -3, 4, -5]) == "60")
