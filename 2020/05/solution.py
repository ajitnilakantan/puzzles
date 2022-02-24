

def get_row(s):
    s = s[:-3]
    s = s.replace('F', '0')
    s = s.replace('B', '1')
    return int(s, 2)

def get_col(s):
    s = s[-3:]
    s = s.replace('L', '0')
    s = s.replace('R', '1')
    return int(s, 2)


def get_code(s):
    return get_row(s) * 8 + get_col(s)



#nums = ["FBFBBFFRLR", "BFFFBBFRRR", "FFFBBBFRRR", "BBFFBBFRLL"]
#
#for num in nums:
#    print(f" {num} -> {get_code(num)}")


with open("Input.txt") as f:
    lines = f.readlines()
# remove whitespace characters like `\n` at the end of each line
lines = [x.strip() for x in lines]

## PART 1
max = 0
for line in lines:
    code = get_code(line)
    if code > max:
        max = code
    print(f" {line} -> {code}")

print(f"max code = {max}")
print(f"num passes = {len(lines)}")

## PART 2
rows, cols = (128, 8) 
arr = [[0]*cols]*rows
arr = [[0 for j in range(cols)] for i in range(rows)]

for line in lines:
    r = get_row(line)
    c = get_col(line)
    arr[r][c] = 1

for i in range(rows):
    print(f"{i:03}: ", end='')
    for j in range(cols):
        print(arr[i][j], end='')
    print("")



