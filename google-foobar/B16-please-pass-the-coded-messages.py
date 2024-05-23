"""
If a number is divisible by 3, the sum of its digits are also divisible by 3.
Divide the digits into buckets of (mod % 3) == 0, 1, and 2 -- sort all
Choose all in the 0 bucket
Choose two from bucket 1 and two from bucket 2 at a time (adds 4 digits at a time)
Choose three at a time from the 1 bucket (adds 3 digits at a time)
Choose three at a time from the 2 bucket (adds 3 digits at a time)
Any remaining in 1 and 2 -- pair up (adds 2 digits at a time)
Concatenate and sort result
"""
def solution(l):
    mod0 = sorted([x for x in l if x%3 == 0])
    mod1 = sorted([x for x in l if x%3 == 1])
    mod2 = sorted([x for x in l if x%3 == 2])
    result = mod0
    while len(mod1) >= 2 and len(mod2) >= 2:
        result.append(mod1.pop())
        result.append(mod1.pop())
        result.append(mod2.pop())
        result.append(mod2.pop())
    while len(mod1) >= 3:
        result.append(mod1.pop())
        result.append(mod1.pop())
        result.append(mod1.pop())
    while len(mod2) >= 3:
        result.append(mod2.pop())
        result.append(mod2.pop())
        result.append(mod2.pop())
    while len(mod1) > 0 and len(mod2) > 0:
        result.append(mod1.pop())
        result.append(mod2.pop())

    # Sort and concatenate digits
    if len(result) == 0:
        return 0
    result.sort(reverse = True)
    num = int(''.join(map(str, result)))
    #print("num={}".format(num))
    return num


#assert(solution([3, 1, 4, 1]) == 4311)
#assert(solution([3, 1, 4, 1, 5, 9]) == 94311)
#assert(solution([0]) == 0)
#assert(solution([0, 0]) == 0)
#assert(solution([8, 8, 8, 1, 1, 3, 0]) == 883110)
