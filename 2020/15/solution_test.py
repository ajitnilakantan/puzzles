import datetime

with open("Input.txt", "r") as fp:
    lines = fp.readlines()

line = lines[0].strip()
toks = line.split(",")

nums = [int(n) for n in toks]
target = 30000000

# Remember last turn (1 indexed) of number
positions = {}
# remember differences of number
differences = {}
for i in range(len(nums)):
    positions[nums[i]] = i+1
    differences[nums[i]] = 0

turn = len(nums)
last_spoken = nums[-1]
for zz in range(target + 5):
    if zz % 1000000 == 0:
        print(f"{datetime.datetime.now()}  :  At zz = {zz}")
    turn += 1

    if last_spoken in positions:
        if differences[last_spoken] == 0:
            # Only one occurence
            last_spoken = 0  #  Need to add zero since have only one occurence
            differences[last_spoken] = turn - positions[last_spoken]
            positions[last_spoken] = turn
        else:
            last_spoken = differences[last_spoken]
            if last_spoken in differences:
                # Update new differences
                differences[last_spoken] = turn - positions[last_spoken]
            else:
                differences[last_spoken] = 0
            positions[last_spoken] = turn
    else:
        # New number
        differences[last_spoken] = 0
        positions[last_spoken] = 0

    # print(f"{turn} {last_spoken}")
    if turn == target:
        print(f"Answer {zz}  {last_spoken}")  # 9007186
