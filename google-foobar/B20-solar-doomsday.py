from math import floor, sqrt

def solution(area):
    result = []
    while area > 0:
        square_root = int(floor(sqrt(area)))
        square_size = square_root * square_root
        num_fit = area // square_size
        area -= num_fit * square_size
        result.extend(num_fit*[square_size])
    return result


assert(solution(15324) == [15129,169,25,1])
assert(solution(12) == [9,1,1,1])

