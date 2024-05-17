import copy
import re

file_name = "Input-modified.txt"

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
print(f"MAX_LEN={max_len}")

#print(f"rule0 = {rules[0]}")
#print(f"strings = {strings}")


def is_string(rule):
    for r in rule:
        if isinstance(r, int):
            return False
    return True


def count_chars(rule):
    count = 0
    for r in rule:
        for n in r:
            if not isinstance(n, int):
                count = count + 1
    return count


def count_ints(rule):
    count = 0
    for r in rule:
        for n in r:
            if isinstance(n, int):
                count = count + 1
    return count


def count_matches(strings, rules0):
    rule_strings = []
    for r in rules0:
        rr = [str(x) for x in r]
        rule_strings.append(''.join(rr))

    count = 0
    for s in strings:
        if s in rule_strings:
            count += 1
    return count


def find_matches(regex, strings):
    for s in strings:
        if re.match(regex, s):
            return True
    return False


# PASS 1
'''
dirty = True
while dirty:
    dirty = False
    for rules_index, rules_list in rules.items():
        rule_index = 0
        while rule_index < len(rules_list):
            rule_list = rules_list[rule_index]
            dirty2 = True
            while dirty2:
                dirty2 = False
                for n_index in range(len(rule_list)):
                    n = rule_list[n_index]
                    if isinstance(n, int) and len(rules[n]) == 1:
                        replacement = rule_list[0:n_index] + rules[n][0] + rule_list[n_index + 1:]
                        rules[rules_index][rule_index] = replacement
                        rule_list = rules[rules_index][rule_index]
                        dirty2 = True
                        dirty = True
                        break
            rule_index += 1
'''
#print(f"RULESPASS1={rules}")
# PASS 2

for index in rules.keys():
    if index == 8 or index == 11:
        continue

    for index2 in rules.keys():
        dirty = True
        while dirty:
            dirty = False
            process_queue = copy.deepcopy(rules[index2])
            replacements = []
            while len(process_queue) > 0:
                rules2 = process_queue.pop()
                if index in rules2:
                    n_index = rules2.index(index)
                    for rule_replace in rules[index]:
                        # print(f"{rule_list} --> ", end='')
                        replacement = rules2[0:n_index] + rule_replace + rules2[n_index + 1:]
                        # print(f"{replacement}")
                        replacements.append(replacement)
                        dirty = True
                else:
                    replacements.append(rules2)
            rules[index2] = replacements

#print(f"PASS2 RULES={rules}")
print(f"PASS2 RULES0={rules[0]}")


dirty = True
while dirty:
    dirty = False
    process_queue = copy.deepcopy(rules[0])
    replacements = []
    while len(process_queue) > 0:
        rule = process_queue.pop()
        # print(f"Process {rule}")
        regex = "^"
        for x in rule:
            if isinstance(x, int):
                break
            regex += x
        regex += ".*$"
        regex = "^" + ''.join(['.*' if isinstance(x, int) else x for x in rule]) + ".*$"
        if not find_matches(regex, strings):
            # print(f"  NO MATCH on {rule} = {regex}")
            continue
        if len(rule) > max_len:
            # print(f"  toobig on {rule}")
            continue
        if is_string(rule):
            # print(f"  IS_STRING {rule}")
            replacements.append(rule)
            continue
        n_index = 0
        n = None
        while n_index < len(rule):
            n = rule[n_index]
            if isinstance(n, int):
                break
            n_index += 1
        # print(f"  found n_index = {n_index} n={n}")
        for rule_replace in rules[n]:
            # print(f"{rule_list} --> ", end='')
            replacement = rule[0:n_index] + rule_replace + rule[n_index + 1:]
            # print(f"{replacement}")
            regex = "^" + ''.join(['.*' if isinstance(x, int) else x for x in replacement]) + ".*$"
            if len(replacement) <= max_len and find_matches(regex, strings):
                replacements.append(replacement)
                dirty = True
    # print(f"  --> replacements = {replacements}")
    rules[0] = copy.deepcopy(replacements)
    print(f"Intermendiate Solution (len={len(rules[0])}) = {count_matches(strings, rules[0])}")


#print(f"PASS3 RULES={rules}")
print(f"PASS3 RULES0={len(rules[0])} = {rules[0]}")


print(f"Solution = {count_matches(strings, rules[0])}")

rule_strings = []
for r in rules[0]:
    rr = [str(x) for x in r]
    rule_strings.append(''.join(rr))

for s in strings:
    if s in rule_strings:
        print(f" '{s}' matches")
