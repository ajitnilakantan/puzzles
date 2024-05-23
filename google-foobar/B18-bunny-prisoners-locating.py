"""
|11
| 7 12
| 4  8 13
| 2  5  9 14
| 1  3  6 10 15

When y=1, the x coordinate follows the arithmetic series sum(i=1..x) (i) (where x starts counting at 1) 
  which equals f(x) = x * (x+1) / 2

The y coordinate also follows the arithmetic series with an offset: 
Given an x coordinate and f(x) as above, series along 'x' follows and y >= 2
  f(y) = f(x) + sum(x, x+1, x+2, ... x+(y-1)) 
       = f(x) + (y-1) * (x+(x+(y-2))) // 2
"""
def solution(x, y):
    fx = x*(x+1) // 2
    fy = fx + (y-1) * (x + (x+(y-2))) // 2
    return str(fy)

assert(solution(1, 1) == "1")
assert(solution(1, 4) == "7")
assert(solution(3, 1) == "6")
assert(solution(3, 2) == "9")
assert(solution(5, 10) == "96")
