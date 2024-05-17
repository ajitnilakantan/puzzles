# with open("Input.txt") as f:
#    grps = [x.strip().split() for x in f.read().split("\n\n")]

with open("Input.txt", "r") as fp:
    lines = fp.readlines()

# remove whitespace characters like `\n` at the end of each line
lines = [x.strip() for x in lines] 
# lines = [x.replace(",", "") for x in lines] 
# lines = [x.replace(".", "") for x in lines] 

visited = set()
acc = 0
line_no = 0

while True:
    if line_no in visited:
        print(f"break at '{line_no}' '{visited}'")
        break
    else:
        visited.add(line_no)
    tok = lines[line_no].split(' ')
    instruction, val = tok[0], int(tok[1])
    print(f"line='{line_no}' inst = '{instruction}' val='{val}'")
    if line_no == 595 or instruction == 'nop':
        line_no += 1
    if instruction == 'acc':
        acc += val
        line_no += 1
    if instruction == 'jmp':
        line_no += val
    if line_no >= len(lines):
        print(f"Past end break at '{line_no}'")

print(f"Solution: acc = {ssacc}")
