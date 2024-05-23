# This Python file uses the following encoding: utf-8
from __future__ import generators
import math


"""
Solving this problem has 2 parts:
Part 1:  Checking if a pair of players with b1 and b2 bananas end up in a tie, or go into an infinite loop
 - The naive implementation is to keep looping by doubling the lower of b1,b2 and subtracting the lower from the higher
 - Keep a history of visited pairs.
 - Stop if we see a repeat in the history (looping) or if b1 == b2  (tie)
 - Small optimization:
   - Can divide b1, b2 by gcd(b1, b2)
   - the greater (e.g. b2)  is more than 3 times the smaller (e.g. b1) then you can calculate the the repeated amounts
     to add in 1 step:
            # If the numbers are far apart, can calculate the difference: based on sum(k=0..n) 2**k == 2**(k+1)-1
            # After n iterations b1 increases by (2**n - 1)*b1  and  b2
            # decreases by this amount.
            n = math.ceil(math.log((b2 - b1) / 2 / b1, 2))
            b2 = b2 - (2**n - 1) * b1
            b1 = b1 + (2**n - 1) * b1
  - can "memoize" previous solutions.
  ===> However, when looking at the patterns of solutions, you see that most loop, and very few tie.
  - For a tie, the average of b1, b2 (i.e. (b1+b2)/2) must be a whole number as equal amounts are added/subtracted from b1/b2
    This means you can have a tie only if (b1+b2) is even (i.e. both even, or both odd)
  - Also looking at the pattern, there is an interesting pattern: For ties the average divides evenly by the gcd and is always
    a power of two (i.e.   (b1 + b2) / 2 / gcd(b1, b2)   does not have a remainder and is always a power of 2)
  - I don't know why this is true, but it gives a 1-liner to detect if there is a tie:
    is a tie =>   (b1+b2) is even  AND   (b1 + b2) / 2 / gcd(b1, b2)  is a power of 2

Part 2:  Finding the best matching of guards to minimize the number of ties (maximize number of infinite loops)
  - The naive approach is to take all possible pairing   and  enumerate though the pairing-sets.
    This works, but is slow. (Seems to get exponentially slower).  Can add speedups like stopping the search early, but is still slow.
  - A better approach is to use the Blossom algorithm (https://en.wikipedia.org/wiki/Blossom_algorithm) to do a 
    maximal cardinality match.  We use the implementation in
    https://code.activestate.com/recipes/221251-maximum-cardinality-matching-in-general-graphs/
"""

################################################################################
###  BEGIN EDMONDS' BLOSSOM-CONTRACTION ALGORITHM ##############################
# Use the solver from: https://code.activestate.com/recipes/221251-maximum-cardinality-matching-in-general-graphs/

# Find maximum cardinality matching in general undirected graph
# D. Eppstein, UC Irvine, 6 Sep 2003

#from __future__ import generators

if 'True' not in globals():
    globals()['True'] = not None
    globals()['False'] = not True

class unionFind:
    '''Union Find data structure. Modified from Josiah Carlson's code,
http://aspn.activestate.com/ASPN/Cookbook/Python/Recipe/215912
to allow arbitrarily many arguments in unions, use [] syntax for finds,
and eliminate unnecessary code.'''

    def __init__(self):
        self.weights = {}
        self.parents = {}

    def __getitem__(self, object):
        '''Find the root of the set that an object is in.
Object must be hashable; previously unknown objects become new singleton sets.'''

        # check for previously unknown object
        if object not in self.parents:
            self.parents[object] = object
            self.weights[object] = 1
            return object
        
        # find path of objects leading to the root
        path = [object]
        root = self.parents[object]
        while root != path[-1]:
            path.append(root)
            root = self.parents[root]
        
        # compress the path and return
        for ancestor in path:
            self.parents[ancestor] = root
        return root

    def union(self, *objects):
        '''Find the sets containing the given objects and merge them all.'''
        roots = [self[x] for x in objects]
        heaviest = max([(self.weights[r],r) for r in roots])[1]
        for r in roots:
            if r != heaviest:
                self.weights[heaviest] += self.weights[r]
                self.parents[r] = heaviest

def matching(G, initialMatching = {}):
    '''Find a maximum cardinality matching in a graph G.
G is represented in modified GvR form: iter(G) lists its vertices;
iter(G[v]) lists the neighbors of v; w in G[v] tests adjacency.
The output is a dictionary mapping vertices to their matches;
unmatched vertices are omitted from the dictionary.

We use Edmonds' blossom-contraction algorithm, as described e.g.
in Galil's 1986 Computing Surveys paper.'''

    # Copy initial matching so we can use it nondestructively
    matching = {}
    for x in initialMatching:
        matching[x] = initialMatching[x]


    # Form greedy matching to avoid some iterations of augmentation
    for v in G:
        if v not in matching:
            for w in G[v]:
                if w not in matching:
                    matching[v] = w
                    matching[w] = v
                    break

    def augment():
        '''Search for a single augmenting path.
Return value is true if the matching size was increased, false otherwise.'''
    
        # Data structures for augmenting path search:
        #
        # leader: union-find structure; the leader of a blossom is one
        # of its vertices (not necessarily topmost), and leader[v] always
        # points to the leader of the largest blossom containing v
        #
        # S: dictionary of leader at even levels of the structure tree.
        # Dictionary keys are names of leader (as returned by the union-find
        # data structure) and values are the structure tree parent of the blossom
        # (a T-node, or the top vertex if the blossom is a root of a structure tree).
        #
        # T: dictionary of vertices at odd levels of the structure tree.
        # Dictionary keys are the vertices; T[x] is a vertex with an unmatched
        # edge to x.  To find the parent in the structure tree, use leader[T[x]].
        #
        # unexplored: collection of unexplored vertices within leader of S
        #
        # base: if x was originally a T-vertex, but becomes part of a blossom,
        # base[t] will be the pair (v,w) at the base of the blossom, where v and t
        # are on the same side of the blossom and w is on the other side.

        leader = unionFind()
        S = {}
        T = {}
        unexplored = []
        base = {}
        
        # Subroutines for augmenting path search.
        # Many of these are called only from one place, but are split out
        # as subroutines to improve modularization and readability.
        
        def blossom(v,w,a):
            '''Create a new blossom from edge v-w with common ancestor a.'''
            
            def findSide(v,w):
                path = [leader[v]]
                b = (v,w)   # new base for all T nodes found on the path
                while path[-1] != a:
                    tnode = S[path[-1]]
                    path.append(tnode)
                    base[tnode] = b
                    unexplored.append(tnode)
                    path.append(leader[T[tnode]])
                return path
            
            a = leader[a]   # sanity check
            path1,path2 = findSide(v,w), findSide(w,v)
            leader.union(*path1)
            leader.union(*path2)
            S[leader[a]] = S[a] # update structure tree

        topless = object()  # should be unequal to any graph vertex
        def alternatingPath(start, goal = topless):
            '''Return sequence of vertices on alternating path from start to goal.
Goal must be a T node along the path from the start to the root of the structure tree.
If goal is omitted, we find an alternating path to the structure tree root.'''
            path = []
            while 1:
                while start in T:
                    v, w = base[start]
                    vs = alternatingPath(v, start)
                    vs.reverse()
                    path += vs
                    start = w
                path.append(start)
                if start not in matching:
                    return path     # reached top of structure tree, done!
                tnode = matching[start]
                path.append(tnode)
                if tnode == goal:
                    return path     # finished recursive subpath
                start = T[tnode]
                
        def pairs(L):
            '''Utility to partition list into pairs of items.
If list has odd length, the final pair is omitted silently.'''
            i = 0
            while i < len(L) - 1:
                yield L[i],L[i+1]
                i += 2
            
        def alternate(v):
            '''Make v unmatched by alternating the path to the root of its structure tree.'''
            path = alternatingPath(v)
            path.reverse()
            for x,y in pairs(path):
                matching[x] = y
                matching[y] = x

        def addMatch(v, w):
            '''Here with an S-S edge vw connecting vertices in different structure trees.
Find the corresponding augmenting path and use it to augment the matching.'''
            alternate(v)
            alternate(w)
            matching[v] = w
            matching[w] = v
            
        def ss(v,w):
            '''Handle detection of an S-S edge in augmenting path search.
Like augment(), returns true iff the matching size was increased.'''
    
            if leader[v] == leader[w]:
                return False        # self-loop within blossom, ignore
    
            # parallel search up two branches of structure tree
            # until we find a common ancestor of v and w
            path1, head1 = {}, v
            path2, head2 = {}, w
    
            def step(path, head):
                head = leader[head]
                parent = leader[S[head]]
                if parent == head:
                    return head     # found root of structure tree
                path[head] = parent
                path[parent] = leader[T[parent]]
                return path[parent]
                
            while 1:
                head1 = step(path1, head1)
                head2 = step(path2, head2)
                
                if head1 == head2:
                    blossom(v, w, head1)
                    return False
                
                if leader[S[head1]] == head1 and leader[S[head2]] == head2:
                    addMatch(v, w)
                    return True
                
                if head1 in path2:
                    blossom(v, w, head1)
                    return False
                
                if head2 in path1:
                    blossom(v, w, head2)
                    return False    

        # Start of main augmenting path search code.

        for v in G:
            if v not in matching:
                S[v] = v
                unexplored.append(v)

        current = 0     # index into unexplored, in FIFO order so we get short paths
        while current < len(unexplored):
            v = unexplored[current]
            current += 1

            for w in G[v]:
                if leader[w] in S:  # S-S edge: blossom or augmenting path
                    if ss(v,w):
                        return True

                elif w not in T:    # previously unexplored node, add as T-node
                    T[w] = v
                    u = matching[w]
                    if leader[u] not in S:
                        S[u] = w    # and add its match as an S-node
                        unexplored.append(u)
                        
        return False    # ran out of graph without finding an augmenting path
                        
    # augment the matching until it is maximum
    while augment():
        pass

    return matching


#G = {0: [3, 4, 5], 1: [2, 4, 5], 2: [1, 5], 3: [0, 4, 5], 4: [0, 1, 3], 5: [0, 1, 2, 3]}
#ret = matching(G)
#print(f"ret = {ret}")

### END EDMONDS' BLOSSOM-CONTRACTION ALGORITHM #################################
################################################################################


def gcd(a,b):
    a = abs(a)
    b = abs(b)
    while b > 0:
        a, b = b, a % b
    return 1 if a == 0 else a

def lcm(a,b):
    return (a*b) // gcd(a,b)


def has_infinite_loop(b1, b2):
    """ Banana Game: Return True of we get into an infinite loop. False if there is a tie """
    # Reduce
    all_gcd = gcd(b1, b2)
    b1 //= all_gcd
    b2 //= all_gcd

    if (b1 + b2) % 2 == 1:
        # The sum cannot be odd. We can only terminate in a tie at the average (b1+b2)/2
        return True

    if ((b2+b1)//2) % gcd(b2, b1) == 0:
        n = (b2+b1) // 2 // gcd(b2, b1)
        if (n & (n-1) == 0) and n != 0:
            return False
    return True


'''
memories = {}
def has_infinite_loop_naive(b1, b2):
    """ Banana Game: Return True of we get into an infinite loop. False if there is a tie """
    _b1, _b2 = b1, b2

    if (b1 + b2) % 2 == 1:
        return True

    if (b1 > b2):
        b1, b2 = b2, b1

    if memories.get((b1, b2)) != None:
        return memories[(b1, b2)]

    visited = [(b1, b2)]
    while b1 != b2:
        # Reduce
        all_gcd = gcd(b1, b2)
        b1 /= all_gcd
        b2 /= all_gcd

        if (b1 + b2) % 2 == 1:
            for pair in visited:
                memories[pair] = True
            return True

        if b2 // b1 > 3:
            # If the numbers are far apart, can calculate the difference: based on sum(k=0..n) 2**k == 2**(k+1)-1
            # After n iterations b1 increases by (2**n - 1)*b1  and  b2
            # decreases by this amount.
            n = math.ceil(math.log((b2 - b1) / 2 / b1, 2))
            b2 = b2 - (2**n - 1) * b1
            b1 = b1 + (2**n - 1) * b1
        else:
            b2 -= b1
            b1 += b1

        if (b1 > b2):
            b1, b2 = b2, b1

        # Reuse
        if memories.get((b1, b2)) != None:
            ret = memories[(b1, b2)]
            for pair in visited:
                memories[pair] = ret
            return ret

        if (b1, b2) in visited:
            # Loop detected
            for pair in visited:
                memories[pair] = True
            return True

        visited.append((b1, b2))
    else:
        # Recycle
        # Tie detected
        for pair in visited:
            memories[pair] = False
        return False


def all_pairs(lst):
    """ Generate all possible pair from a list: https://stackoverflow.com/questions/5360220/how-to-split-a-list-into-pairs-in-all-possible-ways """
    """
    >>> print(list(all_pairs([0,1,2,3])))
    [[(0, 1), (2, 3)], [(0, 2), (1, 3)], [(0, 3), (1, 2)]]
    >>> print(list(all_pairs([0,1,2])))
    [[(1, 2)], [(0, 2)], [(0, 1)]
    """
    if len(lst) < 2:
        yield []
        return
    if len(lst) % 2 == 1:
        # Handle odd length list
        for i in range(len(lst)):
            for result in all_pairs(lst[:i] + lst[i + 1:]):
                yield result
    else:
        a = lst[0]
        for i in range(1, len(lst)):
            pair = (a, lst[i])
            for rest in all_pairs(lst[1:i] + lst[i + 1:]):
                yield [pair] + rest


def solution_brute_force(banana_list):
    """ Enumerate through all possible pairings count which one has the most infinite loops.
        Works, but is too slow """
    num_guards = len(banana_list)
    guards = [n for n in range(num_guards)]

    target_max_busy_guards = num_guards if num_guards % 2 == 0 else num_guards - 1

    max_busy_guards = 0
    min_free_guards = len(banana_list) + 1
    for pairing_set in all_pairs(guards):
        busy_guards = 0
        free_guards = 0
        for pair in pairing_set:
            if has_infinite_loop(banana_list[pair[0]], banana_list[pair[1]]):
                busy_guards += 2
            else:
                free_guards += 2
            if free_guards > min_free_guards:
                break
        if max_busy_guards < busy_guards:
            max_busy_guards = busy_guards
            if max_busy_guards == target_max_busy_guards:
                break
        if min_free_guards > free_guards:
            min_free_guards = free_guards

    return len(guards) - max_busy_guards
'''

def solution(banana_list):
    """ Use the blossom algorithm to find the maximal pairing """
    num_guards = len(banana_list)
    guards = [n for n in range(num_guards)]

    # Step 1:  Calculate the prefered pairings (i.e. which end up in infinite loops)
    tie_pairs = set()
    for g1 in range(0, num_guards):
        for g2 in range(g1+1, num_guards):
           if not has_infinite_loop(banana_list[g1], banana_list[g2]):
               tie_pairs.add((g1, g2))
               tie_pairs.add((g2, g1))

    # Step 2: Create the preference table
    preferences = {}
    for n in range(num_guards):
        preferences[n] = [g for g in range(num_guards) if g != n and not (n, g) in tie_pairs]

    # Step 3: Match up the ones that loop
    best_match = matching(preferences)

    # Remove duplicates and count
    matches = set()
    for g1, g2 in best_match.items(): 
        matches.add((g1,g2) if g1 < g2 else (g2, g1))

    free_guards = 0
    for m in matches:
        if m in tie_pairs:
            free_guards += 2

    # Add unmatched
    free_guards += num_guards - 2 * len(matches)

    return free_guards


#assert(2 == solution([1, 1]))
#assert(0 == solution([1, 7, 3, 21, 13, 19]))
#assert(0 == solution([11666727, 54901249, 56144917, 34968148, 20459771, 51467979, 63908392, 25476012, 84494608, 10734980, 36099588, 95489176, 85023708, 35580401, 87290884, 57325004, 89924896, 40027806, 74609848, 3783952, 36828061, 61026138, 71511010, 95342954, 13782308, 13798855, 14727919, 51986126, 21260935, 55607619, 6750024, 55791951, 69357897, 86061548, 29024126, 89649639, 52153853, 50042550, 6074693, 1703936, 92243345, 21903993, 11221606, 27322662, 4023413, 59836161, 9886259, 70738085, 91171749, 68937313, 97797338, 12577681, 50870585, 78045229, 18726016, 16474738, 65438573, 93056258, 13913850, 73264421, 44936615, 34085521, 85498309, 36655985, 66720369, 85978171, 7725979, 17451654, 59436973, 92796289, 85590498, 1948084, 41996024, 86208620, 14536808, 47810121, 25899310, 97704439, 38175887, 13746481]) )
#assert(1 == solution([3, 5, 7, 11, 13, 17, 19, 23, 29, 31, 37, 41, 43, 47, 53, 59, 61, 67, 71, 73, 79, 83, 89, 97, 101, 103, 107, 109, 113, 127, 131, 137, 139, 149, 151, 157, 163, 167, 173, 179, 181, 191, 193, 197, 199])  )


"""
for b1 in range(1, 1000, 2):
    for b2 in range(1, b1, 2):
        pow2 = False
        if ((b2+b1)//2) % gcd(b2, b1) == 0:
            n = (b2+b1) // 2 // gcd(b2, b1)
            if (n & (n-1) == 0) and n != 0:
                pow2 = True
        if not has_infinite_loop(b1, b2):
            print(f"T ({b2}, {b1}),  {gcd(b2, b1)} {(b2+b1)/2} {(b2+b1)/2/gcd(b2, b1)}  {'ZZZZ' if not pow2 else pow2}")
        else:
            print(f"L ({b2}, {b1}),  {gcd(b2, b1)} {(b2+b1)/2} {(b2+b1)/2/gcd(b2, b1)}  {'ZZZZ' if pow2 else pow2}")

"""
