file_name = "Input.txt"

'''
with open(file_name, "r") as fp:
    lines = fp.readlines()
lines = [x.strip() for x in lines]

line = lines[0].strip()
toks = line.split(",")
nums = [int(n) for n in toks]
'''

'''
with open(file_name, "r") as fp:
    lines = fp.readlines()
lines = [x.strip() for x in lines]
'''

with open(file_name, "r") as fp:
    lines = fp.readlines()
lines = [x.strip() for x in lines]

line_no = 0
valid_sets = {}
valid_set_names = []
while lines[line_no] != "your ticket:":
    line = lines[line_no]
    line_no += 1
    if not line:
        continue
    toks = line.split(':')
    set_name = toks[0]
    ranges = toks[1].split(" or ")
    r = ranges[0].strip().split("-")
    range1 = set(range(int(r[0]), int(r[1])+1))
    r = ranges[1].strip().split("-")
    range2 = set(range(int(r[0]), int(r[1])+1))
    print(f"setname = {set_name} ranges = {ranges},  {range1}, {range2}")
    valid_sets[set_name] = [range1, range2]
    valid_set_names.append(set_name)
    toks = line.split(",")

your_tickets = []
line_no += 1
while lines[line_no] != "nearby tickets:":
    line = lines[line_no]
    line_no += 1
    if not line:
        continue
    toks = line.split(",")
    print(f"toks1 = {toks}")
    your_tickets += [int(x) for x in toks]

nearby_tickets = []
line_no += 1
while line_no < len(lines):
    line = lines[line_no]
    line_no += 1
    if not line:
        continue
    toks = line.split(",")
    nearby_tickets.append([int(x) for x in toks])

print(f"your_tickets = {your_tickets}")
print(f"nearby_tickets = {nearby_tickets}")
result = 0

invalid_entries = []
for n in nearby_tickets:
    for t in n:
        found = False
        for k,v in valid_sets.items():
            if t in v[0] or t in v[1]:
                found = True
                break
        if not found:
            invalid_entries.append(t)
print(f"Invalid entries = {invalid_entries}")
result = sum(invalid_entries)
print(f"Solution1 = {result}")
