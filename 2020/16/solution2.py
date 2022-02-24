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
    # print(f"setname = {set_name} ranges = {ranges},  {range1}, {range2}")
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
    # print(f"toks1 = {toks}")
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

# print(f"your_tickets = {your_tickets}")
# print(f"nearby_tickets = {nearby_tickets}")
result = 0

invalid_entries = []
valid_nearby_tickets = []
for n in nearby_tickets:
    valid_ticket = True
    for t in n:
        found = False
        for k,v in valid_sets.items():
            if t in v[0] or t in v[1]:
                found = True
                break
        if not found:
            valid_ticket = False
            invalid_entries.append(t)
    if valid_ticket:
        valid_nearby_tickets.append(n)
# print(f"Invalid entries = {invalid_entries}")

# print(f"valid_nearby_tickets({len(valid_nearby_tickets)}) = {valid_nearby_tickets}")
result = sum(invalid_entries)

valid_assignments = []
for n in valid_nearby_tickets:
    valid_assignment_list = []
    for t in range(len(n)):
        valid_assignment = set()
        for k,v in valid_sets.items():
            if n[t] in v[0] or n[t] in v[1]:
                valid_assignment.add(k)
        valid_assignment_list.append(valid_assignment)
    valid_assignments.append(valid_assignment_list)




# print(f"valid_assignments = {valid_assignments}")
intersect = valid_assignments[0]
for i in range(1, len(valid_assignments)):
    for j in range(len(intersect)):
        intersect[j] = intersect[j].intersection(valid_assignments[i][j])

print(f"intersected_valid_assignments = {intersect}")

while True:
    for i in range(len(intersect)):
        if len(intersect[i]) == 1:
            element = next(iter(intersect[i]))   #  Get first element
            for j in range(len(intersect)):
                if i != j and element in intersect[j]:
                    intersect[j].remove(element)

    all_single = True
    for i in range(len(intersect)):
        if len(intersect[i]) != 1:
            all_single = False
            break
    if all_single:
        break

print(f"final intersected_valid_assignments = {intersect}")

result = 1
your_ticket = your_tickets
print(f"your_ticket = {your_ticket}")

for i in range(len(intersect)):
    element = next(iter(intersect[i]))
    if element.startswith("departure"):
        result *= your_ticket[i]
print(f"Solution2 = {result}")
