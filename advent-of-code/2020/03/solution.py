with open("Input.txt") as f:
    lines = f.readlines()
# remove whitespace characters like `\n` at the end of each line
lines = [x.strip() for x in lines]

num_lines = len(lines)
width = len(lines[0])

print(f"{num_lines} lines of width {width}")

hit_count = 0
x, y = 0, 0
while y < num_lines - 1:
    y += 1
    x = (x + 3) % width
    if lines[y][x] == '#':
        hit_count += 1
    if y == num_lines - 1:
        print(f"last line = '{lines[y]}'")
print(f"Hit {hit_count} trees!")
