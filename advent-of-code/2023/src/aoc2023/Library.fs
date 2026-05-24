namespace aoc2023

open System
open Xunit

module Marker =
    let public marker () = 0

module fileio =
    open System.Text.RegularExpressions
    let linebreakRegex = Regex(@"\r\n?|\n", RegexOptions.Compiled)

    let public linesFromString (str: string) : string list =
        linebreakRegex.Split(str) |> Array.toList

    let public linesFromFile (filePath: string) : string list =
        System.IO.File.ReadAllLines(System.IO.Path.Join(__SOURCE_DIRECTORY__, filePath))
        |> Array.toList

    let tokenize (data: string) seps =
        let splitopts =
            System.StringSplitOptions.TrimEntries
            ||| System.StringSplitOptions.RemoveEmptyEntries

        let seps: char array = seps |> Seq.toArray
        data.Split(seps, splitopts) |> Array.toList

    // Break list of strings (lines) into a list of list of strings
    // broken at empty lines
    let public chunkLines (lines: string list) : List<string list> =
        let mutable result = []
        let mutable chunk = []

        for line in lines do
            if line = "" then
                if chunk.Length > 0 then result <- result @ [ chunk ]
                chunk <- []
            else
                chunk <- chunk @ [ line ]

        if chunk.Length > 0 then result <- result @ [ chunk ]

        result

module fileio_test =
    [<Fact>]
    let ``test fileio`` () =
        let data = "abc\ndef\n\nxyz\n\n\n123"
        let lines = data |> fileio.linesFromString
        let chunks = lines |> fileio.chunkLines
        Assert.Equivalent([ "abc"; "def"; "xyz"; ""; "123" ], lines)
        Assert.Equivalent([ [ "abc"; "def" ]; [ "xyz" ]; [ "123" ] ], chunks)
        // Ignore trailing newline
        let data = data + "\n"
        let lines = data |> fileio.linesFromString
        let chunks = lines |> fileio.chunkLines
        Assert.Equivalent([ "abc"; "def"; "xyz"; ""; "123" ], lines)
        Assert.Equivalent([ [ "abc"; "def" ]; [ "xyz" ]; [ "123" ] ], chunks)


module debug =
    let enable_highlight = true
    let highlight_on = if enable_highlight then "\x1B[0;33m" else ""
    let highlight_off = if enable_highlight then "\x1B[39;49m" else ""

    let printfn format =
        Printf.ksprintf (fun (s: string) -> Printf.printfn "%s%s%s" highlight_on s highlight_off) format

    let printf format =
        Printf.ksprintf (fun (s: string) -> Printf.printf "%s%s%s" highlight_on s highlight_off) format

    /// let demo() =
    ///    let fileName = "myfile.txt"
    ///    debug.info <| sprintf "Opening file %s" fileName
    let private log =
        let lockObj = obj ()

        fun color s ->
            lock lockObj (fun _ ->
                let reset = if color = "" then "" else "\x1B[0m"
                Printf.printf "%s%s%s" color s reset)

    let darkyellow = log "\x1B[33m"
    let yellow = log "\x1B[93m"
    let darkgreen = log "\x1B[32m"
    let green = log "\x1B[92m"
    let darkred = log "\x1B[31m"
    let red = log "\x1B[91m"
    let normal = log ""


module itertools =
    open System.Collections.Concurrent

    let product xs ys =
        seq {
            for x in xs do
                for y in ys do
                    yield x, y
        }
    // Simple memoize function
    let memoize f =
        let dict = System.Collections.Generic.Dictionary<_, _>()

        fun c ->
            match dict.TryGetValue c with

            | true, value -> value
            | _ ->
                let value = f c
                dict.Add(c, value)
                value
    // Specific memoizer for 2 curried arguments
    let memoize2 f =
        let m = memoize (fun (a, b) -> f a b)
        fun a b -> m (a, b)

    // Specific memoizer for 3 curried arguments
    let memoize3 f =
        let m = memoize (fun (a, b, c) -> f a b c)
        fun a b c -> m (a, b, c)

    // 1. Define the recursive memoization wrapper
    let memoizeRec2 f =
        // Type annotations on the dictionary clarify what 'a and 'b are
        let cache = ConcurrentDictionary<'a, Lazy<'b>>()

        let rec g x =
            // Uses Lazy to prevent duplicate computations on concurrent threads
            // Wrap the anonymous function explicitly into a System.Func delegate
            let factory = Func<'a, Lazy<'b>>(fun key -> lazy (f g key))
            cache.GetOrAdd(x, factory).Value

        g

    let memoizeRec (f: ('a -> 'b) -> 'a -> 'b) =
        let cache = ConcurrentDictionary<'a, Lazy<'b>>()

        let rec g (x: 'a) : 'b =
            // F# converts the closure automatically if the generic signatures match cleanly
            cache.GetOrAdd(x, fun (key: 'a) -> lazy (f g key)).Value

        g


module itertools_test =
    [<Fact>]
    let ``test product`` () =
        let a = [ 1; 2; 3 ]
        let b = [ "a"; "b" ]
        let expected = [ (1, "a"); (1, "b"); (2, "a"); (2, "b"); (3, "a"); (3, "b") ]
        Assert.Equivalent(expected, itertools.product a b)

    [<Fact>]
    let ``test memoize`` () =
        // Example usage. single tuple argument
        let expensiveFunc (x, y, z) = x * y * z // Single tuple argument
        let memoizedFunc = itertools.memoize expensiveFunc
        Assert.Equal(30, memoizedFunc (2, 3, 5)) // Computes
        Assert.Equal(30, memoizedFunc (2, 3, 5)) // Returns cached value

        // Three arguments
        let expensiveFunc x y z = x + y + z
        let memoizedFunc = itertools.memoize expensiveFunc // Three arguments
        Assert.Equal(10, memoizedFunc 2 3 5) // Computes
        Assert.Equal(10, memoizedFunc 2 3 5) // Returns cached value

        // Recursive memoization
        // Define the Fibonacci logic using the wrapper's loop variable (g)
        let fibMem =
            itertools.memoizeRec (fun g n ->
                match n with
                | x when x = 0I -> 0I // Using BigInteger (I) to handle large numbers
                | x when x = 1I -> 1I
                | _ -> g (n - 1I) + g (n - 2I))

        // 3. Execution: Should be quick
        Assert.Equal(354224848179261915075I, fibMem 100I)


module gridio =
    let read_grid (lines: string list) isPadded default_value : char[,] =
        // If isPadded, add extra rows/columns around the grid. Makes loops easier, avoiding boundary conditions.
        assert (lines.Length > 0)
        assert (lines[0].Length > 0)
        let width, height = lines[0].Length, lines.Length

        // Make sure it is rectanglular
        lines |> List.iter (fun l -> assert (l.Length = width))

        let grid =
            if isPadded then
                let initfn (y: int) (x: int) =
                    match (x, y) with
                    | (0, _) -> default_value
                    | (w, _) when w = width + 1 -> default_value
                    | (_, 0) -> default_value
                    | (_, h) when h = height + 1 -> default_value
                    | _ ->
                        // -1 for padding
                        lines[y - 1][x - 1]

                Array2D.initBased 0 0 (height + 2) (width + 2) initfn
            else
                let initfn (y: int) (x: int) = lines[y][x]
                Array2D.initBased 0 0 height width initfn

        grid

    let print_grid (grid: 'T[,]) print_cell =
        // E.g. gridio.print_grid grid (fun cell -> printf "%c" cell)
        let width, height = grid.GetLength(1), grid.GetLength(0)

        for y in 0 .. height - 1 do
            for x in 0 .. width - 1 do
                print_cell grid.[y, x]

            printfn ""

    // Return a list of (y, x) tuple coordinates between [starty,endy]x[startx,endx]
    // inclusive of start/end. Optionally skip (0,0) in enumeration
    let enumerate_coordinates starty endy startx endx =
        seq {
            for y in starty..endy do
                for x in startx..endx do
                    yield (y, x)
        }
    // Same as enumerate_coordinates origin±window_size, except we skip origin
    let enumerate_neighbours (y, x) window_size =
        assert (window_size > 0)

        enumerate_coordinates (y - window_size) (y + window_size) (x - window_size) (x + window_size)
        |> Seq.filter (fun x -> x <> (0, 0))

module gridio_test =
    [<Fact>]
    let ``test grid`` () =
        let data = "123\n456\n789"
        let data = fileio.linesFromString data
        let grid = gridio.read_grid data false '.'
        let width, height = grid.GetLength(1), grid.GetLength(0)
        Assert.Equal(3, width)
        Assert.Equal(3, height)
        let expected = [ [ '1'; '2'; '3' ]; [ '4'; '5'; '6' ]; [ '7'; '8'; '9' ] ]

        for index in 0 .. height - 1 do
            let row = grid.[index, *]
            Assert.Equal(expected[index], row)

        let expected = [ [ '1'; '4'; '7' ]; [ '2'; '5'; '8' ]; [ '3'; '6'; '9' ] ]

        for index in 0 .. width - 1 do
            let col = grid.[*, index]
            Assert.Equal(expected[index], col)

        let grid = gridio.read_grid data true '.'

        let expected =
            [ [ '.'; '.'; '.'; '.'; '.' ]
              [ '.'; '1'; '2'; '3'; '.' ]
              [ '.'; '4'; '5'; '6'; '.' ]
              [ '.'; '7'; '8'; '9'; '.' ]
              [ '.'; '.'; '.'; '.'; '.' ] ]

        let width, height = grid.GetLength(1), grid.GetLength(0)
        Assert.Equal(5, width)
        Assert.Equal(5, height)

        for index in 0 .. height - 1 do
            let row = grid.[index, *]
            Assert.Equal(expected[index], row)


module FPQ =
    /// Basic Functional Leftist Heap
    /// All functions are pure, and the data structure Heap is immutable
    type Heap<'a when 'a: comparison> =
        | Empty
        | Node of 'a * int * Heap<'a> * Heap<'a> // Value, Rank/NPL, Left Child, Right Child

    // Helper function to get the rank of a queue
    let private rank =
        function
        | Empty -> 0
        | Node(_, r, _, _) -> r

    let private makeNode v l r =
        if rank l >= rank r then
            Node(v, rank r + 1, l, r)
        else
            Node(v, rank l + 1, r, l)

    // A balancing function (meld or merge) is the core operation.
    // This is the functional equivalent of the "heapify" process.
    let rec private merge h1 h2 =
        match h1, h2 with
        | Empty, h -> h
        | h, Empty -> h
        | Node(v1, _, l1, r1), Node(v2, _, l2, r2) -> if v1 <= v2 then makeNode v1 l1 (merge r1 h2) else makeNode v2 l2 (merge r2 h1)

    // Check if heap is empty
    let is_empty h = h = Empty
    // Inserts a new element by merging an element (as a single Node)
    // with the existing queue structure.
    let enqueue v h = merge (Node(v, 1, Empty, Empty)) h

    // Get the minimum element. Returns an Option type because the queue could be empty.
    let peek =
        function
        | Empty -> None
        | Node(v, _, _, _) -> Some v

    // Deletes the minimum element.
    let dequeue =
        function
        | Empty -> Empty // failwith "Queue is empty"
        | Node(_, _, l, r) -> merge l r


module FPQ_test =
    [<Fact>]
    let ``test priority queue`` () =
        let pq = FPQ.Heap.Empty |> FPQ.enqueue 1
        let pq = FPQ.enqueue 2 pq
        let pq = FPQ.enqueue 5 pq
        let pq = FPQ.enqueue 2 pq
        let pq = FPQ.enqueue 9 pq

        let ret = FPQ.peek pq
        Assert.Equal(Some(1), ret)
        let ret = FPQ.peek pq
        Assert.Equal(Some(1), ret)
        let pq = FPQ.dequeue pq
        let ret = FPQ.peek pq
        Assert.Equal(Some(2), ret)
        let pq = FPQ.dequeue pq
        let ret = FPQ.peek pq
        Assert.Equal(Some(2), ret)
        let pq = FPQ.dequeue pq
        let ret = FPQ.peek pq
        Assert.Equal(Some(5), ret)
        let pq = FPQ.dequeue pq
        let pq = FPQ.dequeue pq
        Assert.True(FPQ.is_empty pq)

module queuelib =
    open System.Collections.Generic

    type MutableQueue<'T>() =
        let inner = Queue<'T>()

        member _.Enqueue(x: 'T) = inner.Enqueue(x)
        member _.Dequeue() = inner.Dequeue()
        member _.IsEmpty = inner.Count = 0

module queue_test =
    [<Fact>]
    let ``test queue`` () =
        // Example Usage:
        let mq = queuelib.MutableQueue<int>()
        Assert.True mq.IsEmpty
        mq.Enqueue 10
        Assert.False mq.IsEmpty
        let val1 = mq.Dequeue()
        Assert.Equal(10, val1)
        Assert.True mq.IsEmpty


module ImmutableQueue =
    type Queue<'T> = Queue of 'T list * 'T list

    let empty = Queue([], [])

    let enqueue x (Queue(front, back)) =
        match front with
        | [] -> Queue([ x ], []) // Keep front non-empty if possible
        | _ -> Queue(front, x :: back)

    let dequeue (Queue(front, back)) =
        match front with
        | [] -> None, empty
        | [ x ] -> Some x, Queue(List.rev back, []) // Refill front from back
        | x :: xs -> Some x, Queue(xs, back)

    let is_empty (Queue(front, back)) = front.Length = 0 && back.Length = 0

module immutablequeue_test =
    [<Fact>]
    let ``test immutablequeue`` () =
        // Example Usage:
        let q = ImmutableQueue.empty
        let q = ImmutableQueue.enqueue 10 q
        let q = q |> ImmutableQueue.enqueue 20
        let q = q |> ImmutableQueue.enqueue 30
        let v, q = ImmutableQueue.dequeue q
        Assert.Equal(Some 10, v)
        let v, q = ImmutableQueue.dequeue q
        Assert.Equal(Some 20, v)
        let v, q = ImmutableQueue.dequeue q
        Assert.Equal(Some 30, v)
        let v, q = ImmutableQueue.dequeue q
        Assert.Equal(None, v)
        let v, q = ImmutableQueue.dequeue q
        Assert.Equal(None, v)
        let q = ImmutableQueue.enqueue 10 q
        let v, q = ImmutableQueue.dequeue q
        Assert.Equal(Some 10, v)

module graphsearch =

    /// Purely functional A* search using immutable F# collections
    let aStar
        (start: 'T)
        (goal: 'T)
        (getNeighbors: 'T -> Map<'T, 'T> -> 'T seq)
        (cost: 'T -> 'T -> float)
        (heuristic: 'T -> float)
        (is_goal: 'T -> 'T -> bool)
        : 'T list option =

        /// Reconstructs path by recursively looking up parents in the immutable Map
        let rec reconstructPath current (cameFrom: Map<'T, 'T>) acc =
            match cameFrom.TryFind(current) with
            | Some parent -> reconstructPath parent cameFrom (current :: acc)
            | None -> current :: acc

        /// Core recursive search loop
        /// openSet: Set<(float * 'T)> - Stores (fScore, position) for automatic sorting
        /// gScores: Map<'T, float>    - Actual cost from start to node
        /// cameFrom: Map<'T, 'T>     - Breadcrumb trail for path reconstruction
        let rec search (openSet: FPQ.Heap<float * 'T>) (gScores: Map<'T, float>) (cameFrom: Map<'T, 'T>) =
            if openSet |> FPQ.is_empty then
                None // Frontier exhausted, no path found
            else
                // Set.minElement acts as the priority queue's "Dequeue"
                let (currentF, currentPos) = (FPQ.peek openSet).Value
                let remainingOpen = FPQ.dequeue openSet

                if is_goal currentPos goal then
                    Some(reconstructPath currentPos cameFrom [])
                else
                    // Evaluate neighbors and fold them into the current state
                    let (nextOpen, nextG, nextCame) =
                        getNeighbors currentPos cameFrom
                        |> Seq.fold
                            (fun ((oSet: FPQ.Heap<float * 'T>), (gMap: Map<'T, float>), (cMap: Map<'T, 'T>)) neighbor ->
                                let tentativeG = gMap.[currentPos] + cost currentPos neighbor
                                let currentNeighborG = gMap.TryFind(neighbor) |> Option.defaultValue infinity

                                if tentativeG < currentNeighborG then
                                    let fScore = tentativeG + heuristic neighbor
                                    // Update maps and add new potential path to the frontier
                                    (FPQ.enqueue (fScore, neighbor) oSet, gMap.Add(neighbor, tentativeG), cMap.Add(neighbor, currentPos))
                                else
                                    (oSet, gMap, cMap))
                            (remainingOpen, gScores, cameFrom)

                    search nextOpen nextG nextCame

        // Initial state: Start node with a gScore of 0 and initial fScore
        let initialGScores = Map.empty.Add(start, 0.0)
        let initialOpen = FPQ.Empty |> FPQ.enqueue (heuristic start, start)

        search initialOpen initialGScores Map.empty

    open System.Collections.Generic

    /// Generic Dijkstra algorithm
    /// getNeighbors: 'node -> 'node seq
    /// getCost: 'node -> 'node -> int
    /// source: 'node
    let dijkstra getNeighbors getCost source =
        let distances = Dictionary<'node, float>()
        let pq = PriorityQueue<'node, float>()

        // Initialize source
        distances.[source] <- 0.0
        pq.Enqueue(source, 0)

        while pq.Count > 0 do
            let mutable u = Unchecked.defaultof<'node>
            let mutable distU = 0.0

            if pq.TryDequeue(&u, &distU) then
                // Only process if this is the shortest known distance to u
                if
                    distU
                    <= (if distances.ContainsKey(u) then distances.[u] else System.Int32.MaxValue)
                then
                    for v in getNeighbors u do
                        let weight = getCost u v
                        let newDist = distU + weight

                        if not (distances.ContainsKey(v)) || newDist < distances.[v] then
                            distances.[v] <- newDist
                            pq.Enqueue(v, newDist)

        distances


    open System.Collections.Generic

    /// Generic Dijkstra search that returns the shortest path to a target
    /// getNeighbors: 'node -> 'node seq
    /// getCost: 'node -> 'node -> int
    let dijkstraPath getNeighbors getCost source target =
        let distances = Dictionary<'node, float>()
        let predecessors = Dictionary<'node, 'node>()
        let pq = PriorityQueue<'node, float>()

        distances.[source] <- 0.0
        pq.Enqueue(source, 0)

        let mutable found = false

        while pq.Count > 0 && not found do
            let mutable u = Unchecked.defaultof<'node>
            let mutable distU = 0.0

            if pq.TryDequeue(&u, &distU) then
                if u = target then
                    found <- true
                elif
                    distU
                    <= (if distances.ContainsKey(u) then distances.[u] else System.Int32.MaxValue)
                then
                    for v in getNeighbors u do
                        let weight = getCost u v
                        let newDist = distU + weight

                        if not (distances.ContainsKey(v)) || newDist < distances.[v] then
                            distances.[v] <- newDist
                            predecessors.[v] <- u
                            pq.Enqueue(v, newDist)

        if found then
            // Reconstruct path by backtracking from target to source
            let path = List<'node>()
            let mutable curr = target
            path.Add(curr)

            while curr <> source do
                curr <- predecessors.[curr]
                path.Add(curr)

            let finalPath = path |> Seq.toList
            Some(distances.[target], finalPath)
        else
            None

    let rec findAllPathsRecursive get_neighbours is_target pathSoFar =
        seq {
            let currentNode = List.head pathSoFar

            if is_target currentNode then
                yield List.rev pathSoFar // Emit the completed path
            else
                let neighbors = get_neighbours currentNode

                for neighbor in neighbors do
                    // Avoid cycles by checking if neighbor is in current path
                    if not (List.contains neighbor pathSoFar) then
                        // yield! flattens the nested sequence of paths
                        yield! findAllPathsRecursive get_neighbours is_target (neighbor :: pathSoFar)
        }

    let findAllPaths (neighbors: 'a -> 'a seq) (isGoal: 'a -> bool) (start: 'a) : seq<'a list> =
        // Each state in our stack tracks:
        // (current_node, path_so_far_reversed, unvisited_neighbors_list, visited_set)
        let initialNeighbors = neighbors start |> Seq.toList
        let initialVisited = Set.singleton start
        let initialPath = [ start ]
        let initialStack = [ (start, initialPath, initialNeighbors, initialVisited) ]

        // State machine function for Seq.unfold
        let rec nextState stack =
            match stack with

            | [] -> None // Stack is empty, terminates the sequence
            | (curr, path, remainingNeighbors, visited) :: tail ->
                if isGoal curr then
                    // Goal found: pop it to prevent infinite loops, return path, and update stack
                    Some(List.rev path, tail)
                else
                    match remainingNeighbors with

                    | [] ->
                        // No neighbors left to explore at this level: backtrack
                        nextState tail
                    | next :: restNeighbors ->
                        let updatedCurrentState = (curr, path, restNeighbors, visited)

                        if Set.contains next visited then
                            // Cycle detected: skip this neighbor and continue evaluation
                            nextState (updatedCurrentState :: tail)
                        else
                            let nextNeighbors = neighbors next |> Seq.toList
                            let nextVisited = Set.add next visited
                            let nextPath = next :: path
                            let nextStateNode = (next, nextPath, nextNeighbors, nextVisited)
                            // Push next node and updated current node back to stack
                            nextState (nextStateNode :: updatedCurrentState :: tail)

        seq {
            if isGoal start then yield [ start ]
            yield! Seq.unfold nextState initialStack
        }
    let findAllPathsWithWeights (neighbors: 'a -> ('a * int) seq) (isGoal: 'a -> bool) (start: 'a) : seq<'a list * int> =
        // Each stack frame holds: (CurrentNode, CurrentPath, CurrentWeight, RemainingNeighborsList)
        let initialState = 
            let firstFrame = (start, [start], 0, neighbors start |> Seq.toList)
            ([firstFrame], Set.singleton start)

        initialState

        |> Seq.unfold (fun (stack, visited) ->
            match stack with
            | [] -> None // Stack is empty; termination condition
            | (curr, path, weight, neighborsLeft) :: restStack ->
                match neighborsLeft with

                | [] -> 
                    // No neighbors left for this node. Backtrack.
                    let nextVisited = Set.remove curr visited
                    Some (None, (restStack, nextVisited))
                | (next, edgeWeight) :: remainingNeighbors ->
                    // Fix: Update the current frame to reflect remaining neighbors
                    let updatedCurrentFrame = (curr, path, weight, remainingNeighbors)
                    let updatedStack = updatedCurrentFrame :: restStack

                    if Set.contains next visited then
                        // Cycle detected: skip this neighbor
                        Some (None, (updatedStack, visited))
                    else
                        let nextPath = next :: path
                        let nextWeight = weight + edgeWeight
                        let nextVisited = Set.add next visited

                        // FIXED: Removed the assignment operator '=' that caused the type error
                        let nextNeighbors = neighbors next |> Seq.toList
                        let nextFrame = (next, nextPath, nextWeight, nextNeighbors)
                        let nextStack = nextFrame :: updatedStack

                        if isGoal next then
                            Some (Some (List.rev nextPath, nextWeight), (nextStack, nextVisited))
                        else
                            Some (None, (nextStack, nextVisited))
        )
        |> Seq.choose id


    let findAllPathsWithWeights2 (neighbors: 'a -> ('a * int) seq) (isGoal: 'a -> bool) (start: 'a) : seq<'a list * int> =
        seq {
            // Stack stores: (current_node, current_path, current_weight, remaining_neighbors_enumerator)
            // Using an enumerator lets us resume processing exactly where we left off.
            let initialEnum = (neighbors start).GetEnumerator()
            let stack = System.Collections.Generic.Stack<'a * 'a list * int * System.Collections.Generic.IEnumerator<'a * int>>()

            stack.Push((start, [start], 0, initialEnum))

            // Check if the start node itself satisfies the goal condition
            if isGoal start then
                yield ([start], 0)

            while stack.Count > 0 do
                let (curr, path, weight, enum) = stack.Peek()

                if enum.MoveNext() then
                    let (next, edgeWeight) = enum.Current

                    // Avoid cycles by checking if the node is already in the path
                    if not (List.contains next path) then
                        let nextPath = next :: path
                        let nextWeight = weight + edgeWeight

                        if isGoal next then
                            // Found a valid path; reverse it to restore correct chronological order
                            yield (List.rev nextPath, nextWeight)

                        // Push the next node's transitions onto the stack to explore deeper (DFS)
                        let nextEnum = (neighbors next).GetEnumerator()
                        stack.Push((next, nextPath, nextWeight, nextEnum))
                else
                    // Clean up the iterator resources when a node's neighbors are fully exhausted
                    enum.Dispose()
                    stack.Pop() |> ignore
        }

    /// <summary>
    /// Finds the longest path(s) from a starting node to any valid goal node iteratively,
    /// utilizing dynamic programming and memoization via immutable F# structures.
    /// </summary>
    /// <param name="neighbors">A function that takes a node and returns a sequence of its neighbors along with their respective edge weights.</param>
    /// <param name="isGoal">A predicate function that returns true if a given node is a destination goal.</param>
    /// <param name="start">The starting node for the path exploration.</param>
    /// <returns>A sequence of lists, where each list represents an optimal (longest) path from the start node to a goal node.</returns>
    let findLongestPathIterative (neighbors: 'a -> ('a * int) seq) (isGoal: 'a -> bool) (start: 'a) : seq<'a list> =

        // DP Memoization Table: Maps a node to its (current_max_distance, path_taken_in_reverse)
        // Initialized with the start node at distance 0
        let mutable distances = Map.empty |> Map.add start (0, [ start ])

        // Frontier list acting as our iterative stack/queue to explore nodes
        let mutable frontier = [ start ]

        // Tracks the absolute longest distance found to any goal node so far
        let mutable maxGoalDist = System.Int32.MinValue

        // Accumulator holding all paths that achieve the maxGoalDist
        let mutable optimalPaths = []

        // Strictly iterative loop replacing traditional call-stack recursion
        while not (List.isEmpty frontier) do
            match frontier with
            | current :: rest ->
                // Dequeue/pop the current node and update the tracking frontier pointer
                frontier <- rest

                // Retrieve the verified longest distance and path to the current node
                let currentDist, currentPath = Map.find current distances

                // Explore all immediate outbound paths from the current node
                for (neighbor, weight) in neighbors current do
                    let newDist = currentDist + weight
                    let newPath = neighbor :: currentPath

                    // Dynamic Programming Lookup: Determine if the newly calculated path
                    // is strictly longer than any previously recorded path to this neighbor.
                    let shouldUpdate =
                        match Map.tryFind neighbor distances with
                        | Some(existingDist, _) -> newDist > existingDist
                        | None -> true // First time discovering this node

                    // If it's a better path, memoize the new distance and queue the neighbor
                    if shouldUpdate then
                        distances <- Map.add neighbor (newDist, newPath) distances
                        frontier <- neighbor :: frontier

                // Goal State Evaluation: Check if the current node satisfies the objective
                if isGoal current then
                    if currentDist > maxGoalDist then
                        // Found a path that strictly beats our historical maximum distance
                        maxGoalDist <- currentDist
                        optimalPaths <- [ currentPath ]
                    elif currentDist = maxGoalDist then
                        // Found an alternative path that ties the current maximum distance
                        optimalPaths <- currentPath :: optimalPaths

            | [] -> () // Safety catch-all for an empty frontier state

        // Post-processing: Because paths are constructed efficiently by prepending items (O(1)),
        // they must be reversed back to chronological order (Start -> Goal) before returning.
        optimalPaths |> List.map List.rev |> Seq.ofList


    /// <summary>
    /// Compresses a graph by collapsing all linear paths of degree-2 nodes into single weighted edges.
    /// </summary>
    /// 
    /// <remarks>
    /// This version utilizes an optimized multi-source flood fill (BFS) to map old nodes to 
    /// compressed core nodes, eliminating processing bottlenecks on long linear chains.
    /// </remarks>
    /// 
    /// <param name="nodes">The complete list of all vertices/nodes present in the graph.</param>
    /// <param name="getNeighbors">A function that returns the immediate neighbors for any given node.</param>
    /// 
    /// <typeparam name="'Node">The type of the vertex. Must support structural comparison for Map keys.</typeparam>
    /// 
    /// <returns>
    /// A tuple containing:
    /// <br/>1. <c>'Node list</c>: The remaining core nodes.
    /// <br/>2. <c>Map&lt;'Node, ('Node * int) list&gt;</c>: The new compressed adjacency list.
    /// <br/>3. <c>Map&lt;'Node, 'Node&gt;</c>: Optimized lookup table mapping every original node to its closest core node.
    /// </returns>
    let compressGraph (nodes: 'Node list) (getNeighbors: 'Node -> 'Node list) : 'Node list * Map<'Node, ('Node * int) list> * Map<'Node, 'Node> =

        // Step 1: Pre-calculate neighbor counts for every node to establish topology.
        let degreeMap = 
            nodes 

            |> List.map (fun node -> node, getNeighbors node)
            |> Map.ofList

        // Step 2: Core nodes are junctions (degree != 2).
        let coreNodes = 
            nodes |> List.filter (fun n -> 
                match Map.tryFind n degreeMap with

                | Some neighbors -> List.length neighbors <> 2
                | None -> false
            )

        // Step 3: Iteratively trace a path from a core node into a specific direction.
        let tracePath (startNode: 'Node) (firstStep: 'Node) : 'Node * int =
            let (endNode, _, totalWeight) =
                Seq.initInfinite (fun i -> i)

                |> Seq.scan (fun (curr, prev, weight) _ ->
                    match Map.tryFind curr degreeMap with
                    | Some neighbors when List.length neighbors = 2 ->
                        let nextNode = neighbors |> List.find (fun n -> n <> prev)
                        (nextNode, curr, weight + 1)

                    | _ -> 
                        (curr, prev, weight)
                ) (firstStep, startNode, 1) // Pass the initial state tuple here
                |> Seq.pairwise
                |> Seq.find (fun (state1, state2) -> state1 = state2)

                |> fst

            (endNode, totalWeight)

        // Step 4: Construct the compressed adjacency list for the surviving core nodes.
        let compressedEdges =
            coreNodes
            |> List.map (fun node ->
                let neighbors = Map.tryFind node degreeMap |> Option.defaultValue []
                let edges = neighbors |> List.map (fun nextNode -> tracePath node nextNode)
                node, edges
            )

            |> Map.ofList

        // Step 5: Multi-source BFS/Flood Fill to map old nodes to core nodes.
        let initialMap = coreNodes |> List.map (fun n -> n, n) |> Map.ofList

        let initialQueue = 
            coreNodes 

            |> List.collect (fun core -> 
                let neighbors = Map.tryFind core degreeMap |> Option.defaultValue []
                neighbors 

                |> List.filter (fun n -> Map.tryFind n degreeMap |> Option.map List.length |> Option.defaultValue 0 = 2)
                |> List.map (fun neighbor -> neighbor, core)
            )

        let finalOldToNewMap =
            initialQueue

            |> List.fold (fun (currentMap, activeQueue) _ ->
                match activeQueue with
                | [] -> (currentMap, [])
                | (currNode, coreOwner) :: restQueue ->
                    if Map.containsKey currNode currentMap then
                        (currentMap, restQueue)
                    else
                        let updatedMap = Map.add currNode coreOwner currentMap
                        let neighbors = Map.tryFind currNode degreeMap |> Option.defaultValue []
                        let nextSteps = 
                            neighbors

                            |> List.filter (fun n -> Map.tryFind n degreeMap |> Option.map List.length |> Option.defaultValue 0 = 2)
                            |> List.filter (fun n -> not (Map.containsKey n updatedMap))

                            |> List.map (fun n -> n, coreOwner)

                        (updatedMap, restQueue @ nextSteps)
            ) (initialMap, initialQueue)
            |> fst

        (coreNodes, compressedEdges, finalOldToNewMap)

    // type Multigraph<'n, 'e> = Map<'n, ('n * 'e) list> when 'n : comparison
    let findAllPaths_multigraph (start: 'n) (target: 'n) (get_neighbours: 'n -> ('n * 'e) list option) : ('n * 'e) list list =
        let rec search current visited path =
            if current = target then
                [ List.rev path ]
            else
                match get_neighbours current with
                | None -> []
                | Some edges ->
                    edges
                    |> List.filter (fun (next, _) -> not (Set.contains next visited))
                    |> List.collect (fun (next, data) -> search next (Set.add next visited) ((next, data) :: path))

        search start (Set.singleton start) []


module graphsearch_test =
    open System

    [<Fact>]
    let ``test astar`` () =
        let graph =
            ".#....\n\
                     .#....\n\
                     ......\n\
                     #....."

        let graph = gridio.read_grid (fileio.linesFromString graph) false '.'
        let width, height = graph.GetLength(1), graph.GetLength(0)
        let goal = (height - 1, width - 1)

        let get_neighbours (g: char array2d) (rc: int * int) =
            let r, c = rc
            let width, height = graph.GetLength(1), graph.GetLength(0)
            let neighbours = [ r - 1, c; r + 1, c; r, c - 1; r, c + 1 ]

            let neighbours =
                neighbours
                |> Seq.filter (fun (r, c) -> r >= 0 && r < height && c >= 0 && c < width)

            let neighbours = neighbours |> Seq.filter (fun (r, c) -> g.[r, c] <> '#')
            neighbours

        let get_neighbours_with_history (g: char array2d) (rc: int * int) _ = get_neighbours g rc

        let get_heuristic (goal: int * int) (node: int * int) : float =
            float (Math.Abs(fst node - fst goal) + Math.Abs(snd node - snd goal))

        let get_dist_between (node: int * int) (neighbour: int * int) : float =
            float (Math.Abs(fst node - fst neighbour) + Math.Abs(snd node - snd neighbour))

        let is_goal current goal = current = goal

        let path =
            graphsearch.aStar (0, 0) goal (graph |> get_neighbours_with_history) get_dist_between (goal |> get_heuristic) is_goal

        Assert.Equivalent(Some(Seq.ofList [ (3, 5); (2, 5); (2, 4); (2, 3); (2, 2); (2, 1); (2, 0); (1, 0); (0, 0) ]), path)


        let distances =
            graphsearch.dijkstra (graph |> get_neighbours) get_dist_between (0, 0)

        Assert.Equal(8.0, distances.[goal])
        Assert.Equal(0.0, distances.[(0, 0)])

        let path =
            graphsearch.dijkstraPath (graph |> get_neighbours) get_dist_between (0, 0) (height - 1, width - 1)

        Assert.Equal(8.0, fst path.Value)
        Assert.Equal(9, snd path.Value |> List.length)

    [<Fact>]
    let ``test getAllPaths`` () =
        // Example Usage:
        let graph = Map [ 'A', [ 'B'; 'C' ]; 'B', [ 'D' ]; 'C', [ 'B'; 'D' ] ]

        let get_neighbours (g: Map<'a, 'a list>) (node: 'a) : 'a list =
            g |> Map.tryFind node |> Option.defaultValue []

        let is_target (goal: 'a) (node: 'a) = goal = node

        let pathSeqRec =
            graphsearch.findAllPathsRecursive (graph |> get_neighbours) ('D' |> is_target) [ 'A' ]

        let pathSeqIterative =
            graphsearch.findAllPaths (graph |> get_neighbours >> Seq.ofList) ('D' |> is_target) 'A'

        // Access elements lazily
        // pathSeq |> Seq.iter (printfn "Path found: %A")
        Assert.Equal([ [ 'A'; 'B'; 'D' ]; [ 'A'; 'C'; 'B'; 'D' ]; [ 'A'; 'C'; 'D' ] ], pathSeqRec)
        Assert.Equal([ [ 'A'; 'B'; 'D' ]; [ 'A'; 'C'; 'B'; 'D' ]; [ 'A'; 'C'; 'D' ] ], pathSeqIterative)

        // Example Usage:
        let graph = Map [ 'A', [ 'B'; 'C'; 'B' ]; 'B', [ 'D' ]; 'C', [ 'B' ] ]

        let get_neighbours (g: Map<'a, 'a list>) (node: 'a) =
            g |> Map.tryFind node |> Option.defaultValue []

        let pathSeqRec =
            graphsearch.findAllPathsRecursive (graph |> get_neighbours) ('D' |> is_target) [ 'A' ]

        let pathSeqIterative =
            graphsearch.findAllPaths (graph |> get_neighbours >> Seq.ofList) ('D' |> is_target) 'A'

        // Access elements lazily
        // pathSeq |> Seq.iter (printfn "Path found: %A")
        Assert.Equal([ [ 'A'; 'B'; 'D' ]; [ 'A'; 'C'; 'B'; 'D' ]; [ 'A'; 'B'; 'D' ] ], pathSeqRec)
        Assert.Equal([ [ 'A'; 'B'; 'D' ]; [ 'A'; 'C'; 'B'; 'D' ]; [ 'A'; 'B'; 'D' ] ], pathSeqIterative)


    [<Fact>]
    let ``test getAllPaths_multigraph`` () =
        // Example Usage:
        let graph =
            Map.ofList [ "A", [ ("B", "Fast Road"); ("B", "Scenic Route") ]; "B", [ ("C", "Bridge") ] ]

        let get_neighbours (g: Map<'a, 'b list>) (node: 'a) = g |> Map.tryFind node

        let paths = graphsearch.findAllPaths_multigraph "A" "C" (graph |> get_neighbours)
        // Output:
        // [ [("B", "Fast Road"); ("C", "Bridge")];
        //   [("B", "Scenic Route"); ("C", "Bridge")] ]

        Assert.Equal<List<List<string * string>>>([ [ ("B", "Fast Road"); ("C", "Bridge") ]; [ ("B", "Scenic Route"); ("C", "Bridge") ] ], paths)

    [<Fact>]
    let ``test compressGraph`` () =
        let nodes = ["A"; "B"; "C"; "D"]
        // A - B - C - D
        let getNeighbors node =
            match node with

            | "A" -> ["B"]
            | "B" -> ["A"; "C"]
            | "C" -> ["B"; "D"]

            | "D" -> ["C"]
            | _   -> []

        let (newNodes, edges, oldToNew) = graphsearch.compressGraph nodes getNeighbors

        // Remaining merged nodes
        Assert.Equivalent(["A"; "D"], newNodes)

        // Weighted Edges
        Assert.Equivalent(Map.ofList [("A", [("D", 3)]); ("D", [("A", 3)])], edges)

        // Old to merged node mapping
        Assert.Equivalent( Map.ofList[("A", "A"); ("B", "A"); ("C", "D"); ("D", "D")],  oldToNew)


module math =
    // Greatest common divisor of two numbers
    let rec gcd<'T when 'T :> System.Numerics.INumber<'T> and 'T: equality> (a: 'T) (b: 'T) : 'T =
        match (a, b) with
        | (x, z) when z = LanguagePrimitives.GenericZero<'T> -> x
        | (z, y) when z = LanguagePrimitives.GenericZero<'T> -> y
        | (a, b) -> gcd b (a % b)

    // Least common multiple of two numbers
    // let rec lcm<'T when 'T :> System.Numerics.INumber<'T> and 'T: equality> (a: 'T) (b: 'T) : 'T = a * b / (gcd a b)
    let rec inline lcm<'T when ^T :> System.Numerics.INumber< ^T > and ^T: equality> (a: 'T) (b: 'T) : 'T = a * b / (gcd a b)

    // Least common multiple of a list of numbers
    let rec lcmList<'T when 'T :> System.Numerics.INumber<'T> and 'T: equality> (data: 'T list) : 'T =
        match data with
        | a :: b :: [] -> lcm a b
        | a :: b -> lcm a (lcmList b)
        | [] -> LanguagePrimitives.GenericOne<'T>

module math_test =
    [<Fact>]
    let ``test gcd and lcm`` () =

        Assert.Equal(12, (math.lcm 4 6))
        Assert.Equal(6L, math.lcm 2L 3L)
        Assert.Equal(60L, math.lcmList [ 2L; 3L; 4L; 5L; 6L ])

module collections =
    /// Bi directional map.
    /// It stores correspondences of two values.
    /// It yields correspond value from another value of the pair.
    type BiMap<'a, 'b when 'a: comparison and 'b: comparison>(item1s: 'a list, item2s: 'b list) =
        // reusing standard F# library's map to implement find functions
        let item1IsKey = List.zip item1s item2s |> Map.ofList
        let item2IsKey = List.zip item2s item1s |> Map.ofList
        member __.findBy1 key = Map.find key item1IsKey
        member __.tryFindBy1 key = Map.tryFind key item1IsKey
        member __.findBy2 key = Map.find key item2IsKey
        member __.tryFindBy2 key = Map.tryFind key item2IsKey
        member __.Length() = item1s.Length

    // all_pairs [1;2;3;4] |> Seq.toList;;
    let rec all_pairs l =
        seq {
            match l with
            | h :: t ->
                for e in t do
                    yield h, e

                yield! all_pairs t
            | _ -> ()
        }

    /// C# Dictionary to F# Map
    let toMap dictionary =
        (dictionary :> seq<_>) |> Seq.map (|KeyValue|) |> Map.ofSeq

    // Merge multiple maps, handling collisions by creating a list
    let mergeMaps maps =
        let addOrAppend map key value =
            map
            |> Map.change key (function
                | Some list -> Some(value :: list)
                | None -> Some [ value ])

        let folder acc map = Map.fold addOrAppend acc map

        List.fold folder Map.empty maps
    // Folder function to handle collisions by creating a list of values
    let multiListToMap acc (key, value) =
        match Map.tryFind key acc with
        | Some existingValues -> Map.add key (value :: existingValues) acc
        | None -> Map.add key [ value ] acc

module collections_test =
    [<Fact>]
    let ``test bimap`` () =
        let keys = [ 0; 1; 2; 3; 4 ]
        let vals = [ "zero"; "one"; "two"; "three"; "four" ]
        let bm = collections.BiMap(keys, vals)
        Assert.Equal(Some(1), bm.tryFindBy2 "one")
        Assert.Equal(None, bm.tryFindBy2 "five")
        Assert.Equal(Some("two"), bm.tryFindBy1 2)
        Assert.Equal(None, bm.tryFindBy1 5)

        Assert.Throws<System.Collections.Generic.KeyNotFoundException>(fun () -> bm.findBy1 (5) :> obj)
        |> ignore

        Assert.Throws<System.Collections.Generic.KeyNotFoundException>(fun () -> bm.findBy2 ("five") :> obj)
        |> ignore

        Assert.Equal(Some(4), bm.tryFindBy2 "four")
        Assert.Equal(Some("four"), bm.tryFindBy1 4)
        Assert.Equal(4, bm.findBy2 "four")
        Assert.Equal("four", bm.findBy1 4)

    [<Fact>]
    let ``test all_pairs`` () =
        let data = [ 1; 2; 3; 4 ]
        let expected = [ (1, 2); (1, 3); (1, 4); (2, 3); (2, 4); (3, 4) ]
        Assert.Equivalent(expected, collections.all_pairs data)

    [<Fact>]
    let ``test mergeMaps`` () =
        let list1 = Map [ "a", 1; "b", 2 ]
        let list2 = Map [ "a", 3; "c", 4 ]
        let list3 = Map [ "b", 5; "a", 6 ]
        let merged = collections.mergeMaps [ list1; list2; list3 ]
        let expected = Map.ofList [ ("a", [ 6; 3; 1 ]); ("b", [ 5; 2 ]); ("c", [ 4 ]) ]
        Assert.Equivalent(expected, merged)

    [<Fact>]
    let ``test multiListToMap`` () =
        let data = [ ("a", 1); ("b", 2); ("a", 3); ("c", 4); ("b", 5) ]
        // Fold over the list of pairs to create the map
        let finalMap =
            data
            |> List.fold collections.multiListToMap Map.empty
            |> Map.map (fun _ v -> List.rev v) // Optional: reverse to keep original order

        let expected = Map.ofList [ ("a", [ 1; 3 ]); ("b", [ 2; 5 ]); ("c", [ 4 ]) ]
        Assert.Equivalent(expected, finalMap)

// Result: Map [("a", [1; 3]); ("b", [2; 5]); ("c", [4])]
module dayxxtest =
    type Marker = interface end

    let public Solve () = printfn "Solver from dayxxtest"
