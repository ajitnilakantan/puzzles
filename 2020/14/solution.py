import re

def bit_not(n: int, numbits: int = 64):
    """ Python's ints are signed, so use this instead of tilda """
    return (1 << numbits) - 1 - n


with open("Input.txt", "r") as fp:
    lines = fp.readlines()

zero_mask = 0xFFFF
ones_mask = 0
locations = []
values = []
results = {}

for line in lines:
    if line.startswith("mask"):
        mask = line.strip()
        zero_mask = 0xFFFFFFFFFFFFFFFF
        ones_mask = 0
        for i in range(len(mask)):
            if mask[-(i+1)] == '0':
                zero_mask &= bit_not(1<<i)
            if mask[-(i+1)] == '1':
                ones_mask |= 1<<i
        continue

    toks = line
    toks = toks.replace("[", "")
    toks = toks.replace("]", "")
    toks = toks.replace(" ", "")
    toks = toks.replace("mem", "")
    toks = toks.split("=")
    location = int(toks[0])
    value = int(toks[1])
    locations.append(location)
    values.append(value)
    results[location] = (value & zero_mask) | ones_mask

result = 0
for k,v in results.items():
    print(f" k = {k} v= {v}")
    result += v

print(f"Solution1 = {result}")  # 9628746976360
