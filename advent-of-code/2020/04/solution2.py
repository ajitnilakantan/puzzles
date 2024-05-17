valid = set(['ecl', 'pid', 'eyr', 'hcl', 'byr', 'iyr', 'cid', 'hgt'])
valid_np = set(['ecl', 'pid', 'eyr', 'hcl', 'byr', 'iyr', 'hgt'])

print(valid)
print(valid_np)

def is_valid(passport):
    print(f"[{passport}]")
    keys = set()
    toks = passport.split(" ")
    for tok in toks:
        kv = tok.split(":")
        if kv[0]:
            keys.add(kv[0])
            try:
                if kv[0] == 'byr':
                    val = int(kv[1])
                    if not 1920 <= val <= 2002:
                        print(f"bad {kv[0]} {kv[1]}")
                        return False
                if kv[0] == 'iyr':
                    val = int(kv[1])
                    if not 2010 <= val <= 2020:
                        print(f"bad {kv[0]} {kv[1]}")
                        return False
                if kv[0] == 'eyr':
                    val = int(kv[1])
                    if not 2020 <= val <= 2030:
                        print(f"bad {kv[0]} {kv[1]}")
                        return False
                if kv[0] == 'hgt':
                    val = int(kv[1][:-2])
                    if kv[1][-2:] != "in" and kv[1][-2:] != "cm":
                        print(f"bad {kv[0]} {kv[1]}")
                        return False
                    elif kv[1][-2:] == "in" and not 59 <= val <= 76:
                        print(f"bad {kv[0]} {kv[1]}")
                        return False
                    elif kv[1][-2:] == "cm" and not 150 <= val <= 193:
                        print(f"bad {kv[0]} {kv[1]}")
                        return False
                if kv[0] == 'hcl':
                    val = kv[1]
                    if val[0] != '#' or len(val) != 7:
                        print(f"bad {kv[0]} {kv[1]}")
                        return False
                    for v in val[1:]:
                        if v not in "0123456789abcdefABCDEF":
                            print(f"bad {kv[0]} {kv[1]}")
                            return False
                if kv[0] == 'ecl':
                    val = kv[1]
                    if val not in ['amb',  'blu',  'brn',  'gry',  'grn',  'hzl',  'oth']:
                        print(f"bad {kv[0]} {kv[1]}")
                        return False
                if kv[0] == 'pid':
                    val = kv[1]
                    if len(val) != 9:
                        print(f"bad {kv[0]} {kv[1]}")
                        return False
                    for v in val:
                        if v not in "0123456789":
                            print(f"bad {kv[0]} {kv[1]}")
                            return False
            except Exception as e:
                print(f"GOT EXCEPTION ON {passport} {kv} {e}")
                return False
    if keys == valid or keys == valid_np:
        print("    return True")
        return True
    else:
        print("  ret False")
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
#for p in passports:
#    print(p)

num_valid = 0
for p in passports:
    if is_valid(p):
        num_valid += 1
print(f"{num_valid} valid passports")

# 102 too high
