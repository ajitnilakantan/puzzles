module aoc2023.day14

type internal Marker =
    interface
    end

type Direction =
    | N = 0
    | W = 1
    | S = 2
    | E = 3

// Calculate the sum of the "stones" in a column. The rows are numbered N..1.  This is used only for North facing results
let sum_column width height (stones: Map<int, int>) : int =
    stones
    |> Map.toSeq
    |> Seq.map (fun (wall, count) ->
        // inclusive range
        let o_top = wall + 1
        let o_bottom = o_top + count - 1

        [ height - o_top .. -1 .. height - o_bottom ]
        |> List.sum)
    |> Seq.sum

/// Get the row,col positions of the stones in the given direction
let get_stones (stones: Map<int, int> list) (dir: Direction) : (int * int) seq =
    seq {
        for col, column in stones |> Seq.indexed do
            for wall, count in column |> Map.toSeq do
                match dir with
                | Direction.N ->
                    let o_top = wall + 1 // inclusive range
                    let o_bottom = o_top + count - 1
                    yield! [ for index in o_top..o_bottom -> (index, col) ]
                | Direction.W ->
                    let o_top = wall + 1 // inclusive range
                    let o_bottom = o_top + count - 1
                    yield! [ for index in o_top..o_bottom -> (col, index) ]
                | Direction.S ->
                    let o_top = wall - 1 // inclusive range
                    let o_bottom = o_top - count + 1
                    yield! [ for index in o_top .. -1 .. o_bottom -> (index, col) ]
                | Direction.E ->
                    let o_top = wall - 1 // inclusive range
                    let o_bottom = o_top - count + 1
                    yield! [ for index in o_top .. -1 .. o_bottom -> (col, index) ]
                | _ -> failwith "Invalid direction"
    }
// Create a new board by filling in columns
let grid_from_columns (grid: char array2d) (columns: Map<int, int> list) (dir: Direction) : char array2d =
    let width, height = grid.GetLength 1, grid.GetLength 0

    let newgrid =
        Array2D.init width height (fun row col -> if grid[row, col] = '#' then '#' else '.')

    let stone_coords = get_stones columns dir

    stone_coords
    |> Seq.iter (fun (row, col) -> newgrid[row, col] <- 'O')

    newgrid

// Find the "N" weigh score for the given columns
let score_columns (grid: char array2d) (columns: Map<int, int> list) (dir: Direction) =
    let width, height = grid.GetLength 1, grid.GetLength 0
    let mutable score = 0

    grid_from_columns grid columns dir
    |> Array2D.iteri (fun row col v -> if v = 'O' then score <- score + (height - row))

    score

// Parse the nth column.  Return a map of "wall" positions (position of #) and
// the number of "stones" (O characters) below it.  The very first wall is initialized to -1
let parse_column_north (grid: char array2d) (col: int) : Map<int, int> =
    let width, height = grid.GetLength 1, grid.GetLength 0
    assert (col >= 0 && col < width)
    let mutable walls = Map.empty<int, int> // count of stones below current_wall
    let mutable current_wall = -1
    walls <- walls |> Map.add current_wall 0 // Initialize count

    [ 0 .. height - 1 ]
    |> Seq.iter (fun row ->
        match grid[row, col] with
        | '#' ->
            current_wall <- row
            walls <- walls |> Map.add current_wall 0
        | 'O' ->
            walls <-
                walls
                |> Map.add current_wall (1 + (walls |> Map.find current_wall))
        | _ -> ())

    walls


// For each grid (c,r) coordinate, map the "#" that it is under. For the grids facing N,E,S,W we
// orient all so that the coordinate system is as if it is facing north. I.e. the direction the
// board is "tilted" in is the very top row.
let calc_coord_to_index (grid: char array2d) = // return 4-tuple of grids
    let width, height = grid.GetLength 1, grid.GetLength 0
    let gn = Array2D.init height width (fun _ _ -> 0)
    let gw = Array2D.init height width (fun _ _ -> 0)
    let gs = Array2D.init height width (fun _ _ -> 0)
    let ge = Array2D.init height width (fun _ _ -> 0)

    // gn
    [ 0 .. width - 1 ] // "columns"
    |> Seq.iter (fun col ->
        let mutable current_wall = -1

        [ 0 .. height - 1 ]
        |> Seq.iter (fun row ->
            if grid[row, col] = '#' then current_wall <- row
            gn[row, col] <- current_wall))
    // gw
    [ 0 .. height - 1 ] // "columns"
    |> Seq.iter (fun row ->
        let mutable current_wall = -1

        [ 0 .. width - 1 ]
        |> Seq.iter (fun col ->
            if grid[row, col] = '#' then current_wall <- col
            gw[row, col] <- current_wall))
    // gs
    [ 0 .. width - 1 ] // "columns"
    |> Seq.iter (fun col ->
        let mutable current_wall = height

        [ height - 1 .. -1 .. 0 ]
        |> Seq.iter (fun row ->
            if grid[row, col] = '#' then current_wall <- row
            gs[row, col] <- current_wall))
    // ge
    [ 0 .. height - 1 ] // "columns"
    |> Seq.iter (fun row ->
        let mutable current_wall = width

        [ width - 1 .. -1 .. 0 ]
        |> Seq.iter (fun col ->
            if grid[row, col] = '#' then current_wall <- col
            ge[row, col] <- current_wall))

    gn, gw, gs, ge



/// <summary>Create new columns based on the stone positions</summary>
let fill_map (numcolumns: int) (g: int array2d) (coords: (int * int) seq) (dir: Direction) : Map<int, int> list =
    let map = [| for _ in [ 1..numcolumns ] -> Map.empty<int, int> |] // E -> N

    coords
    |> Seq.iter (fun (row, col) ->
        let wall = g[row, col]

        match dir with
        | Direction.N
        | Direction.S -> // By columns
            match map[col] |> Map.tryFind wall with
            | Some n -> map[col] <- map[col] |> Map.add wall (n + 1)
            | None -> map[col] <- map[col] |> Map.add wall 1
        | Direction.W
        | Direction.E -> // By rows
            match map[row] |> Map.tryFind wall with
            | Some n -> map[row] <- map[row] |> Map.add wall (n + 1)
            | None -> map[row] <- map[row] |> Map.add wall 1
        | _ -> failwith "Invalid direction")

    map |> Array.toList

// Go through the cycle of N->W->S->E
let perform_cycle (grid: char array2d) (rounds: int) : Map<int, int> list =
    let gn, gw, gs, ge = calc_coord_to_index grid
    let width, height = grid.GetLength 1, grid.GetLength 0

    // Initialize to "N"
    let mutable columns =
        [ 0 .. width - 1 ]
        |> List.map (fun col -> col |> parse_column_north grid)

    let mutable cache =
        Map.empty<Map<int, int> list * Direction, Map<int, int> list * int> // hash -> columns, round

    let mutable round = 0
    let mutable ok = true
    let mutable zzz = 15

    while ok && round < rounds do
        for dir, prevdir, numcolumns, g in
            [ Direction.N, Direction.E, width, gn
              Direction.W, Direction.N, height, gw
              Direction.S, Direction.W, width, gs
              Direction.E, Direction.S, height, ge ] do
            let mutable newcolumns = [] // = [||]

            match cache |> Map.tryFind (columns, dir) with
            | Some (c, lastround) ->
                newcolumns <- c

                if dir = Direction.E
                   && round <> lastround
                   && (rounds - round - 1) % (round - lastround) = 0 then

                    ok <- false

            | None ->
                if round = 0 && dir = Direction.N then
                    // Bootstrap cache
                    newcolumns <-
                        [ 0 .. width - 1 ]
                        |> List.map (fun col -> col |> parse_column_north grid)
                else
                    let stone_coords = get_stones columns prevdir
                    newcolumns <- fill_map numcolumns g stone_coords dir

                    cache <-
                        cache
                        |> Map.add (columns, dir) (newcolumns, round)

            columns <- newcolumns // for dir loop

        round <- round + 1 // while loop

    columns



let parse_data (data: string list) : char array2d =
    let grid = gridio.read_grid data false '.'
    // let width, height = grid.GetLength 1, grid.GetLength 0
    grid


let SolvePart1 data =
    let grid = parse_data data
    let width, height = grid.GetLength 1, grid.GetLength 0

    let solution =
        [ 0 .. width - 1 ]
        |> List.map (fun col ->
            col
            |> parse_column_north grid
            |> sum_column width height)
        |> List.sum

    solution

let SolvePart2 data =
    let grid = parse_data data
    let finalcolumns = perform_cycle grid 1000000000
    let solution = score_columns grid finalcolumns Direction.E

    solution

let public Solve () =
    printfn $"Solve from {typeof<Marker>.DeclaringType}"
    let data = fileio.linesFromFile "day14.txt"

    let stopWatch = System.Diagnostics.Stopwatch.StartNew()
    let solution = SolvePart1 data
    printfn "Part1 = %A" solution
    stopWatch.Stop()
    printfn "Elapsed time %ims" stopWatch.ElapsedMilliseconds
    assert (109665 = solution)

    let stopWatch = System.Diagnostics.Stopwatch.StartNew()
    let solution = SolvePart2 data
    printfn "Part2 = %A" solution
    stopWatch.Stop()
    printfn "Elapsed time %ims" stopWatch.ElapsedMilliseconds
    assert (96061 = solution)

// #################################### //
open Xunit

type Tests() =
    let data =
        "O....#....\n\
         O.OO#....#\n\
         .....##...\n\
         OO.#O....O\n\
         .O.....O#.\n\
         O.#..O.#.#\n\
         ..O..#O..O\n\
         .......O..\n\
         #....###..\n\
         #OO..#...."


    [<Fact>]
    let ``Test Part1`` () =
        let data = fileio.linesFromString data
        let grid = parse_data data
        let width, height = grid.GetLength 1, grid.GetLength 0

        let ret =
            [ 0 .. width - 1 ]
            |> List.map (fun col ->
                col
                |> parse_column_north grid
                |> sum_column width height)
            |> List.sum

        Assert.Equal(136, ret)

    [<Fact>]
    let ``Test Part2`` () =
        let data = fileio.linesFromString data
        let mutable grid = parse_data data
        let width, height = grid.GetLength 1, grid.GetLength 0

        let finalcolumns = perform_cycle grid 1000000000
        let ret = score_columns grid finalcolumns Direction.E

        Assert.Equal(64, ret)
