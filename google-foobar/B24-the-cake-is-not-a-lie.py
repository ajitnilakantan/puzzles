def solution(s):
    for n in range(1, len(s)//2 + 1):
        if len(s) % n != 0:
            # Only consider divisors of 's' from smallest, up
            continue;
        # Run the checks to make sure the pattern repeats
        substring = s[0:n]
        for repeats in range(0, len(s), n):
            if substring != s[repeats:repeats+n]:
                # Mismatch... try next offset
                break
        else:
            # Found a match!
            return len(s) // n

    # Worst case: single big piece
    return 1

assert(solution("abcabcabcabc") == 4)
assert(solution("abccbaabccba") == 2)
assert(solution("bcdabcda") == 2)
