def my_key(val):
    # Split each revnumber
    rev = val.split(".")
    num_rev = len(rev)
    major = rev[0]
    minor = -1 if num_rev < 2 else int(rev[1])
    revision =  -1 if num_rev < 3 else int(rev[2])
    return [major, minor, revision]

def solution(l):
    sorted_l = sorted(l, key=my_key)
    return sorted_l

assert(solution(["1.11", "2.0.0", "1.2", "2", "0.1", "1.2.1", "1.1.1", "2.0"]) == ["0.1","1.1.1","1.2","1.2.1","1.11","2","2.0","2.0.0"])
assert(solution(["1.1.2", "1.0", "1.3.3", "1.0.12", "1.0.2"]) == ["1.0","1.0.2","1.0.12","1.1.2","1.3.3"])
