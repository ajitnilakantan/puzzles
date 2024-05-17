import copy
import re

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

index = 0
rules = {}
while lines[index] != "":
    toks = lines[index].split(":")
    rule_no = int(toks[0].strip())
    rule = toks[1].strip()
    if rule.startswith('"'):
        rule = rule.strip('"')
        rules[rule_no] = [[rule]]
    else:
        rules[rule_no] = []
        toks = rule.split("|")
        for tok in toks:
            tok = tok.strip()
            if tok == "":
                continue
            rule = []
            nums = tok.split(" ")
            for num in nums:
                if num == "":
                    continue
                rule.append(int(num))
            rules[rule_no].append(rule)
    index += 1
index += 1
strings = []
max_len = 0
while index < len(lines):
    if len(lines[index]) > max_len:
        max_len = len(lines[index])
    strings.append(lines[index])
    index += 1

#print(f"rule0 = {rules[0]}")
#print(f"strings = {strings}")

def is_string(rule):
    for r in rule:
        for n in r:
            if isinstance(n, int):
                return False
    return True

processed_rules = []
dirty = True
while dirty:
    for index, rule in rules.items():
        if index not in processed_rules and is_string(rule):
            processed_rules.append(index)
            for index2, rule2 in rules.items():
                if index != index2:
                    for r in rule2:
                        if index in r:
                            dirty = True
                            pos = r.index(index)
                            replacement = rule[0:pos] + r + rule[pos + 1:]
print(f"RULE0 = {rules[0]}")
rule0_strings = []
for rule in rule0:
    rule0_strings.append(''.join(rule))

count = 0
for s in strings:
    if s in rule0_strings:
        count += 1

#print(f"rule0 = {rules[0]}")
#print(f"rule0_strings = {rule0_strings}")

print(f"Solution1 = {count}")

rule0 = rules[0]

dirty = True
while dirty:
    dirty = False
    for rule_no in range(len(rule0)):
        # print(f"Processing {rule_no} of {len(rule0)}")
        rule = copy.deepcopy(rule0[rule_no])
        if len(rule) > max_len:
            continue
        for num in range(len(rule)):
            if isinstance(rule[num], int):
                for new_rule in rules[rule[num]]:
                    replacement = rule[0:num] + new_rule + rule[num + 1:]
                    if replacement not in rule0:
                        # print(f"Add replacement '{replacement}' from '{new_rule}'")
                        rule0.append(replacement)
                        dirty = True
            if dirty:
                break
        if dirty:
            # print(f"  Deleteing {rule_no} = {rule0[rule_no]}")
            del rule0[rule_no]
            break
rule0_strings = []
for rule in rule0:
    rule0_strings.append(''.join(rule))

count = 0
for s in strings:
    if s in rule0_strings:
        count += 1

#print(f"rule0 = {rules[0]}")
#print(f"rule0_strings = {rule0_strings}")

print(f"Solution1 = {count}")
