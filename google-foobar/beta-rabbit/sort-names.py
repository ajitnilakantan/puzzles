a_to_z = "abcdefghijklmnopqrstuvwxyz"
assert(len(a_to_z) == 26)

char_value = {a_to_z[i]: i+1 for i in range(len(a_to_z))}
def my_key(val):
    result = 0
    for c in val:
        result += char_value[c]
    return result

def solution(names):
    sorted_l = sorted(names, key=my_key)
    sorted_l.reverse()
    return sorted_l


assert(solution(names = ["annie", "bonnie", "liz"]) == ["bonnie", "liz", "annie"])
assert(solution(names = ["abcdefg", "vi"]) == ["vi", "abcdefg"])

