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
for line in lines:
    instr, val = line[:1], int(line[1:])
    print(f"{instr}, {val}")
    if instr == 'F':
        pos_x += directions[cur_direction][0] * val
        pos_y += directions[cur_direction][1] * val
    if instr == 'E':
        pos_x += val
    if instr == 'S':
        pos_y -= val
    if instr == 'W':
        pos_x -= val
    if instr == 'N':
        pos_y += val
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
print(f"solution1 = {abs(pos_x) + abs(pos_y)}")  # 2847
