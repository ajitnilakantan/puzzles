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
    n = 0
    for i in range(10):
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
    tile_sides[l].append((tile_no, (l, t, r, b)))
    tile_sides[R(l)].append((tile_no, (R(l), b, R(r), t)))
    tile_sides[r].append((tile_no, (r, R(t), l, R(b))))
    tile_sides[R(r)].append((tile_no, (R(r), R(b), R(l), R(t))))
    tile_sides[t].append((tile_no, (t, l, b, r)))
    tile_sides[R(t)].append((tile_no, (R(t), r, R(b), l)))
    tile_sides[b].append((tile_no, (b, R(l), t, R(r))))
    tile_sides[R(b)].append((tile_no, (R(b), R(l), R(t), R(r))))
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
        l2,t2,r2,b2 = opts[1]
        tile_copy = dict(tile_dict)
        del tile_copy[key2]
        if r1 == l2:  # r == l
            # r -> l
            run = (l2, t2, r2, b2)  #
            find_runs(run, key2, tile_copy, run_size-1, so_far + [(key2, run)], results)
        if r1 == R(l2):  # flip horiz
            run = (R(l2), b2, R(r2), t2) #
            find_runs(run, key2, tile_copy, run_size-1, so_far + [(key2, run)], results)
        if r1 == r2:  # flipv
            run = (r2, R(t2), l2, R(b2))  #
            find_runs(run, key2, tile_copy, run_size-1, so_far + [(key2, run)], results)
        if r1 == R(r2):  # rot 180
            run = (R(r2), R(b2), R(l2), R(t2)) #
            find_runs(run, key2, tile_copy, run_size-1, so_far + [(key2, run)], results)
        if r1 == t2:  # rot270+fliph
            run = (t2, l2, b2, r2)
            find_runs(run, key2, tile_copy, run_size-1, so_far + [(key2, run)], results)
        if r1 == R(t2):  # rot270
            run = (R(t2), r2, R(b2), l2)
            find_runs(run, key2, tile_copy, run_size-1, so_far + [(key2, run)], results)
        if r1 == b2:  # rot 90
            run = (b2, R(l2), t2, R(r2)) #
            find_runs(run, key2, tile_copy, run_size-1, so_far + [(key2, run)], results)
        if r1 == R(b2):  # rot 90 + fliph
            run = (R(b2), R(l2), R(t2), R(r2))
            find_runs(run, key2, tile_copy, run_size-1, so_far + [(key2, run)], results)
    return

def find_vertical(run, run_size, all_results, results):
    if len(run) == run_size:
        results.append(run)
        return

    for next_row_index in range(len(all_results)):
        if next_row_index in run:
            # Already in current run_size
            continue
        used_keys = set()
        for run_index in run:
            for res in all_results[run_index]:
                for tup in res:
                    used_keys.add(res[0])
        next_row_keys = set()
        for res in all_results[next_row_index]:
            for tup in res:
                next_row_keys.add(res[0])
        if not used_keys.isdisjoint(next_row_keys):
            # common keys
            continue

        # check if stitched
        ok = True
        for col in range(size):
            if all_results[run[-1]][col][1][3] != all_results[next_row_index][col][1][1]:
                ok = False
            if not ok:
                break
        if not ok:
            continue
        # Recurse
        find_vertical(run + [next_row_index], run_size, all_results, results)
    return


all_results = []
for k,v in tiles.items():
    l,t,r,b = v
    tiles_copy = dict(tiles)
    del tiles_copy[k]
    results = []
    # print(f"k= '{k}'")
    run = (l, t, r, b)
    find_runs(run, k, tiles_copy, size, [(k, run)], results) #
    for res in results:
        all_results.append(res)

    results = []
    run = (R(l), b, R(r), t) #
    find_runs(run, k, tiles_copy, size, [(k, run)], results) #
    for res in results:
        all_results.append(res)

    results = []
    run = (r, R(t), l, R(b))  #
    find_runs(run, k, tiles_copy, size, [(k, run)], results) #
    for res in results:
        all_results.append(res)

    results = []
    run = (R(r), R(b), R(l), R(t)) #
    find_runs(run, k, tiles_copy, size, [(k, run)], results) #
    for res in results:
        all_results.append(res)

    results = []
    run = (t, l, b, r)
    find_runs(run, k, tiles_copy, size, [(k, run)], results) #
    for res in results:
        all_results.append(res)

    results = []
    run = (R(t), r, R(b), l)
    find_runs(run, k, tiles_copy, size, [(k, run)], results) ##
    for res in results:
        all_results.append(res)

    results = []
    run = (b, R(l), t, R(r)) #
    find_runs(run, k, tiles_copy, size, [(k, run)], results) ##
    for res in results:
        all_results.append(res)

    results = []
    run = (R(b), R(l), R(t), R(r))
    find_runs(run, k, tiles_copy, size, [(k, run)], results) #
    for res in results:
        all_results.append(res)

#print(f"ALL ({len(all_results)}) = ")
#for i in range(len(all_results)):
#    print(f"  {i}: {all_results[i]}")
#print(f"len = {len(all_results)}")

print(f"ALL_RESULTS=")
for ar in all_results:
    print(f"  {ar}")
print(f"Got {len(all_results)} all_results")

tile_keys = set([k for k,_ in tiles_data.items()])
print(f"tile_keys = {tile_keys}")

used_keys = set()
for run_index in [0, 2]:
    for res in all_results[run_index]:
        for tup in res:
            used_keys.add(res[0])
print(used_keys)

results = []
for i in range(len(all_results)):
    find_vertical([i], size, all_results, results)
    if results:
        r0 = results[0]
        good_squares = [all_results[x] for x in r0]
        print(f"found  {results}")
        print(f"good_squares  {good_squares}")
        break

for i in range(size):
    print(f"{i}: {good_squares[i]}")
solution = good_squares[0][0][0] * good_squares[0][-1][0] * good_squares[-1][0][0] * good_squares[-1][-1][0]
print(f"Solution1 = {solution}")


'''
tile_queue = deque(all_results)
while True:
    work_queue = deque()
'''


'''
solution = None
good_squares = None
for squares in itertools.permutations(all_results, size):
    ok = True
    # print(f"Square = {squares}")
    if len(squares) != size:
        print("ERRR")
        0/0
    for s in squares:
        if len(s) != size:
            print("ERRRRRR")
            0/0
    for col in range(size):
        for row in range(size-1):
            if squares[row][col][1][3] != squares[row+1][col][1][1]:
                ok = False
                break
        if not ok:
            break
    # check no repeats
    ids = set([squares[row][col][0] for col in range(size) for row in range(size)])
    if len(ids) != size * size:
        ok = False
    if ok:
        # print(f"GOOD SQUARE:  {squares}\n\n")
        good_squares = squares
        break
for i in range(size):
    print(f"{i}: {good_squares[i]}")
result = good_squares[0][0][0] * good_squares[0][-1][0] * good_squares[-1][0][0] * good_squares[-1][-1][0]
print(f"Solution1 = {result}")
'''
