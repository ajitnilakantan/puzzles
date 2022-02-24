# TOO SLOW
import datetime

with open("Input.txt", "r") as fp:
    lines = fp.readlines()

line = lines[0].strip()
toks = line.split(",")
nums = [int(n) for n in toks]

turns  = [len(nums) - i for i in range(len(nums))]
turn = len(nums)
spoken = [x for x in nums]
spoken.reverse()
print(spoken)
print(turns)
target = 30000000
for zz in range(target + 5):
    if zz % 1000000 == 0:
        print(f"{datetime.datetime.now()}  :  At zz = {zz}")
    index = 0
    turn += 1
    try:
        index = spoken[1:].index(spoken[0])
    except ValueError:
        index = -1
    # print(f"imdx of {spoken[0]} = {index}")
    # print(f"  {spoken}    {turns}")
    if index == -1:
        # print(f"push 0")
        spoken.insert(0, 0)
        turns.insert(0, turn)
    else:
        del spoken[index+1]
        val = turns[0] - turns[index+1]
        del turns[index+1]
        # print(f"push {val}")
        spoken.insert(0, val)   # index + 1
        turns.insert(0, turn)
    s = spoken
    # print(f"{turn} {zz:0d}:  {s}     {turns}")
    if turn == target:
        print(f"Answer {zz}  {spoken[0]}")  # 203
