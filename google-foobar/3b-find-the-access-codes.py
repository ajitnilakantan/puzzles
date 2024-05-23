# Fast division check (by precomputing for denominator)
# See: https://lemire.me/blog/2019/02/08/faster-remainders-when-the-divisor-is-a-constant-beating-compilers-and-libdivide
precompute_c = {}  # To memoize the "c" precomputed numbers
is_divisible_cache = {}
def is_divisible(num, factor):
    # Check if (num % factor) == 0
    cache_factor = is_divisible_cache.get(factor)
    if not cache_factor:
        c = precompute_c.get(factor)
        if not c:
            c = (0xFFFFFFFFFFFFFFFF // factor) + 1
            precompute_c[factor] = c

        is_div = (num * c) & 0xFFFFFFFFFFFFFFFF <= (c - 1) & 0xFFFFFFFFFFFFFFFF
        is_divisible_cache[factor] = {num: is_div}
        return is_div
    else:
        cache_val = cache_factor.get(num)
        if not cache_val:
            c = precompute_c.get(factor)
            if not c:
                c = (0xFFFFFFFFFFFFFFFF // factor) + 1
                precompute_c[factor] = c

            is_div = (num * c) & 0xFFFFFFFFFFFFFFFF <= (c - 1) & 0xFFFFFFFFFFFFFFFF
            is_divisible_cache[factor][num] = is_div
            return is_div
        else:
            return cache_val

"""
def fast_divide(num, factor):
    # Return num // factor
    c = precompute_c.get(factor)
    if not c:
        c = (0xFFFFFFFFFFFFFFFF // factor) + 1
        precompute_c[factor] = c

    return ((c * num) >> 64) & 0xFFFFFFFF
"""

def solution(l):
    total_count = 0
    num_divisors = [0] * len(l)
    for i in range(0, len(l)):
        for j in range(i + 1, len(l)):
            if is_divisible(l[j], l[i]):
                num_divisors[j] += 1
                total_count += num_divisors[i]

    return total_count

#print(solution([2, 2, 3, 3, 8, 8, 4, 4, 5, 5, 6, 6]))
#print(solution([1, 2, 3, 4, 5, 6]))
#print(solution([6, 5, 4, 3, 2, 1]))

#print(solution([1, 1, 1]))
#print(solution([1, 1, 1, 2, 1, 1, 1]))

#print(solution([9, 1, 2, 3, 4, 5, 6, 5, 4, 2, 1, 2, 3, 4, 5, 6, 4, 5, 6]))
