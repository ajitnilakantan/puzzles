module aoc2023.day16

type internal Marker = interface end

open System // Required for the [<Flags>] attribute and HasFlag method

[<Flags>]
type Direction =
    | None = 0
    | Up = 1
    | Right = 2
    | Down = 4
    | Left = 8

type Coord = { x: int; y: int; dir: Direction }

let get_neighbors (grid: char array2d) (visited: Direction array2d) (coord: Coord) : Coord list =
    let width, height = grid.GetLength 1, grid.GetLength 0
    let mutable neighbors = []

    // "."
    let dot_neighbors =
        Map.ofList
            [ (Direction.Up, [ { x = 0; y = -1; dir = Direction.Up } ])
              (Direction.Right, [ { x = 1; y = 0; dir = Direction.Right } ])
              (Direction.Down, [ { x = 0; y = 1; dir = Direction.Down } ])
              (Direction.Left, [ { x = -1; y = 0; dir = Direction.Left } ]) ]
    // "-"
    let hyphen_neighbors =
        Map.ofList
            [ (Direction.Up, [ { x = -1; y = 0; dir = Direction.Left }; { x = 1; y = 0; dir = Direction.Right } ])
              (Direction.Right, [ { x = 1; y = 0; dir = Direction.Right } ])
              (Direction.Down, [ { x = -1; y = 0; dir = Direction.Left }; { x = 1; y = 0; dir = Direction.Right } ])
              (Direction.Left, [ { x = -1; y = 0; dir = Direction.Left } ]) ]

    // "|"
    let bar_neighbors =
        Map.ofList
            [ (Direction.Up, [ { x = 0; y = -1; dir = Direction.Up } ])
              (Direction.Right, [ { x = 0; y = -1; dir = Direction.Up }; { x = 0; y = 1; dir = Direction.Down } ])
              (Direction.Down, [ { x = 0; y = 1; dir = Direction.Down } ])
              (Direction.Left, [ { x = 0; y = -1; dir = Direction.Up }; { x = 0; y = 1; dir = Direction.Down } ]) ]

    // "/"
    let slash_neighbors =
        Map.ofList
            [ (Direction.Up, [ { x = 1; y = 0; dir = Direction.Right } ])
              (Direction.Right, [ { x = 0; y = -1; dir = Direction.Up } ])
              (Direction.Down, [ { x = -1; y = 0; dir = Direction.Left } ])
              (Direction.Left, [ { x = 0; y = 1; dir = Direction.Down } ]) ]

    // "\"
    let backslash_neighbors =
        Map.ofList
            [ (Direction.Up, [ { x = -1; y = 0; dir = Direction.Left } ])
              (Direction.Right, [ { x = 0; y = 1; dir = Direction.Down } ])
              (Direction.Down, [ { x = 1; y = 0; dir = Direction.Right } ])
              (Direction.Left, [ { x = 0; y = -1; dir = Direction.Up } ]) ]

    let all_neighbors =
        Map.ofList
            [ ('.', dot_neighbors)
              ('-', hyphen_neighbors)
              ('|', bar_neighbors)
              ('/', slash_neighbors)
              ('\\', backslash_neighbors) ]

    let neighbors = all_neighbors.[grid.[coord.y, coord.x]].[coord.dir]
    // Offset the delta to the current position
    let neighbors =
        neighbors
        |> List.map (fun n ->
            { n with
                x = n.x + coord.x
                y = n.y + coord.y })
    // Filter out off-grid neighbors
    let neighbors =
        neighbors
        |> List.filter (fun n -> n.x >= 0 && n.y >= 0 && n.x < width && n.y < height)
    // Filter out visited neighbors
    let neighbors =
        neighbors |> List.filter (fun n -> not (visited.[n.y, n.x].HasFlag n.dir))

    neighbors


let flood_fill (grid: char array2d) (start: Coord) =
    let width, height = grid.GetLength 1, grid.GetLength 0
    let visited = Array2D.init height width (fun _ _ -> Direction.None)

    // Bootstrap with initial position
    let mutable workqueue: Coord list = [ start ]

    while workqueue <> [] do
        let coord = workqueue |> List.head
        workqueue <- workqueue |> List.tail

        if not (visited.[coord.y, coord.x].HasFlag coord.dir) then
            visited.[coord.y, coord.x] <- visited.[coord.y, coord.x] ||| coord.dir
            workqueue <- get_neighbors grid visited coord @ workqueue

    visited

let score_visited (visited: Direction array2d) =
    let mutable score = 0

    visited
    |> Array2D.iteri (fun _ _ v -> if v <> Direction.None then score <- score + 1)

    score

let max_score_all_visited (grid: char array2d) =
    let width, height = grid.GetLength 1, grid.GetLength 0

    let start =
        [ for x in 0 .. width - 1 -> { x = x; y = 0; dir = Direction.Down } ]
        @ [ for x in 0 .. width - 1 ->
                { x = x
                  y = height - 1
                  dir = Direction.Up } ]
        @ [ for y in 0 .. height - 1 -> { x = 0; y = y; dir = Direction.Right } ]
        @ [ for y in 0 .. height - 1 ->
                { x = width - 1
                  y = y
                  dir = Direction.Left } ]


    let mutable max_score = 0
    let all_visited = Array2D.init height width (fun _ _ -> Direction.None)

    for coord in start do
        if not (all_visited.[coord.y, coord.x].HasFlag coord.dir) then
            let visited = flood_fill grid coord

            for x in 0 .. width - 1 do
                all_visited.[0, x] <- all_visited.[0, x] ||| visited.[0, x]

                all_visited.[height - 1, x] <- all_visited.[height - 1, x] ||| visited.[height - 1, x]

            for y in 0 .. height - 1 do
                all_visited.[y, 0] <- all_visited.[y, 0] ||| visited.[y, 0]

                all_visited.[y, width - 1] <- all_visited.[y, width - 1] ||| visited.[y, width - 1]

            let score = score_visited visited
            if score > max_score then max_score <- score

    max_score


let SolvePart1 data =
    let grid = gridio.read_grid data false '.'
    let visited = flood_fill grid { x = 0; y = 0; dir = Direction.Right }
    let solution = score_visited visited
    solution

let SolvePart2 data =
    let grid = gridio.read_grid data false '.'
    let solution = max_score_all_visited grid
    solution

let public Solve () =
    printfn $"Solve from {typeof<Marker>.DeclaringType}"
    let data = fileio.linesFromFile "day16.txt"

    let stopWatch = System.Diagnostics.Stopwatch.StartNew()
    let solution = SolvePart1 data
    printfn "Part1 = %A" solution
    stopWatch.Stop()
    printfn "Elapsed time %ims" stopWatch.ElapsedMilliseconds
    assert (7870 = solution)

    let stopWatch = System.Diagnostics.Stopwatch.StartNew()
    let solution = SolvePart2 data
    printfn "Part2 = %A" solution
    stopWatch.Stop()
    printfn "Elapsed time %ims" stopWatch.ElapsedMilliseconds
    assert (8143 = solution)

// #################################### //
open Xunit

type Tests() =
    let data =
        [ @".|...\...."
          @"|.-.\....."
          @".....|-..."
          @"........|."
          @".........."
          @".........\"
          @"..../.\\.."
          @".-.-/..|.."
          @".|....-|.\"
          @"..//.|...." ]

    [<Fact>]
    let ``Test Part1`` () =
        // let data = fileio.linesFromString data
        let data = data
        let grid = gridio.read_grid data false '.'
        let visited = flood_fill grid { x = 0; y = 0; dir = Direction.Right }
        let ret = score_visited visited
        Assert.Equal(46, ret)

    [<Fact>]
    let ``Test Part2`` () =
        // let data = fileio.linesFromString data
        let data = data
        let grid = gridio.read_grid data false '.'
        let ret = max_score_all_visited grid

        Assert.Equal(51, ret)
