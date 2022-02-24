def test(s,seq): # can we produce string s using rules list seq?
    if s == '' or seq == []: return s == '' and seq == [] # if both empty, True; if one, False
    r = rules[seq[0]]
    if '"' in r:
        return test(s[1:], seq[1:]) if s[0] in r else False # strip first character, False if cannot
    else:
        return any(test(s, t + seq[1:]) for t in r) # expand first term in seq rules list

def parse(s): # return rule pair (n,e) e.g. (2, [[2,3],[3,2]]) or (42,'"a"')
    n,e = s.split(": ")
    return (int(n),e) if '"' in e else (int(n), [[int(r) for r in t.split()] for t in e.split("|")])

rule_text, messages = [x.splitlines() for x in open("Input-modified.txt").read().split("\n\n")]
rules = dict(parse(s) for s in rule_text)
print("Part 1:", sum(test(m,[0]) for m in messages))

rule_text += ["8: 42 | 42 8","11: 42 31 | 42 11 31"]
rules = dict(parse(s) for s in rule_text)
print("Part 2:", sum(test(m,[0]) for m in messages))
