from collections import Counter


with open("Input.txt") as f:
    lines = f.readlines()
# remove whitespace characters like `\n` at the end of each line
lines = [x.strip() for x in lines] 

counter = 0
for line in lines:
    tokens = line.split(" ")
    counts = tokens[0].split("-")
    min = int(counts[0])
    max = int(counts[1])
    chr = str(tokens[1])
    passwd = str(tokens[2])
    cnt = passwd.count(chr[0])
    ccc = Counter(passwd)
    cnt2 = ccc[chr]
    # print(f"{passwd.count(chr)} {cnt2}")
    matches = 0
    for i in range(len(passwd)):
        if passwd[i] == chr[0]:
            matches += 1
    print(f"'{min}' to '{max}' for '{chr}' in '{passwd}' len={len(passwd)} count={cnt} = {matches}")
    if min <= cnt <= max:
        counter += 1

print(f"Got {counter} good passwords out of {len(lines)}")
