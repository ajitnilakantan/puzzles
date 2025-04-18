module aoc2023.day10

type internal Marker =
    interface
    end

type Coord = int * int
let CoordDefault = Coord(0, 0)

let add_to_map (key: 'a) (value: 'b) (map: Map<'a, 'b list>) =
    let mutable map = map
    if not (Map.containsKey key map) then map <- map |> Map.add key []
    let v = (Map.find key map)
    map <- map |> Map.add key (v @ [ value ])
    map

let parse_grid (grid: char [,]) =
    let height, width = grid.GetLength(0), grid.GetLength(1)
    // let mutable incoming = Map.empty<Coord, Coord list>
    let mutable outgoing = Map.empty<Coord, Coord list>
    let north, east, south, west = (-1, 0), (0, 1), (1, 0), (0, -1) // (deltay,deltax)
    let mutable start = CoordDefault

    // Take into account the 1-cell border
    for (y, x) in gridio.enumerate_coordinates 1 (height - 2) 1 (width - 2) do
        // Return the (deltay,deltax)s represented by the symbol
        let dirs =
            match grid[y, x] with
            | '|' -> Some([ north; south ])
            | '-' -> Some([ east; west ])
            | 'L' -> Some([ north; east ])
            | 'J' -> Some([ north; west ])
            | '7' -> Some([ south; west ])
            | 'F' -> Some([ south; east ])
            | '.' -> None
            | 'S' ->
                start <- (y, x)
                None
            | _ -> None

        let add_delta a b = (fst a + fst b, snd a + snd b)

        match dirs with
        | Some (d) ->
            d
            |> List.iter (fun v -> outgoing <- add_to_map (y, x) (add_delta (y, x) v) outgoing)
        | None -> ()

    // Add outgoing pointers for "S". Assume there are only two
    for (y, x) in gridio.enumerate_neighbours start 1 do
        let is_linked neighbour start outgoing =
            outgoing |> Map.containsKey neighbour
            && outgoing
               |> Map.find neighbour
               |> List.contains start

        let neighbour = (y, x)
        if is_linked neighbour start outgoing then outgoing <- add_to_map start neighbour outgoing

    // "S" should have only 2 paths in/out
    assert
        (Map.containsKey start outgoing
         && (Map.find start outgoing).Length = 2)

    start, outgoing

let find_closed_path (grid: char [,]) (outgoing: Map<Coord, Coord list>) (start: Coord) =
    // Loop around until we end up back at start. Return pathlen
    let mutable current = start
    let mutable visited = Set.singleton start
    let mutable counter = 0
    let mutable finished = false

    let outgoing_count (yx: Coord) (outgoing: Map<Coord, Coord list>) =
        if Map.containsKey yx outgoing then (Map.find yx outgoing).Length else 0

    let next_neighbour pos (outgoing: Map<Coord, Coord list>) (visited: Set<Coord>) start =
        outgoing[pos]
        |> Seq.tryFind (fun (neighbour: Coord) ->
            (outgoing_count neighbour outgoing = 2
             && not (visited.Contains neighbour))
            || (visited.Count > 2 && neighbour = start))

    while not finished do
        let next = next_neighbour current outgoing visited start

        match next with
        | Some (v) -> current <- v
        | None -> failwithf "Error: Cannot find loop"

        visited <- visited |> Set.add current
        counter <- counter + 1
        if current = start then finished <- true

    counter, visited

type State =
    | Inside
    | Outside

type NodeType =
    | Corner
    | Horizontal
    | Vertical

let toggle_state s = if s = Inside then Outside else Inside

let is_linked node other (outgoing: Map<Coord, Coord list>) : bool = outgoing[node] |> List.contains other

let node_type node visited outgoing =
    let (row, col) = node
    let prev = (row, col - 1)
    let next = (row, col + 1)
    let up = (row - 1, col)
    let down = (row + 1, col)

    if is_linked node prev outgoing
       && is_linked node next outgoing then
        Horizontal
    elif is_linked node up outgoing
         && is_linked node down outgoing then
        Vertical
    else
        Corner


let count_interior (grid: char [,]) (visited: Set<Coord>) (outgoing: Map<Coord, Coord list>) =
    // Scan horizontally, row by row transitioning outside/inside
    // whem we hit a path coordinate.  Ignore moving horizonally or vertically along a wall, i.e. when
    // the incoming neighbour is on the same row or column.
    let height, width = grid.GetLength(0), grid.GetLength(1)
    let mutable total_inside = 0

    let is_linked node other (outgoing: Map<Coord, Coord list>) : bool = outgoing[node] |> List.contains other

    //debug.printfn "GRID="
    //gridio.print_grid grid (fun cell -> printf "%c" cell)

    for row in 0 .. height - 1 do
        let mutable state = Outside // start Outside
        let mutable on_edge = false // track when we are on a horizontal edge
        let mutable on_edge_from_below = false // Are we entering the edge from below or above

        for col in 0 .. width - 1 do
            let prev = (row, col - 1)
            let next = (row, col + 1)
            let up = (row - 1, col)
            let down = (row + 1, col)

            match grid[row, col] with
            | _ when
                visited.Contains(row, col)
                && node_type (row, col) visited outgoing = Corner
                && is_linked (row, col) next outgoing
                ->
                // Entering a corner. Keep track of that + if we enter from above or below
                assert (not on_edge)
                on_edge <- true
                on_edge_from_below <- if (is_linked (row, col) down outgoing) then true else false
                ()
            | '-' when visited.Contains(row, col) ->
                // In a horizontal edge.  Ignore
                ()
            | _ when
                visited.Contains(row, col)
                && node_type (row, col) visited outgoing = Corner
                && is_linked (row, col) prev outgoing
                ->
                // Second corner, leaving edge. No change if we leave ⊔ or ⊓  / Change for ╭--╯ ╰--╮  and
                assert (on_edge)
                on_edge <- false

                if ((on_edge_from_below
                     && is_linked (row, col) up outgoing)
                    || (not on_edge_from_below
                        && is_linked (row, col) down outgoing)) then
                    state <- toggle_state state

                //debug.printfn "toggle(%A)" (row, col)
                ()
            | _ when visited.Contains(row, col) ->
                state <- toggle_state state
                //debug.printfn "toffle(%A)" (row, col)
                ()
            | _ when state = Inside && not (visited.Contains(row, col)) ->
                total_inside <- total_inside + 1
                //debug.printfn "INC at rc=%A,%A" row col
                ()
            | _ -> ()

    total_inside

let SolvePart1 data =
    let grid = gridio.read_grid data true '.'
    let start, outgoing = parse_grid grid
    let path_len, _ = find_closed_path grid outgoing start
    let solution = path_len / 2

    solution

let SolvePart2 data =
    let grid = gridio.read_grid data true '.'
    let start, outgoing = parse_grid grid
    let _, visited = find_closed_path grid outgoing start
    let solution = count_interior grid visited outgoing
    solution

let public Solve () =
    printfn $"Solve from {typeof<Marker>.DeclaringType}"
    let data = fileio.linesFromFile "day10.txt"

    let stopWatch = System.Diagnostics.Stopwatch.StartNew()
    let solution = SolvePart1 data
    printfn "Part1 = %A" solution
    stopWatch.Stop()
    printfn "Elapsed time %ims" stopWatch.ElapsedMilliseconds
    assert (6812 = solution)

    let stopWatch = System.Diagnostics.Stopwatch.StartNew()
    let solution = SolvePart2 data
    printfn "Part2 = %A" solution
    stopWatch.Stop()
    printfn "Elapsed time %ims" stopWatch.ElapsedMilliseconds
    assert (527 = solution)

// #################################### //
open Xunit

type Tests() =
    let data =
        ".....\n\
         .S-7.\n\
         .|.|.\n\
         .L-J.\n\
         ....."

    let data2 =
        "..F7.\n\
         .FJ|.\n\
         SJ.L7\n\
         |F--J\n\
         LJ..."

    [<Fact>]
    let ``Test Part1`` () =
        let data = fileio.linesFromString data
        let grid = gridio.read_grid data true '.'
        let start, outgoing = parse_grid grid
        let path_len, _ = find_closed_path grid outgoing start
        let solution = path_len / 2
        Assert.Equal(4, solution)

        let data = fileio.linesFromString data2
        let grid = gridio.read_grid data true '.'
        let start, outgoing = parse_grid grid
        let path_len, _ = find_closed_path grid outgoing start
        let solution = path_len / 2
        Assert.Equal(8, solution)

    let data3 =
        "...........\n\
         .S-------7.\n\
         .|F-----7|.\n\
         .||.....||.\n\
         .||.....||.\n\
         .|L-7.F-J|.\n\
         .|..|.|..|.\n\
         .L--J.L--J.\n\
         ..........."

    let data4 =
        "...........\n\
         .S-------7.\n\
         .|F-----7|.\n\
         .||.....||.\n\
         .||.....||.\n\
         .|L--7F-J|.\n\
         .|...||..|.\n\
         .L---JL--J.\n\
         ..........."

    let data5 =
        ".F----7F7F7F7F-7....\n\
         .|F--7||||||||FJ....\n\
         .||.FJ||||||||L7....\n\
         FJL7L7LJLJ||LJ.L-7..\n\
         L--J.L7...LJS7F-7L7.\n\
         ....F-J..F7FJ|L7L7L7\n\
         ....L7.F7||L7|.L7L7|\n\
         .....|FJLJ|FJ|F7|.LJ\n\
         ....FJL-7.||.||||...\n\
         ....L---J.LJ.LJLJ..."

    let data6 =
        "FF7FSF7F7F7F7F7F---7\n\
         L|LJ||||||||||||F--J\n\
         FL-7LJLJ||||||LJL-77\n\
         F--JF--7||LJLJIF7FJ-\n\
         L---JF-JLJIIIIFJLJJ7\n\
         |F|F-JF---7IIIL7L|7|\n\
         |FFJF7L7F-JF7IIL---7\n\
         7-L-JL7||F7|L7F-7F7|\n\
         L.L7LFJ|||||FJL7||LJ\n\
         L7JLJL-JLJLJL--JLJ.L"

    [<Fact>]
    let ``Test Part1`` () =
        let test_data =
            [ ("data", data, 1)
              ("data2", data2, 1)
              ("data3", data3, 4)
              ("data4", data4, 5)
              ("data5", data5, 8)
              ("data6", data6, 10) ]

        // let test_data = [ ("data3", data3, 4) ]

        for label, data, expected in test_data do
            let data = fileio.linesFromString data
            let grid = gridio.read_grid data true '.'
            let start, outgoing = parse_grid grid
            let _, visited = find_closed_path grid outgoing start
            let solution = count_interior grid visited outgoing
            Assert.Equal($"{label}:{expected}", $"{label}:{solution}")
