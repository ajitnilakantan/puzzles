module aoc2023.day18

open System

type internal Marker = interface end

type Direction =
    | U
    | D
    | L
    | R

type Command =
    {
      // For part 1
      dir: Direction
      amount: int
      color: int
      // For part 2
      dir2: Direction
      amount2: int }

type Position =
    { mutable x: int
      mutable y: int }
    // Overload the '+' operator as a static member
    static member (+)(p1: Position, p2: Position) = { x = p1.x + p2.x; y = p1.y + p2.y }

let parse_data (data: string list) : Command list =
    let commands =
        data
        |> List.map (fun s ->
            let tokens = fileio.tokenize s "\x20(#)"
            assert (tokens.Length = 3)
            let dir = tokens[0]
            let amount = int tokens[1]
            let color = Convert.ToInt32(tokens[2], 16)
            let color_str = tokens[2]
            assert (color_str.Length = 6)

            { dir =
                match dir with
                | "U" -> U
                | "D" -> D
                | "L" -> L
                | "R" -> R
                | _ -> failwithf "Invalid direction: %A from line %A" dir s
              amount = amount
              color = color
              dir2 =
                match color_str[5] with
                | '0' -> R
                | '1' -> D
                | '2' -> L
                | '3' -> U
                | _ -> failwithf "Invalid direction: %A from line %A" color_str s
              amount2 = Convert.ToInt32(color_str[0..4], 16) })

    commands

let delta (dir: Direction) (amount: int) : Position =
    match dir with
    | U -> { x = 0; y = -amount }
    | D -> { x = 0; y = amount }
    | L -> { x = -amount; y = 0 }
    | R -> { x = amount; y = 0 }

let get_vertices (commands: Command list) get_delta =
    let mutable current = { x = 0; y = 0 }
    let mutable vertices = [ { x = 0; y = 0 } ]
    let mutable perimeter = 0L

    for command in commands do
        let d = get_delta command
        current <- current + d
        vertices <- List.append vertices [ current ]

        perimeter <- perimeter + Math.Abs(int64 d.x) + Math.Abs(int64 d.y)

    vertices |> List.toArray, perimeter

/// Shoelace formula  en.wikipedia.org/wiki/Shoelace_formula
/// How it works for Concave Polygons Vertex Order:
/// List your polygon's vertices (x_{0},y_{0}),(x_{1},y_{1}),\dots ,(x_{n-1},y_{n-1}) in order (clockwise or counter-clockwise), repeating the first vertex at the end.
/// Cross-Multiplication:Sum the products: x_{0}y_{1}+x_{1}y_{2}+\dots +x_{n-1}y_{0} … (down-right diagonals).
/// Sum the products: y_{0}x_{1}+y_{1}x_{2}+\dots +y_{n-1}x_{0} (up-right diagonals).
/// Area Calculation: Subtract the second sum from the first, take the absolute value, and divide by 2: A=\frac{1}{2}|(\text{Sum\ 1})-(\text{Sum\ 2})|.
let shoelace (vertices: Position array) =
    let len = vertices.Length

    let sum1 =
        [ 0 .. len - 1 ]
        |> Seq.sumBy (fun i -> int64 vertices[i].x * int64 vertices[(i + 1) % len].y)

    let sum2 =
        [ 0 .. len - 1 ]
        |> Seq.sumBy (fun i -> int64 vertices[(i + 1) % len].x * int64 vertices[i].y)

    Math.Abs(sum1 - sum2) / 2L

let SolvePart1 data =
    let commands = parse_data data

    let vertices, perimeter =
        get_vertices commands (fun command -> delta command.dir command.amount)

    let area = shoelace vertices
    let area = area + perimeter / 2L + 1L
    let solution = area
    solution

let SolvePart2 data =
    let commands = parse_data data

    let vertices, perimeter =
        get_vertices commands (fun command -> delta command.dir2 command.amount2)

    let area = shoelace vertices
    let area = area + perimeter / 2L + 1L
    let solution = area
    solution

let public Solve () =
    printfn $"Solve from {typeof<Marker>.DeclaringType}"
    let data = fileio.linesFromFile "day18.txt"

    let stopWatch = System.Diagnostics.Stopwatch.StartNew()
    let solution = SolvePart1 data
    printfn "Part1 = %A" solution
    stopWatch.Stop()
    printfn "Elapsed time %ims" stopWatch.ElapsedMilliseconds
    assert (38188L = solution)

    let stopWatch = System.Diagnostics.Stopwatch.StartNew()
    let solution = SolvePart2 data
    printfn "Part2 = %A" solution
    stopWatch.Stop()
    printfn "Elapsed time %ims" stopWatch.ElapsedMilliseconds
    assert (93325849869340L = solution)

// #################################### //
open Xunit

type Tests() =
    let data =
        "R 6 (#70c710)\n\
         D 5 (#0dc571)\n\
         L 2 (#5713f0)\n\
         D 2 (#d2c081)\n\
         R 2 (#59c680)\n\
         D 2 (#411b91)\n\
         L 5 (#8ceee2)\n\
         U 2 (#caa173)\n\
         L 1 (#1b58a2)\n\
         U 2 (#caa171)\n\
         R 2 (#7807d2)\n\
         U 3 (#a77fa3)\n\
         L 2 (#015232)\n\
         U 2 (#7a21e3)"

    [<Fact>]
    let ``Test Part1`` () =
        let data = fileio.linesFromString data
        let commands = parse_data data

        let vertices, perimeter =
            get_vertices commands (fun command -> delta command.dir command.amount)

        let area = shoelace vertices
        let area = area + perimeter / 2L + 1L
        Assert.Equal(62L, area)


    [<Fact>]
    let ``Test Part2`` () =
        let data = fileio.linesFromString data
        let commands = parse_data data

        let vertices, perimeter =
            get_vertices commands (fun command -> delta command.dir2 command.amount2)

        let area = shoelace vertices
        let area = area + perimeter / 2L + 1L
        Assert.Equal(952408144115L, area)
