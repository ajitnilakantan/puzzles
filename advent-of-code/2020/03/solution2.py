with open("Input.txt") as f:
    lines = f.readlines()
# remove whitespace characters like `\n` at the end of each line
lines = [x.strip() for x in lines]

num_lines = len(lines)
width = len(lines[0])

print(f"{num_lines} lines of width {width}")

diffs = [(1, 1), (3, 1), (5, 1), (7, 1), (1, 2)]
hits = []
for diff in diffs:
    hit_count = 0
    x, y = 0, 0
    while y < num_lines - 1:
        y += diff[1]
        x = (x + diff[0]) % width
        if lines[y][x] == '#':
            hit_count += 1
        if y == num_lines - 1:
            print(f"last line = '{lines[y]}'")
    print(f"Hit {hit_count} trees for {diff}!")
    hits.append(hit_count)

print(f"{hits}")
