import copy

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

print(lines)
width_0 = len(lines[0])
height_0 = len(lines)


num_iterations = 6
num_slices = (num_iterations) * 2 + 1
print(f"num_iterations = {num_iterations}  num_slices={num_slices} width_0 = {width_0} height_0={height_0}")

width = width_0 + 2*num_iterations + 1
height = height_0 + 2*num_iterations + 1
slices = []
for i in range(num_slices+4):
    slices.append([[0 for x in range(width + 4)] for y in range(height + 4)])

for j in range(height_0):
    for i in range(width_0):
        slices[0+num_iterations+1][j+num_iterations+1][i+num_iterations+1] = 1 if lines[j][i] == '#' else 0

for j in range(height):
    for i in range(width):
        print(slices[0][j][i], end='')
    print("")
print("")

for iteration in range(num_iterations):
    slices_copy = copy.deepcopy(slices)
    # for k in range(-num_slices // 2 + 1, num_slices // 2):
    for k in range(1, num_slices + 1):
        print(f"k = {k}")
        for j in range(1, height + 1):
            for i in range(1, width + 1):
                neigbors = [
                    (0, 0, 1),
                    (0, 0, -1),
                    (0, 1, 0),
                    (0, 1, 1),
                    (0, 1, -1),
                    (0, -1, 0),
                    (0, -1, 1),
                    (0, -1, -1),
                    (1, 0, 0),
                    (1, 0, 1),
                    (1, 0, -1),
                    (1, 1, 0),
                    (1, 1, 1,),
                    (1, 1, -1),
                    (1, -1, 0),
                    (1, -1, 1),
                    (1, -1, -1),
                    (-1, 0, 0),
                    (-1, 0, 1),
                    (-1, 0, -1),
                    (-1, 1, 0),
                    (-1, 1, 1),
                    (-1, 1, -1),
                    (-1, -1, 0),
                    (-1, -1, 1),
                    (-1, -1, -1)]
                count = 0
                for n in neigbors:
                    if slices_copy[k+n[0]][j+n[1]][i+n[2]]:
                        count += 1
                    if count >= 4:
                        break
                if slices_copy[k][j][i]:
                    if count == 2 or count == 3:
                        pass
                    else:
                        slices[k][j][i] = 0
                else:
                    if count == 3:
                        slices[k][j][i] = 1

for j in range(height):
    for i in range(width):
        print(slices[0][j][i], end='')
    print("")
print("")

for j in range(height):
    for i in range(width):
        # print(slices[num_iterations][j][i], end='')
        print(slices[-1][j][i], end='')
    print("")
print("")

count = 0
# for k in range(-num_slices // 2 + 1, num_slices // 2):
print(f"loop {len(slices)} x {len(slices[0])} x {len(slices[0][0])}")
for k in range(len(slices)):
    for j in range(len(slices[0])):
        for i in range(len(slices[0][0])):
            if slices[k][j][i]:
                count += 1

print(f"Solution1 = {count}")  # 324
