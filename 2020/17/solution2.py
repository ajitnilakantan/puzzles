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
slices_k = []
for k in range(num_slices+4):
    slices_k.append([[0 for x in range(width + 4)] for y in range(height + 4)])

slices = []
for l in range(num_slices+4):
    slices.append(copy.deepcopy(slices_k))

for j in range(height_0):
    for i in range(width_0):
        slices[0+num_iterations+1][0+num_iterations+1][j+num_iterations+1][i+num_iterations+1] = 1 if lines[j][i] == '#' else 0

for j in range(height):
    for i in range(width):
        print(slices[0][0][j][i], end='')
    print("")
print("")

for iteration in range(num_iterations):
    slices_copy = copy.deepcopy(slices)
    for l in range(1, num_slices + 1):
        for k in range(1, num_slices + 1):
            # print(f"k = {k}")
            for j in range(1, height + 1):
                for i in range(1, width + 1):
                    count = 0
                    for n0 in range(-1, 1+1):
                        for n1 in range(-1, 1+1):
                            for n2 in range(-1, 1+1):
                                for n3 in range(-1, 1+1):
                                    if n0 == 0 and n1 == 0 and n2 == 0 and n3 == 0:
                                        continue
                                    if slices_copy[l+n0][k+n1][j+n2][i+n3]:
                                        count += 1
                    if slices_copy[l][k][j][i]:
                        if count == 2 or count == 3:
                            pass
                        else:
                            slices[l][k][j][i] = 0
                    else:
                        if count == 3:
                            slices[l][k][j][i] = 1


count = 0
# for k in range(-num_slices // 2 + 1, num_slices // 2):
print(f"loop {len(slices)} x {len(slices[0])} x {len(slices[0][0])}")
for l in range(len(slices)):
    for k in range(len(slices[0])):
        for j in range(len(slices[0][0])):
            for i in range(len(slices[0][0][0])):
                if slices[l][k][j][i]:
                    count += 1

print(f"Solution2 = {count}")  # 1836



"""

DIM = 4
deltas = list(itertools.product([-1,0,1], repeat=DIM))
deltas.remove((0,)*DIM


# ##

import sys, numpy, scipy.ndimag
D = 4

cube = numpy.array([[c == "#" for c in line[:-1]] for line in sys.stdin])
cube = numpy.expand_dims(cube, axis=tuple(range(D - 2)))
N = numpy.ones(shape=(3,) * D)
N[(1,) * D] = 0

for _ in range(6):
    cube = numpy.pad(cube, 1).astype(int)
    cnt = scipy.ndimage.convolve(cube, N, mode="constant", cval=0)
    cube = (cube == 1) & ((cnt == 2) | (cnt == 3)) | (cube == 0) & (cnt == 3)

print(numpy.sum(cube))

# ##

for dim in [3,4]:
    data      = [ [c=='#' for c in line] for line in day_17_input.splitlines() ]
    planes    = { (x,y,0,0)[:dim] : data[x][y] for x in range(len(data)) for y in range(len(data)) }
    deltas    = [ d for d in itertools.product([-1,0,1], repeat=dim) if d!=(0,0,0,0)[:dim] ]

    vecplus   = lambda o,d: tuple(a+b for a,b in zip(o,d))
    neighbors = lambda n,l: sum(n.get(vecplus(l,d),0) for d in deltas)
    survive   = lambda p,l,n: int(n in (2,3)) if p.get(l,0) else int(n==3)

    for i in range(1,7):
        planes = { loc : survive(planes,loc,neighbors(planes,loc))
                   for loc in itertools.product(range(0-i,len(data)+i), repeat=dim) }

    print(f'part {dim-2}:', sum(planes.values()))

# ##
gs = open('Day17-Data.txt').read().split()
gd = {(x,y,0,0) for y in range(len(gs)) for x in range(len(gs[0])) if gs[y][x] == '#'}

DIM = 4
deltas = list(itertools.product([-1,0,1], repeat=DIM))
deltas.remove((0,)*DIM)


def vec_add(v1,v2):
    return tuple([sum(x) for x in zip(v1, v2)])

for i in range(6):
    nd = dict()
    for cube in gd:
        for d in dxyzw:
            nd[vec_add(cube,d)] = nd.get(vec_add(cube,d),0) + 1

    newgd = {cube for cube in gd if nd.get(cube) == 2}
    gd = newgd.union({cube for cube in nd if nd.get(cube) == 3})

print("Number of cubes after 6 iterations:",len(gd))



"""
