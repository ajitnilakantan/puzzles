with open("Input.txt", "r") as fp:
    lines = fp.readlines()

# remove whitespace characters like `\n` at the end of each line
lines = [x.strip() for x in lines]


# clockwise
#   N
#  W  E
#    S

directions = [(1, 0), (0, -1), (-1, 0), (0, 1)]
cur_direction = 0  # East
pos_x, pos_y = (0, 0)
w_x, w_y = 10, 1
waypoint = [[w_x, w_y], [w_y, -w_x], [-w_x, -w_y], [-w_y, w_x]]
for line in lines:
    instr, val = line[:1], int(line[1:])
    print(f"{instr}, {val}")
    if instr == 'F':
        # pos_x += directions[cur_direction][0] * val * waypoint[cur_direction][0]
        # pos_y += directions[cur_direction][1] * val * waypoint[cur_direction][1]
        pos_x += val * waypoint[cur_direction][0]
        pos_y += val * waypoint[cur_direction][1]
    if instr == 'E':
        if cur_direction == 0:
            w_x += val
        elif cur_direction == 1:
            w_y += val
        elif cur_direction == 2:
            w_x -= val
        elif cur_direction == 3:
            w_y -= val
        waypoint = [[w_x, w_y], [w_y, -w_x], [-w_x, -w_y], [-w_y, w_x]]
    if instr == 'S':
        if cur_direction == 0:
            w_y -= val
        elif cur_direction == 1:
            w_x += val
        elif cur_direction == 2:
            w_y += val
        elif cur_direction == 3:
            w_x -= val
        waypoint = [[w_x, w_y], [w_y, -w_x], [-w_x, -w_y], [-w_y, w_x]]
    if instr == 'W':
        if cur_direction == 0:
            w_x -= val
        elif cur_direction == 1:
            w_y -= val
        elif cur_direction == 2:
            w_x += val
        elif cur_direction == 3:
            w_y += val
        waypoint = [[w_x, w_y], [w_y, -w_x], [-w_x, -w_y], [-w_y, w_x]]
    if instr == 'N':
        if cur_direction == 0:
            w_y += val
        elif cur_direction == 1:
            w_x -= val
        elif cur_direction == 2:
            w_y -= val
        elif cur_direction == 3:
            w_x += val
        waypoint = [[w_x, w_y], [w_y, -w_x], [-w_x, -w_y], [-w_y, w_x]]
    if instr == 'R':
        if val == 90:
            cur_direction = (cur_direction + 1) % 4
        elif val == 180:
            cur_direction = (cur_direction + 2) % 4
        elif val == 270:
            cur_direction = (cur_direction + 3) % 4
        else:
            print(f"UNKNOSN R {val}")
            raise Exception
    if instr == 'L':
        if val == 90:
            cur_direction = (cur_direction - 1 + 4) % 4
        elif val == 180:
            cur_direction = (cur_direction - 2 + 4) % 4
        elif val == 270:
            cur_direction = (cur_direction - 3 + 4) % 4
        else:
            print(f"UNKNOSN R {val}")
            raise Exception

print(f"pos= {pos_x}, {pos_y}")
print(f"solution2 = {abs(pos_x) + abs(pos_y)}")  # 104367
