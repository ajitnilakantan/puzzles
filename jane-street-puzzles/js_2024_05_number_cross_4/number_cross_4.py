from typing import (
    Callable,
    Any,
    TypeVar,
    Generator,
    Iterator,
    Sequence,
    Generic,
    Type,
)
from typing_extensions import ParamSpec
import itertools
from copy import deepcopy
import math
from math import isqrt
from dataclasses import dataclass
from timeit import default_timer as timer

# See: https://mypy.readthedocs.io/en/stable/generics.html#declaring-decorators
Pfunc = ParamSpec("Pfunc")
Tfunc = TypeVar("Tfunc")


def time_method(func: Callable[Pfunc, Tfunc]) -> Callable[Pfunc, Tfunc]:
    # Decorate functions to time with @time_method
    def wrapper(*args: Pfunc.args, **kwargs: Pfunc.kwargs) -> Tfunc:
        t1 = timer()
        result = func(*args, **kwargs)
        t2 = timer()
        print(f"{func.__name__}() executed in {(t2-t1):.6f}s")
        return result

    return wrapper


T = TypeVar("T")
TP = TypeVar("TP", bound="Grid[Any]")


class Grid(list[list[T]], Generic[T]):
    # E.g.:
    # g = Grid.fromsize(5, 4, int); g= Grid.fromsize(3, 2, float); g = Grid.fromsize(3, 2, list[str])
    # g = Grid.fromdata([[1,2,3],[4,5,6],[7,8,9]])
    def __init__(self, initdata: list[list[T]]) -> None:
        super(Grid, self).__init__(initdata)

    @classmethod
    def fromsize(
        cls: Type[TP],
        height: int,
        width: int,
        member_type: Any,  # noqa: ANN401
    ) -> TP:  # noqa: ANN401
        return cls([[member_type() for col in range(width)] for row in range(height)])

    @classmethod
    def fromdata(cls: Type[TP], data: list[list[T]]) -> TP:
        return cls(data)


@dataclass
class SolutionData:
    def __init__(self, height: int, width: int, line_patterns: list[list[int]]) -> None:
        self.height = height
        self.width = width
        self.line_patterns = deepcopy(line_patterns)

        # Map normalized tuple of digits e.g. (1,2,2) to list of possible values e.g. [100..455..999]
        # self.subpatterns: dict[tuple[int, ...], list[int]] = {}

        # For each row, for each column -> have a list of possible normalized tuples
        # that are valid.  E.g. for the example grid, the fifth row is
        # [1, 2, 2, 3, 3],  so the possible (normalized) subpatterns are, per position:
        # 0: (1,2),(1,2,2),(1,2,2,3),(1,2,2,3,3) -> (1, 2),(1,2,2),(1,2,2,3),(1,2,2,3,3)
        # 1: (2,2),(2,2,3),(2,2,3,3) -> (1,1),(1,1,2),(1,1,2,2)
        # 2: (2,3),(2,3,3) -> (1,2),(1,2,2)
        # 3: (3,3) -> (1,1)
        # 4: none possible
        # Indexed by row, and then by column
        self.row_col_tuples: Grid[list[tuple[int, ...]]] = Grid.fromsize(height, width, list[tuple[int, ...]])

        # Map of normalized digits to list of possible n-digit values
        # self.values: DefaultDict[list[int]] = defaultdict(lambda: [])
        for row in range(height):
            self.__compute_row_col_tuples(row)

        # For each computed value (e.g. fibonacci number), if it matches one of the tuples
        # in a given row, then add it to the dictionary mapping tuple->value
        self.row_valid_values: Grid[list[int]] = Grid.fromsize(height, width, list[int])

    @staticmethod
    def __normalize_pattern(pattern: tuple[int, ...]) -> tuple[int, tuple[int, ...]]:
        # Reduce and normalize the tuple e.g. (5, 9, 9) and (1, 3, 3) will both map to (1, 2, 2)
        # Return tuple(number-of-unique-digits, normalized-tuple)
        new_map: dict[int, int] = {}
        index = 1
        for d in pattern:
            if d not in new_map:
                new_map[d] = index
                index += 1
        unique_digits = len(new_map)
        new_pattern = tuple([new_map[x] for x in pattern])
        return (unique_digits, new_pattern)

    def __compute_row_col_tuples(self, row: int) -> None:
        # Subdivide each row of the grid into possible subpatterns
        # for possible numbers.  E.g. for the example grid, the fifth row is
        # [1, 2, 2, 3, 3],  so the possible (normalized) subpatterns are, per position:
        # 0: (1,2),(1,2,2),(1,2,2,3),(1,2,2,3,3) -> (1, 2), (1,2,2), (1,2,2,3), (1,2,2,3,3)
        # 1: (2,2),(2,2,3),(2,2,3,3) -> (1,1),(1,1,2)(1,1,2,2)
        # 2: (2,3),(2,3,3) -> (1,2).(1,2,2)
        # 3: (3,3) -> (1,1)
        # 4: none possible
        line_pattern = self.line_patterns[row]
        for col in range(0, self.width):
            # Create all the tuples of size 2..(width-col)
            patterns = [tuple(itertools.islice(line_pattern, col, col + w)) for w in range(2, self.width - col + 1)]
            for pattern in patterns:
                ndigits, p = SolutionData.__normalize_pattern(pattern)
                if p not in self.row_col_tuples[row][col]:
                    self.row_col_tuples[row][col].append(p)

    def get_possible_values(self, row: int, col: int = -1) -> Generator[int, None, None]:
        # Get all the numbers matching the subpatterns in a row.
        # e.g. (1,2,2) -> [100, 111,...500,511,522...988,999]
        # i.e. first digit 1-9 (leading digit is never 0), second = third digit and between 0-9
        all_tups: set[tuple[int, ...]] = set()
        for c in range(self.width) if col == -1 else range(col, col + 1):
            for tup in self.row_col_tuples[row][c]:
                all_tups.add(tup)
        for tup in all_tups:
            unique_digits = len(set(tup))
            min = 10 ** (unique_digits - 1)
            max = 10 ** (unique_digits)
            for i in range(min, max):
                # Get all numbers fitting the pattern
                n = 0
                for d in tup:
                    n = 10 * n + int(str(i)[d - 1])
                    yield n

    @staticmethod
    def __match_tuples(references: list[tuple[int, ...]], value: int) -> bool:
        # Compare two normalized tuples.
        # E.g. if reference = (1,2,3,3) then the following all
        # return True: (1,1,1,1,) (1,1,2,2) (1,2,2,2) (1,2,1,1), (1,2,3,3) etc.
        # I.e. any pattern with the last two the same.
        for reference in references:
            vals: dict[int, int] = {}
            tup = tuple(int(ch) for ch in str(value))
            if len(reference) != len(tup):
                continue
            ok = True
            for ref_digit, digit in zip(reference, tup):
                if ref_digit in vals and vals[ref_digit] != digit:
                    ok = False
                    break
                vals[ref_digit] = digit
            if ok:
                return True
        return False

    def match_value(self, row: int, value: int) -> bool:
        # Check to see if "value" matches any valid sequence for the specified row
        result: bool = False
        for col in range(0, self.width):
            if SolutionData.__match_tuples(self.row_col_tuples[row][col], value):
                if value not in self.row_valid_values[row][col]:
                    self.row_valid_values[row][col].append(value)
                result = True
        return result

    def generate_sequence(self, row: int, max_len: int, current: str) -> Generator[str, None, None]:
        # Generate all possible numberings for a row in the grid.
        # E.g. for the example, the first row:
        # E.g. [[".343."], ["13.55"], ["13775"], [".3375"], ["7337."]]
        if len(current) == max_len - 1:
            if current[-2] == ".":
                return
            else:
                ret = current + "."
                if validate_line(row, max_len, ret, self.line_patterns):
                    yield ret
                else:
                    return
        elif len(current) == max_len:
            assert current[-2] != ".", "Second to last can never be '.'"
            if validate_line(row, max_len, current, self.line_patterns):
                yield current
            else:
                return
        elif len(current) > max_len:
            return
        elif len(current) == 0:
            for val in self.row_valid_values[row][len(current) + 1]:
                v = str(val)
                if len(v) <= max_len - 1:
                    yield from self.generate_sequence(row, max_len, "." + v)
            for val in self.row_valid_values[row][len(current)]:
                v = str(val)
                if len(v) <= max_len:
                    yield from self.generate_sequence(row, max_len, v)
        elif len(current) < max_len - 1:
            for val in self.row_valid_values[row][len(current) + 1]:
                v = str(val)
                new = current + "." + v
                yield from self.generate_sequence(row, max_len, new)


# Check a single line - make sure each island has a different numbering
def validate_line(row: int, width: int, solution_line: str, line_patterns: list[list[int]]) -> bool:
    # Validate each row - adjacent islands have different number assignments
    for col in range(1, width):
        prev_val, val = line_patterns[row][col - 1], line_patterns[row][col]
        if prev_val != val and prev_val != -1 and val != -1 and solution_line[col - 1] == solution_line[col]:
            return False
    return True


# Return boolean, and in case of failure line of failure (0..height-1)
def validate_solution(
    solution: Sequence[str],
    line_patterns: list[list[int]],
    bad_pairs: set[tuple[str, str]],
) -> tuple[bool, int]:
    height = len(line_patterns)
    width = len(line_patterns[0])

    # Validate each row - adjacent islands have different number assignments
    for row, col in itertools.product(range(height), range(1, width)):
        prev_val, val = line_patterns[row][col - 1], line_patterns[row][col]
        if prev_val != val and prev_val != -1 and val != -1 and solution[row][col - 1] == solution[row][col]:
            print(f"SHould not be here")
            return (False, row)

    for row, col in itertools.product(range(1, height), range(width)):
        # Validate no two shaded cells are in the same column
        if (solution[row], solution[row - 1]) in bad_pairs:
            return (False, row)
        if solution[row][col] == "." and solution[row - 1][col] == ".":
            bad_pairs.add((solution[row], solution[row - 1]))
            return (False, row)
        # Validate each column - adjacent islands have different number assignments
        prev_val, val = line_patterns[row - 1][col], line_patterns[row][col]
        if prev_val != val and prev_val != -1 and val != -1 and solution[row - 1][col] == solution[row][col]:
            bad_pairs.add((solution[row], solution[row - 1]))
            return (False, row)

    # Clone, because we modify it
    line_patterns = deepcopy(line_patterns)

    # Shaded cells can split the "islands"
    resplit_islands(line_patterns, solution)

    # Validate all numbers map to same value
    mapping: dict[int, str] = {}
    for row, (s, lp) in enumerate(zip(solution, line_patterns)):
        for i in range(len(s)):
            if s[i] == ".":
                continue
            if lp[i] not in mapping:
                mapping[lp[i]] = s[i]
                continue
            if mapping[lp[i]] != s[i]:
                return (False, row)

    return (True, height)


def flood_fill(grid: list[list[int]], row: int, col: int, new_value: int) -> None:
    height = len(grid)
    width = len(grid[0])

    old_value = grid[row][col]
    grid[row][col] = new_value
    if row > 0 and grid[row - 1][col] == old_value:
        flood_fill(grid, row - 1, col, new_value)
    if row < height - 1 and grid[row + 1][col] == old_value:
        flood_fill(grid, row + 1, col, new_value)
    if col > 0 and grid[row][col - 1] == old_value:
        flood_fill(grid, row, col - 1, new_value)
    if col < width - 1 and grid[row][col + 1] == old_value:
        flood_fill(grid, row, col + 1, new_value)


def resplit_islands(grid: list[list[int]], solution: Sequence[str]) -> None:
    # Shaded cells can split the "islands"
    # Mark the shaded cell with a special value
    height = len(grid)
    width = len(grid[0])

    marker = height * width
    # Add an offset so islands get different numbers
    offset = marker

    for row, col in itertools.product(range(height), range(width)):
        if solution[row][col] != ".":
            continue
        val = grid[row][col]
        same_color_count = 0
        if row > 0 and grid[row - 1][col] == val:
            same_color_count += 1
        if row < height - 1 and grid[row + 1][col] == val:
            same_color_count += 1
        if col > 0 and grid[row][col - 1] == val:
            same_color_count += 1
        if col < width - 1 and grid[row][col + 1] == val:
            same_color_count += 1
        if same_color_count >= 2:
            grid[row][col] = -1
            if row > 0 and grid[row - 1][col] == val:
                offset += 1
                flood_fill(grid, row - 1, col, offset)
            if row < height - 1 and grid[row + 1][col] == val:
                offset += 1
                flood_fill(grid, row + 1, col, offset)
            if col > 0 and grid[row][col - 1] == val:
                offset += 1
                flood_fill(grid, row, col - 1, offset)
            if col < width - 1 and grid[row][col + 1] == val:
                offset += 1
                flood_fill(grid, row, col + 1, offset)

    # Mark the shaded cells
    for row in range(height):
        for col in range(width):
            if solution[row][col] == ".":
                grid[row][col] = -1


# See: https://stackoverflow.com/questions/42197572/variable-levels-of-nested-loops-with-inner-breaks-using-itertools
# See: https://stackoverflow.com/questions/4190966/python-itertools-skipping-ahead
class SkipUp(Exception):
    def __init__(self, numSkips: int) -> None:
        self.numSkips = numSkips
        super(SkipUp, self).__init__(numSkips)


_T_co = TypeVar("_T_co", covariant=True)


def breakableProduct(*sets: list[_T_co]) -> Generator[list[_T_co] | None, None, None]:
    if not sets:
        yield []
        return
    first, rest = sets[0], sets[1:]
    for item in first:
        subProd = breakableProduct(*rest)
        for items in subProd:
            try:
                yield [item] + items
            except SkipUp as e:
                if e.numSkips == 0:
                    yield None
                    # yield []  # None
                    break
                else:
                    e.numSkips -= 1
                    yield subProd.throw(e)


@time_method
def get_fibonacci(min: int, max: int, row: int, sd: SolutionData) -> None:
    print(">get_fibonacci")
    num1 = 0
    num2 = 1
    next_number = num2

    while next_number < max:
        if next_number >= min:
            sd.match_value(row, next_number)
        num1, num2 = num2, next_number
        next_number = num1 + num2
    print("<get_fibonacci")


@time_method
def get_multiple_of_n(min: int, max: int, n: int, row: int, sd: SolutionData) -> None:
    # The naive method of getting all multiples between min and max
    # is too inefficient.
    # Instead look at all the possible sub-patterns of size between
    # 2 and width digits inclusive
    print(f">get_multiple_of_n{n}")
    for v in sd.get_possible_values(row):
        if v >= min and v < max and v % n == 0:
            sd.match_value(row, v)
    print(f"<get_multiple_of_n{n}")


def is_nth_power(value: int, n: int) -> bool:
    # Check if value is the nth power of some number
    guess = value ** (1.0 / n)
    iguess = int(guess)
    if iguess**n == value:
        return True
    iguess = iguess + 1
    if iguess**n == value:
        return True
    return False


@time_method
def get_nth_power(min: int, max: int, power: int, row: int, sd: SolutionData) -> None:
    print(f">get_nth_power{power}")
    for v in sd.get_possible_values(row):
        if v >= min and v < max and is_nth_power(v, power):
            sd.match_value(row, v)
    """
    start = 0
    next = start**power
    while next < max:
        start += 1
        if next >= min:
            sd.match_value(row, next)
        next = start**power
    """
    print(f"<get_nth_power{power}")


@time_method
def get_powers_of_n(min: int, max: int, n: int, row: int, sd: SolutionData) -> None:
    print(f">get_powers_of_n{n}")
    start = 0
    while True:
        val = n**start
        start += 1
        if val < min:
            continue
        if val >= max:
            break
        sd.match_value(row, val)
    print(f"<get_powers_of_n{n}")


def is_palindrome(s: str) -> bool:
    return s == s[::-1]


def getPalindrome() -> Generator[int, None, None]:
    """
    Generator for palindromes.
    Generates palindromes, starting with 0.
    A palindrome is a number which reads the same in both directions.
    See: https://stackoverflow.com/questions/17435448/palindrome-generator
    """
    from itertools import count

    yield 0
    for digits in count(1):
        first = 10 ** ((digits - 1) // 2)
        for s in map(str, range(first, 10 * first)):
            yield int(s + s[-(digits % 2) - 1 :: -1])


def allPalindromes(minP: int, maxP: int) -> list[int]:
    """Get a sorted list of all palindromes in interval [minP, maxP]."""
    palindromGenerator = getPalindrome()
    palindromeList = []
    for palindrome in palindromGenerator:
        if palindrome >= maxP:
            break
        if palindrome < minP:
            continue
        palindromeList.append(palindrome)
    return palindromeList


@time_method
def get_palindromes(min: int, max: int, row: int, sd: SolutionData) -> None:
    print(f">get_palindromes")
    for v in sd.get_possible_values(row):
        if v >= min and v < max and is_palindrome(str(v)):
            sd.match_value(row, v)
    """
    palindromes = allPalindromes(min, max)
    for p in palindromes:
        sd.match_value(row, p)
    """
    print(f"<get_palindromes")


@time_method
def get_palindromes_plus_n(min: int, max: int, n: int, row: int, sd: SolutionData) -> None:
    # ret = allPalindromes(min, max - 1)
    # ret = [str(x + n) for x in ret]
    # return ret
    print(f">get_palindromes_plus_n{n}")
    """
    palindromes = allPalindromes(min - n, max - n)
    for p in palindromes:
        sd.match_value(row, p + n)
    """
    for v in sd.get_possible_values(row):
        if v >= min and v < max and is_palindrome(str(v - n)):
            sd.match_value(row, v)
    print(f"<get_palindromes_plus_n{n}")


@time_method
def get_palindromes_multiple_of_n(min: int, max: int, n: int, row: int, sd: SolutionData) -> None:
    print(f">get_palindromes_multiple_of_n{n}")
    """
    palindromes = allPalindromes(min, max)
    for p in palindromes:
        if p % n == 0:
            sd.match_value(row, p)
    """
    for v in sd.get_possible_values(row):
        if v >= min and v < max and is_palindrome(str(v)) and v % n == 0:
            sd.match_value(row, v)
    print(f"<get_palindromes_multiple_of_n{n}")


def gen_primes() -> Iterator[int]:
    """Generate an infinite sequence of prime numbers."""
    # Maps composites to primes witnessing their compositeness.
    # This is memory efficient, as the sieve is not "run forward"
    # indefinitely, but only as long as required by the current
    # number being tested.
    #
    D = {}

    # The running integer that's checked for primeness
    q = 2

    while True:
        if q not in D:
            # q is a new prime.
            # Yield it and mark its first multiple that isn't
            # already marked in previous iterations
            #
            yield q
            D[q * q] = [q]
        else:
            # q is composite. D[q] is the list of primes that
            # divide it. Since we've reached q, we no longer
            # need it in the map, but we'll mark the next
            # multiples of its witnesses to prepare for larger
            # numbers
            #
            for p in D[q]:
                D.setdefault(p + q, []).append(p)
            del D[q]

        q += 1


@time_method
def get_primes_to_power_primes(min: int, max: int, row: int, sd: SolutionData) -> None:
    print(f">get_primes_to_power_primes")
    max_prime = isqrt(max)
    primes = []
    for p in gen_primes():
        if p > max_prime:
            break
        primes.append(p)
    powers = primes.copy()
    all_prime_powers: list[int] = []
    for base in primes:
        for power in powers:
            val = base**power
            if val < min:
                continue
            if val >= max:
                break
            all_prime_powers.append(val)

    for v in all_prime_powers:
        sd.match_value(row, v)
    print(f"<get_primes_to_power_primes")


def sum_digits(n: int) -> int:
    r = 0
    while n:
        r, n = r + n % 10, n // 10
    return r


@time_method
def get_sum_of_digits_is(min: int, max: int, width: int, n: int, row: int, sd: SolutionData) -> None:
    print(f">get_sum_of_digits_is{n}")
    for w in range(2, width + 1):
        for val in find_numbers_summing_to_value(w, n):
            if val >= min and val < max:
                sd.match_value(row, val)
    print(f"<get_sum_of_digits_is{n}")


# See: https://stackoverflow.com/questions/61287896/how-to-find-all-n-digit-numbers-with-a-given-digitsum
def find_numbers_summing_to_value(
    num_digits: int,
    desired_sum: int,
    max_iterations: int = 100000,
    max_in_digit: int = 9,
) -> Iterator[int]:
    a = [0 for i in range(num_digits)]
    a[0] = desired_sum - 1
    a[num_digits - 1] = 1
    all_numbers_found = False
    while not all_numbers_found and max_iterations > 0:
        # step 1: while a[0] is too large: redistribute to the left
        i = 0
        while a[i] > max_in_digit:
            if i == num_digits - 1:
                all_numbers_found = True
                break
            a[i + 1] += a[i] - max_in_digit
            a[i] = max_in_digit
            i += 1
        if all_numbers_found:
            break

        num = sum(10**i * a[i] for i, n in enumerate(a))
        # print(f"{num:}")  # print(a[::-1])
        yield num

        # step 2:  add one to the penultimate group, while group already full: set to 0 and increment the
        #   group left of it;
        #   while the surplus is too large (because a[0] is too small) repeat the incrementing
        i0 = 1
        surplus = 0
        while True:  # needs to be executed at least once, and repeated if the surplus became too large
            i = i0
            while True:  # increment a[i] by 1, which can carry to the left
                if i == len(a):
                    all_numbers_found = True
                    break
                else:
                    if a[i] == max_in_digit:
                        a[i] = 0
                        surplus -= max_in_digit
                        i += 1
                    else:
                        a[i] += 1
                        surplus += 1
                        break
            if all_numbers_found:
                break
            if a[0] >= surplus:
                break
            else:
                surplus -= a[i0]
                a[i0] = 0
                i0 += 1

        # step 3: a[0] should absorb the surplus created in step 1, although a[0] can get out of bounds
        a[0] -= surplus
        surplus = 0
        max_iterations -= 1

    assert all_numbers_found, "all_numbers_found not true"
    assert max_iterations > 0, "max_iterations exceeded"


# Return product of decimal digits in 'n'.
def prod_digits(n: int) -> int:
    r = 1
    while n:
        r, n = r * n % 10, n // 10
    return r


def is_product_ending_one(n: int) -> bool:
    r = 1
    while n:
        d = n % 10
        if d != 1 and d != 3 and d != 7 and d != 9:
            return False
        r, n = r * d, n // 10
    return r % 10 == 1


@time_method
def get_product_digits_ends_in_one(min: int, max: int, width: int, row: int, sd: SolutionData) -> None:
    print(f">get_product_digits_ends_in_one")
    # Only possible digits are 1, 3, 7, 9
    for v in sd.get_possible_values(row):
        if v >= min and v < max and is_product_ending_one(v):
            sd.match_value(row, v)
    """
    for w in range(2, width + 1):
        digits = []
        for _ in range(w):
            digits.append([1, 3, 7, 9])
        for x in itertools.product(*digits):
            prod = 1
            for d in x:
                prod *= d
            if prod % 10 == 1:
                val = functools.reduce(lambda sub, ele: sub * 10 + ele, x)
                sd.match_value(row, val)
    """
    print(f"<get_product_digits_ends_in_one")


def get_solution_sum(solution: list[str]) -> int:
    sum = 0
    if solution is None:
        return sum
    for s in solution:
        for val in list(filter(None, s.split("."))):
            sum += int(val)
    return sum


@time_method
def run_solver(line_patterns: list[list[int]], inputs: list[list[str]]) -> list[str]:
    print(f"inputs = {[len(x) for x in inputs]} = {math.prod([len(x) for x in inputs])} = {[len(set(x)) for x in inputs]}")
    bad_pairs: set[tuple[str, str]] = set()
    total = math.prod([len(x) for x in inputs])
    counter = 0
    prod = breakableProduct(*inputs)
    for s in prod:
        if counter % 1000000 == 0:
            pp = 1
            for i in range(len(s)):
                pp *= inputs[i].index(s[i]) + 1
            print(f"{counter} / {pp} = {100.*pp/total:.2f}%: ", end="")
            for i in range(len(s)):
                print(f"{inputs[i].index(s[i])}/{len(inputs[i])} ", end="")
            print("")
        counter += 1
        is_valid, fail_row = validate_solution(s, line_patterns, bad_pairs)
        if is_valid:
            print("Valid solution=")
            print(s)
            return s
        else:
            prod.throw(SkipUp(fail_row))
            continue
    print(f"Done solver")
    return []


def test_example_provided() -> int:
    line_patterns = [
        [1, 2, 3, 4, 5],
        [1, 2, 3, 5, 5],
        [1, 2, 3, 3, 5],
        [1, 2, 2, 3, 5],
        [1, 2, 2, 3, 3],
    ]

    height = len(line_patterns)
    width = len(line_patterns[0])
    min = 10
    max = 10**width

    solution_data = SolutionData(height, width, line_patterns)

    get_powers_of_n(min, max, 7, row=0, sd=solution_data)
    get_fibonacci(min, max, row=1, sd=solution_data)
    get_multiple_of_n(min, max, 5, row=2, sd=solution_data)
    get_nth_power(min, max, 3, row=3, sd=solution_data)
    get_palindromes(min, max, row=4, sd=solution_data)

    inputs = [
        list(solution_data.generate_sequence(0, width, "")),
        list(solution_data.generate_sequence(1, width, "")),
        list(solution_data.generate_sequence(2, width, "")),
        list(solution_data.generate_sequence(3, width, "")),
        list(solution_data.generate_sequence(4, width, "")),
    ]

    # pprint(vars(solution_data))
    solution = run_solver(line_patterns, inputs)
    print(f"solution={solution}")
    solution_sum = get_solution_sum(solution)
    print(f"Solution_sum = {solution_sum}")
    assert solution_sum == 24898, "Example answer should be 24898"
    print(f"==========\n\n")
    return solution_sum


def main_problem() -> int:
    line_patterns = [
        [1, 1, 1, 2, 2, 2, 4, 4, 5, 5, 5],
        [1, 3, 3, 3, 2, 2, 4, 5, 5, 5, 7],
        [1, 3, 3, 2, 2, 2, 4, 5, 5, 5, 7],
        [1, 3, 3, 2, 2, 6, 6, 5, 7, 7, 7],
        [1, 3, 2, 2, 5, 5, 6, 5, 7, 9, 7],
        [1, 5, 5, 5, 5, 5, 5, 5, 9, 9, 8],
        [11, 5, 5, 5, 5, 13, 13, 5, 9, 9, 9],
        [11, 11, 12, 5, 12, 13, 13, 5, 5, 9, 5],
        [11, 11, 12, 12, 12, 13, 13, 5, 5, 5, 5],
        [11, 12, 12, 11, 11, 11, 13, 5, 5, 5, 10],
        [11, 11, 11, 11, 11, 13, 13, 13, 5, 5, 10],
    ]

    height = len(line_patterns)
    width = len(line_patterns[0])
    min = 10
    max = 10**width
    sd = SolutionData(height, width, line_patterns)

    get_nth_power(min, max, 2, row=0, sd=sd)
    get_palindromes_plus_n(min, max, 1, row=1, sd=sd)
    get_primes_to_power_primes(min, max, row=2, sd=sd)
    get_sum_of_digits_is(min, max, width, 7, row=3, sd=sd)
    get_fibonacci(min, max, row=4, sd=sd)
    get_nth_power(min, max, 2, row=5, sd=sd)
    get_multiple_of_n(min, max, 37, row=6, sd=sd)
    get_palindromes_multiple_of_n(min, max, 23, row=7, sd=sd)
    get_product_digits_ends_in_one(min, max, width, row=8, sd=sd)
    get_multiple_of_n(min, max, 88, row=9, sd=sd)
    get_palindromes_plus_n(min, max, -1, row=10, sd=sd)

    inputs = [
        list(sd.generate_sequence(0, width, "")),
        list(sd.generate_sequence(1, width, "")),
        list(sd.generate_sequence(2, width, "")),
        list(sd.generate_sequence(3, width, "")),
        list(sd.generate_sequence(4, width, "")),
        list(sd.generate_sequence(5, width, "")),
        list(sd.generate_sequence(6, width, "")),
        list(sd.generate_sequence(7, width, "")),
        list(sd.generate_sequence(8, width, "")),
        list(sd.generate_sequence(9, width, "")),
        list(sd.generate_sequence(10, width, "")),
    ]

    solution = run_solver(line_patterns, inputs)
    solution_sum = get_solution_sum(solution)
    print(f"Solution_sum = {solution_sum}")
    assert solution_sum == 88243711283, f"Answer should be 88243711283, got {solution_sum}"
    # Valid solution= ['11122233444', '13332.3444.', '1331.734449', '133.100411.', '13.144.4181', '1444.444889', '74444.74888', '7714177.989', '77111779999', '.1144.79992', '444443.3992'] # noqa E501
    # run_solver() executed in 14628.041546s (4hrs)
    # Solution_sum = 88243711283
    return solution_sum


def test0() -> None:
    pass


def main() -> None:
    # test0()
    test_example_provided()
    main_problem()


if __name__ == "__main__":
    main()
