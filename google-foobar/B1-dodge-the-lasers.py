# This Python file uses the following encoding: utf-8
"""
- Searching for 'floor(sqrt(2)) + floor(2*sqrt(2)) + floor(3*sqrt(2)) ...' immediately yields the Beatty Sequence (A001951)
- See: https://en.wikipedia.org/wiki/Beatty_sequence
       https://math.stackexchange.com/questions/2052179/how-to-find-sum-i-1n-left-lfloor-i-sqrt2-right-rfloor-a001951-a-beatty-s
       https://math.stackexchange.com/questions/2096603/how-to-evaluate-this-sum-sum-limits-i-1n-left-lfloor-sqrt2-cdot-i (which uses Sturmian sequences)
- Rayleigh's Theorem:
  For any s in a Beatty sequence floor(k * s),   floor(k * t) is also a "complementary" Beatty sequence where 1/s + 1/t = 1
- For the problem, s = sqrt(2)  and 1/sqrt(2) + 1/t = 1 ==> t = 2 + sqrt(2) = 2 + s
- From Rayleigh's Theorem, the sequence of B(s, i) and B(t, i) (Beatty sequences for s, t) are "complementary" 
  and cover the natural numbers without repetition.  e.g.
  B(s, i): OEIS A001951:  1, 2,  4, 5, 7, 8, 9, 11, 12, 14, 15,   floor(i * sqrt(2))
  i = 1..11
  B(t, j): OEIS A001952:       3,     6,      10,     13,         floor(i * (2+sqrt(2))) = 2i + floor(i*sqrt(2)) = 2i+B(s, i)
  j = 1..4
- Adding B(s, i) i=1..11  and   B(t, j) j=1..4  
   =  Σ(i=1..11) B(s, i) +  Σ(j=1..4) B(t, j)
   =  Σ (k=1..n) k  where n=15  = n(n+1)/2    (where n=15)
  But B(t, j) can also be rewritten so
   =  Σ(i=1..11) B(s, i) +  Σ(j=1..4) B(s, j)  +  Σ(j=1..4) (2*j)
   =  Σ(i=1..11) B(s, i) +  Σ(j=1..4) B(s, j)  +  2*m(m+1)/2  (where m=4)
- The upper limits for the B(s) and B(t) sums are related:
  If you sum B(s, i) from i = 1..N  then the upper limit of B(t, j) is floor(N * (s / t))
  i.e. you sum B(t, j) from j = 1..floor(N * (sqrt(2) / (2+sqrt(2))))

- We can rewrite:
- Σ{i=1..N} B(s,i)  +   Σ{j=1..M} B(t,j)  =  Σ{k=1..P)} k   =   (P)(P+1)/2
  where M = floor(N * (sqrt(2) / (2+sqrt(2))))
    and P = B(s,N) = floor(N*sqrt(2))
 Substituting:   Σ{j=1..M} B(t,j)  =  (M)(M+1)  +  Σ{j=1..M} B(s,j)
 We get:
- Σ{i=1..N} B(s,i) + (M)(M+1)  +  Σ{j=1..M} B(s,j) =  (P)(P+1)/2
  ==> Σ{i=1..N} B(s,i) =  (P)(P+1)/2  - (M)(M+1)  -  Σ{j=1..M} B(s,j)
- Therefore we have the reccurence relation:
  Write SIGMA_B(s,N) =  Σ{i=1..N} B(s,i)
  ==> SIGMA_B(s, N) = (P)(P+1)/2  - (M)(M+1)  -  SIGMA_B(s, M)
      where M = floor(N * (sqrt(2) / (2+sqrt(2))))
        and P = B(s,N) = floor(N*sqrt(2))
- The recurrence should converge fast because M approximately halves each iteration. I.e. (sqrt(2) / (2+sqrt(2))) = 0.4142  
"""
from math import ceil, floor, sqrt
import decimal

"""
# Sanity checking the Rayleigh Theorem
for N in [5, 77]:
    nums = []
    for i in range(1, N+1):
        bs_i = floor(i*sqrt(2.0)); nums.append(bs_i); #print(f"N={N} nums={nums}")

    print(nums)
    M = floor((N+1) * (sqrt(2) / (2+sqrt(2))))
    for i in range(1, M+1):
        bt_i = 2*i + floor(i*sqrt(2.0)); nums.append(bt_i); #print(f"M={M} nums={nums}")
    nums.sort(); print(f"sorted={nums}")
    if len(nums) != nums[-1]:
        print(f" ERROR N={N} M={M}")
"""


"""
# Solution below works, but we seem to lose precision with big
# numbers with sqrt(2).  Try using Decimal instead
def sigma_beatty(N):
    # print(f"Calculate sigma_beatty({N})")
    if N == 2:
        return 3
    elif N == 1:
        return 1
    else:
        M = long(floor(N * (sqrt(2.0) / (2+sqrt(2.0)))))
        P = long(floor(N*sqrt(2.0)))
        return (P)*(P+1)//2 - (M)*(M+1) - sigma_beatty(M)
"""
# Try use n: decimal.Decimal to maintain precision
def sigma_beatty(N, sqrt2):
    # print(f"Calculate sigma_beatty({N})")
    if N == 2:
        return 3
    elif N == 1:
        return 1
    else:
        M = (N * (sqrt2 / (2+sqrt2))).to_integral_value(rounding=decimal.ROUND_FLOOR)
        P = (N*sqrt2).to_integral_value(rounding=decimal.ROUND_FLOOR)
        return (P)*(P+1)//2 - (M)*(M+1) - sigma_beatty(M, sqrt2)

def solution(s):
    decimal.getcontext().prec = 1000
    sqrt2 = decimal.Decimal(2).sqrt()
    return str(sigma_beatty(decimal.Decimal(s), sqrt2))


# assert(solution('5') == "19")
# assert(solution('77') == "4208")

