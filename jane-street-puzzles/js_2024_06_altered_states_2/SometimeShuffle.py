# https://stackoverflow.com/questions/65134808/iterate-though-all-permutations-randomly
import hashlib
import math


def prf_under_n(N, key, tweak):
    b = math.ceil(math.log(N, 2))
    c = 0
    while True:
        h = ""
        while len(h) * 4 < b:
            h += hashlib.sha256("prf_under_n,{},{},{},{}".format(c, N, key, tweak).encode("ascii")).hexdigest()
            c += 1

        rand = int(h, 16) & (2**b - 1)
        if rand < N:
            return rand

def prf_coinflip(i, key, tweak):
    return hashlib.sha256("prf_coinflip,{},{},{}".format(i, key, tweak).encode("ascii")).digest()[0] & 1


class SometimeShuffle:
    def __init__(self, N, key):
        self.N = N
        self.key = key
        self.rounds = 7 * math.ceil(math.log(N, 2))
        self.round_keys = [prf_under_n(N, key, r) for r in range(self.rounds)]
        if N > 1:
            self.recurse = SometimeShuffle(N//2, key)

    def __call__(self, x):
        if self.N == 1:
            return x

        for r, K in enumerate(self.round_keys):
            x2 = (K - x) % self.N
            if prf_coinflip(max(x, x2), self.key, r):
                x = x2

        if x < self.N // 2:
            return self.recurse(x)

        return x


p = SometimeShuffle(10, 2)
print(f"type={type(p)} p={p} 2={p(2)} = {p(2)}")
print([p(i) for i in range(10)])
