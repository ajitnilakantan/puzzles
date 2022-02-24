#  https://www.redblobgames.com/grids/hexagons/
# cube coordinates



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


def get_landing(line):
    x, y, z = 0, 0, 0
    index = 0
    while index < len(line):
        if line[index] == 'e':
            index += 1
            x += 1
            y -= 1
        elif line[index] == 'w':
            index += 1
            x -=1
            y+=1
        elif line[index:index+2] == 'nw':
            index += 2
            z -= 1
            y += 1
        elif line[index:index+2] == 'se':
            index += 2
            z += 1
            y -= 1
        elif line[index:index+2] == 'ne':
            index += 2
            z -= 1
            x += 1
        elif line[index:index+2] == 'sw':
            index += 2
            z += 1
            x -= 1
        else:
            0 / 0
    return (x, y, z)


def neighbors(x, y, z):
    return [(x+1, y,   z-1),
            (x+1, y-1, z),
            (x,   y-1, z+1),
            (x-1, y,   z+1),
            (x-1, y+1, z),
            (x,   y+1, z-1)]

flipped = set()

for line in lines:
    # "e, se, sw, w, nw, ne"
    # e:  z unchanged x+=1 y-=1
    # w:  z unchanged x-=1 y+=1
    # nw: x unchanged z-=1 y+=1
    # se: x unchanged z+=1 y-=1
    # ne: y unchanged z-=1 x+=1
    # sw: y unchanged z+=1 x-=1
    landing = get_landing(line)
    if landing in flipped:
        flipped.remove(landing)
    else:
        flipped.add(landing)

def count_black_neighbors(x, y, z, flipped):
    count = 0
    for n in neighbors(x, y, z):
        if n in flipped:
            count += 1
    return count

print(f"Flipped = {flipped}")
print(f"Solution1 = {len(flipped)}")

x_min, x_max = 0, 0
y_min, y_max = 0, 0
z_min, z_max = 0, 0
for f in flipped:
    if f[0] < x_min:
        x_min = f[0]
    if f[0] > x_max:
        x_max = f[0]
    if f[1] < y_min:
        y_min = f[1]
    if f[1] > y_max:
        y_max = f[1]
    if f[2] < z_min:
        z_min = f[2]
    if f[2] > z_max:
        z_max = f[2]
print(f"limits = x= {x_min} {x_max} y= {y_min} {y_max} z= {z_min} {z_max}")
day = 0
while day < 100:
    day = day+1
    # Expand limits
    x_min -= 2
    y_min -= 2
    z_min -= 2
    x_max += 2
    y_max += 2
    z_max += 2
    flipped_copy = set(flipped)
    black_expanded = set(flipped)
    for f in flipped_copy:
        for n in neighbors(f[0], f[1], f[2]):
            black_expanded.add(n)
    for f in black_expanded:
        count = count_black_neighbors(f[0], f[1], f[2], flipped_copy)
        if f in flipped_copy:
            # black
            if count == 0 or count > 2:
                flipped.remove(f)
        else:
            # white
            if count == 2:
                flipped.add(f)

    '''
    for x in range(x_min, x_max):
        for y in range(y_min, y_max):
            for z in range(z_min, z_max):
                count = count_black_neighbors(x, y, z, flipped_copy)
                if (x, y, z) in flipped_copy:
                    # black
                    if count == 0 or count > 2:
                        flipped.remove((x, y, z))
                else:
                    # white
                    if count == 2:
                        flipped.add((x, y, z))
    '''
    print(f"Day {day} count = {len(flipped)}")
