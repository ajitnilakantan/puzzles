with open("Input.txt") as f:
    lines = f.readlines()
# remove whitespace characters like `\n` at the end of each line
lines = [x.strip() for x in lines] 

counter = 0
for line in lines:
    tokens = line.split(" ")
    counts = tokens[0].split("-")
    min = int(counts[0])
    max = int(counts[1])
    chr = str(tokens[1])
    passwd = str(tokens[2])
    pos1 = passwd[min-1] == chr[0]
    pos2 = passwd[max-1] == chr[0]
    print(f"'{min}' to '{max}' for '{chr}' in '{passwd}' {pos1} {pos2}")
    if pos1 != pos2:
        # Equivalent of boolean xor
        counter += 1

print(f"Got {counter} good passwords out of {len(lines)}")
