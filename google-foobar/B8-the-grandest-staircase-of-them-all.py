"""
Searching on "list sum to a number" leads to the "subset sum problem" (https://en.wikipedia.org/wiki/Subset_sum_problem)
which can be solved somewhat efficiently using dynamic programming.
"""

# Based on https://stackoverflow.com/questions/18305843/find-all-subsets-that-sum-to-a-particular-value
def total_subsets_matching_sum(numbers, sum):
    array = [1] + [0] * (sum)
    for current_number in numbers:
        for num in range(sum - current_number, -1, -1):
            if array[num]:
                array[num + current_number] += array[num]
    return array[sum]

def solution(n):
    numbers = [x for x in range(1, n)]
    return total_subsets_matching_sum(numbers, n)

assert(solution(200) == 487067745)
assert(solution(3) == 1)
