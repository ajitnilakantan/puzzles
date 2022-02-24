file_name = "Input1.txt"

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

'''
with open(file_name, "r") as fp:
    lines = fp.readlines()
lines = [x.strip() for x in lines]
'''


subject_number = 7
mod_value = 20201227

card_public_key = 2069194
door_public_key = 16426071
#card_public_key = 5764801 # Sample
#door_public_key = 17807724 # Sample

card_loop_size = 0
door_loop_size = 0
value = 1
while value != card_public_key:
    card_loop_size += 1
    value = (value * subject_number) % mod_value
print(f"card_loop_size = {card_loop_size}")

value = 1
while value != door_public_key:
    door_loop_size += 1
    value = (value * subject_number) % mod_value
print(f"door_loop_size = {door_loop_size}")

card_private_key = 1
for i in range(card_loop_size):
    card_private_key = (card_private_key * door_public_key) % mod_value
print(f"card_private_key = {card_private_key}")

door_private_key = 1
for i in range(door_loop_size):
    door_private_key = (door_private_key * card_public_key) % mod_value
print(f"door_private_key = {door_private_key}")

#Get 11576351 both times
