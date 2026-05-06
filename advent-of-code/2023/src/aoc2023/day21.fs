module aoc2023.day21

type internal Marker = interface end


let explore (grid: char array2d) (start_rc: int * int) (steps: int) =
    let width, height = grid.GetLength 1, grid.GetLength 0
    let mutable visited = Set.empty // Set.singleton start_rc
    let mutable frontier = Set.singleton start_rc
    let mutable reachable = Set.empty

    let neighbours = [ -1, 0; 1, 0; 0, -1; 0, 1 ] // NSEW rc
    let isOdd x = x % 2 = 1
    let isEven x = x % 2 = 0
    let evenSteps = isEven steps

    for step in [ 1..steps ] do
        let mutable newfrontier = Set.empty

        for rc in frontier do
            neighbours
            |> List.map (fun (r, c) -> fst rc + r, snd rc + c)
            |> List.filter (fun (r, c) -> r >= 0 && r < height && c >= 0 && c < width)
            |> List.filter (fun (r, c) -> grid[r, c] <> '#')
            |> List.filter (fun rc -> not (visited |> Set.contains rc))
            |> List.iter (fun rc ->
                // If "steps" is even, every even count is reachable, if odd then odd count is reachable
                if evenSteps then
                    if isEven step then reachable <- reachable |> Set.add rc
                else if isOdd step then
                    reachable <- reachable |> Set.add rc

                visited <- visited |> Set.add rc
                newfrontier <- newfrontier |> Set.add rc)

        frontier <- newfrontier

    if evenSteps then reachable <- reachable |> Set.add start_rc
    reachable

let expand_grid (grid: char array2d) (duplicate_count: int) =
    // duplicate to e.g.5x5 times
    let width, height = grid.GetLength 1, grid.GetLength 0

    let newgrid =
        Array2D.init (duplicate_count * height) (duplicate_count * width) (fun _row _col -> '.')

    grid
    |> Array2D.iteri (fun row col v ->
        seq {
            for dup_r in 0 .. duplicate_count - 1 do
                for dup_c in 0 .. duplicate_count - 1 do
                    yield dup_r, dup_c
        }
        |> Seq.iter (fun (dup_r, dup_c) ->
            let r = dup_r * height + row
            let c = dup_c * width + col
            newgrid[r, c] <- v

            if
                (dup_r <> duplicate_count / 2 || dup_c <> duplicate_count / 2)
                && newgrid[r, c] = 'S'
            then
                newgrid[r, c] <- '.'))

    newgrid

let subgrid (grid: int array2d) ((grid_r, grid_c): int * int) ((original_height, original_width): int * int) =
    let subgrid =
        grid[grid_r * original_height .. (grid_r + 1) * original_height - 1, grid_c * original_width .. (grid_c + 1) * original_width - 1]

    subgrid

let run_simulation (grid: char array2d) (height: int) (width: int) (tile_radius: int) =
    let duplicate_count = 2 * tile_radius + 1
    let newgrid = expand_grid grid duplicate_count
    let _newwidth, _newheight = newgrid.GetLength 1, newgrid.GetLength 0

    let r, c, _v =
        newgrid
        |> Array2D.mapi (fun x y v -> x, y, v) // Map to include indices
        |> Seq.cast<int * int * char> // Flatten to a 1D sequence of (x, y, v) tuples
        |> Seq.find (fun (_, _, v) -> v = 'S') // Find the first element that matches

    let start_rc = r, c
    let steps = height / 2 + tile_radius * height
    let reachable = explore newgrid start_rc steps

    let tileSummary = Array2D.init duplicate_count duplicate_count (fun _row _col -> 0)

    for tile_r, tile_c in itertools.product (seq { 0 .. (duplicate_count - 1) }) (seq { 0 .. (duplicate_count - 1) }) do
        let mutable count = 0

        for row, col in itertools.product (seq { 0 .. height - 1 }) (seq { 0 .. width - 1 }) do
            let r, c = tile_r * height + row, tile_c * width + col
            if reachable |> Set.contains (r, c) then count <- count + 1

        tileSummary[tile_r, tile_c] <- count

    tileSummary, reachable

let calculate_totel (n_o, s_o, e_o, w_o, n_i, s_i, e_i, w_i, nw_o, nw_i, ne_o, ne_i, sw_o, sw_i, se_o, se_i, centre, off_centre) tile_radius =
    // sum of first N odd numbers staring at 1 is N^2
    // sum of first N even numbers starting at 2 is N*(N+1)
    let half = int64 (tile_radius / 2)

    let total =
        int64 (n_o + s_o + e_o + w_o)
        + int64 (n_i + s_i + e_i + w_i)
        + int64 tile_radius * int64 (nw_o + ne_o + sw_o + se_o)
        + int64 (tile_radius - 1) * int64 (nw_i + ne_i + sw_i + se_i)
        + 4L * (half - 1L) * half * int64 centre
        + int64 centre
        + 4L * half * half * int64 off_centre
        - 4L * int64 off_centre

    total

let simulate (grid: char array2d) (height: int) (width: int) =
    let tile_radius = 6 // pick an even number
    let tileSummary, reachable = run_simulation grid height width tile_radius
    // Pick out the key values
    // The extreme points of the diamond
    let n_o = tileSummary[0, tile_radius]
    let s_o = tileSummary[2 * tile_radius, tile_radius]
    let e_o = tileSummary[tile_radius, 0]
    let w_o = tileSummary[tile_radius, 2 * tile_radius]
    // The adjacent to exteme points of the diamond
    let n_i = tileSummary[1, tile_radius]
    let s_i = tileSummary[2 * tile_radius - 1, tile_radius]
    let e_i = tileSummary[tile_radius, 1]
    let w_i = tileSummary[tile_radius, 2 * tile_radius - 1]
    // The outer/inner diagnonals
    let nw_o = tileSummary[0, tile_radius - 1]
    let nw_i = tileSummary[1, tile_radius - 1]
    let ne_o = tileSummary[0, tile_radius + 1]
    let ne_i = tileSummary[1, tile_radius + 1]
    let sw_o = tileSummary[2 * tile_radius, tile_radius - 1]
    let sw_i = tileSummary[2 * tile_radius - 1, tile_radius - 1]
    let se_o = tileSummary[2 * tile_radius, tile_radius + 1]
    let se_i = tileSummary[2 * tile_radius - 1, tile_radius + 1]
    // The repeating values
    let centre = tileSummary[tile_radius, tile_radius]
    let off_centre = tileSummary[tile_radius, tile_radius - 1]
    n_o, s_o, e_o, w_o, n_i, s_i, e_i, w_i, nw_o, nw_i, ne_o, ne_i, sw_o, sw_i, se_o, se_i, centre, off_centre


let SolvePart1 data =
    let grid = gridio.read_grid data false '.'
    let _width, _height = grid.GetLength 1, grid.GetLength 0

    let r, c, _v =
        grid
        |> Array2D.mapi (fun x y v -> x, y, v) // Map to include indices
        |> Seq.cast<int * int * char> // Flatten to a 1D sequence of (x, y, v) tuples
        |> Seq.find (fun (_, _, v) -> v = 'S') // Find the first element that matches

    let start_rc = r, c
    let reachable = explore grid start_rc 64
    let solution = reachable.Count
    solution

let SolvePart2 data =
    let grid = gridio.read_grid data false '.'
    let width, height = grid.GetLength 1, grid.GetLength 0
    let tile_radius = 26501365 / height

    let simulation = simulate grid height width
    let solution = calculate_totel simulation tile_radius

    solution

let public Solve () =
    printfn $"Solve from {typeof<Marker>.DeclaringType}"
    let data = fileio.linesFromFile "day21.txt"

    let stopWatch = System.Diagnostics.Stopwatch.StartNew()
    let solution = SolvePart1 data
    printfn "Part1 = %A" solution
    stopWatch.Stop()
    printfn "Elapsed time %ims" stopWatch.ElapsedMilliseconds
    assert (3642 = solution)

    let stopWatch = System.Diagnostics.Stopwatch.StartNew()
    let solution = SolvePart2 data
    printfn "Part2 = %A" solution
    stopWatch.Stop()
    printfn "Elapsed time %ims" stopWatch.ElapsedMilliseconds
    assert (608603023105276L = solution)

// #################################### //
open Xunit

type Tests() =
    let data =
        "...........\n\
         .....###.#.\n\
         .###.##..#.\n\
         ..#.#...#..\n\
         ....#.#....\n\
         .##..S####.\n\
         .##..#...#.\n\
         .......##..\n\
         .##.#.####.\n\
         .##..##.##.\n\
         ..........."


    [<Fact>]
    let ``Test Part1`` () =
        let data = fileio.linesFromString data
        let grid = gridio.read_grid data false '.'
        let _width, _height = grid.GetLength 1, grid.GetLength 0

        let r, c, _v =
            grid
            |> Array2D.mapi (fun x y v -> x, y, v) // Map to include indices
            |> Seq.cast<int * int * char> // Flatten to a 1D sequence of (x, y, v) tuples
            |> Seq.find (fun (_, _, v) -> v = 'S') // Find the first element that matches

        let start_rc = r, c
        let reachable = explore grid start_rc 6
        Assert.Equal(16, reachable.Count)


    [<Fact>]
    let ``Test Part2 example`` () =
        let data = fileio.linesFromString data
        let grid = gridio.read_grid data false '.'
        let width, height = grid.GetLength 1, grid.GetLength 0

        let simulation = simulate grid height width

        for tile_radius in 1..15 do
            let tileSummary, reachable = run_simulation grid height width tile_radius

            let tileCount =
                tileSummary
                |> Array2D.mapi (fun r c v -> r, c, v)
                |> Seq.cast<int * int * int>
                |> Seq.sumBy (fun (_r, _c, v) -> int64 v)

            // debug.printfn
            //     "radius=%A repeat=%Ax%A tileSum=%A reachableSum=%A tileSummary: "
            //     tile_radius
            //     (2 * tile_radius + 1)
            //     (2 * tile_radius + 1)
            //     tileCount
            //     (reachable |> Set.count)

            // gridio.print_grid tileSummary (fun cell -> printf "%2d " cell)
            Assert.Equal(tileCount, int64 (reachable |> Set.count))

            if tile_radius >= 6 && tile_radius % 2 = 0 then
                let total = calculate_totel simulation tile_radius
                Assert.Equal(tileCount, total)


    [<Fact(Skip = "Slow test")>]
    let ``Test Part2 data`` () =
        let data = fileio.linesFromFile "day21.txt"
        let grid = gridio.read_grid data false '.'
        let width, height = grid.GetLength 1, grid.GetLength 0

        let simulation = simulate grid height width

        for tile_radius in 2..2..6 do
            let tileSummary, reachable = run_simulation grid height width tile_radius

            let tileCount =
                tileSummary
                |> Array2D.mapi (fun r c v -> r, c, v)
                |> Seq.cast<int * int * int>
                |> Seq.sumBy (fun (_r, _c, v) -> int64 v)

            debug.printfn
                "radius=%A repeat=%Ax%A tileSum=%A reachableSum=%A RealTileSummary: "
                tile_radius
                (2 * tile_radius + 1)
                (2 * tile_radius + 1)
                tileCount
                (reachable |> Set.count)

            if tile_radius >= 6 && tile_radius % 2 = 0 then
                let total = calculate_totel simulation tile_radius
                Assert.Equal(tileCount, total)

            gridio.print_grid tileSummary (fun cell -> printf "%4d " cell)
            Assert.Equal(tileCount, int64 (reachable |> Set.count))
