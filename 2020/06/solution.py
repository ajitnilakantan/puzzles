def count_unique_letters(line):
    s = set()
    for c in line:
        s.add(c)
    return len(s)

with open("Input.txt") as f:
    data = f.read()
    lines = data.split("\n\n")
    # lines = f.readlines()

# remove whitespace characters like `\n` at the end of each line
lines = [x.strip() for x in lines]
lines = [x.replace('\n', '') for x in lines]

#print(lines)
count = 0
for line in lines:
    count += count_unique_letters(line)
print(f"Part1: {count}")


### Part 2


def count_common_letters(line):
    sets = []
    lines = line.split('\n')
    for l in lines:
        s = set()
        for c in l:
            s.add(c)
        sets.append(s)
    intersect = sets[0]
    for s in sets:
        intersect = intersect.intersection(s)
    return len(intersect)

with open("Input.txt") as f:
    data = f.read()
    lines = data.split("\n\n")
    # lines = f.readlines()

# remove whitespace characters like `\n` at the end of each line
lines = [x.strip() for x in lines]
#lines = [x.replace('\n', '') for x in lines]

#print(lines)
count = 0
for line in lines:
    count += count_common_letters(line)
print(f"part2:  {count}")
