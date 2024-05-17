with open("Input.txt", "r") as fp:
    line = fp.readline()
    depart = int(line)
    all_busses = fp.readline()

print(depart)
all_busses = all_busses.split(",")
busses = [int(x) for x in all_busses if x != 'x']

for d in range(depart, depart+9999):
    modulo = [d % x for x in busses]
    minval = min(modulo)
    if minval == 0:
        minbus = modulo.index(min(modulo))
        waittime = d - depart
        solution = waittime * busses[minbus]
        print(f"Solution1 = {solution} d = {d}")
        break


def chinese_remainder_theorem(a, n):
    # x = a1 mod n1
    # x = a2 mod n2
    # ...
    # x = an mod nn
    N = mul(n)
    y = [N // n[i] for i in range(len(n))]
    z = 0

from functools import reduce
def chinese_remainder(n, a):
    sum = 0
    prod = reduce(lambda a, b: a*b, n)
    for n_i, a_i in zip(n, a):
        p = prod // n_i
        sum += a_i * mul_inv(p, n_i) * p
    return sum % prod



def mul_inv(a, b):
    b0 = b
    x0, x1 = 0, 1
    if b == 1: return 1
    while a > 1:
        q = a // b
        a, b = b, a%b
        x0, x1 = x1 - q * x0, x0
    if x1 < 0: x1 += b0
    return x1

a = [0 for x in range(len(busses))]
j = 0
for i in range(0, len(all_busses)):
    if all_busses[i] != 'x':
        a[j] = busses[j] - i
        j += 1

n = busses
print(f"n = {n}  a = {a}")
solution = chinese_remainder(n, a)
print(f"solution2 = {solution}")
