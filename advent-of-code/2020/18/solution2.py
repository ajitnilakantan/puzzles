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

def fix_string(str):
    i = 0
    insert_paren_start = 0
    insert_paren_end = None
    token = ""
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
            return fix_string(paren)
            # print(f"PAREN '{paren}'")
        elif token == "*":
            i = i+1
            # print(f"PLUS")
            operator = "*"
            token = ""
        elif token == "+":
            i = i+1
            # print(f"MULTIPLY")
            insert_paren_end = i
            paren = str[insert_paren_start:insert_paren_end-1]
            # print(f"INSERT AROUND '{paren}'")
            insert_paren_start = i
            insert_paren_end = None
            operator = "+"
            token = ""
            return " ( " + paren + " ) + " + fix_string(str[i:])
        elif token == " ":
            i = i+1
            # print(f"SPACE")
            token = ""
        else:
            i = i+1
            # print(f"TOKEN {token}")
            token = ""
    # print(f"END token = '{token}' s = '{insert_paren_start}' e = '{insert_paren_end}'")
    if insert_paren_start:
        insert_paren_end = len(str)-1
        paren = str[insert_paren_start:]
        # print(f"FINAL INSERT AROUND '{paren}'")
        return str[:insert_paren_start] + " ( " + paren + ")"
    # print(f"FINAL return '{str}'  token = '{token}' s = '{insert_paren_start}' e = '{insert_paren_end}'")
    return " ( " + str + " ) "

def fix_string2(str):
    brace_begin = [0]
    for i in range(len(str)):
        if str[i] == '+':
            lhs = str[0:i-1]
            rhs = str[i+1:]
            print(f" ('{lhs}' + '{rhs}')")
            return " (( " + fix_string2(lhs) + " )  + (" + fix_string2(rhs) + " )) "
    return str

def evaluate_string2(str):
    tokens = []
    operators = []
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
            tokens.append(evaluate_string2(paren))
            token = ""
        elif "0" <= token <= "9":
            i = i+1
            while i < len(str) and "0" <= str[i] <= "9":
                token = token + str[i]
                i = i+1
            # print(f"NUMBER '{token}'")
            tokens.append(token)
            token = ""
        elif token == "+":
            i = i+1
            # print(f"PLUS")
            operator = "+"
            operators.append(operator)
            token = ""
        elif token == "*":
            i = i+1
            # print(f"MULTIPLY")
            operator = "*"
            operators.append(operator)
            token = ""
        elif token == " ":
            i = i+1
            # print(f"SPACE")
            token = ""
        else:
            i = i+1
            # print(f"TOKEN {token}")
            token = ""
    print(f"END tokens = '{tokens}' operators={operators}")
    vals = []
    val = tokens[0]
    for i in range(len(operators)):
        if operators[i] == '+':
            val = int(val) + int(tokens[i+1])
        elif operators[i] == '*':
            vals.append(val)
            val = tokens[i+1]
    vals.append(val)
    print(f"Get VALS '{vals}'")
    result = 1
    for val in vals:
        result *= int(val)
    return result


lines2 = ["1 + 2 * 3 + 4 * 5 + 6",
          "1 + (2 * 3) + (4 * (5 + 6)) ",
          "2 * 3 + (4 * 5)",
          "5 + (8 * 3 + 9 + 3 * 4 * 3)",
          "5 * 9 * (7 * 3 * 3 + 9 * 3 + (8 + 6 * 4))",
          "((2 + 4 * 9) * (6 + 9 * 8 + 6) + 6) + 2 + 4 * 2"
          ]

for line in lines2:
    eval = evaluate_string2(line)
    print(f"EVAL = {eval}")
    print("----")

sum = 0
for line in lines:
    sum += evaluate_string2(line)

#  21022630974613 too low  2102263097461321022630974613
print(f"Solution2 = {sum}")




"""
import ast

class T1(ast.NodeTransformer):
    def visit_Sub(self, n): return ast.copy_location(ast.Mult(), n)

class T2(ast.NodeTransformer):
    def visit_Add(self, n): return ast.copy_location(ast.Mult(), n)
    def visit_Mult(self, n): return ast.copy_location(ast.Add(), n)

def myeval(exp, T, D):
    tree = ast.parse(''.join(D.get(c,c) for c in exp), mode='eval')
    return eval(compile(T().visit(tree), '<string>', 'eval'))

print('Part 1:', sum(myeval(l, T1, {'*':'-'}) for l in open('18/input.txt')))

print('Part 2:', sum(myeval(l, T2, {'+':'*','*':'+'}) for l in open('18/input.txt')))
"""


'''
class T(int):
 def __init__(s,v):s.v=v
 __mul__ = lambda s,o:T(s.v+o.v)
 __sub__ = lambda s,o:T(s.v*o.v)
print(sum(eval(re.sub(r"(\d+)",r"T(\1)",l.translate({42:'-',43:'*'})))for l in open("18.in")))
'''

'''
import re

class I1(int):
    def __add__(self, b):
        return I1(self.real + b.real)
    def __sub__(self, b):
        return I1(self.real * b.real)

class I2(int):
    def __add__(self, b):
        return I2(self.real * b.real)
    def __mul__(self, b):
        return I2(self.real + b.real)

f1 = [ l.replace('*','-') for l in open('input').readlines()]
print(sum([eval(re.sub(r"(\d+)", r"I1(\1)", l)) for l in f1]))

f2 = [ l.replace('+','*').replace('-','+') for l in f1]
print(sum([eval(re.sub(r"(\d+)", r"I2(\1)", l)) for l in f2]))
'''



'''
import ast

swap_p_m = lambda s: s.replace('*', 'OpT').replace('+','*').replace('OpT','+')

class Swapper(ast.NodeTransformer):
    def visit_Add(self, node):
        return ast.Mult()
    def visit_Mult(self, node):
        return ast.Add()

s = 0
for l in open('day2020_18.txt'):
    t = ast.parse(swap_p_m(l.strip()), mode='eval')
    t2 = Swapper().visit(t)
    v = eval(compile(t2,'',mode='eval'))
    s += v
print('Part 2', s)
'''



'''
import ast, sys

def main():
    path = sys.argv[1] if len(sys.argv) > 1 else 'in'
    source = open(path).read()
    source = source.replace('*', '-')
    tree = ast.parse(source)
    traverse(tree)
    res = 0
    for expr in tree.body:
        val = eval(compile(ast.Expression(expr.value), 'N/A', 'eval'))
        res += val
    print(res)

def traverse(node):
    if isinstance(node, ast.Module):
        for child in node.body:
            traverse(child)
    elif isinstance(node, ast.Expr):
        traverse(node.value)
    elif isinstance(node, ast.BinOp):
        visit(node)
        traverse(node.left)
        traverse(node.right)

def visit(binop_node):
    if isinstance(binop_node.op, ast.Sub):
        binop_node.op = ast.Mult()
    elif isinstance(binop_node.op, ast.Add):
        pass
    else:
        1/0


main()
'''
