module aoc2023.day25

type internal Marker = interface end

let add_to_map (key: 'a) (value: 'b) (map: Map<'a, Set<'b>>) =
    let mutable map = map
    if not (Map.containsKey key map) then map <- map |> Map.add key Set.empty
    let v = map |> Map.find key
    map <- map |> Map.add key (v |> Set.add value)
    map

let parse_data lines =
    let mutable edges: Map<string, Set<string>> = Map.empty

    lines
    |> List.map (fun line -> fileio.tokenize line ":\x20")
    |> List.iter (fun tokens ->
        assert (tokens |> List.length > 1)
        let head = tokens.Head // Returns 1
        let tail = tokens.Tail // Returns [2; 3; 4; 5]

        tail
        |> List.iter (fun t ->
            edges <- add_to_map head t edges
            edges <- add_to_map t head edges))

    let nodes = edges |> Map.keys |> List.ofSeq
    nodes, edges

let SolvePart1 data =
    let nodes, edges = parse_data data

    let getNeighbors edges node =
        match edges |> Map.tryFind node with
        | None -> []
        | Some x -> x |> Set.toList

    let partitions, cuts = fordFulkerson.partitionGraph nodes (edges |> getNeighbors)

    let solution =
        partitions |> List.map List.length |> List.fold (fun acc x -> acc * x) 1

    solution

let SolvePart2 data =
    let solution = 0
    solution

let public Solve () =
    printfn $"Solve from {typeof<Marker>.DeclaringType}"
    let data = fileio.linesFromFile "day25.txt"

    let stopWatch = System.Diagnostics.Stopwatch.StartNew()
    let solution = SolvePart1 data
    printfn "Part1 = %A" solution
    stopWatch.Stop()
    printfn "Elapsed time %ims" stopWatch.ElapsedMilliseconds
    assert (591890 = solution)

    let stopWatch = System.Diagnostics.Stopwatch.StartNew()
    let solution = SolvePart2 data
    printfn "Part2 = %A" solution
    stopWatch.Stop()
    printfn "Elapsed time %ims" stopWatch.ElapsedMilliseconds
    assert (0 = solution)

// #################################### //
open Xunit

type Tests() =
    let data =
        "jqt: rhn xhk nvd\n\
         rsh: frs pzl lsr\n\
         xhk: hfx\n\
         cmg: qnr nvd lhk bvb\n\
         rhn: xhk bvb hfx\n\
         bvb: xhk hfx\n\
         pzl: lsr hfx nvd\n\
         qnr: nvd\n\
         ntq: jqt hfx bvb xhk\n\
         nvd: lhk\n\
         lsr: lhk\n\
         rzs: qnr cmg lsr rsh\n\
         frs: qnr lhk lsr"

    [<Fact>]
    let ``Test Part1`` () =
        let data = fileio.linesFromString data
        let nodes, edges = parse_data data

        let getNeighbors edges node =
            match edges |> Map.tryFind node with
            | None -> []
            | Some x -> x |> Set.toList

        let partitions, cuts = fordFulkerson.partitionGraph nodes (edges |> getNeighbors)
        Assert.Equal(2, partitions |> List.length)
        Assert.Equal(3, cuts |> List.length)

        let solution =
            partitions |> List.map List.length |> List.fold (fun acc x -> acc * x) 1

        Assert.Equal(54, solution)
