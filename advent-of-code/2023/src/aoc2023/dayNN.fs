module aoc2023.dayxx

type internal Marker =
    interface
    end

let SolvePart1 data =
    let solution = 0
    solution

let SolvePart2 data =
    let solution = 0
    solution

let public Solve () =
    printfn $"Solve from {typeof<Marker>.DeclaringType}"
    let data = fileio.linesFromFile "dayNN.txt"

    let stopWatch = System.Diagnostics.Stopwatch.StartNew()
    let solution = SolvePart1 data
    printfn "Part1 = %A" solution
    stopWatch.Stop()
    printfn "Elapsed time %ims" stopWatch.ElapsedMilliseconds
    assert (0 = solution)

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
        "a\n\
         b\n\
         c"

    [<Fact>]
    let ``Test Part1`` () =
        let data = fileio.linesFromString data
        Assert.Equal(3, data.Length)
