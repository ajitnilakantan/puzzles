import itertools
import random
from copy import deepcopy
from pathlib import Path
from typing import cast, Generator, TypeVar
from dataclasses import dataclass

##
# High score detected: 216284267
#
# Letters: ONJINECAROIILFMGMSIOUSIOR
# ARIZONA CALIFORNIA COLORADO FLORIDA GEORGIA IDAHO ILLINOIS IOWA MAINE MARYLAND MICHIGAN MINNESOTA MISSOURI MONTANA NEVADA NEWJERSEY NEWMEXICO NEWYORK OHIO OREGON TEXAS UTAH
##
class Board:
    __slots__ = ("_data", "height", "width")

    def __init__(self, height: int, width: int, default_value: int) -> None:
        self.height = height
        self.width = width
        self._data = [default_value for _ in range(height * width)]

    def get(self, row: int, col: int) -> int:
        return self._data[row * self.width + col]

    def set(self, row: int, col: int, value: int) -> None:
        self._data[row * self.width + col] = value

    def get_bit(self, row: int, col: int, bit_number: int) -> int:
        return self._data[row * self.width + col] & (1 << bit_number)

    def set_bit(self, row: int, col: int, bit_number: int) -> None:
        self._data[row * self.width + col] |= 1 << bit_number


BLANK_SPACE = ord(".")


@dataclass
class Solution:
    # Current board, each item is a letter (to int)
    board: Board
    # Each item is a bitmask of states occupying that square
    state_placement: Board
    # flag indicating if that square has had its letter changed
    flipped: Board
    # bitflags of states placed so far
    placed_states: int
    # bitflags of states that have had a character flipped so far
    flipped_states: int

    def __init__(self, height: int, width: int) -> None:
        self.board = Board(height, width, BLANK_SPACE)
        self.state_placement = Board(height, width, 0)
        self.flipped = Board(height, width, 0)
        self.placed_states = 0
        self.flipped_states = 0

    def __repr__(self) -> str:
        return super().__repr__()

    def __str__(self) -> str:
        return f"Solution(placed={bin(self.placed_states)} board={''.join([chr(x) for x in self.board._data])})"


class Solver:
    states: list[str] = []
    populations: list[int] = []
    height = 5
    width = 5

    def load_data(self) -> None:
        data_file = Path(__file__).with_name("states.txt")
        # Read states and populations: tab separated values
        with open(data_file, "r", encoding="utf-8") as f:
            lines = [line.strip("\n").upper() for line in f.readlines()]
            self.states = [x.split("\t")[0].replace("\x20", "").strip() for x in lines]
            self.populations = [int(x.split("\t")[1].strip("\x20").strip()) for x in lines]
        assert len(self.states) == 51, "Should have 51 states"

    def remove_california(self) -> None:
        if "CALIFORNIA" in self.states:
            index = self.states.index("CALIFORNIA")
            del self.states[index]
            del self.populations[index]

    def get_states(self, solution: Solution) -> str:
        result = ""
        for s in range(len(self.states)):
            if solution.placed_states & (1 << s) != 0:
                result += self.states[s] + " "
        return result

    def score_solution(self, solution: Solution) -> int:
        score = 0
        for s in range(len(self.states)):
            if solution.placed_states & (1 << s) != 0:
                score += self.populations[s]
        return score

    def score_solutions(self, solutions: list[Solution]) -> int:
        sorted_solutions = sorted(solutions, key=lambda x: self.score_solution(x), reverse=True)
        return self.score_solution(sorted_solutions[0])

    def get_neighbours(self, solution: Solution, row: int, col: int) -> tuple[list[tuple[int, int]], list[tuple[int, int]]]:
        # Return a tuple of ([filled squares], [blank squares])
        filled:list[tuple[int,int]] = []
        blank:list[tuple[int,int]] = []
        if row == -1 or col == -1:
            # The whole board
            for r, c in itertools.product(range(self.height), range(self.width)):
                if solution.board.get(r,c) == BLANK_SPACE:
                    blank.append((r,c))
                else:
                    filled.append((r,c))
        else:
            # The neighbours of (row,col)
            for r, c in itertools.product(range(row - 1, row + 1 + 1), range(col - 1, col + 1 + 1)):
                if row == r and col == c or r < 0 or r >= self.height or c < 0 or c >= self.width:
                    continue
                elif solution.board.get(r,c) == BLANK_SPACE:
                    blank.append((r,c))
                else:
                    filled.append((r,c))
        return (filled, blank)

    def can_flip(self, solution: Solution, row: int, col: int) -> bool:
        # Can the specified square's letter be changed.  Can happen if none of
        # the states occupying that square have a flip already
        if solution.flipped.get(row, col) != 0:
            return False
        if solution.state_placement.get(row, col) & solution.flipped_states != 0:
            return False
        return True

    def place_word(self, row: int, col: int, state_index: int, solution: Solution, num_to_pick: int = 16) -> Generator[Solution, None, None]:
        # Do DFS, randomly choosing directions. Stop when desired num_to_pick is reached
        # Randomized directions
        direction: list[list[tuple[int, int]]] = []
        for _ in range(len(self.states[state_index])):
            tmpdirs = list(itertools.product(range(-1, 2), range(-1, 2)))  # all possibilites (±1,±1)
            random.shuffle(tmpdirs)
            tmpdirs = list(filter(lambda x: x != (0, 0), tmpdirs))  # exclude (0,0)
            direction.append(tmpdirs)

        stack: list[tuple[int, Solution, int,int]] = [(0, 0, deepcopy(solution), row, col)] # (depth, heading, solution, row, col)
        count = 0
        while stack:

            depth, heading, sol, r, c = stack.pop()
            sol = deepcopy(sol)
            ch = self.states[state_index][depth]
            result:Solution|None = None
            result2:Solution|None = None

            if r < 0 or c < 0 or r >= self.height or c >= self.width:
                ok = False
                continue
            elif sol.board.get(r, c) == BLANK_SPACE:
                ok = True
                sol.board.set(r, c, ord(ch))
                sol.state_placement.set_bit(r, c, state_index)
                depth += 1
                if depth >= len(self.states[state_index]):
                    result= sol
            elif sol.board.get(r, c) == ord(ch):
                ok = True
                # ssol.board.set(r, c, ord(ch))
                sol.state_placement.set_bit(r, c, state_index)
                depth += 1
                if depth >= len(self.states[state_index]):
                    result = sol
            else:
                ok = False
                sol2:Solution|None = deepcopy(sol)
                result2:Solution|None = None
                if self.can_flip(sol, r, c):
                    ok = True
                    assert sol.board.get(r, c) != BLANK_SPACE
                    assert sol.board.get(r, c) != ord(ch)
                    # Overwrite with our letter, and mark flipped
                    sol.board.set(r, c, ord(ch))
                    current_state_placement = sol.state_placement.get_bit(r, c, state_index)
                    sol.state_placement.set_bit(r, c, state_index)
                    sol.flipped.set(r, c, 1)
                    if current_state_placement == 0:
                        # Marked other states as flipped
                        bitflag = sol.state_placement.get(r, c) & ~(1 << state_index)
                    else:
                        # Mark other states as well as this one as flipped
                        # This can happen if the same state crosses over on itself
                        bitflag = sol.state_placement.get(r, c)
                    sol.flipped_states |= bitflag
                    depth += 1
                    if depth >= len(self.states[state_index]):
                        result = sol
                if sol2.flipped.get(r, c) == 0 and (sol2.flipped_states & (1 << state_index)) == 0:
                    ok = True
                    assert sol2.board.get(r, c) != BLANK_SPACE
                    assert sol2.board.get(r, c) != ord(ch)
                    # This hasn't been flipped yet and current state is not yet flipped
                    # sol2.board.set(r, c, ord(ch)) # Don't set. Use a flipped/replacement char
                    sol2.state_placement.set_bit(r, c, state_index)
                    sol2.flipped.set(r, c, 1)
                    # Marked this state as flipped
                    sol2.flipped_states |= 1 << state_index
                    depth += 1
                    if depth >= len(self.states[state_index]):
                        result2 =  sol2
                else:
                    sol2 = None
                if not ok:
                    continue

            if result != None:
                assert ok == True
                sol.placed_states |= 1 << state_index
                yield cast(Solution, result)
                count +=1
                if count >= num_to_pick:
                    return
            if result2 != None:
                assert ok == True
                assert sol2 != None
                sol2.placed_states |= 1 << state_index
                yield cast(Solution, result2)
                count +=1
                if count >= num_to_pick:
                    return
            if result == None and result2 == None:
                assert ok == True
                while heading < 8:
                    newr = r + direction[depth][heading][0]
                    newc = c + direction[depth][heading][1]
                    heading += 1
                    if newr >= 0 and newc >= 0 and newr < self.height and newc < self.width:
                        stack.append((depth, heading, sol, newr, newc))
                        break



    def print_max(self, solutions: list[Solution]) -> None:
        sorted_solutions = sorted(solutions, key=lambda x: self.score_solution(x), reverse=True)
        mid = len(sorted_solutions) // 2
        print(f"num_solutions={len(sorted_solutions)}")
        print(f" min={self.score_solution(sorted_solutions[-1]):011_}={sorted_solutions[-1]}")
        print(f"     {self.get_states(sorted_solutions[-1])}")
        print(f" mid={self.score_solution(sorted_solutions[mid]):011_}={sorted_solutions[mid]}")
        print(f"     {self.get_states(sorted_solutions[mid])}")
        print(f" max={self.score_solution(sorted_solutions[0]):011_}={sorted_solutions[0]}")
        print(f"     {self.get_states(sorted_solutions[0])}")

    def solve(self) -> None:
        # DFS search.  Randomize choices to not search entire tree
        num_to_pick = 2048

        solutions: list[Solution] = [Solution(self.height, self.width)]
        for state_index in range(len(self.states)):
            new_solutions: list[Solution] = []
            for sol in solutions:
                rowcol = list(itertools.product(range(self.height), range(self.width)))
                random.shuffle(rowcol)
                for row, col in rowcol:
                    for new_sol in self.place_word(row, col, state_index, sol):
                        new_solutions.append(new_sol)

                if len(new_solutions) > 2 * num_to_pick:
                    # Sort and pick most promising
                    shuffled_solutions = new_solutions[:]
                    random.shuffle(shuffled_solutions)
                    sorted_solutions = sorted(shuffled_solutions, key=lambda x: self.score_solution(x), reverse=True)
                    new_solutions = sorted_solutions[0:num_to_pick]
                    # Randomly pick rest
                    if len(sorted_solutions) > 2 * num_to_pick:
                        new_solutions.extend(random.sample(sorted_solutions[num_to_pick:], num_to_pick))
                    else:
                        new_solutions.extend(sorted_solutions[num_to_pick:])

                    break

            if new_solutions:
                solutions = new_solutions[:]
            if self.score_solutions(solutions) > 150_000_000:
                print(f"state_index={state_index} len={len(solutions)} ", end="")
                self.print_max(solutions)
        print("====")
        self.print_max(solutions)

    def solve2(self) -> None:
        # DFS search.  Randomize choices to not search entire tree
        num_to_pick = 16

        solutions: list[Solution] = [Solution(self.height, self.width)]
        for state_index in range(len(self.states)):
            new_solutions: list[Solution] = []
            for sol in solutions:
                filled, blank = self.get_neighbours(sol, -1, -1)

                for row, col in filled:
                    for new_sol in self.place_word(row, col, state_index, sol, num_to_pick):
                        new_solutions.append(new_sol)

                if len(blank) > 16:
                    blank = random.sample(blank, 16)
                count = 0
                for row, col in blank:
                    for new_sol in self.place_word(row, col, state_index, sol, num_to_pick):
                        new_solutions.append(new_sol)
                        count += 1
                        if count > 16*num_to_pick:
                            break
                    if count > 16*num_to_pick:
                        break

            if new_solutions:
                solutions = new_solutions[:]
            if self.score_solutions(solutions) > 150_000_000:
                print(f"state_index={state_index} len={len(solutions)} ", end="")
                self.print_max(solutions)
        print("====")
        self.print_max(solutions)


def try_random_states(nocal: bool = False) -> None:
    print(f"===try_random_states===")
    for _ in range(8):
        solver = Solver()
        solver.load_data()
        if nocal:
            solver.remove_california()
        temp = list(zip(solver.states, solver.populations))
        random.shuffle(temp)
        res1, res2 = zip(*temp)
        # res1 and res2 come out as tuples, and so must be converted to lists.
        solver.states, solver.populations = list(res1), list(res2)
        print(f"{solver.states[0:4]} = {solver.populations[0:4]}")
        solver.solve2()


def try_ordered_states(nocal: bool = False) -> None:
    print(f"===try_ordered_states nocal={nocal}===")
    solver = Solver()
    solver.load_data()
    if nocal:
        solver.remove_california()
    temp = list(zip(solver.states, solver.populations))
    temp = sorted(temp, key=lambda x: x[1], reverse=True)
    res1, res2 = zip(*temp)
    # res1 and res2 come out as tuples, and so must be converted to lists.
    solver.states, solver.populations = list(res1), list(res2)
    print(f"{solver.states[0:4]} = {solver.populations[0:4]}")
    solver.solve2()


def try_unordered_states(nocal: bool = False) -> None:
    print(f"===try_unordered_states nocal={nocal}===")
    solver = Solver()
    solver.load_data()
    if nocal:
        solver.remove_california()
    print(f"{solver.states[0:4]} = {solver.populations[0:4]}")
    solver.solve2()


def main() -> None:
    random.seed(10)
    try_unordered_states(False)
    try_unordered_states(True)
    try_ordered_states(False)
    try_ordered_states(True)
    try_random_states(False)
    try_random_states(True)


if __name__ == "__main__":
    main()
