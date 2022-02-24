#with open("Input.txt") as f:
#    grps = [x.strip().split() for x in f.read().split("\n\n")]

with open("Input.txt", "r") as fp:
    lines=fp.readlines()

# remove whitespace characters like `\n` at the end of each line
lines = [x.strip() for x in lines] 
#lines = [x.replace(",", "") for x in lines] 
#lines = [x.replace(".", "") for x in lines] 

def find_acc(skip_line):
    visited = set()
    acc = 0
    line_no = 0
    jmp_count = 0

    while True:
        if line_no in visited:
            print(f"break at '{line_no}' '{visited}'")
            return None
            #break
        else:
            visited.add(line_no)
        tok = lines[line_no].split(' ')
        instruction, val = tok[0], int(tok[1])
        if instruction == 'nop':
            line_no += 1
        if instruction == 'acc':
            acc += val
            line_no += 1
        if instruction == 'jmp':
            if jmp_count == skip_line:
                line_no += 1
            else:
                line_no += val
            jmp_count += 1
        if line_no >= len(lines):
            print(f"Past end break at '{line_no}'")
            return (acc, line_no, skip_line)

ret = (0, 0, 0)
for i in range(len(lines)):
    ret = find_acc(i)
    if ret:
        break

print(f"Solution: acc = {ret}")
