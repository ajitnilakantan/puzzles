namespace aoc2023

module fordFulkerson =

    (* F# implementation that uses the Ford‑Fulkerson (Edmonds‑Karp) max‑flow algorithm to find a global minimum cut in an unweighted undirected graph, returning the two partitions and the cut edges. The graph is given as a list of nodes and a neighbour function. *)
    open System.Collections.Generic

    /// Partition an undirected, unweighted graph into two sets using the max‑flow/min‑cut theorem.
    /// Returns a tuple: (partitions, cutEdges) where
    ///   partitions : a list of two node lists (source side, sink side)
    ///   cutEdges   : list of edges crossing the cut, each as (sourceSideNode, sinkSideNode)
    let partitionGraph (nodes: 'a list) (getNeighbors: 'a -> 'a list) : ('a list list * ('a * 'a) list) when 'a: comparison =

        // ---------- helper: map nodes to/from integer indices ----------
        let nodeToIdx = nodes |> List.mapi (fun i n -> n, i) |> Map.ofList
        let idxToNode = nodes |> List.mapi (fun i n -> i, n) |> Map.ofList
        let n = nodes.Length

        if n < 2 then
            // trivial case: not enough nodes to cut
            ([ nodes; [] ], [])

        else
            // ---------- build initial (constant) capacity matrix ----------
            // capacities[r][c] = 1 if undirected edge exists, else 0
            // we use directed edges (both directions) with capacity 1
            let capacity = Array.init n (fun _ -> Array.zeroCreate<int> n)

            for node in nodes do
                let u = nodeToIdx.[node]

                for vNode in getNeighbors node do
                    let v = nodeToIdx.[vNode]
                    capacity.[u].[v] <- 1 // directed edge u->v
                    capacity.[v].[u] <- 1 // directed edge v->u (makes undirected)

            // ---------- BFS for edmonds‑karp (returns parent array or None) ----------
            let bfs (residual: int[][]) s t =
                let parent = Array.create n -1
                let visited = Array.create n false
                let queue = Queue<int>()
                queue.Enqueue(s)
                visited.[s] <- true

                while queue.Count > 0 && not visited.[t] do
                    let u = queue.Dequeue()

                    for v in 0 .. n - 1 do
                        if (not visited.[v]) && residual.[u].[v] > 0 then
                            visited.[v] <- true
                            parent.[v] <- u

                            if v = t then
                                // early exit
                                queue.Clear()
                            else
                                queue.Enqueue(v)

                if visited.[t] then Some parent else None

            // ---------- max‑flow between source s and sink t ----------
            // returns (flowValue, residual matrix)
            let maxFlow s t =
                // copy capacity into a mutable residual matrix
                let residual = Array.init n (fun i -> Array.copy capacity.[i])
                let mutable flow = 0
                let mutable keep = true

                while keep do
                    match bfs residual s t with
                    | None -> keep <- false
                    | Some parent ->
                        // find bottleneck
                        let mutable v = t
                        let mutable bottle = System.Int32.MaxValue

                        while v <> s do
                            let u = parent.[v]
                            bottle <- min bottle residual.[u].[v]
                            v <- u
                        // augment flow along the path
                        v <- t

                        while v <> s do
                            let u = parent.[v]
                            residual.[u].[v] <- residual.[u].[v] - bottle
                            residual.[v].[u] <- residual.[v].[u] + bottle
                            v <- u

                        flow <- flow + bottle

                flow, residual

            // ---------- source side of min cut: reachable from s in residual ----------
            let sourceSide (residual: int[][]) s =
                let visited = Array.create n false

                let rec dfs u =
                    visited.[u] <- true

                    for v in 0 .. n - 1 do
                        if (not visited.[v]) && residual.[u].[v] > 0 then dfs v

                dfs s
                visited // boolean array: true = in source side

            // ---------- global min‑cut by fixing one node as source ----------
            let sourceIdx = 0
            let mutable bestCutValue = System.Int32.MaxValue
            let mutable bestSourceSet: bool array = Array.create n false
            let mutable bestCutEdges: ('a * 'a) list = []

            for tIdx in 1 .. n - 1 do
                let flow, residual = maxFlow sourceIdx tIdx

                if flow < bestCutValue then
                    bestCutValue <- flow
                    let side = sourceSide residual sourceIdx
                    bestSourceSet <- side
                    // construct cut edges (undirected, list each edge once)
                    let cutEdges =
                        [ for u in 0 .. n - 1 do
                              if side.[u] then
                                  for v in 0 .. n - 1 do
                                      if (not side.[v]) && capacity.[u].[v] > 0 then
                                          yield (idxToNode.[u], idxToNode.[v]) ]

                    bestCutEdges <- cutEdges

            // ---------- prepare result ----------
            let sourceNodes =
                [ for i in 0 .. n - 1 do
                      if bestSourceSet.[i] then idxToNode.[i] ]

            let sinkNodes =
                [ for i in 0 .. n - 1 do
                      if not bestSourceSet.[i] then idxToNode.[i] ]

            ([ sourceNodes; sinkNodes ], bestCutEdges)

module fordFulkerson_test =

    open Xunit
    open fordFulkerson

    [<Fact>]
    let ``simple test`` () =
        let sampleNodes = [ 1; 2; 3; 4; 5; 6 ]

        let getNeighbors node =
            match node with

            | 1 -> [ 2 ]
            | 2 -> [ 1; 3 ]
            | 3 -> [ 2; 4 ]

            | 4 -> [ 3; 5 ]
            | 5 -> [ 4; 6 ]
            | 6 -> [ 5 ]
            | _ -> []

        let partitions, cuts = partitionGraph sampleNodes getNeighbors
        // Output:
        // Partitions: [[1]; [2; 3; 4; 5; 6]]
        // Cut Edges: [(1, 2)]
        Assert.Equivalent([ [ 1 ]; [ 2; 3; 4; 5; 6 ] ], partitions)
        Assert.Equivalent([ (1, 2) ], cuts)
