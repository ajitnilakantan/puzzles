# This Python file uses the following encoding: utf-8
from __future__ import division
from __future__ import print_function

from collections import Counter

"""
We can use a SAT solver to find a solution.  For each element of the grid (e.g. 'A') look
at its neighbors:  A B  to the right and below
                   C D
- If A is true that means only one of A,B,C,D is true in the previous iteration, which can be written as:
    Only 1 of 4: (A ∧ ¬B ∧ ¬C ∧ ¬D) ∨ (¬A ∧ B ∧ ¬C ∧ ¬D) ∨ (¬A ∧ ¬B ∧ C ∧ ¬D) ∨ (¬A ∧ ¬B ∧ ¬C ∧ D)
    CNF form: (¬A ∨ ¬B) ∧ (¬A ∨ ¬C) ∧ (¬A ∨ ¬D) ∧ (A ∨ B ∨ C ∨ D) ∧ (¬B ∨ ¬C) ∧ (¬B ∨ ¬D) ∧ (¬C ∨ ¬D)
- If A is false that means all of A,B,C,D are false / or more than one of A,B,C,D is true in the previous iteration, which can be written as:
    0, 2, 3, or 4 of 4: ~((A ∧ ¬B ∧ ¬C ∧ ¬D) ∨ (¬A ∧ B ∧ ¬C ∧ ¬D) ∨ (¬A ∧ ¬B ∧ C ∧ ¬D) ∨ (¬A ∧ ¬B ∧ ¬C ∧ D))  ==
    CNF form: (¬A ∨ B ∨ C ∨ D) ∧ (A ∨ ¬B ∨ C ∨ D) ∧ (A ∨ B ∨ ¬C ∨ D) ∧ (A ∨ B ∨ C ∨ ¬D)

"""

################################################################################
### BEGIN SAT SOLVER ###########################################################
# Use the SAT solver from https://sahandsaba.com/understanding-sat-by-implementing-a-simple-sat-solver-in-python.html
### SATInstance
"""
Some notes on encoding:
- Variables are encoded as numbers 0 to n - 1.
- Literal v is encoded as 2 * v and ~v as 2 * v + 1. So the foremost
  bit of a literal encodes whether it is negated or not. This can be
  tested simply with checking if l & 1 is 0 or 1.
- To negate a literal, we just have to toggle the foremost bit. This
  can done easily by an XOR with 1: the negation of l is l ^ 1.
- To get a literal's variable, we just need to shift to the right. This
  can be done with l >> 1.
Example: Let's say variable b is encoded with number 3. Then literal b
is encoded as 2 * 3 = 6 and ~b as  2 * 3 + 1 = 7.
"""
#from __future__ import division
#from __future__ import print_function

__author__ = 'Sahand Saba'


class SATInstance(object):
    def parse_and_add_clause(self, line):
        clause = []
        for literal in line.split():
            negated = 1 if literal.startswith('~') else 0
            variable = literal[negated:]
            if variable not in self.variable_table:
                self.variable_table[variable] = len(self.variables)
                self.variables.append(variable)
            encoded_literal = self.variable_table[variable] << 1 | negated
            clause.append(encoded_literal)
        self.clauses.append(tuple(set(clause)))

    def __init__(self):
        self.variables = []
        self.variable_table = dict()
        self.clauses = []

    @classmethod
    def from_file(cls, file):
        instance = cls()
        for line in file:
            line = line.strip()
            if len(line) > 0 and not line.startswith('#'):
                instance.parse_and_add_clause(line)
        return instance

    def literal_to_string(self, literal):
        s = '~' if literal & 1 else ''
        return s + self.variables[literal >> 1]

    def clause_to_string(self, clause):
        return ' '.join(self.literal_to_string(l) for l in clause)

    def assignment_to_string(self, assignment, brief=False, starting_with=''):
        literals = []
        for a, v in ((a, v) for a, v in zip(assignment, self.variables)
                     if v.startswith(starting_with)):
            if a == 0 and not brief:
                literals.append('~' + v)
            elif a:
                literals.append(v)
        return ' '.join(literals)

### watchlist.py
#from __future__ import division
#from __future__ import print_function

from collections import deque
from sys import stderr


__author__ = 'Sahand Saba'
__email__ = 'sahands@gmail.com'


def dump_watchlist(instance, watchlist):
    print('Current watchlist:', file=stderr)
    for l, w in enumerate(watchlist):
        literal_string = instance.literal_to_string(l)
        clauses_string = ', '.join(instance.clause_to_string(c) for c in w)
        print('{}: {}'.format(literal_string, clauses_string), file=stderr)


def setup_watchlist(instance):
    watchlist = [deque() for __ in range(2 * len(instance.variables))]
    for clause in instance.clauses:
        # Make the clause watch its first literal
        watchlist[clause[0]].append(clause)
    return watchlist


def update_watchlist(instance,
                     watchlist,
                     false_literal,
                     assignment,
                     verbose):
    """
    Updates the watch list after literal 'false_literal' was just assigned
    False, by making any clause watching false_literal watch something else.
    Returns False it is impossible to do so, meaning a clause is contradicted
    by the current assignment.
    """
    while watchlist[false_literal]:
        clause = watchlist[false_literal][0]
        found_alternative = False
        for alternative in clause:
            v = alternative >> 1
            a = alternative & 1
            if assignment[v] is None or assignment[v] == a ^ 1:
                found_alternative = True
                #del watchlist[false_literal][0]
                watchlist[false_literal].popleft()
                watchlist[alternative].append(clause)
                break

        if not found_alternative:
            if verbose:
                dump_watchlist(instance, watchlist)
                print('Current assignment: {}'.format(
                      instance.assignment_to_string(assignment)),
                      file=stderr)
                print('Clause {} contradicted.'.format(
                      instance.clause_to_string(clause)),
                      file=stderr)
            return False
    return True

### recursive_sat.py
#from __future__ import division
#from __future__ import print_function

from sys import stderr

#from .watchlist import update_watchlist

def solve(instance, watchlist, assignment, d, verbose):
    """
    Iteratively solve SAT by assigning to variables d, d+1, ..., n-1. Assumes
    variables 0, ..., d-1 are assigned so far. A generator for all the
    satisfying assignments is returned.
    """

    # The state list wil keep track of what values for which variables
    # we have tried so far. A value of 0 means nothing has been tried yet,
    # a value of 1 means False has been tried but not True, 2 means True but
    # not False, and 3 means both have been tried.
    n = len(instance.variables)
    state = [0] * n

    while True:
        if d == n:
            yield assignment
            d -= 1
            continue
        # Let's try assigning a value to v. Here would be the place to insert
        # heuristics of which value to try first.
        tried_something = False
        for a in [0, 1]:
            if (state[d] >> a) & 1 == 0:
                if verbose:
                    print('Trying {} = {}'.format(instance.variables[d], a),
                          file=stderr)
                tried_something = True
                # Set the bit indicating a has been tried for d
                state[d] |= 1 << a
                assignment[d] = a
                if not update_watchlist(instance, watchlist,
                                        d << 1 | a,
                                        assignment,
                                        verbose):
                    assignment[d] = None
                else:
                    d += 1
                    break

        if not tried_something:
            if d == 0:
                # Can't backtrack further. No solutions.
                return
            else:
                # Backtrack
                state[d] = 0
                assignment[d] = None
                d -= 1

def solve_recursive(instance, watchlist, assignment, d, verbose):
    """
    Recursively solve SAT by assigning to variables d, d+1, ..., n-1. Assumes
    variables 0, ..., d-1 are assigned so far. A generator for all the
    satisfying assignments is returned.
    """
    if d == len(instance.variables):
        yield assignment
        return

    for a in [0, 1]:
        if verbose:
            print('Trying {} = {}'.format(instance.variables[d], a),
                  file=stderr)
        assignment[d] = a
        if update_watchlist(instance,
                            watchlist,
                            (d << 1) | a,
                            assignment,
                            verbose):
            for a in solve(instance, watchlist, assignment, d + 1, verbose):
                yield a

    assignment[d] = None


### END SAT SOLVER #############################################################
################################################################################

def only_one(instance, y, x):
    """
    Only 1 of 4: (A ∧ ¬B ∧ ¬C ∧ ¬D) ∨ (¬A ∧ B ∧ ¬C ∧ ¬D) ∨ (¬A ∧ ¬B ∧ C ∧ ¬D) ∨ (¬A ∧ ¬B ∧ ¬C ∧ D)
    CNF: (¬A ∨ ¬B) ∧ (¬A ∨ ¬C) ∧ (¬A ∨ ¬D) ∧ (A ∨ B ∨ C ∨ D) ∧ (¬B ∨ ¬C) ∧ (¬B ∨ ¬D) ∧ (¬C ∨ ¬D)
    """
    instance.parse_and_add_clause("x{:02d}{:02d} x{:02d}{:02d} x{:02d}{:02d} x{:02d}{:02d}".format(y,x, y,x+1, y+1,x, y+1,x+1))
    instance.parse_and_add_clause("~x{:02d}{:02d} ~x{:02d}{:02d}".format(y,x, y,x+1))
    instance.parse_and_add_clause("~x{:02d}{:02d} ~x{:02d}{:02d}".format(y,x, y+1,x))
    instance.parse_and_add_clause("~x{:02d}{:02d} ~x{:02d}{:02d}".format(y,x, y+1,x+1))
    instance.parse_and_add_clause("~x{:02d}{:02d} ~x{:02d}{:02d}".format(y,x+1, y+1,x))
    instance.parse_and_add_clause("~x{:02d}{:02d} ~x{:02d}{:02d}".format(y,x+1, y+1,x+1))
    instance.parse_and_add_clause("~x{:02d}{:02d} ~x{:02d}{:02d}".format(y+1,x, y+1,x+1))

def none_or_more_than_one(instance, y, x):
    """
    0, 2, 3, or 4 of 4: ~((A ∧ ¬B ∧ ¬C ∧ ¬D) ∨ (¬A ∧ B ∧ ¬C ∧ ¬D) ∨ (¬A ∧ ¬B ∧ C ∧ ¬D) ∨ (¬A ∧ ¬B ∧ ¬C ∧ D))  ==
    CNF: (¬A ∨ B ∨ C ∨ D) ∧ (A ∨ ¬B ∨ C ∨ D) ∧ (A ∨ B ∨ ¬C ∨ D) ∧ (A ∨ B ∨ C ∨ ¬D)
    """
    instance.parse_and_add_clause("~x{:02d}{:02d} x{:02d}{:02d} x{:02d}{:02d} x{:02d}{:02d}".format(y,x, y,x+1, y+1,x, y+1,x+1))
    instance.parse_and_add_clause("x{:02d}{:02d} ~x{:02d}{:02d} x{:02d}{:02d} x{:02d}{:02d}".format(y,x, y,x+1, y+1,x, y+1,x+1))
    instance.parse_and_add_clause("x{:02d}{:02d} x{:02d}{:02d} ~x{:02d}{:02d} x{:02d}{:02d}".format(y,x, y,x+1, y+1,x, y+1,x+1))
    instance.parse_and_add_clause("x{:02d}{:02d} x{:02d}{:02d} x{:02d}{:02d} ~x{:02d}{:02d}".format(y,x, y,x+1, y+1,x, y+1,x+1))

def add_subclause(instance, offset, width, subclause):
    """
    Utility method: add the specified CNF clause (specified as a string) to the solver
    """
    clause_string = ""
    counter = 0
    for s in subclause:
        y = counter // width
        x = offset + counter % width
        clause_string += "x{:02d}{:02d} ".format(y, x) if s else "~x{:02d}{:02d} ".format(y,x)
        counter += 1

    instance.parse_and_add_clause(clause_string)

def initialize_variables(instance, height, width):
    """
    Utility method: add the variable names (specified as 'x_{row}{height}') to the list of
    variables. Pre-add them, so they are added in a known order (left to right, up to down on the grid).
    Makes reading rhe solution easier -- the nth position in the solution corresponds to the nth position
    in the (height+1)x(width+1) expanded grid.
    """
    for y in range(0, height+1):
        for x in range(0, width+1):
            variable  = "x{:02d}{:02d}".format(y, x)
            if variable not in instance.variable_table:
                instance.variable_table[variable] = len(instance.variables)
                instance.variables.append(variable)


def debug_print_solution(s, h, w):
    """ For debugging: pretty print the grid. Not Python2 friendly """
    pass
    """
    print("  ", end='')
    for i in range(0, h*w):
        if (i and i%w == 0):
            print("\n  ", end='')
        print('O' if s[i] else '.', end='')
    print("\n")
    """

##
## SOLUTION1:  Use a SAT solver to get all possible solutions and return the length.
## This works but is TOO SLOW when the width of the grid becomes large (height is under 10)
##
def solution_whole_grid(g):
    """ Use a SAT solver to find all possible solutions. Each grid point is a boolean variable and
        the clauses for each are whether 1 of 4 of the neighbour variables were set (var true) or
        if 0,2,3,or 4 of 4 of the neighbours were set (var false) in the previous iteration.
        Count up all the solutions and return the count. """
    height = len(g)
    width = len(g[0])

    # Initialize the SAT solver
    instance = SATInstance()
    initialize_variables(instance, height, width)

    # The grid (and number of variables) expands from H*W to (H+1)*(W+1)
    # Set up the CNF clauses
    for y in range(0, height):
        for x in range(0, width):
            if g[y][x]:
                only_one(instance, y, x)
            else:
                none_or_more_than_one(instance, y, x)
    # Solve it
    watchlist = setup_watchlist(instance)
    assignment = [None] * len(instance.variables)
    sol = solve(instance, watchlist, assignment, 0, False)

    return len(list(sol))

"""
SOLUTION2:
  Instead of solving the grid in one shot, subdivide it horizontally into "chunks" and
  "stitch" the results together.  E.g. if a grid of with height=5 width=12 is broken into three 4-sized
  chunks -- we can solve 3 smaller height=5 width=4 problems (which expands into a grid of size height=6, width=5).
  The last column of the first subgrid (which is the 5th column of the expanded grid) must match the first column of the
  of the second grid  -and- the last column of the second grid (the 5th column of it) must match the first column of the
  third grid.  This way we have consistent solution(s).
  For each subgrid, we don't care about the interior column -- only first and last columns and the number of solutions that
  correspond to evey pair of a first-last column (i.e. the count of the 'interior' solutions we are ignoring).
  The final step is to take the sum of the product of each count across all the "stitched" solutions

  Additional optimization: remember the solution of subgrids in case they repeat.
"""
memo_solutions = {}
def solve_sub_grid(g, offset_pos, offset_width):
    """ Solve the subgrid starting at y=0, x=offset, with size height=height, width=offset_width (rows/columns)
        Return a tuple of the first and last column solutions as a tuple of 2 tuples and 1 count:
        ((first_column_solution, last_column_solution), number_solutions) """
    # Height/width of original grid
    height = len(g)
    width = len(g[0])

    # Sanity check
    assert(offset_pos < width)
    assert(offset_pos + offset_width <= width)
    assert(offset_width > 0)

    # Check saved solutions:
    hash = ""
    for y in range(0, height):
        for x in range(offset_pos, offset_pos + offset_width):
            hash += "1" if g[y][x] else "0"
    if memo_solutions.get(hash):
        return memo_solutions[hash]

    # Initialize the SAT solver
    instance = SATInstance()
    initialize_variables(instance, height, offset_width)

    # The grid (and number of variables) expands from H*W to (H+1)*(W+1)
    # Set up the CNF clauses
    for y in range(0, height):
        for x in range(offset_pos, offset_pos + offset_width):
            if g[y][x]:
                only_one(instance, y, x-offset_pos)
            else:
                none_or_more_than_one(instance, y, x-offset_pos)
    # Solve it
    watchlist = setup_watchlist(instance)
    assignment = [None] * len(instance.variables)
    sol = solve(instance, watchlist, assignment, 0, False)

    # Accumulate a Counter of the number of solutions to each (solution_to_first_column, solution_to_last_column).
    first_last_list = Counter()
    for s in sol:
        #s = _s.copy()
        assert(len(s) == (height+1) * (offset_width+1))
        first_col = get_column(s, 0, height+1, offset_width+1)
        last_col = get_column(s, offset_width, height+1, offset_width+1)
        #assert(len(first_col) == height+1)
        #assert(len(last_col) == height+1)
        first_last_list[(first_col, last_col)] += 1

    # Save the solution
    memo_solutions[hash] = first_last_list

    return first_last_list

def get_column(solution, column_number, height, width):
    """ For a given solution, provided as a 1D list of true/false values for each element in the grid:
        extract the given column. """
    #col2 = tuple(solution[column_number + i*width] for i in range(0, height)) 
    if height == 9:
        col = solution[column_number] | solution[column_number + width] << 1 | solution[column_number + 2*width] << 2 | \
              solution[column_number + 3*width] << 3 | solution[column_number + 4*width] << 4 | solution[column_number + 5*width] << 5 | \
              solution[column_number + 6*width] << 6 | solution[column_number + 7*width] << 7 | solution[column_number + 8*width] << 8
    elif height == 8:
        col = solution[column_number] | solution[column_number + width] << 1 | solution[column_number + 2*width] << 2 | \
              solution[column_number + 3*width] << 3 | solution[column_number + 4*width] << 4 | solution[column_number + 5*width] << 5 | \
              solution[column_number + 6*width] << 6 | solution[column_number + 7*width] << 7
    elif height == 7:
        col = solution[column_number] | solution[column_number + width] << 1 | solution[column_number + 2*width] << 2 | \
              solution[column_number + 3*width] << 3 | solution[column_number + 4*width] << 4 | solution[column_number + 5*width] << 5 | \
              solution[column_number + 6*width] << 6
    elif height == 6:
        col = solution[column_number] | solution[column_number + width] << 1 | solution[column_number + 2*width] << 2 | \
              solution[column_number + 3*width] << 3 | solution[column_number + 4*width] << 4 | solution[column_number + 5*width] << 5 
    else:
        col = 0
        for i in range(0, height):
            if solution[column_number + i*width]:
                col |= (1<<i)
    return col

def solution(g):
    # return str(solution_whole_grid(g))
    height = len(g)
    width = len(g[0])

    # Step 1: Break the grid up horizonally into smaller chunks and recombine. 5 is a good size
    CHUNK_SIZE = 5
    subgrid_solutions = []
    # Optimization: Keep a hash table of first columns.  Key is first-column, value is array of indices
    hashtable_first_column_list = []
    for offset_pos in range(0, width, CHUNK_SIZE):
        offset_width = CHUNK_SIZE
        if (offset_pos + offset_width > width):
            offset_width = width - offset_pos
        if offset_width <= 0:
            break

        sol = solve_sub_grid(g, offset_pos, offset_width)
        subgrid_solutions.append(sol)
        hashtable_first_column = {}
        index = 0
        for s in sol:
            if hashtable_first_column.get(s[0]):
                hashtable_first_column[s[0]].append(index)
            else:
                hashtable_first_column[s[0]] = [index]
            index += 1
        hashtable_first_column_list.append(hashtable_first_column)

    # Step 2: Iterate through the subgrid solutions, match the last column of one to the first column of the next
    # Initialize with the first subgrid: Array of tuples of tuples-of-first-last-solution-chain + tuple-of-num-solutions
    solution_chain = [ (x, (y,)) for x,y in subgrid_solutions.pop(0).items() ]
    hashtable_first_column_list.pop(0)
    while len(subgrid_solutions) > 0:
        # next_subgrid = subgrid_solutions.pop(0)
        next_subgrid = [ (x, (y,)) for x,y in subgrid_solutions.pop(0).items() ]
        next_subgrid_hashtable = hashtable_first_column_list.pop(0)
        new_solution_chain = []
        for sol_chain1, sol_count1 in solution_chain:
            if next_subgrid_hashtable.get(sol_chain1[-1]):
                for index in next_subgrid_hashtable[sol_chain1[-1]]:
                    sol_chain2 = next_subgrid[index][0]
                    sol_count2 = next_subgrid[index][1]
                    # The last solution matches the first solution of the next
                    # Extend the chain of solutions and the count
                    chain_solution =  sol_chain1 + sol_chain2[1:]
                    chain_count = sol_count1 + sol_count2
                    new_solution_chain.append((chain_solution, chain_count))
        solution_chain = new_solution_chain

    # Step 3: Take the sum of the products of each solution of a subgrid for every "stitched chain" of solutions
    result = 0
    for item in solution_chain:
        sum = 1
        for s in item[1]:
            sum *= s
        result += sum

    # print(f"ZZZ GET {result}")
    return str(result)

"""
assert("4" == solution([[True, False, True], [False, True, False], [True, False, True]]))
assert("254" == solution([[True, False, True, False, False, True, True, True], [True, False, True, False, False, False, True, False], [True, True, True, False, False, False, True, False], [True, False, True, False, False, False, True, False], [True, False, True, False, False, True, True, True]]))
assert("11567" == solution([[True, True, False, True, False, True, False, True, True, False], [True, True, False, False, False, False, True, True, True, False], [True, True, False, False, False, False, False, False, False, True], [False, True, False, False, False, False, True, True, False, False]]))


g = [[True, True, False, True, False, True, False, True, True, False], [True, True, False, False, False, False, True, True, True, False], [True, True, False, False, False, False, False, False, False, True], [False, True, False, False, False, False, True, True, False, False]]
for i in range(0, len(g)):
    g[i] = 8*g[i]
for i in range(0, 4):
    g.append(g[i])
#for i in range(0, 4):
#    g.append(g[i])

print("g = {} x {}".format(len(g), len(g[0])))
sol = solution(g)
print("sol = {}".format(str(sol)))
"""
