""" 
    Solution 1:
    ----------
    Implement the search for the best solution as a bfs.
    At a given n we have the following options:
    = base case: n ==  power of 2 (i.e.  r**p) --> return 'p'  (p steps to half to 1)
    - find the closest power of 2 higher or lower. If there is a tie, take the lower:  2**p
      return |n - 2**p| + p   (i.e. steps to get to the power + steps down from the poer of two to 1)
    - if n is even: halve.  Add 1 to path lenght and add n/2 to queue
    - if n is odd:   add 1 to path lenght and add (n+1)/2 and (n-1)/2 to work queue
    Solution 2:
    ----------
    ==> This works, but it becomes too slow when the input number is very large.
    - Run a loop through the first 1000 numbers to see the patterns behind the operation
    - The function test_sequences() prints the operation applied at each step in the shortest sequence found
      in the BFS.  We really only care about the odd numbers (which have a choice of increment or decrement)
    - At first, there is no pattern, but if you print out the numbers in binary, the choice of increment/decrement
      seems to be somehow related to the ending parity of the bits.
    - This makes sense because for an odd number we have the choice to increment or to decrement.
      An odd number ends in 1 -- both incrementing and decrementing make the last digit 0, but we 
      want to maximize the number of zeros at the end of the number. If you look the next digit over,
      and odd number can end in either 11 or 01.  
      - It is clear that if it ends in 11, we should add one as with the carry, the number will end
        in 100.  The next operation we can divide twice by 2 reducing the bitlenght by 2.   If we had
        subtracted 1, then the number would end in 10 so in the next operation, we can divide by 2 only once
        and reduce by only 1 bit before ending back at an odd number
      - Similarly if the odd number ends in 01, we should subtract 1 to have the number ending in 00, so that
        wit can lose 2 digits in the subsequent divide operations.
    - The outcomes are fixed base on if:
      - the number is a power of 2 (e.g. 2**p) --> 'p' additional steps bring you to '1'
      - the number is even -- keep dividing by 2 'n' times until you hit an odd number. Add 'n' additional steps and
        reduce the digits by 'n'
      - the number is odd. If it ends in 01, decrement.  If it ends in 11, increment
    - Because the outcomes are fixed, we can unroll the entire DFS and have a single loop.
    - Optmize repeated divistion by 2 for even numbers.  Count the trailing 0s and increment steps + right shift number in one
      step: https://stackoverflow.com/questions/40608220/pythonic-way-to-count-the-number-of-trailing-zeros
    Solution 3:
    ----------
    - The solution above is super fast, but still seems too slow for Google's test cases.
    - This solution extends the previous solution by building a simple finite state machine while scanning the bits of the
      number from least significant to most significant. 
    - We move between the states 'STATE_NONE' at initialization, STATE_ZERO when we hit a zero, STATE_ONES for a string of two or more
      ones and STATE_ONE for a single one. These are the states at the current bit before the current bit has been processed (i.e.
      the immediate previous history).
    - The logic is simple: if we hit bit zero:  If the state is STATE_ONES -- there is an overflow so the zero effectively
      becomes 1, and we switch to the 1 case.   Otherwise, we increment the steps (divide by 2)
      If we hit a 1 we switch to STATE_ONE or the STATE_ONES if we are already in STATE_ONE. If the previous state was ZERO we 
      increment the step counter for the increment/decrement operation (which operation depends on the next bit to the left
      but in either case it is still one step). We then increment the step count as long as we are hitting 1s (which
      effectively become zeros) until we hit a 0 or leftmost bit. If we hit a zero we continue the state-transitions.
      If we hit the leftmost bit, there is check for an overflow.
   ===> Still fails verification test... Looking closer there is a special case for the number "3".
      With our logic, we increment and divide twice e..g 3->4->2->1   in three steps,   but 3 seems to 
      be the only number where it is faster to decrement twice  e.g.  3->2->1 in two steps.
      Added this special case handling to Solution 2.
"""

'''
from math import log, ceil, floor

class Node:
    """ A node class for Pathfinding """
    def __init__(self, num, steps, parent = None, operation=None):
        # Current number to work on
        self.num = num
        # Current number of steps
        self.steps = steps
    def __eq__(self, other):
        return self.num == other.num and self.steps == other.steps
    def __str__(self):
        """Gives a short string representation of the variable."""
        return "Node({}, {})".format(self.id, self.steps)
    def __repr__(self):
        """Gives a precise string representation of the variable."""
        return "Node({}, {})".format(self.id, self.steps)

def is_power_of_two(n):
    """ Check if n is a power of 2.
    >>> is_power_of_two(7)
    False
    >>> is_power_of_two(8)
    True
    """
    return (n & (n-1) == 0) and n != 0

def closest_power_of_two(x):
    """ Return the closest power of 2 to a number.
    >>> closest_power_of_two(7)
    3
    >>> closest_power_of_two(6)
    2
    """
    lower, upper = int(floor(log(x, 2))), int(ceil(log(x, 2)))
    diff_lower, diff_upper = x - 2**lower, 2**upper - x
    return lower if  diff_lower <= diff_upper else upper

def closest_power_of_two(x):
    """ Return the closest power of 2 to a number. Return a tuple of the power and the difference.
    >>> closest_power_of_two(7)
    (3, 1)
    >>> closest_power_of_two(6)
    (2, 2)
    """
    lower, upper = int(floor(log(x, 2))), int(ceil(log(x, 2)))
    diff_lower, diff_upper = x - 2**lower, 2**upper - x
    return (lower, diff_lower) if  diff_lower <= diff_upper else (upper, diff_upper)

def bfs(n):
    """ BFS traversal. Accumulate all solutions. Return the best one. """
    best_solution = n + 1

    # List of list of Nodes ( (number_of_pellets, steps_so_far, move_made, parent_node) )
    queue = [ Node(n, 0) ]

    while queue:
        # pop the first path from the queue
        node = queue.pop(0)

        # - Base case
        if is_power_of_two(node.num):
            sol = node.steps + closest_power_of_two(node.num)[0]
            if sol < best_solution:
                new_node = Node(1, sol)
                best_solution = sol
                #print("best = {}".format(best_solution))
        else:
            # - Is even
            if node.num % 2 == 0:
                steps = 0
                num = node.num
                while num % 2 == 0:
                    steps += 1
                    num //= 2
                if node.steps < best_solution:
                    new_node = Node(num, steps + node.steps)
                    queue.append(new_node)
                    #print("append {} {}".format(new_node.num, new_node.steps))
            else:
                # - Is odd - ends with 11 increment
                if node.num & 0b11 == 0b11 and node.steps < best_solution:
                    new_node = Node(node.num + 1, node.steps + 1)
                    queue.append(new_node)
                # - Is odd - ends with 01: decrement
                if node.steps < best_solution:
                    new_node = Node(node.num - 1, node.steps + 1)
                    queue.append(new_node)
                    #print("append {} {}".format(new_node.num, new_node.steps))
                    #print("append {} {}".format(new_node.num, new_node.steps))

    #print("Best solution={}".format(best_solution))
    return best_solution

'''

def iterate_solution(num):
    steps = 0
    while num > 1:
        # - Is even. Count trailing zeros (power of 2 divisor)
        if num == 3:
            steps += 2
            break
        if num & 1 == 0:
            # - Count trailing zeros (power of 2 divisor)
            num_trailing_zeros = (num ^ (num-1)).bit_length() - 1
            steps += num_trailing_zeros
            num = num >> num_trailing_zeros
        else:
            # - Is odd - ends with 11 increment
            if num & 0b11 == 0b11:
                inv_num = ~num
                num_trailing_ones = (inv_num ^ (inv_num-1)).bit_length() - 1
                #num += 1
                num = num >> num_trailing_ones
                num |= 1
                steps += 1 + num_trailing_ones
            # - Is odd - ends with 01: decrement
            else:
                num &= ~1
                #num -= 1
                steps += 1
    return steps

def iterate_solution_state_machine(num):
    STATE_ZERO = 0 # Previous digit 0
    STATE_ONE = 1 # Previous digit 1
    STATE_ONES = 2 # Previous 2 digits 11 (takes precedence over STATE_ONE)
    STATE_NONE = -1 # Starting state

    bit_length = (num).bit_length()
    current_state = STATE_NONE
    previous_state = STATE_NONE
    steps = 0
    mask = 0x1
    n = 0
    # Scan the number from the LSB (n=0)to the MSB (n=bit_length-1).  The MSB is always 1 since
    # we can ignore leading 0s.
    while n < bit_length:
        b = num & mask
        while not b and n<bit_length:
            # Case 1:  0 bit
            if previous_state == STATE_ONES:
                # We have a carry overflow on the increment. Treat as a 1 bit
                previous_state = STATE_ZERO
                b = 1
                break
            steps += 1
            mask = mask << 1
            n += 1
            b = num & mask
            previous_state = STATE_ZERO
        while b and n<bit_length:
            # Case 2:  1 bit
            if n == bit_length - 1:
                n += 1
                if previous_state == STATE_ONE or previous_state == STATE_ONES:
                    steps += 1
                break
            if previous_state == STATE_ONE:
                # Switch from state ONE to state ONES
                previous_state = STATE_ONES
            elif previous_state == STATE_ONES:
                # Stay in state ONES
                pass
            elif previous_state == STATE_ZERO or previous_state == STATE_NONE:
                # The divide by zero for the zero after the increment
                steps += 1
                previous_state = STATE_ONE
            steps += 1
            n += 1
            mask = mask << 1
            b = num & mask
    # There is a special case of "3". Using our algorithm
    return steps

def solution(n):
    ret = iterate_solution(int(n))
    #print("{} --> {}".format(n, ret))
    return str(ret)


assert('5' == solution('15'))
assert('2' == solution('4'))
assert('1285' == solution('809849032849023849038490384093820408309480932849328493820483209480932840343243243434324325434325254354354354353454356456576576585877564645645645645756756857546534634645656456456456456457657658578578585757664564645645645645756765765756765757656767389509384590834958430853485093485034890439850934'))
assert('1266' == solution('99999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999'))
assert('149' == solution(0xFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF))

assert('10' == solution(768))
