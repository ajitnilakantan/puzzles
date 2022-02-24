from collections import defaultdict, deque
from collections import namedtuple
from functools import cmp_to_key
import itertools
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

Item = namedtuple("Item", "ingredients allergies")
items = []
all_ingredients = set()
all_allergies = set()
for line in lines:
    toks = line.split("(")
    ingredients = toks[0].strip().replace(",", "").split(" ")
    allergies = toks[1][9:-1].strip().replace(",", "").split(" ")
    print(f"Ingredients = {ingredients} allergies={allergies}")
    all_ingredients |= set(ingredients)
    all_allergies |= set(allergies)
    items.append(Item(ingredients, allergies))
print(f"All ingredients = {all_ingredients} all_allergies = {all_allergies}")


possible_matches = defaultdict(set)
Match = namedtuple("Match", "ingredient allergies")
for ingredient in all_ingredients:
    for allergy in all_allergies:
        matched = True
        for item in items:
            if allergy in item.allergies and ingredient not in item.ingredients:
                # print(f"Failed {ingredient} {allergy} in {item}")
                matched = False
                continue
        if matched:
            print(f"Possible match of {ingredient} to {allergy}")
            possible_matches[ingredient].add(allergy)

print(f"Possible = {possible_matches}")

# Eliminate all the singletons
processed_singletons = set()
dirty = True
while dirty:
    dirty = False
    for k, v in possible_matches.items():
        if len(v) == 1 and (unique_allergy := next(iter(v))) not in processed_singletons:
            processed_singletons.add(unique_allergy)
            for k2, v2 in possible_matches.items():
                if unique_allergy in v2 and len(v2) > 1:
                    v2.remove(unique_allergy)
                    dirty = True

print(f"PossibleCleaned = {possible_matches}")

safe_ingredients = set()
for ingredient in all_ingredients:
    if ingredient not in possible_matches:
        safe_ingredients.add(ingredient)

count = 0
for ingredient in safe_ingredients:
    for item in items:
        if ingredient in item.ingredients:
            count += 1
print(f"Solution1 = {count}")


unsafe_ingredients = []
for k, v in possible_matches.items():
    unsafe_ingredients.append((k, next(iter(v))))

unsafe_ingredients.sort(key=lambda x: x[1])
print(f"unsafe_ingredients = {unsafe_ingredients}")
ingredient_list = ','.join(x[0] for x in unsafe_ingredients)
print(f"Solution2 = {ingredient_list}")
