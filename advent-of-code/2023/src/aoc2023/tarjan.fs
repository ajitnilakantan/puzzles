namespace aoc2023


(* To split a connected graph into two disjoint sets joined by exactly
one edge, you need to find a bridge (or cut-edge). An edge is a bridge
if its removal increases the number of connected components in the
graph.The standard approach is Tarjan's Bridge-Finding Algorithm,
which uses a single Depth-First Search (DFS) to identify all such
edges in \(O(V + E)\) time.F# Implementation of Tarjan's Algorithm

To identify subgraphs separated by bridges, you can perform a second
traversal after finding the bridges. By "removing" the bridges
(conceptually ignoring them during traversal), the remaining connected
nodes form 2-edge-connected components. Each of these can be assigned
a unique index.F# Implementation to Index SubgraphsThis code extends
the previous logic by grouping nodes into indexed sets based on
whether they remain connected after bridges are removed.

*)
module tarjan =


    /// <summary>
    /// Partitions a graph into islands (2-edge-connected components) using an
    /// iterative (non-recursive) Tarjan bridge-finding DFS.
    /// Improved version with explicit child tracking.
    /// </summary>
    ///
    /// Parameters:
    ///   nodes        – every node in the graph
    ///   getNeighbors – adjacency function; may return both directions for the
    ///                  same undirected edge
    ///
    /// Returns:
    ///   bridges   – list of bridge edges (u, v) whose removal disconnects the graph
    ///   islandMap – maps every node to an integer island ID; nodes that share an
    ///               ID are in the same 2-edge-connected component
    let partitionGraph (nodes: seq<'N>) (getNeighbors: 'N -> seq<'N>) : List<'N * 'N> * Map<'N, int> when 'N: comparison =
        let nodeList = nodes |> Seq.toList

        if List.isEmpty nodeList then
            ([], Map.empty)
        else
            // Build adjacency map
            let adjacency =
                nodeList |> List.map (fun n -> (n, getNeighbors n |> Seq.toList)) |> Map.ofList

            // State for Tarjan's algorithm
            let mutable visited = Set.empty<'N>
            let mutable disc = Map.empty<'N, int>
            let mutable low = Map.empty<'N, int>
            let mutable parent = Map.empty<'N, 'N option>
            let mutable children = Map.empty<'N, Set<'N>> // Track actual children in DFS tree
            let mutable time = 0
            let mutable bridgeSet = Set.empty<'N * 'N>

            // Process each connected component
            for startNode in nodeList do
                if not (Set.contains startNode visited) then
                    // Iterative DFS with explicit state tracking
                    let mutable stack = [ (startNode, None, false) ]

                    while not (List.isEmpty stack) do
                        let (current, parentOpt, returning) = List.head stack
                        stack <- List.tail stack

                        if not returning then
                            // First visit
                            if not (Set.contains current visited) then
                                visited <- Set.add current visited
                                disc <- Map.add current time disc
                                low <- Map.add current time low
                                time <- time + 1
                                parent <- Map.add current parentOpt parent

                                // Add current as child of parent
                                match parentOpt with
                                | Some p ->
                                    let currentChildren = Map.tryFind p children |> Option.defaultValue Set.empty
                                    children <- Map.add p (Set.add current currentChildren) children
                                | None -> ()

                                // Push return marker
                                stack <- (current, parentOpt, true) :: stack

                                // Push unvisited neighbors
                                let neighbors = Map.find current adjacency

                                for neighbor in neighbors do
                                    if not (Set.contains neighbor visited) then
                                        stack <- (neighbor, Some current, false) :: stack
                        else
                            // Returning - update low values
                            let currentLow = Map.find current low
                            let currentDisc = Map.find current disc
                            let myParent = Map.find current parent
                            let myChildren = Map.tryFind current children |> Option.defaultValue Set.empty
                            let mutable newLow = currentLow

                            let neighbors = Map.find current adjacency

                            for neighbor in neighbors do
                                if Set.contains neighbor visited then
                                    if Set.contains neighbor myChildren then
                                        // Neighbor is a child in DFS tree
                                        let neighborLow = Map.find neighbor low
                                        newLow <- min newLow neighborLow

                                        // Bridge condition: low[child] > disc[current]
                                        if neighborLow > currentDisc then
                                            bridgeSet <- Set.add (current, neighbor) bridgeSet
                                    elif Some neighbor <> myParent then
                                        // Back edge to non-parent ancestor
                                        let neighborDisc = Map.find neighbor disc
                                        newLow <- min newLow neighborDisc

                            low <- Map.add current newLow low

            let bridges = Set.toList bridgeSet

            // Find connected components after removing bridges
            let mutable componentMap = Map.empty<'N, int>
            let mutable componentId = 0
            let mutable componentVisited = Set.empty<'N>

            for startNode in nodeList do
                if not (Set.contains startNode componentVisited) then
                    // BFS
                    let mutable queue = [ startNode ]
                    componentVisited <- Set.add startNode componentVisited

                    while not (List.isEmpty queue) do
                        let current = List.head queue
                        queue <- List.tail queue
                        componentMap <- Map.add current componentId componentMap

                        let neighbors = Map.find current adjacency

                        for neighbor in neighbors do
                            let isBridge =
                                Set.contains (current, neighbor) bridgeSet
                                || Set.contains (neighbor, current) bridgeSet

                            if not isBridge && not (Set.contains neighbor componentVisited) then
                                componentVisited <- Set.add neighbor componentVisited
                                queue <- queue @ [ neighbor ]

                    componentId <- componentId + 1

            (bridges, componentMap)

module tarjan_test =

    open Xunit
    open tarjan

    // ── helpers ───────────────────────────────────────────────────────────────

    /// Normalise a bridge list so that edge direction and list order are
    /// irrelevant during comparison.
    let private normalizeBridges (bridges: (int * int) list) : (int * int) list =
        bridges |> List.map (fun (a, b) -> if a < b then a, b else b, a) |> List.sort

    /// Number of distinct island IDs present in the map.
    let private islandCount (m: Map<int, int>) =
        m |> Map.toList |> List.map snd |> List.distinct |> List.length

    // ── linear graph: 1 ── 2 ── 3 ────────────────────────────────────────────

    /// Neighbour function for the linear graph 1-2-3.
    let private linearNeighbors =
        function
        | 1 -> [ 2 ]
        | 2 -> [ 1; 3 ]
        | 3 -> [ 2 ]
        | _ -> []

    [<Fact>]
    let ``Linear graph – finds both bridges`` () =
        let bridges, _ = partitionGraph [ 1; 2; 3 ] (linearNeighbors >> Seq.ofList)
        let expected: (int * int) list = [ (1, 2); (2, 3) ]
        Assert.Equal<(int * int) list>(expected, normalizeBridges bridges)

    [<Fact>]
    let ``Linear graph – every node is its own island`` () =
        let _, islandMap = partitionGraph [ 1; 2; 3 ] (linearNeighbors >> Seq.ofList)
        // Three nodes, no non-bridge edges → three distinct island IDs.
        Assert.Equal(3, islandCount islandMap)
        Assert.NotEqual(islandMap.[1], islandMap.[2])
        Assert.NotEqual(islandMap.[2], islandMap.[3])
        Assert.NotEqual(islandMap.[1], islandMap.[3])

    // ── Y-shaped graph (centre = 2) ───────────────────────────────────────────
    //
    //     1
    //     |
    // 3 - 2 - 4

    /// Neighbour function for the Y-shaped graph with centre node 2.
    let private yNeighbors =
        function
        | 1 -> [ 2 ]
        | 2 -> [ 1; 3; 4 ]
        | 3 -> [ 2 ]
        | 4 -> [ 2 ]
        | _ -> []

    [<Fact>]
    let ``Y-graph – finds all three bridges`` () =
        let bridges, _ = partitionGraph [ 1; 2; 3; 4 ] (yNeighbors >> Seq.ofList)
        let expected: (int * int) list = [ (1, 2); (2, 3); (2, 4) ]
        Assert.Equal<(int * int) list>(expected, normalizeBridges bridges)

    [<Fact>]
    let ``Y-graph – every node is its own island`` () =
        let _, islandMap = partitionGraph [ 1; 2; 3; 4 ] (yNeighbors >> Seq.ofList)
        Assert.Equal(4, islandCount islandMap)
        // All four IDs must be pairwise distinct.
        let ids = [ islandMap.[1]; islandMap.[2]; islandMap.[3]; islandMap.[4] ]
        Assert.Equal(4, ids |> List.distinct |> List.length)

    // ── triangle: 1 ── 2 ── 3 ── 1  (cycle → no bridges) ─────────────────────

    let private triangleNeighbors =
        function
        | 1 -> [ 2; 3 ]
        | 2 -> [ 1; 3 ]
        | 3 -> [ 1; 2 ]
        | _ -> []

    [<Fact>]
    let ``Triangle graph – no bridges`` () =
        let bridges, _ = partitionGraph [ 1; 2; 3 ] (triangleNeighbors >> Seq.ofList)
        Assert.Empty(bridges)

    [<Fact>]
    let ``Triangle graph – all nodes share one island`` () =
        let _, islandMap = partitionGraph [ 1; 2; 3 ] (triangleNeighbors >> Seq.ofList)
        Assert.Equal(1, islandCount islandMap)
        Assert.Equal(islandMap.[1], islandMap.[2])
        Assert.Equal(islandMap.[2], islandMap.[3])

    // ── mixed graph: cycle + pendant edge  ───────────────────────────────────
    //
    //  1 ── 2 ── 4
    //  |    |
    //  └── 3
    //
    //  Triangle 1-2-3 plus pendant edge 2-4.
    //  Only (2,4) is a bridge; 1, 2, 3 share an island; 4 is alone.

    let private mixedNeighbors =
        function
        | 1 -> [ 2; 3 ]
        | 2 -> [ 1; 3; 4 ]
        | 3 -> [ 1; 2 ]
        | 4 -> [ 2 ]
        | _ -> []

    [<Fact>]
    let ``Mixed graph – only the pendant edge 2-4 is a bridge`` () =
        let bridges, _ = partitionGraph [ 1; 2; 3; 4 ] (mixedNeighbors >> Seq.ofList)
        let expected: (int * int) list = [ (2, 4) ]
        Assert.Equal<(int * int) list>(expected, normalizeBridges bridges)

    [<Fact>]
    let ``Mixed graph – cycle nodes share an island, pendant node is separate`` () =
        let _, islandMap = partitionGraph [ 1; 2; 3; 4 ] (mixedNeighbors >> Seq.ofList)
        Assert.Equal(2, islandCount islandMap)
        // 1, 2, 3 all in the same island
        Assert.Equal(islandMap.[1], islandMap.[2])
        Assert.Equal(islandMap.[2], islandMap.[3])
        // 4 is in a different island
        Assert.NotEqual(islandMap.[1], islandMap.[4])
(*
    open System.Collections.Generic

    let partitionGraph (nodes: seq<'N>) (getNeighbors: 'N -> seq<'N>) (foo : bool) : (('N * 'N) list) * (Map<'N, int>) = // where 'N : comparison =
        // Loop states are tracked using pure F# immutable Maps and Lists
        let mutable index = 0
        let mutable indices = Map.empty    // node -> discovery index
        let mutable lowLinks = Map.empty   // node -> low-link index
        let mutable onStack = Map.empty    // node -> bool
        let mutable nodeStack = []         // current DFS path nodes
        let mutable islands = Map.empty    // node -> island ID
        let mutable islandCount = 0
        let mutable bridges = []           // list of (parent, child) bridges

        for startNode in nodes do
            if not (Map.containsKey startNode indices) then
                // Explicit DFS Stack stores: (currentNode, parentNode, remainingNeighbors)
                let mutable dfsStack = [ (startNode, startNode, getNeighbors startNode |> Seq.toList) ]

                // Initialize the root node of this DFS tree
                indices <- Map.add startNode index indices
                lowLinks <- Map.add startNode index lowLinks
                onStack <- Map.add startNode true onStack
                nodeStack <- startNode :: nodeStack
                index <- index + 1

                while not (List.isEmpty dfsStack) do
                    let (u, p, neighbors) = List.head dfsStack
                    dfsStack <- List.tail dfsStack

                    match neighbors with

                    | v :: vs ->
                        // Put the current node back with its remaining tail of neighbors
                        dfsStack <- (u, p, vs) :: dfsStack

                        if v <> p then // Do not traverse backward directly to the immediate parent
                            if not (Map.containsKey v indices) then
                                // Initialize unvisited neighbor v
                                indices <- Map.add v index indices
                                lowLinks <- Map.add v index lowLinks
                                onStack <- Map.add v true onStack
                                nodeStack <- v :: nodeStack
                                index <- index + 1

                                // Push neighbor v to evaluate next
                                dfsStack <- (v, u, getNeighbors v |> Seq.toList) :: dfsStack
                            elif Map.find v onStack then
                                // Back-edge found: update low-link of u
                                let uLow = Map.find u lowLinks
                                let vIdx = Map.find v indices
                                lowLinks <- Map.add u (if vIdx < uLow then vIdx else uLow) lowLinks


                    | [] ->
                        // All neighbors of node 'u' are processed (Post-visit step)
                        if u <> p then
                            // Propagate low-link up to parent 'p'
                            let pLow = Map.find p lowLinks
                            let uLow = Map.find u lowLinks
                            lowLinks <- Map.add p (if uLow < pLow then uLow else pLow) lowLinks

                            // Bridge condition checking
                            if uLow > Map.find p indices then
                                bridges <- (p, u) :: bridges

                        // If u is a root of an island / strongly connected component
                        if Map.find u lowLinks = Map.find u indices then
                            let mutable poppedNodes = []
                            let mutable keepPopping = true

                            while keepPopping do
                                match nodeStack with
                                | x :: xs ->
                                    nodeStack <- xs
                                    onStack <- Map.add x false onStack
                                    poppedNodes <- x :: poppedNodes
                                    if x = u then keepPopping <- false
                                | [] -> keepPopping <- false

                            for node in poppedNodes do
                                islands <- Map.add node islandCount islands
                            islandCount <- islandCount + 1

        bridges, islands

module tarjan_test =
    open Xunit
    open tarjan

    // ─────────────────────────────────────────────────────────────────────────────
    // Helpers
    // ─────────────────────────────────────────────────────────────────────────────

    /// Normalise a bridge list so order doesn't affect equality checks:
    ///   • each pair is sorted  (lo, hi)
    ///   • the list itself is sorted
    let private normBridges (bs : ('N * 'N) list when 'N : comparison) =
        bs
        |> List.map (fun (a, b) -> if a <= b then (a, b) else (b, a))
        |> List.sort

    /// Group the island map into sorted sets for comparison independent of
    /// the specific integer ids assigned.
    let private islandGroups (islands : Map<'N, int>) : Set<'N> list =
        islands
        |> Map.toList
        |> List.groupBy snd
        |> List.map (fun (_, pairs) -> pairs |> List.map fst |> Set.ofList)
        |> List.sortBy (fun s -> s |> Set.minElement)  // stable sort for equality


    // ─────────────────────────────────────────────────────────────────────────────
    // Graph fixtures
    // ─────────────────────────────────────────────────────────────────────────────

    //  Linear:  A─B─C─D─E    (all edges are bridges)
    let linearNodes = seq { 'A'; 'B'; 'C'; 'D'; 'E' }
    let linearAdj = function
        | 'A' -> seq { 'B' }
        | 'B' -> seq { 'A'; 'C' }
        | 'C' -> seq { 'B'; 'D' }
        | 'D' -> seq { 'C'; 'E' }
        | 'E' -> seq { 'D' }
        | _   -> Seq.empty

    //  Y-shaped:
    //
    //    A─B─C─D─E─F
    //            │
    //            G
    //
    //  D is the junction (degree 3).  All edges are bridges.
    let yNodes = seq { 'A'; 'B'; 'C'; 'D'; 'E'; 'F'; 'G' }
    let yAdj = function
        | 'A' -> seq { 'B' }
        | 'B' -> seq { 'A'; 'C' }
        | 'C' -> seq { 'B'; 'D' }
        | 'D' -> seq { 'C'; 'E'; 'G' }
        | 'E' -> seq { 'D'; 'F' }
        | 'F' -> seq { 'E' }
        | 'G' -> seq { 'D' }
        | _   -> Seq.empty

    //  Two triangles joined by a single bridge:
    //
    //    A─B   D─E
    //    │/     ╲│
    //    C ─────  F
    //
    //  The only bridge is C─F.
    let cycleNodes = seq { 'A'; 'B'; 'C'; 'D'; 'E'; 'F' }
    let cycleAdj = function
        | 'A' -> seq { 'B'; 'C' }
        | 'B' -> seq { 'A'; 'C' }
        | 'C' -> seq { 'A'; 'B'; 'F' }
        | 'D' -> seq { 'E'; 'F' }
        | 'E' -> seq { 'D'; 'F' }
        | 'F' -> seq { 'C'; 'D'; 'E' }
        | _   -> Seq.empty

    //  Single node, no edges
    let singleNodes = seq { 42 }
    let singleAdj (_: int) = Seq.empty

    //  Two nodes connected by one edge  (A─B)
    let pairNodes = seq { 'X'; 'Y' }
    let pairAdj = function
        | 'X' -> seq { 'Y' }
        | 'Y' -> seq { 'X' }
        | _   -> Seq.empty


    // ═════════════════════════════════════════════════════════════════════════════
    // Linear graph
    // ═════════════════════════════════════════════════════════════════════════════

    [<Fact>]
    let ``Linear – raw bridges: 4 edges, one per hop`` () =
        let bridges, _ = partitionGraph linearNodes linearAdj false
        let expected = normBridges [ ('A','B'); ('B','C'); ('C','D'); ('D','E') ]
        Assert.Equal<(char*char) list>(expected, normBridges bridges)

    [<Fact>]
    let ``Linear – raw islands: every node is its own singleton`` () =
        let _, islands = partitionGraph linearNodes linearAdj false
        let expected =
            [ Set.singleton 'A'; Set.singleton 'B'; Set.singleton 'C'
              Set.singleton 'D'; Set.singleton 'E' ]
        Assert.Equal<Set<char> list>(expected, islandGroups islands)

    [<Fact>]
    let ``Linear – merge: whole chain collapses to exactly one bridge`` () =
        let bridges, _ = partitionGraph linearNodes linearAdj true
        Assert.Equal(1, bridges.Length)

    [<Fact>]
    let ``Linear – merge: the surviving bridge connects A and E`` () =
        let bridges, _ = partitionGraph linearNodes linearAdj true
        Assert.Equal<(char*char) list>(normBridges [ ('A','E') ], normBridges bridges)

    [<Fact>]
    let ``Linear – merge: island membership is unchanged`` () =
        let _, islandsRaw = partitionGraph linearNodes linearAdj false
        let _, islandsMrg = partitionGraph linearNodes linearAdj true
        Assert.Equal<Set<char> list>(islandGroups islandsRaw, islandGroups islandsMrg)


    // ═════════════════════════════════════════════════════════════════════════════
    // Y-shaped graph
    // ═════════════════════════════════════════════════════════════════════════════

    [<Fact>]
    let ``Y-graph – raw bridges: 6 edges`` () =
        let bridges, _ = partitionGraph yNodes yAdj false
        let expected =
            normBridges [ ('A','B'); ('B','C'); ('C','D'); ('D','E'); ('E','F'); ('D','G') ]
        Assert.Equal<(char*char) list>(expected, normBridges bridges)

    [<Fact>]
    let ``Y-graph – raw islands: all 7 nodes are singletons`` () =
        let _, islands = partitionGraph yNodes yAdj false
        Assert.Equal(7, islands |> Map.toList |> List.map snd |> List.distinct |> List.length)

    [<Fact>]
    let ``Y-graph – merge: exactly 3 bridges remain`` () =
        let bridges, _ = partitionGraph yNodes yAdj true
        Assert.Equal(3, bridges.Length)

    [<Fact>]
    let ``Y-graph – merge: bridges are A─D, D─F, D─G`` () =
        let bridges, _ = partitionGraph yNodes yAdj true
        let expected = normBridges [ ('A','D'); ('D','F'); ('D','G') ]
        Assert.Equal<(char*char) list>(expected, normBridges bridges)

    [<Fact>]
    let ``Y-graph – merge: island membership is unchanged`` () =
        let _, islandsRaw = partitionGraph yNodes yAdj false
        let _, islandsMrg = partitionGraph yNodes yAdj true
        Assert.Equal<Set<char> list>(islandGroups islandsRaw, islandGroups islandsMrg)


    // ═════════════════════════════════════════════════════════════════════════════
    // Two-triangles graph
    // ═════════════════════════════════════════════════════════════════════════════

    [<Fact>]
    let ``Cycles – exactly one bridge: C─F`` () =
        let bridges, _ = partitionGraph cycleNodes cycleAdj false
        Assert.Equal<(char*char) list>(normBridges [ ('C','F') ], normBridges bridges)

    [<Fact>]
    let ``Cycles – two islands: {A,B,C} and {D,E,F}`` () =
        let _, islands = partitionGraph cycleNodes cycleAdj false
        let expected = [ Set.ofList ['A';'B';'C']; Set.ofList ['D';'E';'F'] ]
        Assert.Equal<Set<char> list>(expected, islandGroups islands)

    [<Fact>]
    let ``Cycles – merge: single bridge is preserved`` () =
        let bridgesRaw, _ = partitionGraph cycleNodes cycleAdj false
        let bridgesMrg, _ = partitionGraph cycleNodes cycleAdj true
        Assert.Equal<(char*char) list>(normBridges bridgesRaw, normBridges bridgesMrg)

    [<Fact>]
    let ``Cycles – merge: islands are unchanged`` () =
        let _, islandsRaw = partitionGraph cycleNodes cycleAdj false
        let _, islandsMrg = partitionGraph cycleNodes cycleAdj true
        Assert.Equal<Set<char> list>(islandGroups islandsRaw, islandGroups islandsMrg)


    // ═════════════════════════════════════════════════════════════════════════════
    // Edge cases
    // ═════════════════════════════════════════════════════════════════════════════

    [<Fact>]
    let ``Single node – no bridges`` () =
        let bridges, _ = partitionGraph singleNodes singleAdj false
        Assert.Empty(bridges)

    [<Fact>]
    let ``Single node – exactly one island`` () =
        let _, islands = partitionGraph singleNodes singleAdj false
        Assert.Equal(1, islands |> Map.toList |> List.map snd |> List.distinct |> List.length)

    [<Fact>]
    let ``Pair – exactly one bridge X─Y`` () =
        let bridges, _ = partitionGraph pairNodes pairAdj false
        Assert.Equal<(char*char) list>(normBridges [ ('X','Y') ], normBridges bridges)

    [<Fact>]
    let ``Pair – X and Y are in different islands`` () =
        let _, islands = partitionGraph pairNodes pairAdj false
        Assert.NotEqual(islands.['X'], islands.['Y'])

    [<Fact>]
    let ``Pair – merge: bridge is preserved (no pass-through nodes)`` () =
        let bridges, _ = partitionGraph pairNodes pairAdj true
        Assert.Equal<(char*char) list>(normBridges [ ('X','Y') ], normBridges bridges)


*)
