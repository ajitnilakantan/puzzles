module aoc2023.day05

type internal Marker =
    interface
    end

type MyMap =
    | unknown = 0
    | seeds = 1
    | FIRST = 2
    | seed_to_soil = 2
    | soil_to_fertilizer = 3
    | fertilizer_to_water = 4
    | water_to_light = 5
    | light_to_temperature = 6
    | temperature_to_humidity = 7
    | humidity_to_location = 8
    | LAST = 8

type Mapping =
    { destination: int64
      source: int64
      length: int64 }
    static member Default = { destination = 0; source = 0; length = 0 }

let parse_data data =
    let mutable current = MyMap.unknown

    let splitopts =
        System.StringSplitOptions.TrimEntries
        ||| System.StringSplitOptions.RemoveEmptyEntries

    let mutable seeds = [||]

    let mutable maps: array<Mapping list> =
        Array.init ((MyMap.LAST |> int) + 1) (fun index -> [])

    for line in data do
        match line with
        | "" -> ()
        | x when x.StartsWith("seeds:") ->
            let parts = x.Split(":", splitopts)
            assert (parts.Length = 2)

            seeds <-
                parts[ 1 ].Split(" ", splitopts)
                |> Array.map (int64)
        | "seed-to-soil map:" -> current <- MyMap.seed_to_soil
        | "soil-to-fertilizer map:" -> current <- MyMap.soil_to_fertilizer
        | "fertilizer-to-water map:" -> current <- MyMap.fertilizer_to_water
        | "water-to-light map:" -> current <- MyMap.water_to_light
        | "light-to-temperature map:" -> current <- MyMap.light_to_temperature
        | "temperature-to-humidity map:" -> current <- MyMap.temperature_to_humidity
        | "humidity-to-location map:" -> current <- MyMap.humidity_to_location
        | x ->
            // printfn "Got %A : %A" current x
            let vals = x.Split("\x20", splitopts) |> Array.map (int64)
            assert (vals.Length = 3)

            let m =
                { Mapping.Default with destination = vals[0]; source = vals[1]; length = vals[2] }

            maps[current |> int] <- maps[current |> int] @ [ m ]

    seeds, maps

let apply_map mymap value =
    // Part 1
    let mutable result = value
    let mutable finished = false

    for m in mymap do
        if not finished
           && result >= m.source
           && result < m.source + m.length then
            result <- m.destination + (result - m.source)
            finished <- true

    result

let apply_maps (seeds: int64 array) (maps: list<Mapping> array) =
    // Part 1
    let range = [ int MyMap.FIRST .. int MyMap.LAST ]
    // [ 2; 3; 4] |> List.map( fun v -> List.fold(fun acc f -> f acc) v ff);;
    seeds
    |> Array.map (fun s ->
        range
        |> List.fold (fun acc index -> apply_map (maps[index]) acc) s)


// Intersect closed/open ranges [A0..A1) with [B0..B1)
// Return Optional<intersection>, Optional<A-subract-B list>
// Take care of these conditions
// 1. [A0..A1) [B0..B1)  -> intersection=None         subtraction=Some[(A0, A1)]
// 2. [A0..B0...A1..B1]  -> intersection=Some(B0,A1)  subtraction=Some[(A0, B0)]
// 3. [A0..B0..B1..A1)   -> intersection=Some(B0,B1)  subtraction=Some[(A0, B0); (B1, A1)]
// 4. [B0..A0..A1..B1)   -> intersection=Some(A0,A1)  subtraction=None
// 5. [B0..A0..B1..A1)   -> intersection=Some(A0,B1)  subtraction=Some[(B1..A1)]
// 6. [B0..B1..A0..A1)   -> intersection=None         subtraction=Some[(A0..A1)]
let intersect_ranges (a: (int64 * int64)) (b: (int64 * int64)) =
    // Part 2
    let (a0, a1) = (fst a, snd a)
    let (b0, b1) = (fst b, snd b)
    let mutable intersection: Option<int64 * int64> = None
    let mutable subtraction: Option<(int64 * int64) list> = None
    let mutable casenumber = 0 // to debug unit tests

    if a1 <= b0 then // 1
        intersection <- None
        subtraction <- Some([ (a0, a1) ])
        casenumber <- 1
    elif a0 <= b0 && a1 > b0 && a1 <= b1 then // 2
        intersection <- Some(b0, a1)
        subtraction <- Some([ (a0, b0) ])
        casenumber <- 2
    elif a0 <= b0 && a1 >= b1 then // 3
        intersection <- Some(b0, b1)
        subtraction <- Some([ (a0, b0); (b1, a1) ])
        casenumber <- 3
    elif a0 >= b0 && a1 <= b1 then // 4
        intersection <- Some(a0, a1)
        subtraction <- None
        casenumber <- 4
    elif a0 >= b0 && a0 <= b1 && a1 >= b1 then // 5
        intersection <- Some(a0, b1)
        subtraction <- Some([ (b1, a1) ])
        casenumber <- 5
    elif a0 >= b1 then // 6
        intersection <- None
        subtraction <- Some([ (a0, a1) ])
        casenumber <- 6
    else
        failwithf "Should not be here a=%A b=%A" a b

    // Filter out empty ranges
    intersection <-
        match intersection with
        | Some (x, y) -> if x < y then Some(x, y) else None
        | _ -> None

    subtraction <-
        match subtraction with
        | Some (x) ->
            let y = x |> List.filter (fun (a, b) -> a < b)
            if y.Length > 0 then Some(y) else None
        | _ -> None

    intersection, subtraction, casenumber


let apply_map_range (mymap: Mapping list) (value: (int64 * int64) list) =
    // Part 2
    // Maintained processed and workqueue lists
    let mutable workqueue: (int64 * int64) list = value // Initially a range [from, to)
    let mutable processed: (int64 * int64) list = [] // Initially empty. Fill with intersections
    // printfn "APPLYMAPS %A to %A" mymap value

    // Process the list of seed-ranges one at a time
    while workqueue.Length > 0 do
        match workqueue with
        | head :: tail ->
            workqueue <- tail

            let mutable seedqueue = [ head ]
            // Process each seed, one at a time. When a seed range is intersected
            // with a match-range, you can have more than one range in the subtraction,
            // so keep a list.
            while seedqueue.Length > 0 do
                let mutable foundMatch = false

                match seedqueue with
                | head :: tail ->
                    seedqueue <- tail
                    // See which of the mappings intersect, and apply them
                    for m in mymap do
                        let offset = m.destination - m.source

                        let intersection, subtraction, _ =
                            intersect_ranges head (m.source, m.source + m.length)

                        match intersection, subtraction with
                        | Some (pair), Some (rest) when not foundMatch ->
                            let dest = (fst pair) + offset, (snd pair) + offset
                            processed <- processed @ [ dest ]
                            seedqueue <- seedqueue @ rest
                            foundMatch <- true
                        | Some (pair), None when not foundMatch ->
                            let dest = (fst pair) + offset, (snd pair) + offset
                            processed <- processed @ [ dest ]
                            foundMatch <- true
                        | _ -> () // No intersection or already found.

                    if not foundMatch then processed <- processed @ [ head ]
                | _ -> () // Should not get here. We check if seedqueue.Length > 0

        | _ -> () // Should not get here. We check if workqueue.Length > 0

    processed


let SolvePart1 data =
    let seeds, maps = parse_data data
    let locations = apply_maps seeds maps
    let solution = locations |> Seq.min

    solution

let SolvePart2 data =
    let seeds, maps = parse_data data
    // Read the pairs of seed ranges.  Convert from [start, range) to [start, end)
    let seed_pairs =
        Array.chunkBySize 2 seeds
        |> Array.map (fun xs -> (xs.[0], xs.[0] + xs.[1]))
        |> Array.toList

    let mutable processed: (int64 * int64) list = seed_pairs

    for index in int MyMap.FIRST .. int MyMap.LAST do
        // printfn "index = %A" index
        let mymap = maps[index]
        processed <- apply_map_range mymap processed

    let solution = fst (processed |> List.minBy (fun x -> fst x))
    solution

let public Solve () =
    printfn $"Solve from {typeof<Marker>.DeclaringType}"
    let data = fileio.linesFromFile "day05.txt"

    let stopWatch = System.Diagnostics.Stopwatch.StartNew()
    let solution = SolvePart1 data
    printfn "Part1 = %A" solution
    stopWatch.Stop()
    printfn "Elapsed time %ims" stopWatch.ElapsedMilliseconds
    assert (111627841L = solution)

    let stopWatch = System.Diagnostics.Stopwatch.StartNew()
    let solution = SolvePart2 data
    printfn "Part2 = %A" solution
    stopWatch.Stop()
    printfn "Elapsed time %ims" stopWatch.ElapsedMilliseconds
    assert (69323688L = solution)

// #################################### //
open Xunit

type Tests() =

    let data =
        "seeds: 79 14 55 13\n\
            \n\
            seed-to-soil map:\n\
            50 98 2\n\
            52 50 48\n\
            \n\
            soil-to-fertilizer map:\n\
            0 15 37\n\
            37 52 2\n\
            39 0 15\n\
            \n\
            fertilizer-to-water map:\n\
            49 53 8\n\
            0 11 42\n\
            42 0 7\n\
            57 7 4\n\
            \n\
            water-to-light map:\n\
            88 18 7\n\
            18 25 70\n\
            \n\
            light-to-temperature map:\n\
            45 77 23\n\
            81 45 19\n\
            68 64 13\n\
            \n\
            temperature-to-humidity map:\n\
            0 69 1\n\
            1 0 69\n\
            \n\
            humidity-to-location map:\n\
            60 56 37\n\
            56 93 4"

    [<Fact>]
    let ``Test Part1`` () =
        let data = fileio.linesFromString data
        let seeds, maps = parse_data data
        let locations = apply_maps seeds maps
        Assert.True(Array.forall2 (fun v1 v2 -> v1 = v2) [| 82L; 43L; 86L; 35L |] locations)
        let solution = locations |> Seq.min
        Assert.Equal(35L, solution)

    // Convert to list of tuples (start*length)
    [<Fact>]
    let ``Test Part2`` () =
        let data = fileio.linesFromString data
        let seeds, maps = parse_data data
        Assert.Equal(0, seeds.Length % 2)

        // Read the pairs of seed ranges.  Convert from [start, range) to [start, end)
        let seed_pairs =
            Array.chunkBySize 2 seeds
            |> Array.map (fun xs -> (xs.[0], xs.[0] + xs.[1]))

        Assert.Equal(seeds.Length / 2, seed_pairs.Length)
        ()

    [<Fact>]
    let testIntersection () =
        // Test the different cases + edge conditions
        let testdata =
            [ ((0L, 8L), (9L, 15L), None, Some([ 0L, 8L ]), 1.1)
              ((0L, 9L), (9L, 15L), None, Some([ 0L, 9L ]), 1.2)
              ((0L, 10L), (9L, 15L), Some(9L, 10L), Some([ (0L, 9L) ]), 2.1)
              ((0L, 15L), (9L, 15L), Some(9L, 15L), Some([ (0L, 9L) ]), 2.2)
              ((9L, 13L), (9L, 15L), Some(9L, 13L), None, 2.3)
              ((9L, 15L), (9L, 15L), Some(9L, 15L), None, 2.4)
              ((0L, 17L), (9L, 15L), Some(9L, 15L), Some([ (0L, 9L); (15, 17) ]), 3.1)
              ((9L, 17L), (9L, 15L), Some(9L, 15L), Some([ (15L, 17L) ]), 3.2)
              ((9L, 20L), (9L, 15L), Some(9L, 15L), Some([ 15, 20 ]), 3.3)
              ((12L, 13L), (9L, 15L), Some(12L, 13L), None, 4.1)
              ((12L, 15L), (9L, 15L), Some(12L, 15L), None, 4.2)
              ((12L, 20L), (9L, 15L), Some(12L, 15L), Some([ 15L, 20L ]), 5.1)
              ((15L, 20L), (9L, 15L), None, Some([ 15L, 20L ]), 5.2)
              ((17L, 20L), (9L, 15L), None, Some([ 17L, 20L ]), 6.1) ]

        for d in testdata do
            let (a, b, intersection, subtraction, casenumber) = d
            let i, s, c = intersect_ranges a b
            Assert.Equal((casenumber, int casenumber, intersection, subtraction), (casenumber, c, i, s))

    [<Fact>]
    let test_apply_map_range () =
        // Test the sample data
        let data = fileio.linesFromString data
        let seeds, maps = parse_data data
        // Read the pairs of seed ranges.  Convert from [start, range) to [start, end)
        let seed_pairs =
            Array.chunkBySize 2 seeds
            |> Array.map (fun xs -> (xs.[0], xs.[0] + xs.[1]))
            |> Array.toList

        let mutable processed: (int64 * int64) list = seed_pairs

        for index in int MyMap.FIRST .. int MyMap.LAST do
            // printfn "index = %A" index
            let mymap = maps[index]
            processed <- apply_map_range mymap processed

        // printfn "PROCESSED = %A" processed
        let solution = fst (processed |> List.minBy (fun x -> fst x))
        Assert.Equal(46L, solution)
