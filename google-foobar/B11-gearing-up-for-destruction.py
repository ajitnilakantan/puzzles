from fractions import Fraction

"""
For a gear train (https://en.wikipedia.org/wiki/Gear_train#Idler_gears) the speed differential of the
first and last gears only depends on the ratios of the first and last gears. The intermediate gear sizes
don't matter.

If we look at the different cases:
- 2 pegs
  If we have two gears at positions g0, g1 with radii r0, r1 then
  r0 + r1 = g1 - g0
  r0 = 2 * r1,  or r0 - 2*r1 = 0
  --> r0 = 2*(g1-g0)/3  ;  r1 = 1*(g1-g2)/4

- 3 pegs
:w

which can be rewritten as  [1  1] * [r0] = [g1-g0] 
                           [1 -2]   [r1]   [ 0   ]
If we have  three gears at g0, g1, g2  with radii r0, r1, r2 we have
r0+r1 = g1-g0
r1+r2 = g2-g1
r0 - 2*r2 = 0 
which can be written in matrix form A*x = b 
[1 1  0]    [r0]   [g1-g0]
[0 1  1]  * [r1] = [g2-g1]
[1 0 -2]    [r2]   [0    ]

The expression A*x = b generalizes for any number of gears. The matrix A is a square matrix
The diagonal elements and the diagonal immediately to the right are 1 except
for the lower right which is -2 and the lower left which is 1.
The column vector 'x' is [r0...rn-1] which we solve for (we need r0 and rn-1) and
the column vector 'b' is [g1-g0, g2-g1, g3-g2,..., 0] the differences in g_n (which are given)
and the last element being '0'.

We can solve the equations using regular Gaussian elimination, but if we can convert it to
an upper triangular matrix, we can solve it much faster.
The matrix A is *almost* in upper triangular form -- except for the '1' in the lower right hand corner.
By alternately adding the first row, subtracting the second, adding the next, subtracting the next... to the
last row, we get an upper triangular matrix. The 1s cancel out except for the lower right hand -2...if
the size of the matrix, n, is even we subtract 1, so the -2 becomes -3. If n is odd, we add 1m, so -2 becomes -1

By dividing by -2 or -3 the lower right hand corner element also becomes 1.
Note- We need to apply the same operations to the 'b' vector.
We end up with an A matrix that looks like
n = 3        n = 4
1 1 0     1 1 0 0
0 1 1     0 1 1 0
0 0 1     0 0 1 1
          0 0 0 1

Finally, we can convert to a purely diagonal matrix by eliminating the second 1 in each row.
Working bottom up we subtract the next row from the current row.
We apply all these operations directly on the 'b' vector since we 'A' matrix ends up as the unit matrix.
"""


def solution(pegs):
    num_pegs = len(pegs)

    # Handle degenerate case when num_pegs = 2
    if num_pegs == 2:
        r0 = Fraction(2*(pegs[1]-pegs[0]), 3)
        if r0 <= 0:
            return [-1, -1]
        else:
            return [r0.numerator, r0.denominator]

    # Apply the same operations to 'b' that convert A to upper triangular (we don't really care about 'A')
    # This is the last element of 'b':
    last_radius = 0
    for n in range(0, num_pegs-1, 2):
        last_radius += pegs[n+1]-pegs[n]
    for n in range(1, num_pegs-1, 2):
        last_radius += pegs[n]-pegs[n+1] 
    if last_radius <= 0:
        return [-1, -1]
    
    # Go across, making sure all radii are positive
    next_radius = 2* last_radius if num_pegs % 2 == 1 else 2*last_radius / 3
    for n in range(0, num_pegs-1):
        next_radius = pegs[n+1] - pegs[n] - next_radius
        if next_radius <= 0:
            # All radii must be > 0 to make physical sense
            return [-1, -1]
 
    # Return the first gear ratio
    first_radius = Fraction(2*last_radius, 3) if num_pegs % 2 == 0 else Fraction(2*last_radius,1)
   
    return [first_radius.numerator, first_radius.denominator]


#assert(solution([4, 30, 50]) == [12, 1])
#assert(solution([4, 17, 50]) == [-1, -1])
#print(solution([4, 30, 50, 56]))
#assert(solution([4, 30, 50, 56]) == [8, 1])
