from math import floor, log
import decimal
"""
The most generous payout doubles each level (max out rule 2):
    Sequence 1, 2, 4, ... 2**(n-1)  when n = 1...
    The total payout is geometric series sum(i=1..n)(2**(i-1)) = (2**n-1)
The most stingy sequence follows the Fibonacci sequence (apply rule 3 to the minimum)
    Sequence 1, 1, 2, 3, 5, 8, ... Fn(n)  where n=0...
    The total payout is sum(i=1..n)(Fn(i)) = Fn(n+2) - 1  (see https://en.wikipedia.org/wiki/Fibonacci_number#Combinatorial_identities)

Given n,  you can pay floor(log_2(n+1)) henchmen using the most generous scheme
Given n,  you can pay n(F) = floor(log_phi((F+1) * sqrt5 + 1/2)) - 1 henchmen
    where phi = (1+sqrt(5))/2 =approx= 1.618
(See: https://en.wikipedia.org/wiki/Fibonacci_number#Computation_by_rounding)
"""


def nF(F):
    """ Given a Fibonacci number, return its sequence index.  E.g.
        for '5' return '4'  (from 1, 1, 2, 3, 5, ...) """
    # TODO: Figure out how much precision is actually required
    decimal.getcontext().prec = 1000
    sqrt5 = decimal.Decimal(5).sqrt()
    phi = (1 + sqrt5) / 2
    n = floor(log(F * sqrt5 + decimal.Decimal(0.5), phi))
    return int(n)


def solution(total_lambs):
    num_generous = int(floor(log(total_lambs+1, 2)))
    num_stingy = nF(total_lambs+1) - 2
    #print(f"stingy = {num_stingy} gen = {num_generous}")
    return num_stingy - num_generous


assert(solution(143) == 3)
assert(solution(10) == 1)
