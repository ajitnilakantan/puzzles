module aoc2023.day11

type internal Marker =
    interface
    end

let parse_grid (grid: char [,]) =
    let height, width = grid.GetLength(0), grid.GetLength(1)
    let mutable galaxy_number = []
    let mutable positions = []
    let mutable count = 1

    // Count galaxies
    for y in 0 .. (height - 1) do
        for x in 0 .. (width - 1) do
            if grid[y, x] = '#' then
                galaxy_number <- galaxy_number @ [ count ]
                positions <- positions @ [ (y, x) ]
                count <- count + 1

    // Count empty rows/cols. e.g. a result of [0;0;1;1;1;2;2;2] means that there are
    //  empty (galaxy-free) rows/cols at            2 and 5 - where the cound bumps up.
    // E.g. Grid = [['.'; '.'; '#']
    //              ['#'; '.'; '.']]  row_count: [0; 0] col_count: [0; 0; 1]
    let row i (arr: 'T [,]) =
        arr.[i..i, *] |> Seq.cast<'T> |> Seq.toList

    let col i (arr: 'T [,]) =
        arr.[*, i..i] |> Seq.cast<'T> |> Seq.toList

    let row_count, _ =
        [ 0 .. height - 1 ]
        |> List.map (fun x -> row x grid)
        |> List.map (fun r -> r |> List.forall (fun z -> z = '.'))
        |> List.mapFold (fun acc x -> if x then (acc + 1, acc + 1) else (acc, acc)) 0

    let col_count, _ =
        [ 0 .. width - 1 ]
        |> List.map (fun x -> col x grid)
        |> List.map (fun r -> r |> List.forall (fun z -> z = '.'))
        |> List.mapFold (fun acc x -> if x then (acc + 1, acc + 1) else (acc, acc)) 0

    collections.BiMap(galaxy_number, positions), row_count, col_count

let distance_between p q (galaxy_map: collections.BiMap<int, int * int>) (row_count: int list) (col_count: int list) (expansion: int) =
    let py, px = galaxy_map.findBy1 p
    let qy, qx = galaxy_map.findBy1 q
    let expansion = int64 expansion

    let distance: int64 =
        int64 (abs (qx - px))
        + int64 (abs (qy - py))
        + (expansion - 1L)
          * int64 (abs (row_count[qy] - row_count[py]))
        + (expansion - 1L)
          * int64 (abs (col_count[qx] - col_count[px]))

    distance

let find_distance_sum (galaxy_map: collections.BiMap<int, int * int>) (row_count: int list) (col_count: int list) (expansion: int) =
    let num_galaxies = galaxy_map.Length()
    let mutable sum = 0L

    for p, q in collections.all_pairs [ 1..num_galaxies ] do
        sum <-
            sum
            + distance_between p q galaxy_map row_count col_count expansion

    sum

let SolvePart1 data =
    let grid = gridio.read_grid data false '.'
    let galaxy_map, row_count, col_count = parse_grid grid
    let solution = find_distance_sum galaxy_map row_count col_count 2
    solution

let SolvePart2 data =
    let grid = gridio.read_grid data false '.'
    let galaxy_map, row_count, col_count = parse_grid grid
    let solution = find_distance_sum galaxy_map row_count col_count 1_000_000
    solution

let public Solve () =
    printfn $"Solve from {typeof<Marker>.DeclaringType}"
    let data = fileio.linesFromFile "day11.txt"

    let stopWatch = System.Diagnostics.Stopwatch.StartNew()
    let solution = SolvePart1 data
    printfn "Part1 = %A" solution
    stopWatch.Stop()
    printfn "Elapsed time %ims" stopWatch.ElapsedMilliseconds
    assert (9509330L = solution)

    let stopWatch = System.Diagnostics.Stopwatch.StartNew()
    let solution = SolvePart2 data
    printfn "Part2 = %A" solution
    stopWatch.Stop()
    printfn "Elapsed time %ims" stopWatch.ElapsedMilliseconds
    assert (635832237682L = solution)

// #################################### //
open Xunit

type Tests() =
    let data =
        "...#......\n\
         .......#..\n\
         #.........\n\
         ..........\n\
         ......#...\n\
         .#........\n\
         .........#\n\
         ..........\n\
         .......#..\n\
         #...#....."

    [<Fact>]
    let ``Test Part1`` () =
        let data = fileio.linesFromString data
        let grid = gridio.read_grid data false '.'
        let galaxy_map, row_count, col_count = parse_grid grid

        let d = distance_between 5 9 galaxy_map row_count col_count 2
        Assert.Equal(9L, d)
        let d = distance_between 1 7 galaxy_map row_count col_count 2
        Assert.Equal(15L, d)
        let d = distance_between 3 6 galaxy_map row_count col_count 2
        Assert.Equal(17L, d)
        let d = distance_between 8 9 galaxy_map row_count col_count 2
        Assert.Equal(5L, d)

        let solution = find_distance_sum galaxy_map row_count col_count 2
        Assert.Equal(374L, solution)

    [<Fact>]
    let ``Test Part2`` () =
        let data = fileio.linesFromString data
        let grid = gridio.read_grid data false '.'

        let galaxy_map, row_count, col_count = parse_grid grid
        let solution = find_distance_sum galaxy_map row_count col_count 10
        Assert.Equal(1030L, solution)

        let galaxy_map, row_count, col_count = parse_grid grid
        let solution = find_distance_sum galaxy_map row_count col_count 100
        Assert.Equal(8410L, solution)
