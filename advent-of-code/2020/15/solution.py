with open("Input.txt", "r") as fp:
    lines = fp.readlines()

line = lines[0].strip()
toks = line.split(",")
nums = [int(n) for n in toks]

spoken = [x for x in nums]
spoken.reverse()
turn = 0
print(spoken)
for zz in range(2050):
    index = 0
    try:
        index = spoken[1:].index(spoken[0])
    except ValueError:
        index = -1
    # print(f"imdx of {spoken[0]} = {index}")
    if index == -1:
        spoken.insert(0, 0)
    else:
        spoken.insert(0, index + 1)
    if zz == 2020 - len(nums) - 1:
        print(f"{zz}  {spoken[0]}")  # 203
