# This Python file uses the following encoding: utf-8
import itertools
from collections import Counter
from math import factorial

"""
Searching for "matrix equivalences swap rows and columns" leads to
- https://math.stackexchange.com/questions/1941503/number-of-equivalence-classes-of-matrices-under-switching-rows-and-columns
- https://math.stackexchange.com/questions/2056708/number-of-equivalence-classes-of-w-times-h-matrices-under-switching-rows-and
- https://math.stackexchange.com/questions/2113657/burnsides-lemma-applied-to-grids-with-interchanging-rows-and-columns
- https://en.wikipedia.org/wiki/Burnside%27s_lemma

Looks like a straightforward application of Burnside's Lemma
"""



###############################################################################
# SOLUTION 1 : Apply burnside's lemma to all permutations of the matrix.
#              Works but the implementation to enumerate
#              the entire group becomes TOO SLOW when w,h >= 9
###############################################################################

'''
# Hashable 2D array (simple implementation of hash+eq)
class MyArray(object):
    def __init__(self, w, h, copy_from=None):
        self.w, self.h = w, h
        if copy_from != None:
            # Make copy
            self.grid = [x[:] for x in copy_from]
        else:
            # Initialize to 1,2,3..w*h
            self.grid = [[r*w + c + 1 for c in range(w)] for r in range(h)]

    def __getitem__(self, row):
        return self.grid[row]

    def __getattr__(self, name):
        """Delegate to array."""
        try:
            return getattr(self.array, name)
        except AttributeError:
            raise AttributeError("'MyArray' object has no attribute {}".format(name))

    def __eq__(self, other):
        return False if not other else repr(self) == repr(other)

    def __ne__(self, other):
        return True if not other else repr(self) != repr(other)

    def __str__(self):
        buf = ""
        for r in range(self.h):
            for c in range(self.w):
                buf += str(self.grid[r][c])
                #buf += f"{self.grid[r][c]}"
            buf += "\n"
        return buf

    def __repr__(self):
        buf = ""
        for r in range(self.h):
            buf += repr(self.grid[r])
        return buf

    def __hash__(self):
        return hash(repr(self.grid))

def debug_print_grid_v1(grid, w, h):
    # Pretty print a grid
    pass
    """
    for r in range(h):
        for c in range(w):
            print(f"{g[r][c]}", end='')
        print("  ", end='')
    print("")
    """

def debug_print_all_grids_v1(all_grids, w, h):
    # Pretty print all grids in a row
    pass
    """
    for r in range(h):
        for g in all_grids:
            for c in range(w):
                print(f"{g[r][c]}", end='')
            print("  ", end='')
        print("")
    print("")
    """

def swap_columns_v1(grid, w, h, col_a, col_b):
    """ Swap columns. Return a new copy of grid. """
    # Make copy
    new_grid = MyArray(w, h, grid)
    for r in range(h):
        new_grid[r][col_b], new_grid[r][col_a] = new_grid[r][col_a], new_grid[r][col_b]
    return new_grid

def swap_rows_v1(grid, w, h, row_a, row_b):
    """ Swap rows. Return a new copy of grid. """
    # Make copy
    new_grid = MyArray(w, h, grid)
    for c in range(w):
        new_grid[row_b][c], new_grid[row_a][c] = new_grid[row_a][c], new_grid[row_b][c]
    return new_grid


def find_all_grid_permutations_v1(w, h):
    """
    Find all the permutions of a grid swapping 2 columns or 2 rows
    e.g. for w=2 h=2
    12  21  34  43
    34  43  12  21
    or for w=3 h=2
    123  132  213  231  312  321  456  465  546  564  645  654  
    456  465  546  564  645  654  123  132  213  231  312  321
    """
    ZZZ = 0
    grid = MyArray(w, h)
    all_grids = set()
    # Recursively try all pairs of column and row swaps
    children = [grid]
    while len(children) != 0:
        grid = children.pop()
        # Pairwise columns
        for pair in itertools.combinations(range(w), 2):
            col_a, col_b = pair[0], pair[1]
            new_grid = swap_columns_v1(grid, w, h, col_a, col_b)
            if not new_grid in all_grids:
                all_grids.add(new_grid)
                children.append(new_grid)
        # Pairwise rows
        for pair in itertools.combinations(range(h), 2):
            row_a, row_b = pair[0], pair[1]
            new_grid = swap_rows_v1(grid, w, h, row_a, row_b)
            #if not new_grid in all_grids:
            if not new_grid in all_grids:
                all_grids.add(new_grid)
                children.append(new_grid)
        ZZZ += 1
        if ZZZ % 1000 == 0:
            pass
            #print(f"...len(all_grids)={len(all_grids)}, len(children)={len(children)}")

    return all_grids

def find_cycles_v1(grid, w, h):
    # Find the cycles in the permuted grid. E.g. for:
    # 2 1 3
    # 5 4 6  for w=3 h=2 -> [[1, 2], [3], [4, 5], [6]]
    cycles = []
    nodes = {n:n for n in range(1, w*h+1)}
    nodes = {r*w + c + 1: grid[r][c] for c in range(w) for r in range(h)}
    node_set = set([n for n in range(1, w*h+1)])
    while len(node_set) > 0:
        first = node_set.pop()
        next = first
        cycle = [first]
        while True:
            next = nodes[next]
            if next == first:
                break
            else:
                node_set.remove(next)
                cycle.append(next)
        cycles.append(cycle)
    return cycles

def count_cycles_v1(cycles):
    """ Count the cycles in a given cycle set.  
        count the cycles as a set of x(p, n) where the subscript p is the length of the
        cycle and the superscript n is the number of times the cycle of size p occurs
        E.g. for [[1, 2], [3], [4, 5], [6]]   compute  {x(2, 2), x(1, 2)}
        I.e. a there are 2 cycles of size 2 and 2 of size 1.
        Return as a hashtable where the key is p and the value is n
    """
    counter = Counter()
    for cycle in cycles:
        p = len(cycle)
        counter[p] += 1
    return counter

def group_all_cycles_v1(all_grids, w, h):
    """ Group the cycle sets computed for each grid.
        It is a polynomial addition of the products of variables (x_subp to the power of n)
        Each cycle per grid is expressed as a product.
        E.g. for [[1, 2], [3], [4, 5], [6]]   we had computed  {x(2, 2), x(1, 2)} --> x2^2 * x1^2
        E.g. for [[1], [2], [3], [4], [5], [6]] would get x1^6
        The returned hashtable can be treated as a multi-variable polynomial. The key
        is the product of variable to power and the value the the coefficient. 
        E.g. { ((6, 1)):2 , ((4, 3), (5, 7), (6,2)): 9 } = (2)*(x_6^1) + (9)*(x_4^3*x_5^7*x_6^2)
    """
    counter = Counter()
    for g in all_grids:
        cycles = find_cycles_v1(g, w, h)
        grid_counter = count_cycles_v1(cycles)
        # Accumulate in a sorted tuple which is used as a key for "counter". The value is
        # the number of times it occurs. e.g. a in  a*x1^2*x3^6
        key = ()
        for p, n in grid_counter.items():
            key += (p, n),
        key = tuple(sorted(key)) # sort to make x1*x2 == x2*x1
        counter[key] += 1

    return counter

def evaluate_polynomial_v1(polynomial_counter, x):
    """ Evaluate the multi-variable polynomial (as calculated in group_all_cycles) for 'x' """
    ret = 0
    for k, v in polynomial_counter.items():
        val = v
        for tuple_pair in k:
            val *= (x ** tuple_pair[1])
        ret += val
    return ret

def solution_v1(w, h, s):
    # Step-by-step instructions: https://www.projectrhea.org/rhea/index.php/Walther375Spring2014_Coloring_regular_polygons:_the_theorems_of_Burnside_and_Polya
    #print(f"solution({w}, {h}, {s})")
    all_grids = find_all_grid_permutations_v1(w, h)
    len_g = len(all_grids)  # Size of the group
    poly = group_all_cycles_v1(all_grids, w, h)
    #print(f"POLY = {poly} = {evaluate_polynomial_v1(poly, s)}")
    #print(f"len = {len_g}")
    answer = evaluate_polynomial_v1(poly, s) // len_g
    #print(f"solution({w}, {h}, {s}) = {answer}")
    #print("{} {} {} = {}".format(w, h, s, answer))
    return str(answer)

#assert(solution_v1(2, 2, 2) == "7")
#assert(solution_v1(2, 3, 4) == "430")

"""
# Gets too slow when w or h get over 10-ish
for w in range(2, 12):
    for h in range(2, 12):
        for s in range(2, 10):
            answer = solution_v1(w, h, s)
"""
## END SOLUTION 1
'''

###############################################################################
###############################################################################
###############################################################################
# The attempt above works in solution1, but is TOO SLOW.
# Instead take the approach described in https://math.stackexchange.com/questions/2506511/applying-burnsides-lemma-to-translations
# and also in https://math.stackexchange.com/questions/2113657/burnsides-lemma-applied-to-grids-with-interchanging-rows-and-columns
# SOLUTION 2 : Apply burnside's lemma / Polya's counting. Take into account the cycle index of the permutations from 𝑆w×𝑆h
#              Since the cycles of row permutations and column permutations occur independently and are commutative, we compute
#              the cycle index of each and take the Cartesian product to get the cycle index of the entire the grid.
###############################################################################
###############################################################################
###############################################################################
def gcd(a,b):
    a = abs(a)
    b = abs(b)
    while b > 0:
        a, b = b, a % b
    return 1 if a == 0 else a

def lcm(a,b):
    return (a*b) // gcd(a,b)

'''
# The function Z_Sn(n) below to compute the cycle index works, but is TOO SLOW.
# It enumerates though the n! permutations and computes and accumulates the cycle notation of each.
# Instead, we will use the explicit formula for the cycle index of a "Symmetric Group"
def find_cycles(permutation, size):
    # Find the cycles in the permutation. E.g. size=3
    # 1 2 3 => [[1][2][3]]
    # 2 1 3 => [[12][3]]
    # Returns a list of lists
    cycles = []
    # Hashtable of original positions
    original_list = {x+1:x+1 for x in range(0, size)}
    permuted_list = {n+1:permutation[n] for n in range(size)}
    item_set = set([n+1 for n in range(0, size)])
    while len(item_set) > 0:
        first = item_set.pop()
        next = first
        cycle = [first]
        while True:
            next = permuted_list[next]
            if next == first:
                break
            else:
                item_set.remove(next)
                cycle.append(next)
        cycles.append(cycle)
    return cycles

def Z_Sn(n):
    """ 
    Find 𝑍(𝑆n) : The cycle index for a row of size n.
    e.g. 𝑍(𝑆2) : The 2 permutations of (1, 2) are: (1, 2)  (2, 1)
                 which give 2 cycles of            (1)(2)  (1 2)
                 which give the polynomial   (1/2) * (x1^2  + x2^1)
    e.g. 𝑍(𝑆3) : The 6 permutations of (1, 2, 3) are (1, 2, 3), (1, 3, 2), (2, 1, 3), (2, 3, 1), (3, 1, 2), (3, 2, 1)
                 which gives the cycles              (1)(2)(3), (1)(2 3),  (1 2)(3),  (1 2 3),   (1 2 3)    (1 3)(2)
                 which gives the polynomial  (1/6) * (x1^3 + x1^1 * x2^1 + x2^1 * x1^1 + x3^1 + x3^1 + x2^1 * x1^1)
                 which can be simplified as  (1/6) * (x1^3 + 3 * x1^1 * x2^1 + 2*x3^1)
    Note:  We exclude the division by the group size (1/2, 1/6 in the examples above) in the cycle index we return
    """
    # Initial items e.g. [1, 2, 3..n]
    items = [x for x in range(1,n+1)]
    # All n! permutations 
    permutations = itertools.permutations(items)
    # Returned hashtable of cycle-index (key=cycles as a tuple of 2-tuples, value=coefficient.)
    #  e.g.  4*x1^2*x3^6 ==> key = ((1 2) (3 6)) value = 4
    cycle_index = {}
    for p in permutations:
        # Step 1: Find the cycles for a given permutation
        cycles = find_cycles(permutation=p, size=n)
        # Step 2: 
        # - For the cycles of a permutation, count the number of cycles of length 1, of length 2, ...etc.
        #   E.g. for [[1, 2], [3], [4, 5], [6]]   compute  {x(2, 2), x(1, 2)}
        #   I.e. a there are 2 cycles of size 2 and 2 of size 1.
        # - Count the cycles as a set of x(p, n) where the subscript p is the length of the
        #   cycle and the superscript n is the number of times the cycle of size p occurs
        # - The example corresponds to the polynomial x1**2 + x2**2 (p is the index of the variable, n is the power)
        # - Count as a hashtable where the key is p and the value is n
        counter = Counter()
        for cycle in cycles:
            counter[len(cycle)] += 1
        # Step 3:  Accumulate all the "multi-variable polynomials"
        # Accumulate in a sorted tuple which is used as a key for "counter". The value is
        # the number of times it occurs. e.g. a in  a*x1^2*x3^6
        key = ()
        for pp, nn in counter.items():
            key += (pp, nn),
        key = tuple(sorted(key)) # sort to make x1*x2 == x2*x1
        if not cycle_index.get(key):
            cycle_index[key] = 1
        else:
            cycle_index[key] += 1
    return cycle_index
'''

def Z_Sn(n):
    """ 
    Compute the cycle index of the symmetric group of all permutations.
    We can compute the cycle index explictly instead of enumerating all permutations.
    See: https://en.wikipedia.org/wiki/Cycle_index
         https://mathworld.wolfram.com/SymmetricGroup.html
         http://www-cs-students.stanford.edu/~blynn//polya/cycleindex.html
    Find 𝑍(𝑆n) : The cycle index for a row of size n.
    e.g. 𝑍(𝑆2) : The 2 permutations of (1, 2) are: (1, 2)  (2, 1)
                 which give 2 cycles of            (1)(2)  (1 2)
                 which give the polynomial   (1/2) * (x1^2  + x2^1)
    e.g. 𝑍(𝑆3) : The 6 permutations of (1, 2, 3) are (1, 2, 3), (1, 3, 2), (2, 1, 3), (2, 3, 1), (3, 1, 2), (3, 2, 1)
                 which gives the cycles              (1)(2)(3), (1)(2 3),  (1 2)(3),  (1 2 3),   (1 2 3)    (1 3)(2)
                 which gives the polynomial  (1/6) * (x1^3 + x1^1 * x2^1 + x2^1 * x1^1 + x3^1 + x3^1 + x2^1 * x1^1)
                 which can be simplified as  (1/6) * (x1^3 + 3 * x1^1 * x2^1 + 2*x3^1)
    Note:  We exclude the division by the group size (1/2, 1/6 in the examples above) in the returned cycle index
    """
    cycle_index = {}
    sums = find_all_sums(n)
    # Convert to counters.
    # E.g. for n=4 ==> [[4], [3, 1], [2, 2], [2, 1, 1], [1, 1, 1, 1]] ==> [{4: 1}, {3: 1, 1: 1}, {2: 2}, {1: 2, 2: 1}, {1: 4}]
    sums = [Counter(x) for x in sums]
    # Compute SUM(all_sums) ( n! / (PRODUCT(k=1..n)of(k^j_k * j_k!)) ) * x1**j1 * x2**j2 * x3**j3 ... * x_n**jn 
    # The actual cycle index is multiplied by the size of the permutation group (i.e. 1/n!) but we
    # will ignore it for now, and do the division in the last step of 'solution'

    for cycle in sums:
        # Step 1: calculate the coeffient for each term
        product = 1
        for num,times in cycle.items():
            # e.g. for {3:1, 1:1} loop though num=3 times=1  + num=1 times=1  === 4
            product *= num**times * factorial(times)
        coeff = factorial(n) // product

        # Step 2: calculate the power product of variables
        # Accumulate all the "multi-variable polynomials"
        # Accumulate in a sorted tuple which is used as a key for "counter". The value is
        # the number of times it occurs. e.g. the coeff calculated above in 'a':   a*x1^2*x3^6
        key = ()
        for num,times in cycle.items():
            key += (num, times),
        key = tuple(sorted(key)) # sort to make x1*x2 == x2*x1
        cycle_index[key] = coeff

    return cycle_index

def find_all_sums(amount, hand=None):
    """ 
    Find all sums J_i    where N_i = [1..n] inclusive    and Sum( J_i * N_i ) == n
        E.g. if amount = 1   J = [[1]]  since 1 = 1*1
             if amount = 2   J = [[2], [1, 1]] since 2 = 2*1
             if amount = 3   J = [[3], [2, 1], [1, 1, 1]] since 3 = 2+1 = 3*1
             if amount = 4   J = [[4], [3, 1], [2, 2], [2, 1, 1], [1, 1, 1, 1]] since 4 = 3+1 = 2+2 = 2*1+1 = 4*1
    Similar to the coin change problem
    """
    coins = [x+1 for x in range(0, amount)]
    coins = [x for x in range(amount, 0, -1)]
    hand = [] if hand is None else hand
    if amount == 0:
        yield hand
    for coin in coins:
        # ensures we don't give too much change, and combinations are unique
        if coin > amount or (len(hand) > 0 and hand[-1] < coin):
            continue
        for result in find_all_sums(amount - coin, hand=hand + [coin]):
            yield result

def cartesian_product_index_cycles(ZSw, ZSh):
    """ 
    - Find 𝑍(𝑆w) * 𝑍(𝑆h) : The cartesian product of two cycle indices (that of the rows * that of the columns).
    - A cycle of length 'i' times another independent cycle of length 'j' has a resuting length of lcm(i, j).
    - So multiplying the variables x_i with x_j results in the variable x_(lcm(i, j))  as in the cycle index 
      notation the subscript is the cycle length
    - The superscript (i.e. the power the variable is raised to) is the repetition count of the cycle.
      The product cycle is repeated from i*j to lcm(i, j) so the cycle repetition count increases by a
      factor of (i*j) / lcm(i, j)
      So the the product of the superscripts of the variables  x_i^m * x_j^n, i.e. m, n becomes
      m * n * ( (i*j) / (lcm(i, j) )
    - Thus we loop through the terms in ZSw and ZSh, performing a cartesian product of the terms
      using the calculation:
      \[ x_i^m * x_j^n = x_{lcm(i,j)} ^ {(m*n*i*j/lcm(i,j)} \]
    - The coefficients of the polynomials multiply as usual.
    - Note: We don't include the division by the group size in the cycle indices. Will divide by it in the
            last step of the solution after evaluating the polynomial
    """
    # Perform a pairwise cartesian product of the two cycle_indices
    cycle_index = {}
    for sw,coeff_sw in ZSw.items():
        for sh,coeff_sh in ZSh.items():
            coeff = coeff_sw * coeff_sh
            key = ()
            for w in sw:
                for h in sh:
                    subscript = lcm(w[0], h[0])
                    superscript = w[1]*h[1] * w[0]*h[0] // lcm(w[0], h[0])
                    key += (subscript, superscript),
            key = tuple(sorted(key)) # sort to make x1*x2 == x2*x1
            if not cycle_index.get(key):
                cycle_index[key] = coeff
            else:
                cycle_index[key] += coeff

    return cycle_index



def debug_print_cycle_index(cycle_index):
    """
    For debugging: pretty print cycle index.
    e.g.  {((1, 2),): 1, ((2, 1),): 1}  ==> 1*x₁²  + 1*x₂¹
          {((1, 3),): 1, ((1, 1), (2, 1)): 3, ((3, 1),): 2}  ==> 1*x₁³  + 3*x₁¹ x₂¹  + 2*x₃¹
    """
    # Not python2 friendly
    '''
    superscripts = {'0':'\u2070', '1':'\u00B9', '2':'\u00B2', '3':'\u00B3', '4':'\u2074', '5':'\u2075', '6':'\u2076', '7':'\u2077', '8':'\u2078', '9':'\u2079'}
    subscripts = {'0':'\u2080', '1':'\u2081', '2':'\u2082', '3':'\u2083', '4':'\u2084', '5':'\u2085', '6':'\u2086', '7':'\u2087', '8':'\u2088', '9':'\u2089'}
    count = 0
    for k,v in cycle_index.items():
        print("{}*".format(v), end='')  # The coefficient
        for kk in k:
            print("x", end='')  # variable
            for c in str(kk[0]):
                print(subscripts[c], end='')
            for c in str(kk[1]):
                print(superscripts[c], end=' ')
        count += 1
        if count < len(cycle_index):
            print(" + ", end='')
    print("")
    '''
    pass

def evaluate_polynomial(cycle_index, x):
    """ 
    Evaluate the multi-variable polynomial for 'x'
    e.g. for the cycle index {((1, 3),): 1, ((1, 1), (2, 1)): 3, ((3, 1),): 2}
         which can be expressed as 1*x₁³  + 3*x₁¹ x₂¹  + 2*x₃¹
         Substitute 'x' for x₁, x₂, x₃ and evaluate the polynomial expression.
    """
    ret = 0
    for k, v in cycle_index.items():
        val = v
        for tuple_pair in k:
            val *= (x ** tuple_pair[1])
        ret += val
    return ret


def solution(w, h, s):
    # Compute the row-wise and column-wise cycle indices
    index_w = Z_Sn(w)
    index_h = Z_Sn(h)
    # Perform the cartesian product of the two
    index = cartesian_product_index_cycles(index_w, index_h)
    # Evaluate the cycle index for the given number of colorings
    # Casts to 'long' are for python2
    val = evaluate_polynomial(index, s)
    # Divide by the size of the group
    val = val // factorial(w) // factorial(h)
    return str(val)


"""
index2 = Z_Sn(2)
print("index2 = "); debug_print_cycle_index(index2)
index3 = Z_Sn(3)
print("index3 = "); debug_print_cycle_index(index3)
index = cartesian_product_index_cycles(index2, index3)
print("index = "); debug_print_cycle_index(index)
answer = evaluate_polynomial(index, 4) // (factorial(2) * factorial(3))

"""
#assert(solution(2, 2, 2) == "7")
#assert(solution(2, 3, 4) == "430")

"""
for w in range(3, 12+1):
    for h in range(3, 12+1):
        for s in range(2, 20+1):
            print(w)
            print(h)
            print(s)
            print(solution(w, h, s))
"""
'''
index_a = Z_Sn(9)
print("index_a = "); debug_print_cycle_index(index_a)
index_b = Z_Sn(9)
print("index_b = "); debug_print_cycle_index(index_b)
index = cartesian_product_index_cycles(index_a, index_b)
print("index = "); debug_print_cycle_index(index)
answer = evaluate_polynomial(index, 6) // factorial(6) // factorial(6)
print("answer = "); print(answer)
'''
