module aoc2023.day06

type internal Marker =
    interface
    end

let parse_data (data: string list) =
    assert (data.Length = 2)

    let splitopts =
        System.StringSplitOptions.TrimEntries
        ||| System.StringSplitOptions.RemoveEmptyEntries

    let times =
        data[ 0 ].Split([| ':'; '\x20' |], splitopts)
        |> Array.toList
        |> List.skip 1
        |> List.map (int64)

    let distances =
        data[ 1 ].Split([| ':'; '\x20' |], splitopts)
        |> Array.toList
        |> List.skip 1
        |> List.map (int64)

    times, distances

let find_solutions (time: int64) (distance: int64) =
    // Solving x*(time-x) > distance
    let distance = double distance
    let time = double time
    assert (distance < (time * time) / 4.0)

    let minValue =
        0.5 * time
        - 0.5 * sqrt (time * time - 4.0 * distance)

    let maxValue =
        0.5 * time
        + 0.5 * sqrt (time * time - 4.0 * distance)


    let mutable minValue = System.Math.Ceiling(minValue) |> int64
    let mutable maxValue = System.Math.Floor(maxValue) |> int64
    // Take care of edge cases
    if minValue * ((int64 time) - minValue) = int64 distance then minValue <- minValue + 1L
    if maxValue * ((int64 time) - maxValue) = int64 distance then maxValue <- maxValue - 1L

    (minValue, maxValue)


let SolvePart1 data =
    let times, distances = parse_data data
    let results = List.map2 (fun t d -> find_solutions t d) times distances

    let solution =
        results
        |> List.map (fun x -> snd x - fst x + 1L)
        |> List.fold (*) 1L

    solution

let SolvePart2 data =
    let times, distances = parse_data data
    // Concatenate strings
    let time =
        times
        |> List.map (string)
        |> List.fold (+) ""
        |> int64

    let distance =
        distances
        |> List.map (string)
        |> List.fold (+) ""
        |> int64

    let result = find_solutions time distance

    let solution = snd result - fst result + 1L
    solution

let public Solve () =
    printfn $"Solve from {typeof<Marker>.DeclaringType}"
    let data = fileio.linesFromFile "day06.txt"

    let stopWatch = System.Diagnostics.Stopwatch.StartNew()
    let solution = SolvePart1 data
    printfn "Part1 = %A" solution
    stopWatch.Stop()
    printfn "Elapsed time %ims" stopWatch.ElapsedMilliseconds
    assert (625968L = solution)

    let stopWatch = System.Diagnostics.Stopwatch.StartNew()
    let solution = SolvePart2 data
    printfn "Part2 = %A" solution
    stopWatch.Stop()
    printfn "Elapsed time %ims" stopWatch.ElapsedMilliseconds
    assert (43663323L = solution)

// #################################### //
open Xunit

type Tests() =
    let data =
        "Time:      7  15   30\n\
         Distance:  9  40  200"

    [<Fact>]
    let ``Test Part1`` () =
        let data = fileio.linesFromString data
        let times, distances = parse_data data

        Assert.Equivalent([ 7; 15 ], times)
        Assert.Equivalent([ 9; 40 ], distances)
        let result = find_solutions times[0] distances[0]
        Assert.Equal((2L, 5L), result)
        let results = List.map2 (fun t d -> find_solutions t d) times distances

        let solution =
            results
            |> List.map (fun x -> snd x - fst x + 1L)
            |> List.fold (*) 1L

        Assert.Equal(288L, solution)
