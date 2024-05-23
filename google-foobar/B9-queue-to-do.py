"""
The XOR of the first n numbers follow a pattern.  E.g. if f(n) =  0 xor 1 xor 2 .. xor n 
n:     0   1   2   3   4   5   6   7   8   9  10  11  12 
f(n):  0   1   3   0   4   1   7   0   8   1  11   0  12
       n   1 n+1   0   n   1 n+1   0   n   1 n+1   0   n

i.e. cycles through  n, 1, n+1, 0  if n%4 = 0, 1, 2, 3

For an arbitrary range R to L (inclusive) we can compute f(L) ^ f(R-1) since the numbers from 0 to R-1 (inclusive) get double
xored to 0. 

For a checkpoint grid like:
    start = 17; length = 4
17 18 19 20 /
21 22 23 / 24
25 26 / 27 28
29 / 30 31 32

f(start + 1*(length - 1)) ^ f(start - 0*length - 1) +
f(start + 2*(length - 1)) ^ f(start + 1*length - 1) +
f(start + 3*(length - 1)) ^ f(start + 2*length - 1) +
f(start + 4*(length - 1)) ^ f(start + 4*length - 1)
... "length" times


"""

def xor_series_to(n):
    """ Return 0 ^ 1 ^ 2 .. ^ n """
    if n <= 0:
        return 0
    residue = n % 4
    if residue == 0:
        return n
    elif residue == 1:
        return 1
    elif residue == 2:
        return n+1
    else:
        return 0

def solution(start, length):
    val = 0
    for i in range(length):
        val ^= xor_series_to(start + (i+1)*(length - 1)) ^ xor_series_to(start + (i)*length - 1)
    return val

assert(solution(0, 3) == 2)
assert(solution(17, 4) == 14)
