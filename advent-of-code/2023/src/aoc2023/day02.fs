module aoc2023.day02

type internal Marker =
    interface
    end

type BagPick =
    { mutable red: int
      mutable green: int
      mutable blue: int }
    static member Default = { red = 0; green = 0; blue = 0 }

type Game =
    { mutable number: int
      mutable picks: BagPick list }
    static member Default = { number = 0; picks = [] }

let readGame (data: string) =
    let string_to_pick (str: string) =
        let pick = BagPick.Default
        // str is like "1 red, 2 green, 6 blue" - can be out of order and have missing values
        let tokens =
            str.Split(",", System.StringSplitOptions.TrimEntries)
            |> Array.toList

        assert (tokens.Length > 0)

        for token in tokens do
            let pair =
                token.Split("\x20", System.StringSplitOptions.TrimEntries)
                |> Array.toList

            assert (pair.Length = 2)
            let num = pair[0] |> int

            match pair[1] with
            | "red" -> pick.red <- pick.red + num
            | "green" -> pick.green <- pick.green + num
            | "blue" -> pick.blue <- pick.blue + num
            | x -> failwithf "Unknown match '%A'" x

        pick

    // String like: "Game 1: 3 blue, 4 red; 1 red, 2 green, 6 blue; 2 green"
    let game = Game.Default

    let parts =
        data.Split([| ":" |], System.StringSplitOptions.TrimEntries)
        |> Array.toList

    assert (2 = parts.Length)

    // parts[0] = "Game 1"
    let game_parts =
        parts[0]
            .Split("\x20", System.StringSplitOptions.TrimEntries)
        |> Array.toList

    assert (2 = game_parts.Length)
    assert ("Game" = game_parts[0])
    let game_number = game_parts[1] |> int
    game.number <- game_number

    // parts[1] = 3 blue, 4 red; 1 red, 2 green, 6 blue; 2 green
    let pick_parts =
        parts[1]
            .Split(";", System.StringSplitOptions.TrimEntries)
        |> Array.toList

    game.picks <-
        pick_parts
        |> List.map (fun str -> string_to_pick str)

    game

let readGames data =
    let mutable games = []

    data
    |> List.iter (fun line -> games <- games @ [ readGame line ])

    games


let SolvePart1 data =
    let games = readGames data

    // let scores = games |> List.map (score_game)
    let goal = { red = 12; green = 13; blue = 14 }

    let check_picks game goal =
        game.picks
        |> List.forall (fun p ->
            p.red <= goal.red
            && p.green <= goal.green
            && p.blue <= goal.blue)

    let good_games = games |> List.filter (fun g -> check_picks g goal)

    let answer =
        good_games
        |> List.map (fun x -> x.number)
        |> List.sum

    answer

let SolvePart2 data =
    let games = readGames data

    let score game =
        let max_pick = BagPick.Default

        let max_values =
            { Game.Default with
                number = game.number
                picks = [ max_pick ] }

        game.picks
        |> List.iter (fun p ->
            max_pick.red <- max max_pick.red p.red
            max_pick.green <- max max_pick.green p.green
            max_pick.blue <- max max_pick.blue p.blue)

        max_pick.red * max_pick.green * max_pick.blue

    let answer = games |> List.map (fun g -> score g) |> List.sum

    answer

let public Solve () =
    printfn $"Solve from {typeof<Marker>.DeclaringType}"
    let data = fileio.linesFromFile "day02.txt"

    let stopWatch = System.Diagnostics.Stopwatch.StartNew()
    let solution = SolvePart1 data
    printfn "Part1 = %A" solution
    stopWatch.Stop()
    printfn "Elapsed time %ims" stopWatch.ElapsedMilliseconds
    assert (1931 = solution)


    let stopWatch = System.Diagnostics.Stopwatch.StartNew()
    let solution = SolvePart2 data
    printfn "Part2 = %A" solution
    stopWatch.Stop()
    printfn "Elapsed time %ims" stopWatch.ElapsedMilliseconds
    assert (83105 = solution)
    ()



// #################################### //
open Xunit

[<Fact>]
let ``Test Sample`` () =
    let data =
        "Game 1: 3 blue, 4 red; 1 red, 2 green, 6 blue; 2 green\n\
         Game 2: 1 blue, 2 green; 3 green, 4 blue, 1 red; 1 green, 1 blue\n\
         Game 3: 8 green, 6 blue, 20 red; 5 blue, 4 red, 13 green; 5 green, 1 red\n\
         Game 4: 1 green, 3 red, 6 blue; 3 green, 6 red; 3 green, 15 blue, 14 red\n\
         Game 5: 6 red, 1 blue, 3 green; 2 blue, 1 red, 2 green"

    let data = fileio.linesFromString data
    let games = readGames data
    Assert.Equal(5, games.Length)
    Assert.Equal(8, SolvePart1 data)

[<Fact>]
let ``Test Part2`` () =
    let data =
        "Game 1: 3 blue, 4 red; 1 red, 2 green, 6 blue; 2 green\n\
         Game 2: 1 blue, 2 green; 3 green, 4 blue, 1 red; 1 green, 1 blue\n\
         Game 3: 8 green, 6 blue, 20 red; 5 blue, 4 red, 13 green; 5 green, 1 red\n\
         Game 4: 1 green, 3 red, 6 blue; 3 green, 6 red; 3 green, 15 blue, 14 red\n\
         Game 5: 6 red, 1 blue, 3 green; 2 blue, 1 red, 2 green"

    let data = fileio.linesFromString data
    let games = readGames data
    Assert.Equal(5, games.Length)
    Assert.Equal(2286, SolvePart2 data)
