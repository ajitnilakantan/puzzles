"""
Searching on 'state transition terminal states' leads to Markov chains and searching on 'markov chain terminal states' leads to
Absorbing Markov chain: https://en.wikipedia.org/wiki/Absorbing_Markov_chain

- From the Wikipedia article:
  - Identify the terminal (absorbing) states ('r' in total)  and the transient states ('t' in total)
  - Compute the t*t matrix Q of the probabilities to transition from one transient state to another
  - Compute R which is a t by r matrix of probabilities to transition from a transient state to a terminal state
  - Compute N = (I = Q)**-1    (i.e. the inverse of thre matrix (I=Q))
  - Compute B = N*R  which is the "absorbing probabilities"

Since we are working with fractions, will use the "fraction" package instead of floats
"""

from fractions import Fraction

def gcd(a,b):
    a = abs(a)
    b = abs(b)
    while b > 0:
        a, b = b, a % b
    return 1 if a == 0 else a

def lcm(a,b):
    return (a*b) // gcd(a,b)

# Matrix operations based on: https://integratedmlai.com/matrixinverse/
# Adapted to use fractions
def zero_matrix(num_rows, num_cols):
    return [[0 for x in range(num_cols)] for y in range(num_rows)]
def unit_matrix(num_rows, num_cols):
    return [[1 if x==y else 0 for x in range(num_cols)] for y in range(num_rows)]

def multiply_matrix(A, B):
    rows_A, cols_A = len(A), len(A[0])
    rows_B, cols_B = len(B), len(B[0])
    assert(cols_A == rows_B)

    C = zero_matrix(rows_A, cols_B)
    for i in range(rows_A):
        for j in range(cols_B):
            for ii in range(cols_A):
                C[i][j] += A[i][ii] * B[ii][j]
    return C

def invert_matrix(A):
    # Unit matrix for augmented matrix (Gauss Jordan elimination)
    rows_A, cols_A = len(A), len(A[0])
    assert(cols_A == rows_A)

    I = unit_matrix(rows_A, cols_A)

    for fd in range(rows_A):
        fdScaler = Fraction(1,1) / A[fd][fd]
        for j in range(cols_A):
            A[fd][j] *= fdScaler
            I[fd][j] *= fdScaler
        for i in list(range(rows_A))[0:fd] + list(range(rows_A))[fd+1:]:
            crScaler = A[i][fd]
            for j in range(cols_A):
                A[i][j] = A[i][j] - crScaler * A[fd][j]
                I[i][j] = I[i][j] - crScaler * I[fd][j]
    return I


def solution(m):
    #print(f" m = \n{m}")
    # Step 1 - Identify the terminal and transient states (list of ints)
    transient_states = []
    terminal_states = []
    for row_index, row in enumerate(m):
        is_terminal = True
        for col in row:
            if col != 0:
                is_terminal = False
                break
        if is_terminal:
            terminal_states.append(row_index)
        else:
            transient_states.append(row_index)
    num_transient_states = len(transient_states)
    num_terminal_states = len(terminal_states)
    #print(f"transient = {transient_states}")
    #print(f"terminal = {terminal_states}")

    # Handle the degenerate cases. If num_terminal_states == 1, then return the answer immediately
    # Otherwise get divide by zeros in the calculation of the inverse
    if num_terminal_states == 1:
        # 100% change to end up in the only terminal state
        return [1, 1]

    # Step 2 - Construct 'Q' : 'transient' to 'transient' transition probabilities
    Q = zero_matrix(num_transient_states, num_transient_states)
    for j in range(num_transient_states):
        # Fill row by row
        for i in range(num_transient_states):
            Q[j][i] = m[transient_states[j]][transient_states[i]]
    # Convert to fraction
    for j in range(num_transient_states):
        sum = 0
        for i in range(len(m[0])):
            sum += m[transient_states[j]][i]
        for i in range(num_transient_states):
            Q[j][i] = Fraction(Q[j][i], sum)

    #print(f"Q = \n{Q}")
    # Step 3 - Compute I-Q
    for j in range(num_transient_states):
        for i in range(num_transient_states):
            Q[j][i] = Fraction(1, 1) - Q[j][i] if j == i else -Q[j][i]

    #print(f"I-Q = {Q}")
    # Step 4 - Compute N = Inverse(Q)
    N = invert_matrix(Q)
    #print(f"N = {N}")

    # Step 5 - Construct 'R" : transient x terminal matrix of 'transient' to 'terminal' transition probabilities
    R = zero_matrix(num_transient_states, num_terminal_states)
    for j in range(num_transient_states):
        # Fill row by row
        for i in range(num_terminal_states):
            R[j][i] = m[transient_states[j]][terminal_states[i]]
    # Convert to fraction
    for j in range(num_transient_states):
        sum = 0
        for i in range(len(m[0])):
            sum += m[transient_states[j]][i]
        for i in range(num_terminal_states):
            R[j][i] = Fraction(R[j][i], sum)

    #print(f"R = {R}")
    # Step 6 - Compute absorbing probabilities:  B = N*R
    B = multiply_matrix(N, R)
    #print(f"B = N*R = {B}")

    # Step 7 - "Another property is the probability of being absorbed in the absorbing
    #          state j when starting from transient state i, which is the (i,j)-entry of the matrix"
    #   The ore starts in state 0, so we only care about the first row of B
    # Extract probabilities of row 0
    prob = [ B[0][i] for i in range(num_terminal_states) ]

    # Get the lcm of the denominators, and return in the format required by the question.
    all_lcm = 1
    for i in range(num_terminal_states):
        all_lcm = lcm(all_lcm, prob[i].denominator)
    prob = [ prob[i].numerator * (all_lcm // prob[i].denominator) for i in range(num_terminal_states) ]
    prob.append(all_lcm)

    #print(f"Return {prob}")
    return prob


#assert([7, 6, 8, 21] == solution([[0, 2, 1, 0, 0], [0, 0, 0, 3, 4], [0, 0, 0, 0, 0], [0, 0, 0, 0,0], [0, 0, 0, 0, 0]]))
#assert([0, 3, 2, 9, 14] == solution([[0, 1, 0, 0, 0, 1], [4, 0, 0, 3, 2, 0], [0, 0, 0, 0, 0, 0], [0, 0, 0, 0, 0, 0], [0, 0, 0, 0, 0, 0], [0, 0, 0, 0, 0, 0]]))

# Degenerate cases
#assert([1, 1] == solution([[0, 0], [0, 1]]))
#assert([1, 1] == solution([[0, 1], [0, 0]]))
#assert([1, 1] == solution([[0]]))

