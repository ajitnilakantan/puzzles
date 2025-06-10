module aoc2023.day13

open System

type internal Marker =
    interface
    end

// Manacher's Algorithm - Finding all sub-palindromes in O(N)
// The version dealing with odd length lines is simpler
let manacher (line: int list) : int list =
    // Insert dummy value between each element.  1;2;3 -> x;1;x;2;x;3;x
    let sentinel3 = List.max line + 3

    let line =
        [ sentinel3 ]
        @ (line
           |> List.collect (fun x -> [ x ] @ [ sentinel3 ]))
    // Process the "padded" list
    let len = line.Length
    let sentinel1 = List.max line + 1
    let sentinel2 = List.max line + 2
    let line = [ sentinel1 ] @ line @ [ sentinel2 ]
    let pos = Array.zeroCreate line.Length
    let mutable l, r = 0, 1

    for i in 1..len do
        pos[i] <- max 0 (min (r - i) pos[l + (r - i)])

        while line[i - pos[i]] = line[i + pos[i]] do
            pos[i] <- pos[i] + 1

        if i + pos[i] > r then
            l <- i - pos[i]
            r <- i + pos[i]

    // List<pos, size>
    let ret = pos[1..len] |> List.ofArray
    ret
// let max_pos = ret |> Array.indexed |> Array.maxBy snd |> fst
// Return a tuple (pos, size)
// If "size" is even, then the mispoint of the palindrome is between "pos-1" and "pos"
// If "size" is odd, the oddsized palindrome is centred at "pos"
// if max_pos % 2 = 1 then
//    // Max vlaue at position ((max_pos - 1) / 2) of length ret[max_pos]-1
//    ((max_pos - 1) / 2), ret[max_pos] - 1
// else
//    // Max value just before position (max_pos / 2) of length ret[max_pos]-1
//    (max_pos / 2), ret[max_pos] - 1


let find_reflection (line: int list) : Option<int * int> =
    // Need to have full reflections -- should extend fully to at least one side.
    let is_reflection (pos: int, size: int) (len: int) : bool =
        assert (pos % 2 = 0) // Even sized palindrome

        if pos/2 + size / 2 = len || pos/2 - size / 2 = 0 then
            true
        else
            false

    let line_len = line.Length
    debug.printfn "line(%A) = %A" line_len line
    // Find the largest even pattern that touches an edge
    let sorted_line =
        line
        |> List.indexed
        |> List.sortBy snd
        |> List.filter (fun x -> fst x % 2 = 0)
    debug.printfn " sorted_line = %A" sorted_line

    let pos =
        sorted_line
        |> List.tryFind (fun x -> is_reflection x line_len)

    debug.printfn " pos = %A" pos
    // Option<pos, size>
    pos

let manacher_string (line: string) : int * int =
    let m = manacher (line |> Seq.map int |> Seq.toList)
    let max_pos = m |> List.indexed |> List.maxBy snd |> fst
    // Return a tuple (pos, size)
    // If "size" is even, then the mispoint of the palindrome is between "pos-1" and "pos"
    // If "size" is odd, the oddsized palindrome is centred at "pos"
    if max_pos % 2 = 1 then
        // Max vlaue at position ((max_pos - 1) / 2) of length ret[max_pos]-1
        (max_pos - 1) / 2, m[max_pos] - 1
    else
        // Max value just before position (max_pos / 2) of length ret[max_pos]-1
        max_pos / 2, m[max_pos] - 1


let parse_chunk (chunk: string list) =
    let grid = gridio.read_grid chunk false '.'
    let width, height = grid.GetLength 1, grid.GetLength 0
    // Map . to 0 and # to 1
    let rows =
        [ for index in 0 .. height - 1 ->
              Convert.ToInt32(
                  grid[index, *]
                  |> Seq.map (fun x -> if x = '.' then '0' else '1')
                  |> Array.ofSeq
                  |> String,
                  2
              ) ]

    let cols =
        [ for index in 0 .. width - 1 ->
              Convert.ToInt32(
                  grid[*, index]
                  |> Seq.map (fun x -> if x = '.' then '0' else '1')
                  |> Array.ofSeq
                  |> String,
                  2
              ) ]

    rows, cols


let SolvePart1 data =
    let mutable solution = 0
    let chunks = data |> fileio.chunkLines

    for index, chunk in chunks |> List.indexed do
        let rows, cols = parse_chunk chunk

        let row_reflection = find_reflection rows
        let col_reflection = find_reflection cols

        solution <-
            solution
            + match col_reflection with
              | Some (_, n) -> n
              | _ -> 0

        solution <-
            solution
            + match row_reflection with
              | Some (_, n) -> n * 100
              | _ -> 0

    solution

let SolvePart2 data =
    let solution = 0
    solution

let public Solve () =
    printfn $"Solve from {typeof<Marker>.DeclaringType}"
    let data = fileio.linesFromFile "day13.txt"

    let stopWatch = System.Diagnostics.Stopwatch.StartNew()
    let solution = SolvePart1 data
    printfn "Part1 = %A" solution
    stopWatch.Stop()
    printfn "Elapsed time %ims" stopWatch.ElapsedMilliseconds
    assert (0 = solution) // 57124 too high

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
        "#.##..##.\n\
         ..#.##.#.\n\
         ##......#\n\
         ##......#\n\
         ..#.##.#.\n\
         ..##..##.\n\
         #.#.##.#.\n\
         \n\
         #...##..#\n\
         #....#..#\n\
         ..##..###\n\
         #####.##.\n\
         #####.##.\n\
         ..##..###\n\
         #....#..#"

    [<Fact>]
    let ``Test manacher`` () =
        // let data = fileio.linesFromString data
        Assert.Equal(data.Length, data.Length)
        // grids = parse_data data
        let test_data =
            [ "abcbczz", (2, 3)
              "xaabcdaaaafg", (8, 4)
              "a12344321z", (5, 8)
              "a1234321z", (4, 7)
              "a11z", (2, 2)
              "a111z", (2, 3)
              "ab11z", (3, 2)
              "ab111z", (3, 3) ]

        for line, (expected_pos, expected_size) in test_data do
            let pos, size = manacher_string line
            Assert.Equal((expected_pos, expected_size), (pos, size))
        //debug.printfn "line=%A pos=%A size=%A" line pos size

        let res =
            ".#.##"
            |> Seq.map (fun x -> if x = '.' then '0' else '1')
            |> Array.ofSeq
            |> String

        Assert.Equal("01011", res)

        let res =
            Convert.ToInt32(
                ".#.##"
                |> Seq.map (fun x -> if x = '.' then '0' else '1')
                |> Array.ofSeq
                |> String,
                2
            )

        Assert.Equal(11, res)

    [<Fact>]
    let ``Test Part1`` () =
        // Convert int list to list of zero padded binary strings
        //let toBinary (x: int list) (width: int) =
        //    x
        //    |> List.map (fun x -> Convert.ToString(x, 2).PadLeft(width, '0'))

        let lines = fileio.linesFromString data
        let chunks = lines |> fileio.chunkLines

        let mutable sum = 0

        for index, chunk in chunks |> List.indexed do
            let rows, cols = parse_chunk chunk

            let row_reflection = find_reflection rows
            let col_reflection = find_reflection cols

            sum <-
                sum
                + match col_reflection with
                  | Some (_, n) -> n
                  | _ -> 0

            sum <-
                sum
                + match row_reflection with
                  | Some (_, n) -> n * 100
                  | _ -> 0

        Assert.Equal(405, sum)
