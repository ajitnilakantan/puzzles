def solution(n, b):
    BS="0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ"
    to_base = lambda n, b: "0" if not n else to_base(n//b, b).lstrip("0") + BS[n%b]
    k = len(str(n))
    history = []
    while True:
        str_n = str(n).zfill(k)
        if str_n in history:
            return len(history) - history.index(str_n)
        history.append(str_n)
        x = int(''.join(sorted(str_n, reverse=True)), b)
        y = int(''.join(sorted(str_n)), b)
        z = x - y
        n = int(to_base(z, b))
