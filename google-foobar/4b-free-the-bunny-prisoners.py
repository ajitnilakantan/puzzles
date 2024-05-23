from math import factorial
import itertools

MAX_KEYS = 10

# n Choose r
def nCr(n,r):
    return factorial(n) // (factorial(r) * factorial(n-r))


def validate_any_num_required_subsets(key_distribution, num_required, total_keys_used):
    # Loop through all subsets of size "num_required"
    # If any subset satisfies, return True
    for key_subset in itertools.combinations(key_distribution, num_required):
        if len(set([elem for kd in key_subset for elem in kd])) == total_keys_used:
            #print(f"any tot TRUE={total_keys_used} nr={num_required} len={len(set([elem for kd in key_subset for elem in kd]))} set={set([elem for kd in key_subset for elem in kd])}")
            return True
    #print(f"any FALSE tot={total_keys_used} nr={num_required}")
    return False

def validate_all_num_required_subsets(key_distribution, num_required, total_keys_used):
    # Loop through all subsets of size "num_required"
    # If all subset satisfies, return True
    ok = True
    for key_subset in itertools.combinations(key_distribution, num_required):
        if len(set([elem for kd in key_subset for elem in kd])) != total_keys_used:
            #print(f"all tot FALSE={total_keys_used} nr={num_required} len={len(set([elem for kd in key_subset for elem in kd]))} set={set([elem for kd in key_subset for elem in kd])}")
            ok = False
            break
    #print(f"any {ok} tot={total_keys_used} nr={num_required}")
    return ok

# Brute force all combinations.
# Becomes too slow when num_buns == 5 num_required >= 3
def brute_force(num_buns, num_required):
    # Brute-force distribute num_keys to each bunny out of a pool of total_keys.
    # Try to minimize num_keys
    # total_keys_used = total number of keys distributed among the buns.
    # Duplicates allowed so the same key-number can be used by multiple buns
    # num_keys_per_bun = number of keys each bun gets
    #print(f"num_buns={num_buns} num_required={num_required}")
    if num_required == 1:
        ret = [[0]]*num_buns
        #print(f"FOUND: {ret}")
        return [[0]]*num_buns

    # Total number of keys to be redistributed (duplicates allowed) amongst the num_buns
    for total_keys_used in range(num_required, MAX_KEYS):
        keyset = [k for k in range(0, total_keys_used)]
        #print(f"total_keys_used={total_keys_used}")
        # Total number of keys assigned per bun. Can range from 1..total_keys_used
        # All buns have the same number of keys
        for keys_per_bun in range(1, total_keys_used+1):
            keys = itertools.combinations(keyset, keys_per_bun)
            keys = list(keys)
            if len(keys) < num_buns:
                continue
            #print(f"  keys={keys}")

            # The first bun always has a keyset (0,..,keys_per_bun-1)
            first_bun_keys = tuple(i for i in range(0, keys_per_bun))

            # All the possible distributions of keys_per_bun 
            for key_distribution in itertools.combinations(keys, num_buns):
                # Validate that (num_required-1) doesn't solve the problem (otherwise key distribution is not minimal)
                if validate_any_num_required_subsets(key_distribution, num_required-1, total_keys_used):
                    continue
                # Validate that (num_required) all combination provide all total_keys_used
                if validate_all_num_required_subsets(key_distribution, num_required, total_keys_used):
                    ret = [list(elem) for elem in key_distribution]
                    #print(f"FOUND: {ret}")
                    return [list(elem) for elem in key_distribution]

    #print(f"FOUND: None")
    return None

"""
# Try to brute-force to get a feel of the problem
for num_buns in range(1, 6):
    for num_required in range(1, num_buns+1):
        brute_force(num_buns, num_required)
        print('', flush=True)

num_ | num_ | keys_ | keys_   | Solution
buns | req  | used  | per_bun |
1    | 1    | 1     | 1       | [[0]]
2    | 2    | 1     | 1       | [[0], [0]]
2    | 2    | 2     | 1       | [[0], [1]]
3    | 1    | 1     | 1       | [[0], [0], [0]]
3    | 2    | 3     | 2       | [[0, 1], [0, 2], [1, 2]]
3    | 3    | 3     | 1       | [[0], [1], [2]]
4    | 1    | 1     | 1       | [[0], [0], [0], [0]]
4    | 2    | 4     | 3       | [[0, 1, 2], [0, 1, 3], [0, 2, 3], [1, 2, 3]]
4    | 3    | 6     | 3       | [[0, 1, 2], [0, 3, 4], [1, 3, 5], [2, 4, 5]]
4    | 4    | 4     | 1       | [[0], [1], [2], [3]]
5    | 1    | 1     | 1       | [[0], [0], [0], [0], [0]]
5    | 2    | 5     | 4       | [[0, 1, 2, 3], [0, 1, 2, 4], [0, 1, 3, 4], [0, 2, 3, 4], [1, 2, 3, 4]]
..too slow after this.. 
Solution given for:
5    | 3    | 10        | 6       | [[0, 1, 2, 3, 4, 5], [0, 1, 2, 6, 7, 8], [0, 3, 4, 6, 7, 9], [1, 3, 5, 6, 8, 9], [2, 4, 5, 7, 8, 9]]

- The clue is the check validate_any_num_required_subsets(key_distribution, num_required-1, total_keys_used)
- This needs to compare every subset of (num_required-1) out of the num_buns tentative key-assignments.
- No subset should have all the keys, so each subset of (num_required-1) must be missing at least 1 key.
- There are Choose(num_buns, (num_required-1)) such subsets -- so each one should have at least one unique key that no other has.
- Each bun (in 0..num_buns-1) will be evenly distributed beween these subsets, so the total number
of keys per bun should be Choose(num_buns, num_required) * (num_required / num_buns)
- Check to verify:

num_ | num_ | keys_ | keys_  | Choose(num_buns,   | Choose(num_buns, num_required) *
buns | req  | used  | perbun |  (num_required-1)) |   (num_required / num_buns)
1    | 1    | 1     | 1      | 1                  | 1
2    | 1    | 1     | 1      | 1                  | 1
2    | 2    | 2     | 1      | 2                  | 1
3    | 1    | 1     | 1      | 1                  | 1
3    | 2    | 3     | 2      | 3                  | 2
3    | 3    | 3     | 1      | 3                  | 1
4    | 1    | 1     | 1      | 1                  | 1
4    | 2    | 4     | 3      | 4                  | 3
4    | 3    | 6     | 3      | 6                  | 3
4    | 4    | 4     | 1      | 4                  | 1
5    | 1    | 1     | 1      | 1                  | 1
5    | 2    | 5     | 4      | 5                  | 4
5    | 3    | 10    | 6      | 10                 | 6

- The numbers for the last two pairs of columns match to the logic looks good.
- With similar logic, you should not be able to select a set of num_required
  buns out of the entire num_buns set which has a key missing.
  By the pigeonhole principle, every key should be present in any selection
  of (num_buns-num_required+1) buns out of the total num_buns.
- We distribute the keys evenly through all combinations of Choose(num_buns, num_buns-num_required+1)
"""

def solution(num_buns, num_required):
    # Trivial cases:
    if num_buns == num_required:
        return [[k] for k in range(num_buns)]
    if num_required == 1:
        return [[0] for k in range(num_buns)]

    # Initialize to no keys for each bun
    key_distribution = [[] for _ in range(num_buns)]

    unique_key = 0   # Unique key to add. Start at 0 and keep incrementing
    # Loop through each possible (num_buns-num_required+1) subset and add a unique key
    for key_subset in itertools.combinations(enumerate(key_distribution), num_buns - num_required + 1):
        # Add unique key to each subset
        for k in key_subset:
            k[1].append(unique_key)
        unique_key += 1

    # Just to validate...total number of keys added in loop should match expected value
    total_keys_used = nCr(num_buns, num_required-1)
    keys_per_bun = nCr(num_buns, num_required) * (num_required / num_buns)
    assert(total_keys_used == unique_key)
    return key_distribution


assert(solution(2, 1) == [[0], [0]])
assert(solution(3, 1) == [[0], [0], [0]])
assert(solution(2, 2) == [[0],[1]])
assert(solution(3, 2) == [[0, 1],[0, 2], [1, 2]])
assert(solution(4, 4) == [[0], [1], [2], [3]])
assert(solution(5, 3) == [[0, 1, 2, 3, 4, 5], [0, 1, 2, 6, 7, 8], [0, 3, 4, 6, 7, 9], [1, 3, 5, 6, 8, 9], [2, 4, 5, 7, 8, 9]])
