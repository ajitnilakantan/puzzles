"""
Every "<" moving left will salute each ">" to the left of it once.
We iterate up the string, counting up for eacho ">".  Each time you hit a "<" moving left, you
increment the salute count by twice the ">" count (twice since > and < salute each other)
"""

def solution(s):
    num_salutes = 0
    num_gt = 0
    for c in s:
        if c == ">":
            num_gt += 2
        elif c == "<":
            num_salutes += num_gt
    return num_salutes

assert(2 == solution(">----<"))
assert(4 == solution("<<>><"))
assert(10 == solution("--->-><-><-->-"))
