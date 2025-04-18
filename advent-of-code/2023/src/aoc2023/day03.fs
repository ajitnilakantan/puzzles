module aoc2023.day03

open System.Text.RegularExpressions

type internal Marker =
    interface
    end


let row_to_string (index: int) (grid: char array2d) =
    let row = grid.[index, *]
    let b = System.Text.Encoding.ASCII.GetBytes(row)
    System.Text.Encoding.ASCII.GetString(b)

let find_runs (grid: char array2d) =
    let width, height = grid.GetLength(1), grid.GetLength(0)
    // lines is back to the original list of rows (now with padding)
    let lines = [ for i in 0 .. height - 1 -> row_to_string i grid ]

    // Find all runs of digits
    let re = Regex(@"\d+", RegexOptions.Compiled)

    let runs =
        lines
        |> List.map (fun l -> (re.Matches(l) |> Seq.toList))

    runs

let ok_neighbour ch = System.Char.IsDigit(ch) || ch = '.'

let neighbours_of (row: int) (m: Match) =
    seq {
        // left
        yield row, m.Index - 1
        // right
        yield row, m.Index + m.Length

        for y in [ m.Index - 1 .. m.Index + m.Length ] do
            // above
            yield row - 1, y
            // below
            yield row + 1, y
    }
    |> Seq.toList

let ok_run (grid: char array2d) (index: int) (m: Match) =
    // Validate that the run is ok. No "touching" cell
    // contains a special character.
    assert (m.Value.Length = m.Groups[0].Length)
    // Check all neighbours are "ok"
    neighbours_of index m
    |> List.forall (fun (y, x) -> ok_neighbour grid[y, x])

let ok_matches (grid: char array2d) (index: int) (matches: Match list) =
    seq {
        for m in matches do
            if ok_run grid index m then yield m
    }
    |> Seq.toList

let score_runs (runs: List<List<Match>>) =
    runs
    |> List.collect (fun mc -> mc |> List.map (fun m -> m.Value |> int))
    |> List.sum

let find_gears_around_match gears (grid: char array2d) index m match_index =
    let mutable gears = gears

    // Add kv to Map. Return Map
    let add_gear gears key value =
        let mutable gears = gears

        if not (Map.containsKey key gears) then
            gears <- Map.add key Set.empty<int * int * int> gears

        let s = Set.add value (Map.find key gears)
        gears <- Map.add key s gears
        gears

    neighbours_of index m
    |> List.iter (fun (y, x) ->
        if grid[y, x] = '*' then
            gears <- add_gear gears (y, x) match_index)

    gears

let find_gears (grid: char array2d) (runs: List<List<Match>>) =
    // map coords of gear (x,y) to (row#, match#)
    let mutable gears = Map.empty<int * int, Set<int * int * int>>

    runs
    |> List.indexed
    |> List.iter (fun (index, matches) ->
        matches
        |> List.iteri (fun match_number m ->
            let match_data = index, match_number, (m.Value |> int)
            gears <- find_gears_around_match gears grid index m match_data))

    gears

let SolvePart1 data =
    let grid = gridio.read_grid data true '.'

    let runs = find_runs grid

    let bad_runs =
        runs
        |> List.indexed
        |> List.map (fun (index, matches) -> ok_matches grid index matches)

    let solution = (score_runs runs) - (score_runs bad_runs)

    solution

let SolvePart2 data =
    let grid = gridio.read_grid data true '.'

    let runs = find_runs grid

    let gears = find_gears grid runs

    // printfn "gears = %A" gears

    // let sample = Map [ (1,  Set.ofSeq [(1,2);(2,3);(4,5)]); (2,  Set.ofSeq [(10,11); (12,13)]) ];;
    // sample |> Map.map(fun k v -> v |> Set.toList |> List.map(fun (_,x) -> x) |> Seq.fold (+) 0);;
    let products =
        gears
        |> Map.filter (fun _ value -> value.Count > 1)
        |> Map.map (fun k v ->
            v
            |> Set.toList
            |> List.map (fun (_, _, x) -> x)
            |> Seq.fold (*) 1)

    let sum =
        products
        |> Map.fold (fun state key value -> state + value) 0

    sum




let public Solve () =
    printfn $"Solve from {typeof<Marker>.DeclaringType}"
    let data = fileio.linesFromFile "day03.txt"

    let stopWatch = System.Diagnostics.Stopwatch.StartNew()
    let solution = SolvePart1 data
    printfn "Part1 = %A" solution
    stopWatch.Stop()
    printfn "Elapsed time %ims" stopWatch.ElapsedMilliseconds
    assert (522726 = solution)

    let stopWatch = System.Diagnostics.Stopwatch.StartNew()
    let solution = SolvePart2 data
    printfn "Part2 = %A" (SolvePart2 data)
    stopWatch.Stop()
    printfn "Elapsed time %ims" stopWatch.ElapsedMilliseconds
    assert (81721933 = solution)

// #################################### //
open Xunit

[<Fact>]
let ``Test Part1`` () =
    let data =
        "467..114..\n\
         ...*......\n\
         ..35..633.\n\
         ......#...\n\
         617*......\n\
         .....+.58.\n\
         ..592.....\n\
         ......755.\n\
         ...$.*....\n\
         .664.598.."

    let data = fileio.linesFromString data
    let grid = gridio.read_grid data true '.'
    // Add extra padding on left and right
    Assert.Equal(".467..114...", (row_to_string 1 grid))
    Assert.Equal("..664.598...", (row_to_string 10 grid))
    // printfn "==="
    // gridio.print_grid grid (fun cell -> printf "%c" cell)
    let solution = SolvePart1 data
    Assert.Equal(4361, solution)

    let solution = SolvePart2 data

    Assert.Equal(467835, solution)
