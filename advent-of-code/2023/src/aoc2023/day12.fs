module aoc2023.day12

open System.Text.RegularExpressions
open System.Collections.Generic
open System

type internal Marker =
    interface
    end


let is_valid_position (pattern: string) (damage: int) (position: int) : bool =
    // Check if the specified position is valid. I.e.
    // - Must fit :  position + damage <= len(pattern)
    // - Must have a sequence of "damage" # or ? in a row starting at "position" in pattern
    // - Previous pattern[position-1] must <> "#"
    // - Next pattern[position+damage+1] must <> "#"
    position + damage <= pattern.Length
    && pattern[position .. position + damage - 1]
       |> Seq.forall (fun x -> x = '#' || x = '?')
    && (position = 0 || pattern[position - 1] <> '#')
    && (position + damage = pattern.Length
        || pattern[position + damage] <> '#')

let get_valid_positions_for_damage (pattern: string) (damage: int) : int list =
    // Return a list of valid positions within the pattern that a damage run of
    // length "damage" can be placed. I.e. at position "position":
    let result =
        [ 0 .. pattern.Length - 1 ]
        |> List.collect (fun position ->
            if is_valid_position pattern damage position then
                [ position ]
            else
                [])

    result


let rec tryFindMatch pred list =
    match list with
    | head :: tail ->
        if pred (head) then
            Some(head)
        else
            tryFindMatch pred tail
    | [] -> None

let get_base_case (pattern: string) (damaged: int list) (damaged_values: list<list<int>>) =
    //  Find the initial placement - Left aligning all damaged runs
    let placement = Array.create damaged.Length -1

    let mutable current = 0

    while current < damaged.Length do
        // End of the previous damaged run
        let end_previous =
            if current = 0 then
                0
            else
                placement[current - 1] + damaged[current - 1]

        // Place close to the previous run
        let next =
            damaged_values[current]
            |> tryFindMatch (fun x ->
                if current = 0 then
                    // No uncovered damage between begining and placement
                    x > placement[current]
                    && not (pattern[0 .. x - 1] |> Seq.contains '#')
                else
                    x > placement[current] && x > end_previous)

        if next = None && current = 0 then
            failwithf "Error finding base_case"

        if next = None then
            // backtrack
            placement[current] <- -1
            current <- current - 1
        elif current > 0
             && pattern[end_previous .. next.Value - 1]
                |> Seq.contains '#' then
            // There is an uncovered damaged. Backtrack
            placement[current] <- -1
            current <- current - 1
        else
            // Continue
            placement[current] <- next.Value
            current <- current + 1

    placement


let count_all_matches (pattern: string) (damaged: int list) (check_tail: bool) : int64 * Dictionary<int, int64> =
    let rec count_matches
        (pattern: string)
        (damaged: int list)
        (damaged_values: list<list<int>>)
        (placement: int array)
        (index: int)
        (check_tail: bool)
        (cache: Dictionary<int, int64>)
        : int64 =

        // Available positions at index
        let mutable positions =
            damaged_values[index]
            |> List.filter (fun x -> x >= placement[index])

        if index = damaged.Length - 1 then
            // Check for trailing uncovered damage
            if check_tail then
                positions <-
                    positions
                    |> List.filter (fun x ->
                        not (
                            pattern[x + damaged[index] .. pattern.Length]
                                .Contains('#')
                        ))
        else
            // Check overlap with next
            positions <-
                positions
                |> List.filter (fun x -> x + damaged[index] < placement[index + 1])
            // Check for uncovered damage between this index and index+1
            positions <-
                positions
                |> List.filter (fun x ->
                    not (
                        pattern[x + damaged[index] .. placement[index + 1] - 1]
                            .Contains('#')
                    ))

        if index = 0 then
            // Check no uncovered damage between begining of pattern and placement.
            positions <-
                positions
                |> List.filter (fun x -> not (pattern[ 0 .. x - 1 ].Contains('#')))

        match index with
        | 0 ->
            // Update cache
            let key =
                placement[placement.Length - 1]
                + damaged[placement.Length - 1]
                + 1

            let ok, res = cache.TryGetValue key

            if ok then
                cache.[key] <- res + int64 positions.Length
            else
                cache.[key] <- int64 positions.Length

            // // Validate
            // for p in positions do
            //     let _placement = Array.copy placement
            //     _placement[index] <- p
            //     let pattern2 = pattern |> Array.ofSeq
            //     _placement |> Array.iteri (fun i pp -> Array.fill pattern2 pp damaged[i] '*')
            //     if not (pattern2 |> Array.forall(fun pp -> pp <> '#')) then
            //         debug.printfn "ERROR: pattern=%A dam=%A placement=%A p2=%A" pattern damaged _placement pattern2
            //     assert(pattern2 |> Array.forall(fun pp -> pp <> '#'))

            positions.Length // base case
        | _ when positions.Length = 0 -> 0L
        | _ ->
            let mutable result = 0L

            for p in positions do
                let _placement = Array.copy placement
                _placement[index] <- p
                // Recurse to the previous index
                result <-
                    result
                    + count_matches pattern damaged damaged_values _placement (index - 1) check_tail cache

            result


    // Map of damage -> int list. I.e. for each index list of possible positions it can be placed
    let damaged_values =
        damaged
        |> List.map (fun d -> get_valid_positions_for_damage pattern d)

    let cache = Dictionary<int, int64> HashIdentity.Structural // endoffset,count
    let base_case =
        try
            get_base_case pattern damaged damaged_values
        with
            | _ -> [||]
    if base_case.Length = 0 then
        0L, cache
    else
        let result =
            count_matches pattern damaged damaged_values base_case (damaged.Length - 1) check_tail cache
        // Validate
        // let mutable sum = 0L
        // for kv in cache do
        //     sum <- sum + kv.Value
        // if result <> sum then
        //     debug.printfn "ERROR: mismatch p=%A d=%A result=%A sum=%A cache=%A" pattern damaged result sum cache
        // assert (result=sum)
        result, cache

let merge_dictionaries (destination: Dictionary<_, _>) (source: Dictionary<_, _>) (offset:int) =
    for kv in source do
        if destination.ContainsKey(kv.Key+offset) then
            destination[kv.Key+offset] <- destination[kv.Key+offset] + kv.Value
        else
            destination.Add(kv.Key+offset, kv.Value)

let count_all_matches5 (pattern: string) (damaged: int list) : int64 =
    let pattern5 = pattern |> List.replicate 5 |> String.concat "?"
    let damaged5 = damaged |> List.replicate 5 |> List.concat

    let mutable cache = Dictionary<int, int64> HashIdentity.Structural // endoffset,count
    cache.[0] <- 1

    for depth in 0..4 do
        let newcache = Dictionary<int, int64> HashIdentity.Structural // endoffset,count

        let check_tail = (depth = 4)
        for kv in cache do
            let offset = kv.Key
            let subpattern = pattern5[offset ..]
            let subresult, subcache = count_all_matches subpattern damaged check_tail

            if subresult <> 0 then
                for subkv in subcache do
                    subcache.[subkv.Key] <- kv.Value * subcache.[subkv.Key]

                merge_dictionaries newcache subcache offset

        cache <- newcache

    let mutable result = 0L

    for kv in cache do
        result <- result + kv.Value

    result

let parse_data line =
    let tokens = fileio.tokenize line "\x20"
    assert (tokens.Length = 2)
    let pattern, numbers = tokens[0], tokens[1]
    let damaged = fileio.tokenize numbers "," |> List.map int
    pattern, damaged


let SolvePart1 data =
    let solution =
        data
        |> List.map parse_data
        |> List.map (fun (x, y) -> count_all_matches x y true)
        |> List.map fst
        |> List.sum

    solution

let SolvePart2 data =
    let solution =
        data
        |> List.map parse_data
        |> List.map (fun (x, y) -> count_all_matches5 x y)
        |> List.sum

    solution

let public Solve () =
    printfn $"Solve from {typeof<Marker>.DeclaringType}"
    let data = fileio.linesFromFile "day12.txt"

    let stopWatch = System.Diagnostics.Stopwatch.StartNew()
    let solution = SolvePart1 data
    printfn "Part1 = %A" solution
    stopWatch.Stop()
    printfn "Elapsed time %ims" stopWatch.ElapsedMilliseconds
    assert (7344L = solution)

    let stopWatch = System.Diagnostics.Stopwatch.StartNew()
    let solution = SolvePart2 data
    printfn "Part2 = %A" solution
    stopWatch.Stop()
    printfn "Elapsed time %ims" stopWatch.ElapsedMilliseconds
    assert (1088006519007L = solution)

// #################################### //
open Xunit

type Tests() =
    let data =
        "???.### 1,1,3\n\
         .??..??...?##. 1,1,3\n\
         ?#?#?#?#?#?#?#? 1,3,1,6\n\
         ????.#...#... 4,1,1\n\
         ????.######..#####. 1,6,5\n\
         ?###???????? 3,2,1"

    [<Fact>]
    let ``Test Find Base Case`` () =
        let pattern, damaged = "??????###?", [ 1; 3; 1 ]

        let damaged_values =
            damaged
            |> List.map (fun d -> get_valid_positions_for_damage pattern d)
        // Signature needs to match TestDelegate, which is unit -> unit
        Assert.Throws<System.Exception> (fun () ->
            get_base_case pattern damaged damaged_values
            |> ignore)
        |> ignore

        let pattern, damaged = "??????###??", [ 1; 3; 1 ]

        let damaged_values =
            damaged
            |> List.map (fun d -> get_valid_positions_for_damage pattern d)

        let placement = get_base_case pattern damaged damaged_values
        Assert.Equal([ 0; 6; 10 ], placement)

    [<Fact>]
    let ``Test Part1`` () =

        let ret, _ = count_all_matches "#.#.###" [ 1; 1; 3 ] true
        Assert.Equal(1L, ret)
        let ret, _ = count_all_matches "???.###" [ 1; 1; 3 ] true
        Assert.Equal(1L, ret)
        let ret, _ = count_all_matches ".??..??...?##." [ 1; 1; 3 ] true
        Assert.Equal(4L, ret)
        let ret, _ = count_all_matches "?###????????" [ 3; 2; 1 ] true
        Assert.Equal(10L, ret)

        let data = fileio.linesFromString data |> List.map parse_data
        let expected_results = [ 1L; 4L; 1L; 1L; 4L; 10L ]

        for index, expected in List.indexed expected_results do
            let pattern, damaged = data[index]
            let result, cache = count_all_matches pattern damaged true
            // debug.printfn "p=%A d=%A ret=%A cache=%A" pattern damaged result cache
            Assert.Equal(expected, result)

        let solution =
            data
            |> List.map (fun (x, y) -> count_all_matches x y true)
            |> List.map fst
            |> List.sum

        Assert.Equal(21L, solution)

    [<Fact>]
    let ``Test Part2`` () =
        let data = fileio.linesFromString data |> List.map parse_data
        let expected_results = [ 1L; 16384L; 1L; 16L; 2500L; 506250L ]

        for index, expected in List.indexed expected_results do
            let pattern, damaged = data[index]
            let result= count_all_matches5 pattern damaged
            // debug.printfn "p=%A d=%A ret=%A " pattern damaged result
            Assert.Equal(expected, result)

