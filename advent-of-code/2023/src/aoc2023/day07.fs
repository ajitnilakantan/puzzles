module aoc2023.day07

type internal Marker =
    interface
    end

type Bet =
    { mutable hand: string
      mutable bet: int64 }
    static member Default = { hand = "11111"; bet = 0L }

let rank_hand1 (hand: string) card_value_function =
    // Score:
    // 7-five of a kind
    // 6-four of a king
    // 5-full house
    // 4-three of a kind
    // 3-two pair
    // 2-one pair
    // 1-high card
    assert (hand.Length = 5)

    let card_counts =
        hand
        |> Seq.toList
        |> List.countBy id
        |> List.sortBy (fun x -> -1 * snd x)
    // E.g. "T55J5"  -> [('5', 3); ('T', 1); ('J', 1)]
    let score =
        match card_counts with
        | [ (_, 5) ] -> 7
        | [ (_, 4); (_, 1) ] -> 6
        | [ (_, 3); (_, 2) ] -> 5
        | [ (_, 3); (_, 1); (_, 1) ] -> 4
        | [ (_, 2); (_, 2); (_, 1) ] -> 3
        | [ (_, 2); (_, 1); (_, 1); (_, 1) ] -> 2
        | _ -> 1

    score

let rank_hand2 (hand: string) card_value_function =
    let card_counts =
        hand
        |> Seq.toList
        |> List.countBy id
        |> List.sortBy (fun x -> (-1 * snd x, -(card_value_function (fst x))))
    // E.g. "T55J5"  -> [('5', 3); ('T', 1); ('J', 1)]
    // Replace wildcard (J) with the best card
    // Take care of some special cases
    let mutable new_hand = ""

    if card_counts.Length = 5 && fst card_counts[0] = 'J' then
        // 5 Jacks - replace with A
        new_hand <- hand.Replace("J", "A")
    elif card_counts.Length > 1 && fst card_counts[0] = 'J' then
        // Jack has the highest count. Replace second highest
        new_hand <- hand.Replace("J", string (fst card_counts[1]))
    else
        new_hand <- hand.Replace("J", string (fst card_counts[0]))

    rank_hand1 new_hand card_value_function

let card_value1 c =
    let vals =
        [ 'A', 14
          'K', 13
          'Q', 12
          'J', 11
          'T', 10 ]
        |> Map.ofList

    if vals.ContainsKey c then vals.[c] else c |> string |> int

let card_value2 c =
    // For part 2, J has lowest value
    let vals =
        [ 'A', 14
          'K', 13
          'Q', 12
          'J', 0
          'T', 10 ]
        |> Map.ofList

    if vals.ContainsKey c then vals.[c] else c |> string |> int

let rec compare_lexically h0 h1 score_function =
    match h0, h1 with
    | [], [] -> 0
    | x :: xs, y :: ys ->
        let cx, cy = score_function x, score_function y

        if cx < cy then -1
        elif cx > cy then 1
        else compare_lexically xs ys score_function
    | _, y :: ys -> -1
    | x :: xs, _ -> 1

let compare_hands h0 h1 rank_hand_function card_value_function =
    let r0 = rank_hand_function h0 card_value_function
    let r1 = rank_hand_function h1 card_value_function

    if r0 < r1 then -1
    elif r0 > r1 then 1
    else compare_lexically (h0 |> Seq.toList) (h1 |> Seq.toList) card_value_function

let parse_data data =
    let splitopts =
        System.StringSplitOptions.TrimEntries
        ||| System.StringSplitOptions.RemoveEmptyEntries

    let bets =
        data
        |> List.map (fun (x: string) -> x.Split([| '\x20' |], splitopts))
        |> List.map (fun x -> { Bet.Default with hand = x[0]; bet = int x[1] })

    bets

let SolvePart1 data =
    let bets = parse_data data

    let sorted_bets =
        bets
        |> List.sortWith (fun (x: Bet) (y: Bet) -> compare_hands x.hand y.hand rank_hand1 card_value1)

    let solution =
        sorted_bets
        |> List.indexed
        |> List.fold (fun acc value -> acc + (1L + int64 (fst value)) * ((snd value).bet)) 0L

    solution

let SolvePart2 data =
    let bets = parse_data data

    let sorted_bets =
        bets
        |> List.sortWith (fun (x: Bet) (y: Bet) -> compare_hands x.hand y.hand rank_hand2 card_value2)

    let solution =
        sorted_bets
        |> List.indexed
        |> List.fold (fun acc value -> acc + (1L + int64 (fst value)) * ((snd value).bet)) 0L

    solution

let public Solve () =
    printfn $"Solve from {typeof<Marker>.DeclaringType}"
    let data = fileio.linesFromFile "day07.txt"

    let stopWatch = System.Diagnostics.Stopwatch.StartNew()
    let solution = SolvePart1 data
    printfn "Part1 = %A" solution
    stopWatch.Stop()
    printfn "Elapsed time %ims" stopWatch.ElapsedMilliseconds
    assert (241344943L = solution)

    let stopWatch = System.Diagnostics.Stopwatch.StartNew()
    let solution = SolvePart2 data
    printfn "Part2 = %A" solution
    stopWatch.Stop()
    printfn "Elapsed time %ims" stopWatch.ElapsedMilliseconds
    assert (243101568L = solution) // 243568092too high  243174646 high

// #################################### //
open Xunit

type Tests() =
    let data =
        "32T3K 765\n\
         T55J5 684\n\
         KK677 28\n\
         KTJJT 220\n\
         QQQJA 483"

    [<Fact>]
    let ``Test Part1`` () =
        let data = fileio.linesFromString data
        let bets = parse_data data

        let sorted_bets =
            bets
            |> List.sortWith (fun (x: Bet) (y: Bet) -> compare_hands x.hand y.hand rank_hand1 card_value1)

        let expected =
            [ { hand = "32T3K"; bet = 765 }
              { hand = "KTJJT"; bet = 220 }
              { hand = "KK677"; bet = 28 }
              { hand = "T55J5"; bet = 684 }
              { hand = "QQQJA"; bet = 483 } ]

        // printfn "SORTED=%A\nEXPECTED=%A" sorted_bets expected
        Assert.True((List.forall2 (fun x y -> x = y) expected sorted_bets))

        let score =
            expected
            |> List.indexed
            |> List.fold (fun acc value -> acc + (1L + int64 (fst value)) * ((snd value).bet)) 0L

        Assert.Equal(6440L, score)

        let score =
            sorted_bets
            |> List.indexed
            |> List.fold (fun acc value -> acc + (1L + int64 (fst value)) * ((snd value).bet)) 0L

        Assert.Equal(6440L, score)

    [<Fact>]
    let ``Test Part2`` () =
        let data = fileio.linesFromString data
        let bets = parse_data data

        let sorted_bets =
            bets
            |> List.sortWith (fun (x: Bet) (y: Bet) -> compare_hands x.hand y.hand rank_hand2 card_value2)

        let score =
            sorted_bets
            |> List.indexed
            |> List.fold (fun acc value -> acc + (1L + int64 (fst value)) * ((snd value).bet)) 0L

        Assert.Equal(5905L, SolvePart2 data)
        Assert.Equal(5905L, score)
