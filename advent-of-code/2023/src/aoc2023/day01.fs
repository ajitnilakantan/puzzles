module aoc2023.day01

open System.Text.RegularExpressions

type internal Marker =
    interface
    end

let firstRegex = Regex(@"(\d)", RegexOptions.Compiled)

let firstDigit str =
    let m = firstRegex.Matches(str) //, @"(\d)")

    if m.Count = 0 then None else Some(m[0].Value |> int)

let lastDigit str =
    let m = Regex.Matches(str, @"(\d)(?!.*(\d))")

    if m.Count = 0 then None else Some(m[0].Value |> int)

let digits =
    Map<string, int>
        [ ("0", 0)
          ("1", 1)
          ("2", 2)
          ("3", 3)
          ("4", 4)
          ("5", 5)
          ("6", 6)
          ("7", 7)
          ("8", 8)
          ("9", 9)
          ("zero", 0)
          ("one", 1)
          ("two", 2)
          ("three", 3)
          ("four", 4)
          ("five", 5)
          ("six", 6)
          ("seven", 7)
          ("eight", 8)
          ("nine", 9) ]

let firstDigitV2 str =
    let pat = String.concat "|" digits.Keys
    let regex = $@"{pat}"
    let m = Regex.Matches(str, regex)

    if m.Count = 0 then None else Some(digits[m[0].Value])

let lastDigitV2 str =
    let pat = @"(?=(" + String.concat "|" digits.Keys + "))"
    let regex = $@"{pat}"
    let m = Regex.Matches(str, regex)

    if m.Count = 0 then
        None
    else
        Some(digits[m[m.Count - 1].Groups[1].Value])

let SolvePart1 data =
    let mySum (x, y) =
        if x <> None && y <> None then
            Some(x.Value * 10 + y.Value)
        else
            None

    data
    |> Seq.map (fun x -> (firstDigit x, lastDigit x))
    |> Seq.choose mySum
    |> Seq.sum

let SolvePart2 data =
    let mySum (x, y) =
        if x <> None && y <> None then
            Some(x.Value * 10 + y.Value)
        else
            None

    data
    |> Seq.map (fun x -> (firstDigitV2 x, lastDigitV2 x))
    |> Seq.choose mySum
    |> Seq.sum

let public Solve () =
    printfn $"Solve from {typeof<Marker>.DeclaringType}"
    let data = fileio.linesFromFile "day01.txt"

    let stopWatch = System.Diagnostics.Stopwatch.StartNew()
    let solution = SolvePart1 data
    printfn "Part1 = %A" solution
    stopWatch.Stop()
    printfn "Elapsed time %ims" stopWatch.ElapsedMilliseconds
    assert (54951 = solution)

    let stopWatch = System.Diagnostics.Stopwatch.StartNew()
    let solution = SolvePart2 data
    printfn "Part2 = %A" solution
    stopWatch.Stop()
    printfn "Elapsed time %ims" stopWatch.ElapsedMilliseconds
    assert (55218 = solution)

// #################################### //
open Xunit

[<Theory>]
[<InlineData(1, 2, "1abc2")>]
[<InlineData(3, 8, "pqr3stu8vwx")>]
[<InlineData(1, 5, "a1b2c3d4e5f")>]
[<InlineData(7, 7, "treb7uchet")>]
[<InlineData(-1, -1, "trebuchet")>]
[<InlineData(-1, -1, "a")>]
[<InlineData(-1, -1, "ab")>]
let ``regex matches`` left right str =
    match left with
    | -1 ->
        Assert.Equal(None, firstDigit str)
        Assert.Equal(None, lastDigit str)
    | _ ->
        Assert.Equal(Some left, firstDigit str)
        Assert.Equal(Some right, lastDigit str)


[<Fact>]
let ``simple test`` () =
    let data =
        @"1abc2\n\
                 pqr3stu8vwx\n\
                 a1b2c3d4e5f\n\
                 treb7uchet"

    let data = fileio.linesFromString data
    Assert.Equal(142, SolvePart1 data)

[<Fact>]
let ``simple test part2`` () =
    let data =
        "two1nine\n\
                eightwothree\n\
                abcone2threexyz\n\
                xtwone3four\n\
                4nineeightseven2\n\
                zoneight234\n\
                7pqrstsixteen"

    let data = fileio.linesFromString data
    Assert.Equal(281, SolvePart2 data)

[<Theory>]
[<InlineData(2, 3, "zztwoneighthreezz")>]
[<InlineData(8, 3, "eighthree")>]
[<InlineData(7, 9, "sevenine")>]
let ``regex matches overlapping`` left right str =
    match left with
    | -1 ->
        Assert.Equal(None, firstDigitV2 str)
        Assert.Equal(None, lastDigitV2 str)
    | _ ->
        Assert.Equal(Some left, firstDigitV2 str)
        Assert.Equal(Some right, lastDigitV2 str)
