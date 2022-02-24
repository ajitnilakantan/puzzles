import copy

file_name = "Input.txt"

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

with open(file_name, "r") as fp:
    lines = fp.readlines()
lines = [x.strip() for x in lines]


def evaluate_string(str):
    token = ""
    i = 0
    lhs = None
    rhs = None
    operator = None
    while i < len(str):
        token = token+str[i]
        if token == "(":
            i += 1
            start_paren = i
            end_paren = start_paren
            paren_count = 1
            while paren_count > 0:
                if str[end_paren] == "(":
                    paren_count += 1
                if str[end_paren] == ")":
                    paren_count -= 1
                i = i+1
                end_paren += 1
            paren = str[start_paren:end_paren-1]
            # print(f"PAREN '{paren}'")
            if lhs == None:
                lhs = evaluate_string(paren)
            else:
                rhs = evaluate_string(paren)
            if operator:
                if operator == '+':
                    # print(f"  {lhs} + {rhs}")
                    lhs = lhs + rhs
                    rhs = None
                    operator = None
                if operator == '*':
                    # print(f"  {lhs} * {rhs}")
                    lhs = lhs * rhs
                    rhs = None
                    operator = None
            token = ""
        elif "0" <= token <= "9":
            i = i+1
            while i < len(str) and "0" <= str[i] <= "9":
                token = token + str[i]
                i = i+1
            # print(f"NUMBER '{token}'")
            if lhs == None:
                lhs = int(token)
            else:
                rhs = int(token)
            if operator:
                if operator == '+':
                    # print(f"  {lhs} + {rhs}")
                    lhs = lhs + rhs
                    rhs = None
                    operator = None
                if operator == '*':
                    # print(f"  {lhs} * {rhs}")
                    lhs = lhs * rhs
                    rhs = None
                    operator = None
            token = ""
        elif token == "+":
            i = i+1
            # print(f"PLUS")
            operator = "+"
            token = ""
        elif token == "*":
            i = i+1
            # print(f"MULTIPLY")
            operator = "*"
            token = ""
        elif token == " ":
            i = i+1
            # print(f"SPACE")
            token = ""
        else:
            i = i+1
            # print(f"TOKEN {token}")
            token = ""
    return lhs

"""
eval = evaluate_string("1 + 2 * 3 + 4 * 5 + 6")
print(f"EVAL = {eval}")
print("----")
eval = evaluate_string("1 + (2 * 3) + (4 * (5 + 6))")
print(f"EVAL = {eval}")
"""
sum = 0
for line in lines:
    sum += evaluate_string(line)
print(f"Solution1 = {sum}")
