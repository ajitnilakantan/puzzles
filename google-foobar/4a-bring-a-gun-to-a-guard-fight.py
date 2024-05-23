# See https://stackoverflow.com/questions/41332696/puzzle-how-many-ways-can-you-hit-a-target-with-a-laser-beam-within-four-reflect
# Expand the room with "virtual" reflected shooter and guard positions. Expand to maximum distance limit.
# Get a short list of solutions. Eliminate solutions that hit corners or yourself before guard.

def gcd(a,b):
    a = abs(a)
    b = abs(b)
    while b > 0:
        a, b = b, a % b
    return 1 if a == 0 else a

class Solver:
    def __init__(self, dimensions, your_position, guard_position, distance, debug_mode = False):
        self.width, self.height = dimensions[0], dimensions[1]
        self.your_x, self.your_y = your_position[0], your_position[1]
        self.guard_x, self.guard_y = guard_position[0], guard_position[1]
        self.min_x, self.min_y, self.max_x, self.max_y = -distance-1, -distance-1, distance+self.width+1, distance+self.height+1
        self.distance = distance
        # distance squared, to avoid using square roots
        self.distance2 = distance * distance
        # Expanded room containing virtualized positions (use for visual debugging)
        self.vmat = None
        if debug_mode:
            self.vmat = {}
            for y in range(self.min_y, self.max_y):
                self.vmat[y] = {}
        # list of all real and virual guard and shooter positions
        # key=heading, value=distance
        self.all_guards = {}
        self.all_your = {}
        # list of all corners - if we hit a corner before a guard, it will bounce back and hit yourself.
        # key=heading, value=distance
        self.all_corners = {}
        # for debugging
        self.debug_mode = debug_mode

    """
    def print_grid(self):
        if not self.debug_mode:
            print("print_grid skipped")
            return
        for y in range(self.min_y, self.max_y):
            for x in range(self.min_x, self.max_x):
                print(f"{self.vmat[y][x]}", end='')
            print("")
    """

    # Add (heading,distance2) to the list. For duplicate headings, keep the lowest distance2
    def add_heading_distance_to_list(self, pos_x, pos_y, hashtable):
        heading = (pos_x - self.your_x, pos_y - self.your_y)
        gcd_heading = gcd(heading[0], heading[1])
        heading = heading[0] / gcd_heading, heading[1] / gcd_heading
        distance2 = ((pos_x - self.your_x)**2 + (pos_y - self.your_y)**2)
        val = hashtable.get(heading)
        if ((not val or val > distance2) and distance2 <= self.distance2):
            # Keep the smallest distance2 for a given heading
            hashtable[heading] = distance2

    # Created mirrorred "virtual" copies of the position.
    # Update "hashtable" (your or guard positions) with heading/distance2
    def get_and_mark_virtual_positions(self, pos_x, pos_y, mark_value, other_x, other_y, hashtable):
        self.add_heading_distance_to_list(pos_x, pos_y, hashtable)
        # TODO: Can combine up/down passes to a single loop

        x_axis_positions = [pos_x]
        # Pass1: go across left
        p_x, p_y = pos_x, pos_y
        for x in range(0, self.distance//self.width + 1):
            # Reflections alternate beween p_x from the left wall and p_x from the right wall
            p_x = p_x - 2*(pos_x) if (x%2==0) else p_x - 2*(self.width-pos_x)
            if (not self.min_x <= p_x < self.max_x) or (not self.min_y <= pos_y < self.max_y):
                continue
            if self.debug_mode:
                self.vmat[pos_y][p_x] = mark_value
            x_axis_positions.append(p_x)
            if (pos_y != other_y):
                # Don't add the trivial postions along the x-axis
                self.add_heading_distance_to_list(p_x, pos_y, hashtable)
        # Pass2: go across right
        p_x, p_y = pos_x, pos_y
        for x in range(0, self.distance//self.width + 1):
            # Reflections alternate beween p_x from the left wall and p_x from the right wall
            p_x = p_x + 2*(pos_x) if (x%2!=0) else p_x + 2*(self.width-pos_x)
            if (not self.min_x <= p_x < self.max_x) or (not self.min_y <= pos_y < self.max_y):
                continue
            if self.debug_mode:
                self.vmat[pos_y][p_x] = mark_value
            x_axis_positions.append(p_x)
            if (pos_y != other_y):
                # Don't add the trivial postions along the x-axis
                self.add_heading_distance_to_list(p_x, pos_y, hashtable)
        # Pass3: Go up
        for p_x in x_axis_positions:
            p_y = pos_y
            if (not self.min_x <= p_x < self.max_x) or (not self.min_y <= p_y < self.max_y):
                continue
            if (pos_x == other_x and p_x == pos_x):
                # Don't add the trivial postions along the y-axis
                continue
            for y in range(0, self.distance//self.height + 2):
                p_y = p_y - 2*(pos_y) if (y%2==0) else p_y - 2*(self.height-pos_y)
                if (not self.min_x <= p_x < self.max_x) or (not self.min_y <= p_y < self.max_y):
                    continue
                if self.debug_mode:
                    self.vmat[p_y][p_x] = mark_value
                self.add_heading_distance_to_list(p_x, p_y, hashtable)
        # Pass4: Go down
        for p_x in x_axis_positions:
            p_y = pos_y
            if (not self.min_x <= p_x < self.max_x) or (not self.min_y <= p_y < self.max_y):
                continue
            if (pos_x == other_x and p_x == pos_x):
                # Don't add the trivial postions along the y-axis
                continue
            for y in range(0, self.distance//self.height + 2):
                p_y = p_y + 2*(pos_y) if (y%2!=0) else p_y + 2*(self.height-pos_y)
                if (not self.min_x <= p_x < self.max_x) or (not self.min_y <= p_y < self.max_y):
                    continue
                if self.debug_mode:
                    self.vmat[p_y][p_x] = mark_value
                self.add_heading_distance_to_list(p_x, p_y, hashtable)

        return

    def create_virtual_grid(self):
        # Expand the room by "distance" to allow for "virtual" positions of shooter and guard

        if self.debug_mode:
            # For debugging: Fill room with '1' and virtual rooms with '0'
            for y in range(self.min_y, self.max_y):
                row = {x: 0 if (x<0 or x>self.width or y<0 or y>self.height) else 1 for x in range(self.min_x, self.max_x)}
                self.vmat[y] = row

        if self.debug_mode:
            # Mark shooter = 2 / guard = 3
            self.vmat[self.your_y][self.your_x] = 2
            self.vmat[self.guard_y][self.guard_x] = 3

        # List of all real and virtual guard and shooter positions
        # - Add "virtual" reflected shooters
        self.get_and_mark_virtual_positions(self.your_x, self.your_y, mark_value=2, other_x=self.guard_x, other_y=self.guard_y, hashtable=self.all_your)
        # - Add "virtual" reflected guards
        self.get_and_mark_virtual_positions(self.guard_x, self.guard_y, mark_value=3, other_x=self.your_x, other_y=self.your_y, hashtable=self.all_guards)

        # Mark walls as '.' (not needed, but nice for visualization using method print_grid)
        if self.debug_mode:
            # shift_[xy] used to calculate modulos of negative numbers
            shift_x, shift_y = (self.distance // self.width + 2) * self.width, (self.distance // self.height + 2) * self.height
            for y in range(self.min_y, self.max_y):
                for x in range(self.min_x, self.max_x):
                    if (x + shift_x) % self.width == 0 or (y + shift_y) % self.height == 0:
                        self.vmat[y][x] = '.'

        # - List of all corners - if we hit a corner before a guard, it will bounce back and hit yourself.
        self.all_corners = {}

        #   shift_[xy] used to calculate modulos of negative numbers
        shift_x, shift_y = (self.distance // self.width + 2) * self.width, (self.distance // self.height + 2) * self.height
        for y in range(-(self.distance // self.height + 1)*self.height, (self.distance // self.height + 2)*self.height+1, self.height):
            for x in range(-(self.distance // self.width + 1)*self.width, (self.distance // self.width + 2)*self.width+1, self.width):
                if (not self.min_x <= x < self.max_x) or (not self.min_y <= y < self.max_y):
                    continue
                if (x + shift_x) % self.width == 0 and (y + shift_y) % self.height == 0:
                    if self.debug_mode:
                        # Mark corners as 'x' for debugging
                        self.vmat[y][x] = 'x'
                    # self.all_corners.append((x, y))
                    self.add_heading_distance_to_list(x, y, self.all_corners)


    # Solve the problem:
    # - Loop through all corners
    #   - Save heading/distance2
    # - Loop through all shooter positions (virtual-only - ignore zero-length yourself)
    #   - Save heading/distance2
    # - Loop through all guard positions (real + virtual)
    #   - Save heading/distance2
    # - For each (heading, distance2) in guard positions
    #   - For each shooter + corner position with the same heading -- check if distance2 is less
    #   - If distance2 is less, ignore (you will hit yourself first), otherwise add to results list
    # Use reduced pairs of headings to represent heading
    def solve(self):
        # Loop through guards and accumulate answers
        answers = []

        for heading, distance2 in self.all_guards.items():
            val = self.all_corners.get(heading)
            if val and val < distance2:
                # Check if we hit a corner first
                continue
            val = self.all_your.get(heading)
            if val and val < distance2:
                # Check if we hit ourselves first
                continue
            answers.append((heading, distance2))

        """
        # Remove duplicate headings
        dedupe_answers = {}
        for a in answers:
            val = dedupe_answers.get(a[0])
            if (not val or val > a[1]) and a[1] < self.distance2:
                # Keep the lowest distance2 and ignore any > distance2
                dedupe_answers[a[0]] = a[1]

        answers = []
        for k, v in dedupe_answers.items():
            answers.append((k, v))
        """

        return answers 

##### End class Solver #####


def solution(dimensions, your_position, guard_position, distance):
    solver = Solver(dimensions, your_position, guard_position, distance)
    solver.create_virtual_grid()
    # solver.print_grid()
    directions = solver.solve()
    # print(f"directions(len={len(directions)}) = {directions}")
    return len(directions)


# Should be 7
#assert(7 == solution([3, 2], [1, 1], [2, 1], 4))

# Should be 9
#assert(9 == solution([300,275], [150,150], [185,100], 500))

# who knows???
#assert(499999 == solution([300000,2], [1,1], [2,1], 300000))
