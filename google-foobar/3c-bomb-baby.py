def solution(x, y):
    M = int(x)
    F = int(y)
    count = 0
    # print(f"F={F} M={M}")
    while True:
        # Subtract in chunks instead of 1 unit of F/M at a time
        # If F/M fully divides without a remainder, take one fewer.
        #   E.g.   F=3 M=1 .. take F//M -1 ; F=3 M=2 .. take F//M
        # print(f" F={F} M={M}")
        if F == 0 or M == 0:
            return "impossible"
        elif F > M:
            num = F // M - 1 if F % M == 0 else  F // M
            F = F - num * M
            count += num
        elif M > F:
            num = M // F - 1 if M % F == 0 else M // F
            M = M - num * F
            count += num
        elif F == 1 and M == 1:
            return str(count)
        else:
            return "impossible"

# assert("1" == solution('2', '1'))
# assert("4" == solution('4', '7'))
# assert("impossible" == solution('2', '4'))
# assert("100000000" == solution('1', '100000001'))
