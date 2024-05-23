"""
dots in the following order:
1 4
2 5
3 6

So given the plain text word ""code"", you get the Braille dots:

11 10 11 10
00 01 01 01
00 10 00 00

c = 100100 = 36
o = 101010 = 42
d = 100110 = 38
e = 100010 ????
"""

def test_decode(phrase, braille, letters):
    print("phrase=({}) = {}".format(len(phrase), phrase))
    print("braille=({}) = {}".format(len(braille), braille))
    j = 0
    for i in range(len(phrase)):
        print("{}".format(phrase[i]))
        b = braille[j*6:j*6+6]
        if b == "000001":
            # Seems to indicate uppercase
            j+=1
            b = braille[j*6:j*6+6]
        j+=1
        print("  {} {}".format(b, int('0b'+b, 2)))
        braille_val = int('0b'+b, 2)
        if letters.get(phrase[i]) != None:
            if letters[phrase[i]] != braille_val:
                print("ERROR for char {}. Expected {}, Got {}".format(phrase[i], letters[phrase[i]], braille_val))
        else:
            letters[phrase[i]] = braille_val
    print(letters)

def test():
    from pprint import pprint
    letters = {}
    phrase = "The quick brown fox jumps over the lazy dog"
    braille = "000001011110110010100010000000111110101001010100100100101000000000110000111010101010010111101110000000110100101010101101000000010110101001101100111100011100000000101010111001100010111010000000011110110010100010000000111000100000101011101111000000100110101010110110"
    test_decode(phrase, braille, letters)

    phrase = "code"
    braille = "100100101010100110100010"
    test_decode(phrase, braille, letters)

    phrase = "Braille"
    braille = "000001110000111010100000010100111000111000100010"
    test_decode(phrase, braille, letters)

    pprint(letters)

#test()


def solution(s):
    # The test code above show these character mappings.  Uppercase are preceded by 000001
    uppercase = "000001"
    letters = {' ': 0,
             'a': 32,
             'b': 48,
             'c': 36,
             'd': 38,
             'e': 34,
             'f': 52,
             'g': 54,
             'h': 50,
             'i': 20,
             'j': 22,
             'k': 40,
             'l': 56,
             'm': 44,
             'n': 46,
             'o': 42,
             'p': 60,
             'q': 62,
             'r': 58,
             's': 28,
             't': 30,
             'u': 41,
             'v': 57,
             'w': 23,
             'x': 45,
             'y': 47,
             'z': 43}
    braille = ""
    for c in s:
        if c.isupper():
            braille += uppercase
            c = c.lower()
        braille += "{0:06b}".format(letters[c])
    return braille

assert(solution("code") == "100100101010100110100010")
assert(solution("Braille") == "000001110000111010100000010100111000111000100010")
assert(solution("The quick brown fox jumps over the lazy dog") == "000001011110110010100010000000111110101001010100100100101000000000110000111010101010010111101110000000110100101010101101000000010110101001101100111100011100000000101010111001100010111010000000011110110010100010000000111000100000101011101111000000100110101010110110")
