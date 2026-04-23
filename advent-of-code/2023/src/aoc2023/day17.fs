module aoc2023.day17

open System

type internal Marker = interface end


// FromDirection is the direction of how we came to the current position
type FromDirection =
    | U
    | D
    | L
    | R

[<StructuredFormatDisplay("({r}, {c}, {dir})")>]
type Coord =
    { r: int
      c: int
      dir: FromDirection list }

let get_from_n (coord: Coord) (cameFrom: Map<Coord, Coord>) (n: int) : FromDirection list = coord.dir |> List.truncate (n + 1)

let rec get_path cameFrom current =
    seq {
        yield current

        match Map.tryFind current cameFrom with
        | None -> ()
        | Some next -> yield! get_path cameFrom next
    }

let get_num_in_a_row (coord: Coord) (fromCoord: Coord) (cameFrom: Map<Coord, Coord>) : int =

    let coords =
        if coord.r <> fromCoord.r || coord.c <> fromCoord.c then
            Seq.append (seq { coord }) (get_path cameFrom fromCoord)
        else
            get_path cameFrom fromCoord
        |> Seq.cache

    let num_r = coords |> Seq.takeWhile (fun rc -> rc.r = coord.r) |> Seq.length

    let num_c = coords |> Seq.takeWhile (fun rc -> rc.c = coord.c) |> Seq.length

    Math.Max(num_r, num_c)


let get_neighbours (grid: char array2d) (coord: Coord) (cameFrom: Map<Coord, Coord>) =
    let width, height = grid.GetLength 1, grid.GetLength 0

    let r, c = coord.r, coord.c

    let n =
        [ (r - 1, c, FromDirection.D); (r + 1, c, FromDirection.U); (r, c - 1, FromDirection.R); (r, c + 1, FromDirection.L) ]

    let neighbours =
        [ for (r, c, dir) in n ->
              { r = r
                c = c
                dir = if coord.dir[0] = dir then [ dir ] @ (*from_path*) coord.dir else [ dir ] } ]


    // Limit to grid
    let neighbours =
        neighbours
        |> List.filter (fun (rc) -> rc.r >= 0 && rc.r < height && rc.c >= 0 && rc.c < width)

    // Cannot move back
    let parent = cameFrom.TryFind coord

    let neighbours =
        neighbours
        |> List.filter (fun rc ->
            match parent with
            | Some p when p.r = rc.r && p.c = rc.c -> false
            | _ -> true)


    // Can move max 3 spaces in any direction
    let neighbours =
        neighbours
        |> List.filter (fun rc -> let num_in_a_row = get_num_in_a_row rc coord cameFrom in get_num_in_a_row rc coord cameFrom <= 4)

    neighbours |> List.toSeq

// For Part Two, we can turn only after 4 steps, cannot move more than 10 + must move at least 4 in a row to the goal
let get_neighbours2 (grid: char array2d) (goal: Coord) (coord: Coord) (cameFrom: Map<Coord, Coord>) =
    let width, height = grid.GetLength 1, grid.GetLength 0

    let r, c = coord.r, coord.c
    let num_in_a_row = get_num_in_a_row coord coord cameFrom

    let n_du = [ (r - 1, c, FromDirection.D); (r + 1, c, FromDirection.U) ]

    let n_rl = [ (r, c - 1, FromDirection.R); (r, c + 1, FromDirection.L) ]

    // For the first cell, we don't know which direction we came from. Set to the first direction we move to.
    let coord_is_first = coord.r = 0 && coord.c = 0

    let set_dir dir =
        if coord_is_first then [ dir; dir ]
        elif coord.dir[0] = dir then [ dir ] @ coord.dir
        else [ dir ]

    let neighbours =
        if coord_is_first then
            // First cell. Can go right or down
            [ for (r, c, dir) in n_du @ n_rl -> { r = r; c = c; dir = set_dir dir } ]
        elif num_in_a_row < 5 then
            // Need to move at least 4 in a row before we can turn
            match coord.dir[0] with
            | FromDirection.U
            | FromDirection.D -> [ for (r, c, dir) in n_du -> { r = r; c = c; dir = set_dir dir } ]
            | FromDirection.L
            | FromDirection.R -> [ for (r, c, dir) in n_rl -> { r = r; c = c; dir = set_dir dir } ]
        else
            // 4 or more in a row. Can turn
            [ for (r, c, dir) in n_du @ n_rl -> { r = r; c = c; dir = set_dir dir } ]

    // Limit to grid
    let neighbours =
        neighbours
        |> List.filter (fun (rc) -> rc.r >= 0 && rc.r < height && rc.c >= 0 && rc.c < width)

    // Cannot move back
    let parent = cameFrom.TryFind coord

    let neighbours =
        neighbours
        |> List.filter (fun rc ->
            match parent with
            | Some p when p.r = rc.r && p.c = rc.c -> false
            | _ -> true)


    // Can move max 10 spaces in any direction
    let neighbours =
        neighbours
        |> List.filter (fun rc -> let num_in_a_row = get_num_in_a_row rc coord cameFrom in num_in_a_row <= 11)

    // Must move at least 4 in a row to the goal
    let neighbours =
        neighbours
        |> List.filter (fun rc ->
            let num_in_a_row = get_num_in_a_row rc coord cameFrom in

            not (rc.r = goal.r && rc.c = goal.c && num_in_a_row < 5))


    neighbours |> List.toSeq


let get_heuristic (goal: Coord) (rc: Coord) : float =
    float (Math.Abs(goal.r - rc.r) + Math.Abs(goal.c - rc.c))

let get_cost (grid: char array2d) (rc: Coord) (neighbour: Coord) : float =
    float (int grid[neighbour.r, neighbour.c] - int '0')

let is_goal (current: Coord) (goal: Coord) =
    current.r = goal.r && current.c = goal.c

let SolvePart1 data =
    let grid = gridio.read_grid data false '0'
    let width, height = grid.GetLength 1, grid.GetLength 0

    let start =
        { r = 0
          c = 0
          dir = [ FromDirection.L ] }

    let goal =
        { r = height - 1
          c = width - 1
          dir = [ FromDirection.L ] }

    let path =
        graphsearch.aStar start goal (grid |> get_neighbours) (grid |> get_cost) (goal |> get_heuristic) is_goal

    let solution =
        path
        |> Option.defaultValue []
        |> Seq.sumBy (fun rc -> int grid[rc.r, rc.c] - int '0')

    let solution = solution - (int grid[start.r, start.c] - int '0') // Exclude start
    solution

let SolvePart2 data =
    let grid = gridio.read_grid data false '0'
    let width, height = grid.GetLength 1, grid.GetLength 0

    let start =
        { r = 0
          c = 0
          dir = [ FromDirection.L ] }

    let goal =
        { r = height - 1
          c = width - 1
          dir = [ FromDirection.L ] }

    let path =
        graphsearch.aStar start goal ((grid, goal) ||> get_neighbours2) (grid |> get_cost) (goal |> get_heuristic) is_goal

    let solution =
        path
        |> Option.defaultValue []
        |> Seq.sumBy (fun rc -> int grid[rc.r, rc.c] - int '0')

    let solution = solution - (int grid[start.r, start.c] - int '0') // Exclude start
    solution

let public Solve () =
    printfn $"Solve from {typeof<Marker>.DeclaringType}"
    let data = fileio.linesFromFile "day17.txt"

    let stopWatch = System.Diagnostics.Stopwatch.StartNew()
    let solution = SolvePart1 data
    printfn "Part1 = %A" solution
    stopWatch.Stop()
    printfn "Elapsed time %ims" stopWatch.ElapsedMilliseconds
    assert (1138 = solution) // 1138 = 432836ms

    let stopWatch = System.Diagnostics.Stopwatch.StartNew()
    let solution = SolvePart2 data
    printfn "Part2 = %A" solution
    stopWatch.Stop()
    printfn "Elapsed time %ims" stopWatch.ElapsedMilliseconds
    assert (1312 = solution)

// #################################### //
open Xunit

type Tests() =

    let data =
        "2413432311323\n\
         3215453535623\n\
         3255245654254\n\
         3446585845452\n\
         4546657867536\n\
         1438598798454\n\
         4457876987766\n\
         3637877979653\n\
         4654967986887\n\
         4564679986453\n\
         1224686865563\n\
         2546548887735\n\
         4322674655533"

    let data2 =
        "111111111111\n\
         999999999991\n\
         999999999991\n\
         999999999991\n\
         999999999991"

    [<Fact>]
    let ``Test Part1`` () =
        let data = fileio.linesFromString data
        let grid = gridio.read_grid data false '0'
        let width, height = grid.GetLength 1, grid.GetLength 0

        let start =
            { r = 0
              c = 0
              dir = [ FromDirection.L ] }

        let goal =
            { r = height - 1
              c = width - 1
              dir = [ FromDirection.L ] }

        let path =
            graphsearch.aStar start goal (grid |> get_neighbours) (grid |> get_cost) (goal |> get_heuristic) is_goal

        (*
        debug.printfn "w=%A, h=%A path = %A =" width height path
        let ngrid = grid |> Array2D.copy

        path
        |> Option.defaultValue []
        |> Seq.iter (fun rc -> ngrid[rc.r, rc.c] <- '.')

        gridio.print_grid ngrid (fun cell -> printf "%c" cell)
        *)
        let score =
            path
            |> Option.defaultValue []
            |> Seq.sumBy (fun rc -> int grid[rc.r, rc.c] - int '0')

        let score = score - (int grid[start.r, start.c] - int '0') // Exclude start
        Assert.Equal(102, score)

    [<Fact>]
    let ``Test Part2`` () =
        for expected, data in [ 94, data; 71, data2 ] do
            let grid = gridio.read_grid (fileio.linesFromString data) false '0'
            let width, height = grid.GetLength 1, grid.GetLength 0

            let start =
                { r = 0
                  c = 0
                  dir = [ FromDirection.L ] }

            let goal =
                { r = height - 1
                  c = width - 1
                  dir = [ FromDirection.L ] }

            let path =
                graphsearch.aStar start goal ((grid, goal) ||> get_neighbours2) (grid |> get_cost) (goal |> get_heuristic) is_goal

            (*
            let ngrid = grid |> Array2D.copy

            path
            |> Option.defaultValue []
            |> Seq.iter (fun rc -> ngrid[rc.r, rc.c] <- '.')

            gridio.print_grid ngrid (fun cell -> printf "%c" cell)
            *)

            let score =
                (path
                 |> Option.defaultValue []
                 |> Seq.sumBy (fun rc -> int grid[rc.r, rc.c] - int '0'))
                - (int grid[start.r, start.c] - int '0') // Exclude start

            // debug.printfn "Part2 = %A" score
            Assert.Equal(expected, score)
