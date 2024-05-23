from math import floor
def solution(y, x):
    sum_x = sum(x)
    sum_y = sum(y)
    ret = int( floor( (sum_x-sum_y) / sum_x * 100.0 + 0.5 ) )
    #print(ret)
    return ret

assert(solution(x = [22.2, 46, 100.8], y = [23, 11.1, 50.4]) == 50)
assert(solution(y = [1.0], x = [1.0]) == 0)
assert(solution(y = [2.2999999999999998, 15.0, 102.40000000000001, 3486.8000000000002], x = [23.0, 150.0, 1024.0, 34868.0]) == 90)
