module aoc2023.day13

open System

type internal Marker = interface end

// Manacher's Algorithm - Finding all sub-palindromes in O(N)
// The version dealing with odd length lines is simpler
let manacher (line: int seq) : int list =
    // Insert dummy value between each element.  1;2;3 -> x;1;x;2;x;3;x
    let sentinel3 = Seq.max line + 3

    let line =
        [ sentinel3 ]
        @ (line |> Seq.collect (fun x -> [ x ] @ [ sentinel3 ]) |> Seq.toList)
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

    let ret = pos[1..len] |> List.ofArray
    ret


// Return postion,size of reflection.
let find_reflection (line: int seq) : list<int * int> =
    // Need to have full reflections -- should extend fully to at least one side.
    let is_reflection (pos: int, size: int) (len: int) : bool =
        assert (pos % 2 = 0) // Even sized palindrome

        if pos / 2 + size / 2 = len || pos / 2 - size / 2 = 0 then true else false

    let line_len = line |> Seq.length
    let palindromes = manacher line

    // Find the largest even pattern that touches an edge
    let sorted_line =
        palindromes
        |> List.indexed
        |> List.sortByDescending snd
        |> List.filter (fun x -> fst x % 2 = 0 && snd x > 1)

    let pos = sorted_line |> List.filter (fun x -> is_reflection x line_len)

    // [(pos, size)]
    pos

// product a b |> Seq.iter (fun x -> printf "%A " x);;
let product xl yl =
    seq {
        for x in xl do
            for y in yl do
                yield x, y
    }

let toggle_and_find_reflections
    (width: int)
    (height: int)
    (rows: int array)
    (cols: int array)
    (row_index: int)
    (col_index: int)
    : list<int * int> * list<int * int> =

    // Toggle "smudge" in row and col
    let rtmp, ctmp = rows[row_index], cols[col_index]

    rows[row_index] <- rows[row_index] ^^^ (1 <<< (width - 1 - col_index))

    cols[col_index] <- cols[col_index] ^^^ (1 <<< (height - 1 - row_index))

    let ret = find_reflection rows, find_reflection cols
    rows[row_index] <- rtmp
    cols[col_index] <- ctmp
    ret

let find_reflection2
    (width: int)
    (height: int)
    (rows: int seq)
    (cols: int seq)
    (original_row_reflection: Option<int * int>)
    (original_col_reflection: Option<int * int>)
    : Option<int * int> * Option<int * int> =
    let row_reflection = find_reflection rows
    let col_reflection = find_reflection cols
    let rows = rows |> Seq.toArray
    let cols = cols |> Seq.toArray

    assert (original_row_reflection <> None || original_col_reflection <> None)

    let found =
        product [ 0 .. height - 1 ] [ 0 .. width - 1 ]
        |> Seq.pick (fun (row_index, col_index) ->
            // Find first set of reflections that is different
            let r_ref, c_ref =
                toggle_and_find_reflections width height rows cols row_index col_index

            let f =
                r_ref |> Seq.tryFind (fun x -> Some x <> original_row_reflection), c_ref |> Seq.tryFind (fun x -> Some x <> original_col_reflection)

            if f = (None, None) then None else Some f)

    found


let manacher_string (line: string) : int * int =
    let m = manacher (line |> Seq.map int |> Seq.toList)
    let max_pos = m |> List.indexed |> List.maxBy snd |> fst
    // Return a tuple (pos, size)
    // If "size" is even, then the mispoint of the palindrome is between "pos-1" and "pos"
    // If "size" is odd, the oddsized palindrome is centred at "pos"
    if max_pos % 2 = 1 then
        // Max value at position ((max_pos - 1) / 2) of length ret[max_pos]-1
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

    for chunk in chunks do
        let rows, cols = parse_chunk chunk

        let row_reflection = find_reflection rows
        let col_reflection = find_reflection cols

        solution <-
            solution
            + match col_reflection with
              | [ (n, _) ] -> n / 2
              | _ -> 0

        solution <-
            solution
            + match row_reflection with
              | [ (n, _) ] -> n / 2 * 100
              | _ -> 0

    solution

let SolvePart2 data =
    let mutable solution = 0
    let chunks = data |> fileio.chunkLines

    for chunk in chunks do
        let rows, cols = parse_chunk chunk

        let row_reflection = find_reflection rows
        let col_reflection = find_reflection cols
        assert (row_reflection.Length <= 1 && col_reflection.Length <= 1)

        let row_reflection =
            if row_reflection.Length = 0 then None else Some row_reflection[0]

        let col_reflection =
            if col_reflection.Length = 0 then None else Some col_reflection[0]

        let ret =
            find_reflection2 cols.Length rows.Length rows cols row_reflection col_reflection

        solution <-
            solution
            + match ret with
              | None, None -> 0
              | Some(r, _), Some(c, _) -> r / 2 * 100 + c / 2
              | Some(r, _), None -> r / 2 * 100
              | None, Some(c, _) -> c / 2

    solution

let public Solve () =
    printfn $"Solve from {typeof<Marker>.DeclaringType}"
    let data = fileio.linesFromFile "day13.txt"

    let stopWatch = System.Diagnostics.Stopwatch.StartNew()
    let solution = SolvePart1 data
    printfn "Part1 = %A" solution
    stopWatch.Stop()
    printfn "Elapsed time %ims" stopWatch.ElapsedMilliseconds
    assert (32723 = solution)

    let stopWatch = System.Diagnostics.Stopwatch.StartNew()
    let solution = SolvePart2 data
    printfn "Part2 = %A" solution
    stopWatch.Stop()
    printfn "Elapsed time %ims" stopWatch.ElapsedMilliseconds
    assert (34536 = solution)

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
    let ``Test Reflection`` () =
        // Convert int list to list of zero padded binary strings
        //let toBinary (x: int list) (width: int) =
        //    x
        //    |> List.map (fun x -> Convert.ToString(x, 2).PadLeft(width, '0'))

        let lines = fileio.linesFromString data
        let chunks = lines |> fileio.chunkLines

        Assert.Equal(2, chunks.Length)


        let chunk = chunks[0]
        let rows, cols = parse_chunk chunk
        Assert.Equivalent([ 358; 90; 385; 385; 90; 102; 346 ], rows)
        Assert.Equivalent([ 89; 24; 103; 66; 37; 37; 66; 103; 24 ], cols)

        let row_reflection = find_reflection rows
        let col_reflection = find_reflection cols
        assert (row_reflection.Length <= 1 && col_reflection.Length <= 1)

        let row_reflection =
            if row_reflection.Length = 0 then None else Some row_reflection[0]

        let col_reflection =
            if col_reflection.Length = 0 then None else Some col_reflection[0]

        Assert.Equal(None, row_reflection)
        Assert.Equal(Some(10, 9), col_reflection)

        let chunk = chunks[1]
        let rows, cols = parse_chunk chunk
        Assert.Equivalent([ 281; 265; 103; 502; 502; 103; 265 ], rows)
        Assert.Equivalent([ 109; 12; 30; 30; 76; 97; 30; 30; 115 ], cols)

        let row_reflection = find_reflection rows
        let col_reflection = find_reflection cols
        assert (row_reflection.Length <= 1 && col_reflection.Length <= 1)

        let row_reflection =
            if row_reflection.Length = 0 then None else Some row_reflection[0]

        let col_reflection =
            if col_reflection.Length = 0 then None else Some col_reflection[0]

        Assert.Equal(Some(8, 7), row_reflection)
        Assert.Equal(None, col_reflection)


    [<Fact>]
    let ``Test Part1`` () =
        // Convert int list to list of zero padded binary strings
        //let toBinary (x: int list) (width: int) =
        //    x
        //    |> List.map (fun x -> Convert.ToString(x, 2).PadLeft(width, '0'))

        let lines = fileio.linesFromString data
        let chunks = lines |> fileio.chunkLines

        let mutable sum = 0

        for chunk in chunks do
            let rows, cols = parse_chunk chunk

            let row_reflection = find_reflection rows
            let col_reflection = find_reflection cols
            // In Part1 there is at most 1 reflection
            assert (row_reflection.Length <= 1 && col_reflection.Length <= 1)

            let row_reflection =
                if row_reflection.Length = 0 then None else Some row_reflection[0]

            let col_reflection =
                if col_reflection.Length = 0 then None else Some col_reflection[0]

            sum <-
                sum
                + match col_reflection with
                  | Some(n, _) -> n / 2
                  | _ -> 0

            sum <-
                sum
                + match row_reflection with
                  | Some(n, _) -> n / 2 * 100
                  | _ -> 0

        Assert.Equal(405, sum)

    [<Fact>]
    let ``Test Part2`` () =
        // Convert int list to list of zero padded binary strings
        //let toBinary (x: int list) (width: int) =
        //    x
        //    |> List.map (fun x -> Convert.ToString(x, 2).PadLeft(width, '0'))

        let lines = fileio.linesFromString data
        let chunks = lines |> fileio.chunkLines

        let mutable sum = 0

        Assert.Equal(2, chunks.Length)

        let mutable sum = 0

        let chunk = chunks[0]
        let rows, cols = parse_chunk chunk
        Assert.Equivalent([ 358; 90; 385; 385; 90; 102; 346 ], rows)
        Assert.Equivalent([ 89; 24; 103; 66; 37; 37; 66; 103; 24 ], cols)

        let row_reflection = find_reflection rows
        let col_reflection = find_reflection cols
        // In Part1 there is at most 1 reflection
        assert (row_reflection.Length <= 1 && col_reflection.Length <= 1)

        let row_reflection =
            if row_reflection.Length = 0 then None else Some row_reflection[0]

        let col_reflection =
            if col_reflection.Length = 0 then None else Some col_reflection[0]

        Assert.Equal(None, row_reflection)
        Assert.Equal(Some(10, 9), col_reflection)

        let ret =
            find_reflection2 cols.Length rows.Length rows cols row_reflection col_reflection

        sum <-
            sum
            + match ret with
              | None, None -> 0
              | Some(r, _), Some(c, _) -> r / 2 * 100 + c / 2
              | Some(r, _), None -> r / 2 * 100
              | None, Some(c, _) -> c / 2

        let chunk = chunks[1]
        let rows, cols = parse_chunk chunk
        Assert.Equivalent([ 281; 265; 103; 502; 502; 103; 265 ], rows)
        Assert.Equivalent([ 109; 12; 30; 30; 76; 97; 30; 30; 115 ], cols)

        let row_reflection = find_reflection rows
        let col_reflection = find_reflection cols
        // In Part1 there is at most 1 reflection
        assert (row_reflection.Length <= 1 && col_reflection.Length <= 1)

        let row_reflection =
            if row_reflection.Length = 0 then None else Some row_reflection[0]

        let col_reflection =
            if col_reflection.Length = 0 then None else Some col_reflection[0]

        Assert.Equal(Some(8, 7), row_reflection)
        Assert.Equal(None, col_reflection)

        let ret =
            find_reflection2 cols.Length rows.Length rows cols row_reflection col_reflection

        sum <-
            sum
            + match ret with
              | None, None -> 0
              | Some(r, _), Some(c, _) -> r / 2 * 100 + c / 2
              | Some(r, _), None -> r / 2 * 100
              | None, Some(c, _) -> c / 2

        Assert.Equal(400, sum)

    [<Fact>]
    let ``Test Part3`` () =
        let data =
            ".##..##.#.#.#.#..\n\
             ...##........####\n\
             #.####.#.##.##...\n\
             ........#.#######\n\
             ###..###...####..\n\
             #..##..#.##......\n\
             ........##.#..#..\n\
             #.#..#.####....##\n\
             #..##..#.##.#...#\n\
             #......#.#.###.##\n\
             ...##...#.##..###\n\
             ##.##.##..#......\n\
             ..####..##.#.##..\n\
             ..#..#....###.###\n\
             ..####....###..##\n\
             ###..###.###.....\n\
             ........#####.#.."

        let lines = fileio.linesFromString data
        let chunks = lines |> fileio.chunkLines

        let mutable sum = 0

        let chunk = chunks[0]
        let rows, cols = parse_chunk chunk

        let row_reflection = find_reflection rows
        let col_reflection = find_reflection cols
        // In Part1 there is at most 1 reflection
        assert (row_reflection.Length <= 1 && col_reflection.Length <= 1)

        let row_reflection =
            if row_reflection.Length = 0 then None else Some row_reflection[0]

        let col_reflection =
            if col_reflection.Length = 0 then None else Some col_reflection[0]

        let ret =
            find_reflection2 cols.Length rows.Length rows cols row_reflection col_reflection

        ()
