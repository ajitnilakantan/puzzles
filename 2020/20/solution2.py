import sys, numpy, scipy.ndimage
from collections import defaultdict, deque
import itertools
import copy
import re

file_name = "Input.txt"
size = 12  # 3 or 12 sqrt num_tiles
tile_size = 10

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

tiles = {}
tiles_data = {}
index = 0

# hash of sides and reversed sides: ids
tile_sides = defaultdict(list)

def R(s):
    return ''.join(reversed(s))

while index < len(lines):
    tile_no = int(lines[index][5:9])
    index += 1
    tile_data = []
    for _ in range(10):
        data = ['0' if x == '.' else '1' for x in lines[index]]
        tile_data.append(data)
        index += 1
    index += 1
    tiles_data[tile_no] = tile_data
    # Initially left to right, top to bottom
    l = ''
    for i in range(tile_size):
        l += tile_data[i][0]
    r = ''
    for i in range(tile_size):
        r += tile_data[i][-1]
    t = ''
    for i in range(tile_size):
        t += tile_data[0][i]
    b = ''
    for i in range(tile_size):
        b += tile_data[-1][i]

    tiles[tile_no] = (l, t, r, b)
    tile_sides[l].append((tile_no, (l, t, r, b), '0'))
    tile_sides[R(l)].append((tile_no, (R(l), b, R(r), t), 'h'))
    tile_sides[r].append((tile_no, (r, R(t), l, R(b)), 'v'))
    tile_sides[R(r)].append((tile_no, (R(r), R(b), R(l), R(t)), '180'))
    tile_sides[t].append((tile_no, (t, l, b, r), '90h'))
    tile_sides[R(t)].append((tile_no, (R(t), r, R(b), l), '90'))
    tile_sides[b].append((tile_no, (b, R(l), t, R(r)), '270'))
    tile_sides[R(b)].append((tile_no, (R(b), R(r), R(t), R(l)), '270h'))
print(tiles)
print(tiles_data)
print(len(tiles))
print(len(tile_sides))



def find_runs(tile_data, tile_key, tile_dict, run_size, so_far, results):
    # print(f"Level {run_size} {tile_data} {so_far}")
    if run_size == 1:
        results.append(so_far)
        # print(f"RETURN {so_far}")
        return

    # print(f"find_runs {tile_data} rs = {run_size}")
    l1,t1,r1,b1 = tile_data
    # for k,t in tile_dict.items()
    #   l2,t2,r2,b2 = t
    for opts in tile_sides[r1]:
        key2 = opts[0]
        if key2 == tile_key:
            continue
        if key2 not in tile_dict:
            continue
        l2,t2,r2,b2 = opts[1]
        if r1 == l2:  # r == l
            # r -> l
            op = opts[2]
            run = (l2, t2, r2, b2)  #
            tile_copy = dict(tile_dict)
            del tile_copy[key2]
            find_runs(run, key2, tile_copy, run_size-1, so_far + [(key2, run, op)], results)
        '''
        if r1 == l2:  # r == l
            # r -> l
            op = '0'
            run = (l2, t2, r2, b2)  #
            find_runs(run, key2, tile_copy, run_size-1, so_far + [(key2, run, op)], results)
        if r1 == R(l2):  # flip horiz around x axis
            op = 'h'
            run = (R(l2), b2, R(r2), t2) #
            find_runs(run, key2, tile_copy, run_size-1, so_far + [(key2, run, op)], results)
        if r1 == r2:  # flipv around y axis
            op = 'v'
            run = (r2, R(t2), l2, R(b2))  #
            find_runs(run, key2, tile_copy, run_size-1, so_far + [(key2, run, op)], results)
        if r1 == R(r2):  # rot 180
            op = '180'
            run = (R(r2), R(b2), R(l2), R(t2)) #
            find_runs(run, key2, tile_copy, run_size-1, so_far + [(key2, run, op)], results)
        if r1 == t2:  # rot90+fliph
            op = '90h'
            run = (t2, l2, b2, r2)
            find_runs(run, key2, tile_copy, run_size-1, so_far + [(key2, run, op)], results)
        if r1 == R(t2):  # rot90
            op = '90'
            run = (R(t2), r2, R(b2), l2)
            find_runs(run, key2, tile_copy, run_size-1, so_far + [(key2, run, op)], results)
        if r1 == b2:  # rot 270
            op = '270'
            run = (b2, R(l2), t2, R(r2)) #
            find_runs(run, key2, tile_copy, run_size-1, so_far + [(key2, run, op)], results)
        if r1 == R(b2):  # rot 270 + fliph
            op = '270h'
            run = (R(b2), R(l2), R(t2), R(r2))
            find_runs(run, key2, tile_copy, run_size-1, so_far + [(key2, run, op)], results)
        '''
    return

def find_vertical(run, run_size, all_results, results):
    if len(run) == run_size:
        results.append(run)
        return

    if run == [3]:
        print("3")
        pass

    for next_row_index in range(len(all_results)):
        if run == [3] and next_row_index == 19:
            print("19")
        if next_row_index in run:
            # Already in current run_size
            continue
        used_keys = set()
        for run_index in run:
            for res in all_results[run_index]:
                used_keys.add(res[0])
        next_row_keys = set()
        for res in all_results[next_row_index]:
            next_row_keys.add(res[0])
        if not used_keys.isdisjoint(next_row_keys):
            # common keys
            continue

        # check if stitched
        ok = True
        for col in range(size):
            if all_results[run[-1]][col][1][3] != all_results[next_row_index][col][1][1]:
                ok = False
                break
        if not ok:
            continue
        # Recurse
        find_vertical(run + [next_row_index], run_size, all_results, results)
    return


all_results = []
for k,v in tiles.items():
    l,t,r,b = v
    tiles_copy = copy.deepcopy(tiles)
    del tiles_copy[k]
    results = []
    # print(f"k= '{k}'")
    run = (l, t, r, b)
    op = '0'
    find_runs(run, k, tiles_copy, size, [(k, run, op)], results) #
    for res in results:
        all_results.append(res)

    results = []
    run = (R(l), b, R(r), t) #
    op = 'h'
    find_runs(run, k, tiles_copy, size, [(k, run, op)], results) #
    for res in results:
        all_results.append(res)

    results = []
    run = (r, R(t), l, R(b))  #
    op = 'v'
    find_runs(run, k, tiles_copy, size, [(k, run, op)], results) #
    for res in results:
        all_results.append(res)

    results = []
    run = (R(r), R(b), R(l), R(t)) #
    op = '180'
    find_runs(run, k, tiles_copy, size, [(k, run, op)], results) #
    for res in results:
        all_results.append(res)

    results = []
    run = (t, l, b, r)
    op = '90h'
    find_runs(run, k, tiles_copy, size, [(k, run, op)], results) #
    for res in results:
        all_results.append(res)

    results = []
    run = (R(t), r, R(b), l)
    op = '90'
    find_runs(run, k, tiles_copy, size, [(k, run, op)], results) ##
    for res in results:
        all_results.append(res)

    results = []
    run = (b, R(l), t, R(r)) #
    op = '270'
    find_runs(run, k, tiles_copy, size, [(k, run, op)], results) ##
    for res in results:
        all_results.append(res)

    results = []
    run = (R(b), R(r), R(t), R(l))
    op = '270h'
    find_runs(run, k, tiles_copy, size, [(k, run, op)], results) #
    for res in results:
        all_results.append(res)

#print(f"ALL ({len(all_results)}) = ")
#for i in range(len(all_results)):
#    print(f"  {i}: {all_results[i]}")
#print(f"len = {len(all_results)}")

print(f"ALL_RESULTS=")
for _index,ar in enumerate(all_results):
    for _ar in ar:
        print(f" {_index}: {_ar[0]} {_ar[2]} ", end='')
    print('')
print('')
print(f"Got {len(all_results)} all_results")

tile_keys = set([k for k,_ in tiles_data.items()])
print(f"tile_keys = {tile_keys}")


results = []
for i in range(len(all_results)):
    find_vertical([i], size, all_results, results)

if results:
    r0 = results[0]
    good_squares = [all_results[x] for x in r0]
    print(f"FOUND {len(results)} RESULTS = {results}")
    # print(f"good_squares  {good_squares}")

print("Solutions:")
for res in results:
    for index in res:
        for r in all_results[index]:
            print(f"{r[0]} ", end='')
        print('')
    print('')

for i in range(size):
    print(f"{i}: {good_squares[i]}")
solution = good_squares[0][0][0] * good_squares[0][-1][0] * good_squares[-1][0][0] * good_squares[-1][-1][0]
print(f"Solution1 = {solution}")


def apply_op(data, op):
    data = numpy.array(data)
    if op == '0':
        pass
    elif op == 'h':
        data = numpy.flip(data, 0)
    elif op == 'v':
        data = numpy.flip(data, 1)
    elif op == '180':
        data = numpy.rot90(data, 2)
    elif op == '90h':
        data = numpy.rot90(data, 1)
        data = numpy.flip(data, 0)
    elif op == '90':
        data = numpy.rot90(data, 1)
    elif op == '270':
        data = numpy.rot90(data, 3)
    elif op == '270h':
        data = numpy.rot90(data, 3)
        data = numpy.flip(data, 0)
    else:
        0/0
    return data


# #####
'''
big_image = numpy.zeros((size*tile_size, size*tile_size), int)
for row in range(size):
    for y in range(0, tile_size):
        for col in range(size):
            tile_data = tiles_data[good_squares[row][col][0]]
            tdata = []
            for _r in tile_data:
                tdata.append([0 if _x == '0' else 1 for _x in _r])
            op = good_squares[row][col][2]
            if good_squares[row][col][0] == 1951:
                print(f"key = 1951 tdata=\n{tdata} col={col} y={y}")
            tdata = apply_op(tdata, op)
            if good_squares[row][col][0] == 1951:
                print(f"op = {op} tdata=\n{tdata}")

            for x in range(0, tile_size):
                big_image[row*tile_size + y][col*(tile_size) + (x)] = tdata[y][x]
print(f"BIG_IMAGE = \n{big_image}")
'''
# #####

# Create stiched image

image = numpy.zeros((size*(tile_size-2), size*(tile_size-2)), int)
for row in range(size):
    for y in range(1, tile_size-1):
        for col in range(size):
            tile_data = tiles_data[good_squares[row][col][0]]
            tdata = []
            for _r in tile_data:
                tdata.append([0 if _x == '0' else 1 for _x in _r])
            op = good_squares[row][col][2]
            tdata = apply_op(tdata, op)

            for x in range(1, tile_size-1):
                image[row*(tile_size-2) + y-1][col*(tile_size-2) + (x-1)] = tdata[y][x]
print(f"IMAGE = \n{image}")

'''
image = []
for row in range(size):
    for y in range(1, tile_size-1):
        row_data = [0 for _ in range(size * (tile_size-2))]
        for col in range(size):
            tile_data = tiles_data[good_squares[row][col][0]]
            tdata = []
            for _r in tile_data:
                tdata.append([0 if _x == '0' else 1 for _x in _r])
            op = good_squares[row][col][2]
            tdata = apply_op(tdata, op)

            for x in range(1, tile_size-1):
                row_data[col*(tile_size-2) + (x-1)] = tdata[y][x]
        image.append(row_data)
'''
'''
print("Image=")
for row in image:
    for c in row:
        # print('.' if c == 0 else '#', end='')
        print(c, end='')
    print('')
print('')
'''

array = image
print(f"ARRAY = \n{array}")

kernel = [[0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0],
          [1, 0, 0, 0, 0, 1, 1, 0, 0, 0, 0, 1, 1, 0, 0, 0, 0, 1, 1, 1],
          [0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 0]]
kernel = numpy.array(kernel)
kernel_count = 15
print(f"Kernel = \n{kernel}")
'''
                  #
#    ##    ##    ###
 #  #  #  #  #  #
'''
def find_max_matches(convolve, kernel_count):
    max_vals = 0
    max = 0
    for row in convolve:
        for col in row:
            if col == kernel_count:
                max_vals += 1
            if col > max:
                max = col
    print(f"max_vals={max_vals} max = {max}")
    return max_vals

answer = 0
while True:
    arr = array # 0
    convolve = scipy.ndimage.convolve(arr, kernel, None, 'constant', 0)
    answer = find_max_matches(convolve, kernel_count)
    if answer:
        break

    arr = numpy.flip(array, 0) # h
    convolve = scipy.ndimage.convolve(arr, kernel, None, 'constant', 0)
    answer = find_max_matches(convolve, kernel_count)
    if answer:
        break

    arr = numpy.flip(array, 1) # v
    convolve = scipy.ndimage.convolve(arr, kernel, None, 'constant', 0)
    answer = find_max_matches(convolve, kernel_count)
    if answer:
        break

    arr = numpy.rot90(array, 2) # 180
    convolve = scipy.ndimage.convolve(arr, kernel, None, 'constant', 0)
    answer = find_max_matches(convolve, kernel_count)
    if answer:
        break

    arr = numpy.rot90(array) # 90h
    arr = numpy.flip(arr, 0)
    convolve = scipy.ndimage.convolve(arr, kernel, None, 'constant', 0)
    answer = find_max_matches(convolve, kernel_count)
    if answer:
        break

    arr = numpy.rot90(array, 1) # 90
    convolve = scipy.ndimage.convolve(arr, kernel, None, 'constant', 0)
    answer = find_max_matches(convolve, kernel_count)
    if answer:
        break

    arr = numpy.rot90(array, 3) # 270
    convolve = scipy.ndimage.convolve(arr, kernel, None, 'constant', 0)
    answer = find_max_matches(convolve, kernel_count)
    if answer:
        break

    arr = numpy.rot90(array, 3) # 270h
    arr = numpy.flip(arr, 0)
    convolve = scipy.ndimage.convolve(arr, kernel, None, 'constant', 0)
    answer = find_max_matches(convolve, kernel_count)
    if answer:
        break

unique, counts = numpy.unique(array, return_counts=True)
num1 = dict(zip(unique, counts))[1]
answer = num1 - kernel_count*answer
print(f"Solution2 = {answer}")
