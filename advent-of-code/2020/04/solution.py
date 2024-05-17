valid = set(['ecl', 'pid', 'eyr', 'hcl', 'byr', 'iyr', 'cid', 'hgt'])
valid_np = set(['ecl', 'pid', 'eyr', 'hcl', 'byr', 'iyr', 'hgt'])

print(valid)
print(valid_np)

def is_valid(passport):
    keys = set()
    toks = passport.split(" ")
    for tok in toks:
        kv = tok.split(":")
        if kv[0]:
            keys.add(kv[0])
    if keys == valid or keys == valid_np:
        return True
    else:
        print(f"{keys}")
        return False
    

with open("Input.txt") as f:
    lines = f.readlines()
# remove whitespace characters like `\n` at the end of each line
lines = [x.strip() for x in lines]

passports = []
str = ""
for line in lines:
    if line == "":
        if str:
            passports.append(str)
        str = ""
    else:
        str += " " + line
if str:
    passports.append(str)

print(f"have {len(passports)} passports")
for p in passports:
    print(p)

num_valid = 0
for p in passports:
    if is_valid(p):
        num_valid += 1
print(f"{num_valid} valid passports")
