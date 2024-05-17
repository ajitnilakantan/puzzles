# with open("Input.txt") as f:
#    grps = [x.strip().split() for x in f.read().split("\n\n")]

with open("Input.txt", "r") as fp:
    lines = fp.readlines()

# remove whitespace characters like `\n` at the end of each line
lines = [x.strip() for x in lines] 
lines = [x.replace(",", "") for x in lines] 
lines = [x.replace(".", "") for x in lines]

all_bags = {}

for line in lines:
    split = line.split(" contain ")
    bags = split[0].split(" ")
    bag_color = bags[0] + " " + bags[1]
    bag_contents = split[1].split(" ")
    i = 0
    while i < len(bag_contents):
        if (bag_contents[0] == "no"):
            # Empty bag
            all_bags[bag_color] = set()
            break
        else:
            content_count = int(bag_contents[i])
            content_color = bag_contents[i+1] + " " + bag_contents[i+2]
            i = i + 4
            if bag_color not in all_bags:
                all_bags[bag_color] = set()
            all_bags[bag_color].add((content_count, content_color))

# print(all_bags)

shiny_gold_bags = set()
for bag, contents in all_bags.items():
    for c in contents:
        if c[1] == "shiny gold":
            shiny_gold_bags.add(bag)
print(f"ShinyGold bags = {shiny_gold_bags}")

for i in range(128):
    for bag, contents in all_bags.items():
        for c in contents:
            if c[1] in shiny_gold_bags:
                shiny_gold_bags.add(bag)
print(f"ShinyGold bags = {shiny_gold_bags} {len(shiny_gold_bags)}")



#### PART 2
import collections

shiny_gold = all_bags['shiny gold']

def get_children(bag_color):
    counter = collections.Counter()
    for b in all_bags[bag_color]:
        counter[b[1]] += b[0]
        children = get_children(b[1])
        for k in children.keys():
            children[k] = children[k] * b[0]
        counter = counter + children
    return counter


c = get_children('shiny gold')
print(f" c = {c}")
total = 0
for k in c:
    total += c[k]
print(f" total = {total}")
