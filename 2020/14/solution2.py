import re

def bit_not(n: int, numbits: int = 64):
    """ Python's ints are signed, so use this instead of tilda """
    return (1 << numbits) - 1 - n


with open("Input.txt", "r") as fp:
    lines = fp.readlines()

mask = ""
zero_mask = 0xFFFF
ones_mask = 0
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
        mask = mask[7:]
        continue

    toks = line
    toks = toks.replace("[", "")
    toks = toks.replace("]", "")
    toks = toks.replace(" ", "")
    toks = toks.replace("mem", "")
    toks = toks.split("=")
    location = int(toks[0])
    value = int(toks[1])
    location |= ones_mask

    numx = mask.count("X")
    # use regex to find nth postions of X in mask
    xloc = [m.start() for m in re.finditer(r"X", mask)]
    for n in range(1<<numx):
        new_location = location
        for b in range(0, numx):
            if n & (1 << b):
                # bit set
                new_location |= (1 << (35-xloc[b]))
            else:
                # bit not set
                new_location &= ~(1 << (35-xloc[b]))
        new_location &= 0b111111111111111111111111111111111111
        results[new_location] = value

result = 0
for k,v in results.items():
    result += v

print(f"Solution2 = {result}")  # 4574598714592
