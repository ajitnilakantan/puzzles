# with open("Input.txt") as f:
#    grps = [x.strip().split() for x in f.read().split("\n\n")]

with open("Input.txt", "r") as fp:
    lines = fp.readlines()

# remove whitespace characters like `\n` at the end of each line  fdsf fdsf fdsf fdsf fdsfdfdsf
lines = [x.strip() for x in lines]
nums = [int(x) for x in lines]


def pair_sum(nums, sumval):
    for i in range(len(nums)):
        for j in range(0, i):
            if nums[i] != nums[j] and nums[i] + nums[j] == sumval:
                return True
    return False


def process_nums(nums, preamble_len):
    preamble = nums[0:preamble_len]
    print(f"preamble = {preamble}")
    nums = nums[preamble_len:]
    while len(nums):
        num = nums.pop(0)
        if pair_sum(preamble, num):
            preamble.pop(0)
            preamble.append(num)
        else:
            return num
    return None

answer = process_nums(nums, 25)
print(f"answer = {answer}")
