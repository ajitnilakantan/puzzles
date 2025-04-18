module aoc2023.day04

type internal Marker =
    interface
    end

type Card =
    { number: int
      winning: int Set
      picked: int Set }
    static member Default =
        { number = 0
          winning = Set.empty
          picked = Set.empty }

let read_cards data =
    let split_line (line: string) =
        let splitopts =
            System.StringSplitOptions.TrimEntries
            ||| System.StringSplitOptions.RemoveEmptyEntries

        let parts = line.Split([| ':'; '|' |], splitopts)
        assert (parts.Length = 3)

        let tokens = parts[ 0 ].Split("\x20", splitopts)

        assert (tokens.Length = 2)
        assert (tokens[0] = "Card")
        let number = tokens[1] |> int

        let tokens = parts[ 1 ].Split("\x20", splitopts)

        assert (tokens.Length = 5 || tokens.Length = 10) // demo vs real
        let winning = tokens |> Array.map (int) |> Set.ofSeq

        let tokens = parts[ 2 ].Split("\x20", splitopts)

        assert (tokens.Length = 8 || tokens.Length = 25) // demo vs real
        let picked = tokens |> Array.map (int) |> Set.ofSeq

        { Card.Default with
            number = number
            winning = winning
            picked = picked }

    let cards = data |> List.map (split_line)
    cards

let SolvePart1 data =
    let score (s: int Set) =
        if s.Count = 0 then
            0
        else
            pown 2 (s.Count - 1)

    let cards = read_cards data

    let solution =
        cards
        |> List.map (fun card -> score (Set.intersect card.winning card.picked))
        |> List.sum

    solution

let SolvePart2 data =
    let cards = read_cards data
    let num_cards = cards.Length
    let card_counts = Array.create num_cards 1
    // card_counts[0] <- 1 // Initialize
    let mutable current_card = 0

    // Number of winning matches
    let score_card card =
        (Set.intersect card.winning card.picked).Count

    while current_card < num_cards do
        let score = score_card cards[current_card]
        // printfn "SCORE %A = %A  CURVALUE=%A ALL=%A" current_card score card_counts[current_card] card_counts

        for i in current_card + 1 .. current_card + score do
            // printfn "   UPDATE %A from %A to %A" i card_counts[i] (card_counts[i] + card_counts[current_card])
            card_counts[i] <- card_counts[i] + card_counts[current_card]

        current_card <- current_card + 1


    // printfn "CARDS = %A" card_counts
    let solution = card_counts |> Array.sum
    solution

let public Solve () =
    printfn $"Solve from {typeof<Marker>.DeclaringType}"
    let data = fileio.linesFromFile "day04.txt"

    let stopWatch = System.Diagnostics.Stopwatch.StartNew()
    let solution = SolvePart1 data
    printfn "Part1 = %A" solution
    stopWatch.Stop()
    printfn "Elapsed time %ims" stopWatch.ElapsedMilliseconds
    assert (27059 = solution)

    let stopWatch = System.Diagnostics.Stopwatch.StartNew()
    let solution = SolvePart2 data
    printfn "Part2 = %A" solution
    stopWatch.Stop()
    printfn "Elapsed time %ims" stopWatch.ElapsedMilliseconds
    assert (5744979 = solution)

// #################################### //
open Xunit

[<Fact>]
let ``Test Part1`` () =
    let data =
        "Card 1: 41 48 83 86 17 | 83 86  6 31 17  9 48 53\n\
         Card 2: 13 32 20 16 61 | 61 30 68 82 17 32 24 19\n\
         Card 3:  1 21 53 59 44 | 69 82 63 72 16 21 14  1\n\
         Card 4: 41 92 73 84 69 | 59 84 76 51 58  5 54 83\n\
         Card 5: 87 83 26 28 32 | 88 30 70 12 93 22 82 36\n\
         Card 6: 31 18 13 56 72 | 74 77 10 23 35 67 36 11"

    let data = fileio.linesFromString data
    let result = SolvePart1 data
    Assert.Equal(13, result)

    let result = SolvePart2 data
    Assert.Equal(30, result)
