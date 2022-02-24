from blist import blist
cups = [3, 2, 4, 1, 5]
cups = [3, 8, 9, 1, 2, 5, 4, 6, 7]
cups = [5, 8, 9, 1, 7, 4, 2, 6, 3]
cups = blist(cups)
num_cups = len(cups)
print(f"cups = {cups} num_cups = {num_cups}")


# Rotate left
def rotate_left(lst, n):
    return lst[n:] + lst[:n]

current_cup_index =  0
current_cup = cups[current_cup_index]

counter = 0
current_cup_position = 0
while counter < 100:
    counter += 1
    # print(f"{counter:03d}: cups = {cups}  current_cup[{current_cup_index}] = {current_cup}")
    next_current_cup = cups[(current_cup_index + 4) % num_cups]
    print(f"{counter:03d}: ", end='')
    for _c in cups:
        print(f"({_c}) " if _c == current_cup else f"{_c} ", end='')
    print('')
    taken = []
    c = (current_cup_index + 1) % num_cups
    for i in range(3):
        if (c >= num_cups - i):
            c = 0
        take = cups.pop(c)
        taken.append(take)
    destination = current_cup - 1
    while destination not in cups:
        destination -= 1
        if destination <= 0:
            break
    if destination <= 0:
        destination = max(cups)
    # print(f" cups = {cups}")
    # print(f" taken = {taken}")

    i = cups.index(destination)
    # print(f"destination = {destination} index = {i}")
    while len(taken) > 0:
        last = taken.pop()
        cups.insert(i+1, last)

    current_cup = next_current_cup
    current_cup_index = cups.index(next_current_cup)
    # ZZZ current_cup_position = (current_cup_position + 1) % num_cups
    # print(f"rotate_left by {current_cup_index-current_cup_position}  {current_cup_position} vs {current_cup_index}")
    # ZZZ cups = rotate_left(cups, current_cup_index-current_cup_position)
    # ZZZ current_cup_index = current_cup_position
    # current_cup = cups[current_cup_index]
    # print(f" --> cups = {cups} current_cup={current_cup} index = {current_cup_index}")
    # print("")


print("FINAL")
print(f"cups = {cups} current_cup_index={current_cup_index} current_cup={current_cup} current_cup_position={current_cup_position}")
index = cups.index(1)
result = ''.join(str(cups[i % num_cups]) for i in range(index+1, index+num_cups))
print(f"Solution1 = {result}")
