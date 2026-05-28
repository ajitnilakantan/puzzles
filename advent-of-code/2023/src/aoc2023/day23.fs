module aoc2023.day23

type internal Marker = interface end

let get_neighbours (grid: char array2d) (coord: int * int) =
    let width, height = grid.GetLength 1, grid.GetLength 0
    let r, c = coord

    let neighbours =
        match grid[r, c] with
        | '^' -> [ r - 1, c ]
        | '>' -> [ r, c + 1 ]
        | 'v' -> [ r + 1, c ]
        | '<' -> [ r, c - 1 ]
        | '#' -> []
        | _ -> [ r - 1, c; r + 1, c; r, c - 1; r, c + 1 ]
    // Limit to grid and avoid walls
    let neighbours =
        neighbours
        |> List.filter (fun (rr, cc) -> rr >= 0 && rr < height && cc >= 0 && cc < width && grid[rr, cc] <> '#')

    neighbours

// For part 2, ignore the slopes
let get_neighbours2 (grid: char array2d) (coord: int * int) =
    let width, height = grid.GetLength 1, grid.GetLength 0
    let r, c = coord

    let neighbours = [ r - 1, c; r + 1, c; r, c - 1; r, c + 1 ]
    // Limit to grid and avoid walls
    let neighbours =
        neighbours
        |> List.filter (fun (r, c) -> r >= 0 && r < height && c >= 0 && c < width && grid[r, c] <> '#')

    neighbours

// for part 2 - longest path in and island from previous to next island
//  islandPath is the path through the islands from the the start to goal.
//  index is the index within the path that we are evaluating
let longest_island_path (grid, (islandPath: int list), index, start, goal, bridges, nodeToIslandMap, islandToNodesMap) =
    let start =
        if index = 0 then
            start
        else
            // List.pick & Map.find can throw KeyNotFoundException on error. Should never happen.
            bridges
            |> List.pick (fun (from, too) ->
                if
                    (nodeToIslandMap |> Map.find from = islandPath[index - 1])
                    && (nodeToIslandMap |> Map.find too = islandPath[index])
                then
                    Some(too)
                elif
                    (nodeToIslandMap |> Map.find from = islandPath[index])
                    && (nodeToIslandMap |> Map.find too = islandPath[index - 1])
                then
                    Some(from)
                else
                    None)

    let goal =
        if index = (islandPath |> List.length) - 1 then
            goal
        else
            // List.pick & Map.find can throw KeyNotFoundException on error. Should never happen.
            bridges
            |> List.pick (fun (from, too) ->
                if
                    (nodeToIslandMap |> Map.find from = islandPath[index])
                    && (nodeToIslandMap |> Map.find too = islandPath[index + 1])
                then
                    Some(from)
                elif
                    (nodeToIslandMap |> Map.find from = islandPath[index + 1])
                    && (nodeToIslandMap |> Map.find too = islandPath[index])
                then
                    Some(too)
                else
                    None)

    assert (nodeToIslandMap |> Map.find start = islandPath[index])
    assert (nodeToIslandMap |> Map.find goal = islandPath[index])

    let get_neighbours (grid: char array2d) (island: int) (nodeToIslandMap) (coord: int * int) =
        // Same as get_neighbours2 but we restrict to the current island
        get_neighbours2 grid coord
        |> List.filter (fun c -> nodeToIslandMap |> Map.find c = island)

    let is_target (goal: 'a) (node: 'a) = goal = node
    // Get all paths
    let paths =
        graphsearch.findAllPaths ((grid, islandPath[index], nodeToIslandMap) |||> get_neighbours >> Seq.ofList) (goal |> is_target) start

    // Subtract 1 for number of steps from number of nodes
    let path_lengths = paths |> Seq.map (fun p -> (p |> Seq.length) - 1) |> List.ofSeq

    let max_path = if path_lengths = [] then 0 else path_lengths |> List.max
    max_path

let SolvePart1 data =
    let grid = gridio.read_grid data false '.'
    let width, height = grid.GetLength 1, grid.GetLength 0
    let start = (0, 1) // r,c
    let goal = (height - 1, width - 2) // r,c

    assert ('.' = grid[fst start, snd start])
    assert ('.' = grid[fst goal, snd goal])

    let is_target (ggoal: 'a) (nnode: 'a) = ggoal = nnode

    let paths =
        graphsearch.findAllPaths (grid |> get_neighbours >> Seq.ofList) (goal |> is_target) start
    // Subtract 1 for number of steps from number of nodes
    let path_lengths = paths |> Seq.map (fun p -> (p |> Seq.length) - 1) |> List.ofSeq

    let solution = path_lengths |> List.max
    solution

let SolvePart2 data =
    let grid = gridio.read_grid data false '.'
    let width, height = grid.GetLength 1, grid.GetLength 0
    let start = (0, 1) // r,c
    let goal = (height - 1, width - 2) // r,c

    assert ('.' = grid[fst start, snd start])
    assert ('.' = grid[fst goal, snd goal])

    (*    
    // This works, but is super-slow!
    let is_target (goal: 'a) (node: 'a) = goal = node
    // let paths = graphsearch.findAllPathsIterative (grid |> get_neighbours2) (goal |> is_target) start
    let paths =  graphsearch.findLongestPathIterative (grid |> get_neighbours2 >> List.map(fun x -> x, 1) >> Seq.ofList) (goal |> is_target) start
    // Subtract 1 for number of steps from number of nodes
    let path_lengths = paths |> Seq.map (fun p -> (p |> Seq.length) - 1) |> List.ofSeq

    let solution = path_lengths |> List.max
    solution
    *)

    let nodes =
        grid
        |> Array2D.mapi (fun r c v -> r, c, v) // Map to include indices
        |> Seq.cast<int * int * char> // Flatten to a 1D sequence of (r, c, v) tuples
        |> Seq.filter (fun (r, c, v) -> v <> '#') // Filter all the open spaces
        |> Seq.map (fun (r, c, _v) -> r, c)
        |> List.ofSeq


    let newNodes, edges, oldToNew =
        graphsearch.compressGraph nodes (grid |> get_neighbours2)

    let is_target (goal: 'a) (node: 'a) = goal = node
    let get_neighbours edges node = edges |> Map.find node
    // Get all paths
    let paths =
        graphsearch.findAllPathsWithWeights2 (edges |> get_neighbours >> Seq.ofList) (goal |> is_target) start

    let solution = snd (paths |> Seq.maxBy snd)
    solution


let public Solve () =
    printfn $"Solve from {typeof<Marker>.DeclaringType}"
    let data = fileio.linesFromFile "day23.txt"

    let stopWatch = System.Diagnostics.Stopwatch.StartNew()
    let solution = SolvePart1 data
    printfn "Part1 = %A" solution
    stopWatch.Stop()
    printfn "Elapsed time %ims" stopWatch.ElapsedMilliseconds
    assert (2170 = solution)

    let stopWatch = System.Diagnostics.Stopwatch.StartNew()
    let solution = SolvePart2 data
    printfn "Part2 = %A" solution
    stopWatch.Stop()
    printfn "Elapsed time %ims" stopWatch.ElapsedMilliseconds
    assert (6502 = solution)

// #################################### //
open Xunit

type Tests() =
    let data =
        "#.#####################\n\
         #.......#########...###\n\
         #######.#########.#.###\n\
         ###.....#.>.>.###.#.###\n\
         ###v#####.#v#.###.#.###\n\
         ###.>...#.#.#.....#...#\n\
         ###v###.#.#.#########.#\n\
         ###...#.#.#.......#...#\n\
         #####.#.#.#######.#.###\n\
         #.....#.#.#.......#...#\n\
         #.#####.#.#.#########v#\n\
         #.#...#...#...###...>.#\n\
         #.#.#v#######v###.###v#\n\
         #...#.>.#...>.>.#.###.#\n\
         #####v#.#.###v#.#.###.#\n\
         #.....#...#...#.#.#...#\n\
         #.#########.###.#.#.###\n\
         #...###...#...#...#.###\n\
         ###.###.#.###v#####v###\n\
         #...#...#.#.>.>.#.>.###\n\
         #.###.###.#.###.#.#v###\n\
         #.....###...###...#...#\n\
         #####################.#"

    [<Fact>]
    let ``Test Part1`` () =
        let data = fileio.linesFromString data
        let grid = gridio.read_grid data false '.'
        let width, height = grid.GetLength 1, grid.GetLength 0
        let start = (0, 1) // r,c
        let goal = (height - 1, width - 2) // r,c
        Assert.Equal('.', grid[fst start, snd start])
        Assert.Equal('.', grid[fst goal, snd goal])

        let is_target (goal: 'a) (node: 'a) = goal = node

        let paths =
            graphsearch.findAllPaths (grid |> get_neighbours >> Seq.ofList) (goal |> is_target) start
        // Subtract 1 for number of steps from number of nodes
        let path_lengths = paths |> Seq.map (fun p -> (p |> Seq.length) - 1) |> List.ofSeq

        Assert.Equivalent([ 90; 74; 82; 86; 94; 82 ], path_lengths)
        let solution = path_lengths |> List.max
        Assert.Equal(94, solution)

    [<Fact>]
    let ``Test Part2`` () =
        let data = fileio.linesFromString data
        let grid = gridio.read_grid data false '.'
        let width, height = grid.GetLength 1, grid.GetLength 0
        let start = (0, 1) // r,c
        let goal = (height - 1, width - 2) // r,c
        Assert.Equal('.', grid[fst start, snd start])
        Assert.Equal('.', grid[fst goal, snd goal])

        let is_target (goal: 'a) (node: 'a) = goal = node

        let paths =
            graphsearch.findAllPaths (grid |> get_neighbours2 >> Seq.ofList) (goal |> is_target) start
        // Subtract 1 for number of steps from number of nodes
        let path_lengths = paths |> Seq.map (fun p -> (p |> Seq.length) - 1) |> List.ofSeq

        let solution = path_lengths |> List.max
        Assert.Equal(154, solution)

    [<Fact>]
    let ``Test Part2 Tarjan`` () =
        let data = fileio.linesFromString data
        let grid = gridio.read_grid data false '.'
        let width, height = grid.GetLength 1, grid.GetLength 0
        let start = (0, 1) // r,c
        let goal = (height - 1, width - 2) // r,c
        Assert.Equal('.', grid[fst start, snd start])
        Assert.Equal('.', grid[fst goal, snd goal])


        let nodes =
            grid
            |> Array2D.mapi (fun r c v -> r, c, v) // Map to include indices
            |> Seq.cast<int * int * char> // Flatten to a 1D sequence of (r, c, v) tuples
            |> Seq.filter (fun (r, c, v) -> v <> '#') // Filter all the open spaces
            |> Seq.map (fun (r, c, _v) -> r, c)

        let bridges, nodeToIslandMap =
            tarjan.partitionGraph nodes (grid |> get_neighbours2 >> Seq.ofList)

        /////////
        // Small test. Combine multiple values to Map of key/list values
        let a = [ 0, 1; 2, 3; 0, 9 ]

        let m =
            a
            |> List.map (fun (k, v) -> k, [ v ])
            |> List.groupBy fst
            |> List.map (fun (key, values) -> key, values |> List.collect snd)
            |> Map.ofList

        Assert.Equivalent(Map.ofList [ (0, [ 1; 9 ]); (2, [ 3 ]) ], m)
        // Small test: invert a nodeToIslandMap to an islandToNodesMap
        let _node_to_island = Map.ofList [ 0, 0; 1, 1; 2, 0; 3, 1; 5, 1; 10, 10 ]

        let _island_to_nodes =
            _node_to_island
            |> Map.toSeq
            |> Seq.map (fun (n, i) -> i, [ n ])
            |> Seq.groupBy fst
            |> Seq.map (fun (i, ns) -> i, (ns |> Seq.map snd |> List.concat))
            |> Map.ofSeq
        // alternative: ins = ni |> Map.toSeq |> Seq.groupBy snd |> Seq.map(fun (i, ns) -> i, ns|> Seq.map fst);;
        Assert.Equivalent(Map.ofList [ (0, [ 0; 2 ]); (1, [ 1; 3; 5 ]); (10, [ 10 ]) ], _island_to_nodes)
        // The other way around: island_to_nodes -> node_to_island
        let _island_to_nodes =
            Map.ofList [ 0, [ 1; 2; 3 ]; 1, [ 5; 6 ]; 10, [ 11; 12; 13 ] ]

        let _node_to_island =
            _island_to_nodes
            |> Map.toSeq
            |> Seq.map (fun (index, vals) -> vals |> List.map (fun v -> v, index))
            |> List.concat
            |> Map.ofList

        Assert.Equivalent(Map.ofList [ (1, 0); (2, 0); (3, 0); (5, 1); (6, 1); (11, 10); (12, 10); (13, 10) ], _node_to_island)
        /////////

        // Create a metaGraph of the "islands". The bridges are the edges
        // map <island_index to set [nodes]>
        let islandToNodesMap =
            nodeToIslandMap
            |> Map.toSeq
            |> Seq.map (fun (n, i) -> i, [ n ])
            |> Seq.groupBy fst
            |> Seq.map (fun (island, nodes) -> island, nodes |> Seq.map snd |> List.concat)
            |> Map.ofSeq

        // All island node indices. Looks like metaNodes=set [0; 1; 2]
        let metaNodes = islandToNodesMap |> Map.keys |> Set.ofSeq
        // Map of island to connected island(s)
        let metaEdges =
            bridges
            |> List.map (fun bridge ->
                let fromIsland = nodeToIslandMap |> Map.find (fst bridge)
                let toIsland = nodeToIslandMap |> Map.find (snd bridge)
                [ fromIsland, toIsland; toIsland, fromIsland ])
            |> List.concat // List of bidirectional (fromIsland,toIsland)

        let metaEdges =
            metaEdges
            |> List.map (fun (k, v) -> k, [ v ])
            |> List.groupBy fst
            |> List.map (fun (key, values) -> key, values |> List.collect snd)
            |> Map.ofList // Map of islandNode -> [edge list]

        // Find all paths from start to goal on the islands
        let metaStart = nodeToIslandMap |> Map.find start // r,c
        let metaGoal = nodeToIslandMap |> Map.find goal // r,c
        let is_target (goal: 'a) (node: 'a) = goal = node

        let get_neighbours (edges: Map<int, int list>) (node: int) =
            edges |> Map.tryFind node |> Option.defaultValue []


        let paths =
            graphsearch.findAllPaths (metaEdges |> get_neighbours >> Seq.ofList) (metaGoal |> is_target) metaStart

        // memoize function
        let longest_island_path = itertools.memoize longest_island_path

        let paths_lengths =
            paths
            |> Seq.map (fun path ->
                path
                |> List.mapi (fun index _p -> longest_island_path (grid, path, index, start, goal, bridges, nodeToIslandMap, islandToNodesMap)))
        // Add path.length - 1 to include steps between islands
        let sums =
            paths_lengths
            |> Seq.map (fun path -> (path |> List.sum) + (path |> List.length) - 1)
            |> List.ofSeq

        let max_sum = sums |> List.max
        Assert.Equal(154, max_sum)

    [<Fact>]
    let ``Test Part2 Compress Graph`` () =
        let data = fileio.linesFromString data
        let grid = gridio.read_grid data false '.'
        let width, height = grid.GetLength 1, grid.GetLength 0
        let start = (0, 1) // r,c
        let goal = (height - 1, width - 2) // r,c
        Assert.Equal('.', grid[fst start, snd start])
        Assert.Equal('.', grid[fst goal, snd goal])

        let nodes =
            grid
            |> Array2D.mapi (fun r c v -> r, c, v) // Map to include indices
            |> Seq.cast<int * int * char> // Flatten to a 1D sequence of (r, c, v) tuples
            |> Seq.filter (fun (r, c, v) -> v <> '#') // Filter all the open spaces
            |> Seq.map (fun (r, c, _v) -> r, c)
            |> List.ofSeq


        let newNodes, edges, oldToNew =
            graphsearch.compressGraph nodes (grid |> get_neighbours2)

        Assert.True(oldToNew |> Map.find start = start)
        Assert.True(oldToNew |> Map.find goal = goal)
        let is_target (goal: 'a) (node: 'a) = goal = node
        let get_neighbours edges node = edges |> Map.find node
        // Get all paths
        let paths =
            graphsearch.findAllPathsWithWeights (edges |> get_neighbours >> Seq.ofList) (goal |> is_target) start

        let solution = snd (paths |> Seq.maxBy snd)
        Assert.Equal(154, solution)
