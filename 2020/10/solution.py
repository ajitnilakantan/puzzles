# with open("Input.txt") as f:
#    grps = [x.strip().split() for x in f.read().split("\n\n")]

import collections

with open("Input.txt", "r") as fp:
    lines = fp.readlines()

# remove whitespace characters like `\n` at the end of each line
lines = [x.strip() for x in lines]
nums = [int(x) for x in lines]

device_joltage = max(nums) + 3
nums.append(device_joltage)

nums = sorted(nums)

cur_voltage = 0
counter = collections.Counter()

while len(nums) > 0:
    num = nums.pop(0)
    print(f"got num = {num}")
    if num > cur_voltage and num - cur_voltage <= 3:
        counter[num - cur_voltage] += 1
        cur_voltage = num
    else:
        break

print(f"num_1 = {counter[1]} num_3= ={counter[3]} answer = {counter[1] * counter[3]}")
print(f"counter = {counter}")
