module aoc2023.day08

type internal Marker =
    interface
    end

let parse_data (data: string list) =
    assert (data.Length > 2)
    let paths = data[0]

    let splitopts =
        System.StringSplitOptions.TrimEntries
        ||| System.StringSplitOptions.RemoveEmptyEntries

    let tokens =
        data
        |> List.skip 2
        |> List.map (fun (x: string) -> x.Split([| '\x20'; '='; ','; '('; ')' |], splitopts))

    let left = tokens |> List.map (fun x -> x[0], x[1])
    let right = tokens |> List.map (fun x -> x[0], x[2])
    let left = left |> Map.ofSeq
    let right = right |> Map.ofSeq
    paths, left, right

let count_path_length start goal (paths: string) left right =
    let len: int64 = paths.Length
    let mutable counter: int64 = 0
    let mutable current = start

    while current <> goal do
        let mymap = if paths[int (counter % len)] = 'L' then left else right
        current <- mymap |> Map.find current
        counter <- counter + 1L

    counter

let count_path_length_for_ghosts (start: string) (goal: string) (paths: string) left right =
    let len: int64 = paths.Length
    let mutable counter: int64 = 0
    let mutable finished = false

    let mutable current: string list =
        left
        |> Map.keys
        |> Seq.filter (fun (x: string) -> x.EndsWith(start))
        |> Seq.toList

    let mutable goal_positions: int64 array = Array.zeroCreate<int64> current.Length

    while not finished
          && not (
              current
              |> List.forall (fun (x: string) -> x.EndsWith(goal))
          ) do
        let mymap: Map<string, string> =
            if paths[int (counter % len)] = 'L' then left else right


        current <-
            (current
             |> List.map (fun (x: string) -> mymap |> Map.find x))

        counter <- counter + 1L

        current
        |> List.iteri (fun index x -> if x.EndsWith(goal) && goal_positions[index] = 0 then goal_positions[index] <- counter)

        if goal_positions |> Array.forall (fun x -> x <> 0) then
            finished <- true
            // Calculate the LCM of the positions when each "ghost" ended up in a goal position
            let goal_positions = goal_positions |> Array.toList

            // If the goal is found too soon, add the length of the search pattern to make
            // the module operations correct for the LCM later
            let goal_positions =
                goal_positions
                |> List.map (fun x -> if x < len then x + len else x)

            counter <- math.lcmList (goal_positions)

    counter

let SolvePart1 data =
    let paths, left, right = parse_data data
    let solution = count_path_length "AAA" "ZZZ" paths left right
    solution

let SolvePart2 data =
    let paths, left, right = parse_data data
    let solution = count_path_length_for_ghosts "A" "Z" paths left right
    solution

let public Solve () =
    printfn $"Solve from {typeof<Marker>.DeclaringType}"
    let data = fileio.linesFromFile "day08.txt"

    let stopWatch = System.Diagnostics.Stopwatch.StartNew()
    let solution = SolvePart1 data
    printfn "Part1 = %A" solution
    stopWatch.Stop()
    printfn "Elapsed time %ims" stopWatch.ElapsedMilliseconds
    assert (19199L = solution)

    let stopWatch = System.Diagnostics.Stopwatch.StartNew()
    let solution = SolvePart2 data
    printfn "Part2 = %A" solution
    stopWatch.Stop()
    printfn "Elapsed time %ims" stopWatch.ElapsedMilliseconds
    assert (13663968099527L = solution)

// #################################### //
open Xunit

type Tests() =
    let data =
        "RL\n\
         \n\
         AAA = (BBB, CCC)\n\
         BBB = (DDD, EEE)\n\
         CCC = (ZZZ, GGG)\n\
         DDD = (DDD, DDD)\n\
         EEE = (EEE, EEE)\n\
         GGG = (GGG, GGG)\n\
         ZZZ = (ZZZ, ZZZ)"

    let data2 =
        "LLR\n\
         \n\
         AAA = (BBB, BBB)\n\
         BBB = (AAA, ZZZ)\n\
         ZZZ = (ZZZ, ZZZ)"

    let data3 =
        "LR\n\
         \n\
         11A = (11B, XXX)\n\
         11B = (XXX, 11Z)\n\
         11Z = (11B, XXX)\n\
         22A = (22B, XXX)\n\
         22B = (22C, 22C)\n\
         22C = (22Z, 22Z)\n\
         22Z = (22B, 22B)\n\
         XXX = (XXX, XXX)"

    [<Fact>]
    let ``Test Part1`` () =
        let data = fileio.linesFromString data
        let paths, left, right = parse_data data
        Assert.Equal("RL", paths)
        let path_len = count_path_length "AAA" "ZZZ" paths left right
        Assert.Equal(2L, path_len)

        let data2 = fileio.linesFromString data2
        let paths, left, right = parse_data data2
        Assert.Equal("LLR", paths)
        let path_len = count_path_length "AAA" "ZZZ" paths left right
        Assert.Equal(6L, path_len)

    [<Fact>]
    let ``Check distinct keys`` () =
        // All keys should be distinct, otherwise there is ambiguity on where to go next
        let data = fileio.linesFromFile "day08.txt"

        let splitopts =
            System.StringSplitOptions.TrimEntries
            ||| System.StringSplitOptions.RemoveEmptyEntries

        let keys =
            data
            |> List.skip 2
            |> List.map (fun (x: string) -> x.Split([| '\x20'; '='; ','; '('; ')' |], splitopts))
            |> List.map (fun x -> x[0])

        let distinct_keys = keys |> List.distinct
        Assert.Equal(keys.Length, distinct_keys.Length)

    [<Fact>]
    let ``Test Part2`` () =
        let data = fileio.linesFromString data3
        let paths, left, right = parse_data data
        Assert.Equal("LR", paths)
        let path_len = count_path_length_for_ghosts "A" "Z" paths left right
        Assert.Equal(6L, path_len)
