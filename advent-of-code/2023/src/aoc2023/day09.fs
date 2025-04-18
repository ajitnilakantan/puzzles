module aoc2023.day09

type internal Marker =
    interface
    end


let parse_data data =
    fileio.tokenize data "\x20" |> List.map (int64)

let find_differences line =
    // find successive differences.  The result list is one less in length
    // e.g.    3   6  10  15  21  28 => 2   3   4   5   6   7
    // e.g.    2   3   4   5   6   7 => 1   1   1   1   1
    // e.g.    1   1   1   1   1     => 0   0   0   0
    line
    |> List.pairwise
    |> List.map (fun x -> snd x - fst x)

let find_all_differences line =
    // Keep applying find_differences until all values are zero
    let mutable current = line
    let mutable all_differences: List<int64 list> = [ current ]

    while current |> List.exists (fun x -> x <> 0) do
        current <- find_differences current
        all_differences <- all_differences @ [ current ]

    // Calculate the upper right value
    let next_value_upper_right =
        all_differences
        |> List.sumBy (fun x -> x |> List.last)

    // Calculate the upper left value. Alternate subtract/add
    let next_value_upper_left =
        all_differences
        |> List.indexed
        |> List.sumBy (fun (ind, x) ->
            (List.head x)
            * (if (ind % 2) = 0 then 1L else -1L))

    all_differences, next_value_upper_right, next_value_upper_left


let SolvePart1 data =
    let solution =
        data
        |> List.map (parse_data)
        |> List.map (fun x -> find_all_differences x)
        |> List.sumBy (fun (_, x, _) -> x)

    solution

let SolvePart2 data =
    let solution =
        data
        |> List.map (parse_data)
        |> List.map (fun x -> find_all_differences x)
        |> List.sumBy (fun (_, _, x) -> x)

    solution

let public Solve () =
    printfn $"Solve from {typeof<Marker>.DeclaringType}"
    let data = fileio.linesFromFile "day09.txt"

    let stopWatch = System.Diagnostics.Stopwatch.StartNew()
    let solution = SolvePart1 data
    printfn "Part1 = %A" solution
    stopWatch.Stop()
    printfn "Elapsed time %ims" stopWatch.ElapsedMilliseconds
    assert (1641934234L = solution)

    let stopWatch = System.Diagnostics.Stopwatch.StartNew()
    let solution = SolvePart2 data
    printfn "Part2 = %A" solution
    stopWatch.Stop()
    printfn "Elapsed time %ims" stopWatch.ElapsedMilliseconds
    assert (975L = solution)

// #################################### //
open Xunit

type Tests() =

    let data =
        "0 3 6 9 12 15\n\
         1 3 6 10 15 21\n\
         10 13 16 21 30 45"

    [<Fact>]
    let ``Test Part1`` () =
        let data = fileio.linesFromString data

        let next_values =
            data
            |> List.map (parse_data)
            |> List.map (fun x -> find_all_differences x)
            |> List.map (fun (_, x, _) -> x)

        Assert.Equivalent([ 18L; 28L; 68L ], next_values)

        let solution =
            data
            |> List.map (parse_data)
            |> List.map (fun x -> find_all_differences x)
            |> List.sumBy (fun (_, x, _) -> x)

        Assert.Equal(114L, solution)

    [<Fact>]
    let ``Test Part2`` () =
        let data = fileio.linesFromString data

        let differences =
            data
            |> List.map (parse_data)
            |> List.map (fun x -> find_all_differences x)

        let prev_values = differences |> List.map (fun (_, _, x) -> x)
        // debug.printfn "PREV = %A" prev_values
        Assert.Equivalent([ -3L; 0L; 5L ], prev_values)
        let solution = differences |> List.sumBy (fun (_, _, x) -> x)
        Assert.Equal(2L, solution)
