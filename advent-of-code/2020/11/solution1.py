# with open("Input.txt") as f:
#    grps = [x.strip().split() for x in f.read().split("\n\n")]

import collections

with open("Input.txt", "r") as fp:
    lines = fp.readlines()

# remove whitespace characters like `\n` at the end of each line
lines = [x.strip() for x in lines]

seats = [[c for c in s] for s in lines]
seat_history = [seats]
print(f"seats = {seats}")


def iterate_seats(seats):
    new_seats = [[c for c in s] for s in seats]
    # print(f"new_seats = {new_seats}")
    for r in range(len(seats)):
        for c in range(len(seats[0])):
            # Rule 1
            if seats[r][c] == 'L':
                neighbors = ((-1, -1), (-1, 0), (-1, 1), (0, -1), (0, 1), (1, -1), (1, 0), (1, 1))
                neighbors_occupied = False
                for n in neighbors:
                    pr, pc = r + n[0], c + n[1]
                    if pr >= 0 and pr < len(seats) and pc >= 0 and pc < len(seats[0]):
                        if seats[pr][pc] == '#':
                            neighbors_occupied = True
                            break
                    else:
                        continue
                    if neighbors_occupied:
                        break
                if not neighbors_occupied:
                    new_seats[r][c] = '#'
            # Rule 2
            if seats[r][c] == '#':
                neighbors = ((-1, -1), (-1, 0), (-1, 1), (0, -1), (0, 1), (1, -1), (1, 0), (1, 1))
                neighbors_occupied = 0
                for n in neighbors:
                    pr, pc = r + n[0], c + n[1]
                    if pr >= 0 and pr < len(seats) and pc >= 0 and pc < len(seats[0]):
                        if seats[pr][pc] == '#':
                            neighbors_occupied += 1
                if neighbors_occupied >= 4:
                    new_seats[r][c] = 'L'
    return new_seats


def get_occupied_count(seats):
    result = 0
    for r in range(len(seats)):
        for c in range(len(seats[0])):
            if seats[r][c] == '#':
                result += 1
    return result


def print_seats(seats):
    for s in seats:
        for c in s:
            print(c, end='')
        print("")
    print("")


print_seats(seats)
while True:
    new_seats = iterate_seats(seats)
    if new_seats == seats:
        break
    # print_seats(seats)
    seats = new_seats
print_seats(seats)


print(f"Answer1 = {get_occupied_count(seats)}")
