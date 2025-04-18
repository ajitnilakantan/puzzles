module aoc2023.day12

open System.Text.RegularExpressions
open System.Collections.Generic

type internal Marker =
    interface
    end

type path_component =
    { offset: int
      count: int64
      path: int64 list }
    static member Default = { offset = 0; count = 1; path = [] }

let kount_all_matches (pattern: string) (damaged: int list) : int64 * Dictionary<int, int64> =
    // let pattern5 = pattern |> List.replicate 5 |> String.concat "?"
    // let damaged5 = damaged |> List.replicate 5 |> List.concat
    let valid_position (pattern: string) (damage: int) (position: int) : bool =
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

    let get_damaged_values (pattern: string) (damage: int) : int list =
        // Return a list of valid positions within the pattern that a damage run of
        // length "damage" can be placed. I.e. at position "position":
        let result =
            [ 0 .. pattern.Length - 1 ]
            |> List.collect (fun position ->
                if valid_position pattern damage position then
                    [ position ]
                else
                    [])

        result

    let base_case (pattern: string) (damaged: int list) (damaged_values: list<list<int>>) =
        // Find the initial placement - Left aligning all damaged runs
        let placement = Array.zeroCreate damaged.Length
        placement[0] <- damaged_values[0][0]

        for index in 1 .. damaged.Length - 1 do
            placement[index] <- damaged_values[index]
                                |> List.find (fun p -> p > placement[index - 1] + damaged[index - 1])

        placement

    let prune_damaged_values (initial_placement: int array) (pattern: string) (damaged: int list) (damaged_values: list<list<int>>) =
        // Optimization: Prune "damaged_values". Ensure that they do not overlap the max of the next.
        let pruned_damaged_values, _ =
            (damaged_values, pattern.Length + 1)
            ||> List.mapFoldBack (fun v acc ->
                let w = v |> List.filter (fun x -> x < acc - 1)
                let wacc = w |> List.last
                w, wacc)

        // Optimization: Prune "damaged_values". Ensure that they do not overlap the min of the previous.
        let damaged_values = pruned_damaged_values

        let pruned_damaged_values, _ =
            (-2, damaged_values)
            ||> List.mapFold (fun acc v ->
                let w = v |> List.filter (fun x -> x > acc + 1)

                match w with
                //                | [] -> [], -1
                | _ ->
                    let wacc = w |> List.head
                    w, wacc)

        pruned_damaged_values

    // let all_hashes_covered (pattern: string) (damaged: int list) (placement: int array) : bool =
    //     // - All the hashes from placement to the end are covered. If not it means there are uncounted damaged
    //     let pattern = pattern |> Array.ofSeq

    //     placement
    //     |> Array.iteri (fun i p -> Array.fill pattern p damaged[i] '*')

    //     pattern |> Array.forall (fun p -> p <> '#')

    let available_positions
        (pattern: string)
        (index: int)
        (damaged: int list)
        (damaged_values: list<list<int>>)
        (placement: int array)
        : int list =
        // Check if:
        // - Not overlapping previous
        // - Not overlapping next
        // - All the hashes from placement to the end are covered. If not it means there are uncounted damaged
        let endpos =
            if index = placement.Length - 1 then
                pattern.Length - 1
            else
                placement[index + 1] - 1

        let positions =
            damaged_values[index]
            |> List.filter (fun x ->
                (index = 0
                 || x > placement[index - 1] + damaged[index - 1])
                && (index = placement.Length - 1
                    || placement[index + 1] > x + damaged[index])
                && Seq.forall //x + damaged[index] >= endpos ||
                    (fun z -> z <> '#')
                    (pattern[x + damaged[index] + 1 .. endpos]))

        // Check there is no # between index and index + 1
        let finish =
            if index = placement.Length - 1 then
                placement.Length
            else
                placement[index + 1]

        let positions =
            positions
            |> List.filter (fun x ->
                let start = x + damaged[index]

                pattern[start .. finish - 1]
                |> Seq.forall (fun p -> p <> '#'))

        // Check there is no # between 0 and index=1
        match index with
        | 0 ->
            positions
            |> List.filter (fun x ->
                pattern[0 .. x - 1]
                |> Seq.forall (fun p -> p <> '#'))
        | _ -> positions


    let rec count_matches
        (pattern: string)
        (damaged: int list)
        (damaged_values: list<list<int>>)
        (placement: int array)
        (index: int)
        (cache: Dictionary<int, int64>)
        : int64 =

        let positions = available_positions pattern index damaged damaged_values placement

        match index with
        | _ when positions.Length = 0 -> 0
        | 0 -> // base case
            let mutable result = positions.Length

            let key =
                placement[placement.Length - 1]
                + damaged[placement.Length - 1]
                + 1

            let ok, res = cache.TryGetValue key
            if ok then cache.[key] <- res + 1L else cache.[key] <- 1L

            result
        | _ ->
            let mutable result = 0L

            for p in positions do
                let _placement = Array.copy placement
                _placement[index] <- p

                result <-
                    result
                    + count_matches pattern damaged damaged_values _placement (index - 1) cache

            result

    // Map of damage -> int list. I.e. for each index list of possible positions it can be placed
    let damaged_values =
        damaged
        |> List.map (fun d -> get_damaged_values pattern d)

    let initial_placement = base_case pattern damaged damaged_values

    let damaged_values =
        prune_damaged_values initial_placement pattern damaged damaged_values

    let cache = Dictionary<int, int64> HashIdentity.Structural // endoffset,count

    let result =
        count_matches pattern damaged damaged_values initial_placement (initial_placement.Length - 1) cache

    debug.printfn " Count = %A" result

    result, cache

let kount_all_matches5 (pattern: string) (damaged: int list) : int64 =
    let append_dictionary (destination: Dictionary<_, _>) (source: Dictionary<_, _>) =
        for kv in source do
            if destination.ContainsKey(kv.Key) then
                assert (destination[kv.Key] = kv.Value)
            else
                destination.Add(kv.Key, kv.Value)

    let offset_key (d: Dictionary<_, _>) (offset: int) : Dictionary<_, _> =
        let result = Dictionary<int, int64> HashIdentity.Structural // endoffset,count

        for kv in d do
            // Add offset
            result[kv.Key + offset] <- kv.Value

        result

    0L

let valid_position (pattern: string) (damage: int) (position: int) : bool =
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

let valid_positions (pattern: string) (damage: int) : int list =
    // Return a list of valid positions within the pattern that a damage run of
    // length "damage" can be placed. I.e. at position "position":
    let result =
        [ 0 .. pattern.Length - 1 ]
        |> List.collect (fun position ->
            if valid_position pattern damage position then
                [ position ]
            else
                [])

    result





let count_all_matches (pattern: string) (damaged: int list) : int64 * Dictionary<int, int64> =
    // Check all position in pattern where you can place each damaged-count.
    // Recursively enumerate solution, starting which everything packed to the left.
    // At each iteration move the tail one valid position to the right

    let all_hashes_covered (pattern: string) (damaged: int list) (placement: int array) : bool =
        // - All the hashes from placement to the end are covered. If not it means there are uncounted damaged
        let pattern = pattern |> Array.ofSeq

        placement
        |> Array.iteri (fun i p -> Array.fill pattern p damaged[i] '*')

        pattern |> Array.forall (fun p -> p <> '#')

    let available_positions
        (pattern: string)
        (index: int)
        (damaged: int list)
        (damaged_values: list<list<int>>)
        (placement: int array)
        : int list =
        // Check if:
        // - Not overlapping previous
        // - Not overlapping next
        // - All the hashes from placement to the end are covered. If not it means there are uncounted damaged
        let endpos =
            if index = placement.Length - 1 then
                pattern.Length - 1
            else
                placement[index + 1] - 1

        let positions =
            damaged_values[index]
            |> List.filter (fun x ->
                (index = 0
                 || x > placement[index - 1] + damaged[index - 1])
                && (index = placement.Length - 1
                    || placement[index + 1] > x + damaged[index])
                && Seq.forall //x + damaged[index] >= endpos ||
                    (fun z -> z <> '#')
                    (pattern[x + damaged[index] + 1 .. endpos]))

        positions


    let base_case (pattern: string) (damaged: int list) (damaged_values: list<list<int>>) =
        // Find the initial placement - Left aligning all damaged runs
        let placement = Array.zeroCreate damaged.Length

        try
            placement[0] <- damaged_values[0][0]

            for index in 1 .. damaged.Length - 1 do
                placement[index] <- damaged_values[index]
                                    |> List.find (fun p -> p > placement[index - 1] + damaged[index - 1])

            Some placement
        with
        | :? KeyNotFoundException -> None
        | :? System.ArgumentException -> None



    let prune_damaged_values (initial_placement: int array) (damaged: int list) (damaged_values: list<list<int>>) =
        // Optimization: Prune "damaged_values". Ensure that they do not overlap the max of the next.
        damaged_values
        |> List.mapi (fun index d ->
            match index with
            | _ when index = damaged_values.Length - 1 -> d
            | _ ->
                d
                |> List.filter (fun x -> x < List.last damaged_values[index + 1]))

    let rec count_matches
        (pattern: string)
        (damaged: int list)
        (damaged_values: list<list<int>>)
        (placement: int array)
        (index: int)
        (cache: Dictionary<int, int64>)
        : int64 =

        let positions = available_positions pattern index damaged damaged_values placement

        match index with
        | _ when positions.Length = 0 -> 0
        | 0 -> // base case
            let mutable result = 0L

            for p in positions do
                let _placement = Array.copy placement
                _placement[index] <- p

                if all_hashes_covered pattern damaged _placement then
                    result <- result + 1L

                    let key =
                        _placement[_placement.Length - 1]
                        + damaged[_placement.Length - 1]
                        + 1

                    let ok, res = cache.TryGetValue key
                    if ok then cache.[key] <- res + 1L else cache.[key] <- 1L

            result
        | _ ->
            let mutable result = 0L

            for p in positions do
                let _placement = Array.copy placement
                _placement[index] <- p

                result <-
                    result
                    + count_matches pattern damaged damaged_values _placement (index - 1) cache

            result

    // Map of damage -> int list. I.e. for each index list of possible positions it can be placed
    let damaged_values =
        damaged
        |> List.map (fun d -> valid_positions pattern d)

    let _initial_placement = base_case pattern damaged damaged_values

    match _initial_placement with
    | None ->
        let cache = Dictionary<int, int64> HashIdentity.Structural
        0L, cache
    | Some initial_placement ->
        // Prune damaged values. Optional optimization.
        let damaged_values = prune_damaged_values initial_placement damaged damaged_values
        let cache = Dictionary<int, int64> HashIdentity.Structural // endoffset,count

        let result =
            count_matches pattern damaged damaged_values initial_placement (initial_placement.Length - 1) cache

        result, cache

let append_dictionary (destination: Dictionary<_, _>) (source: Dictionary<_, _>) =
    for kv in source do
        if destination.ContainsKey(kv.Key) then
            assert (destination[kv.Key] = kv.Value)
        else
            destination.Add(kv.Key, kv.Value)

let offset_key (d: Dictionary<_, _>) (offset: int) : Dictionary<_, _> =
    let result = Dictionary<int, int64> HashIdentity.Structural // endoffset,count

    for kv in d do
        // Add offset
        result[kv.Key + offset] <- kv.Value

    result



let count_all_matches5 (pattern: string) (damaged: int list) : int64 =

    let count_matches_from_offset (pattern: string) (damaged: int list) (offset: int) (part: int) : Dictionary<int, int64> =

        let pattern5 = pattern |> List.replicate 5 |> String.concat "?"
        let result = Dictionary<int, int64> HashIdentity.Structural // endoffset,count

        let minPacking = (damaged |> List.sum) + damaged.Length - 1
        let mutable endoffset_begin = offset + minPacking - 1
        let endoffset_end = pattern5.Length - 1 - part * (minPacking - 1)

        try
            while endoffset_begin > 0
                  && endoffset_begin < pattern5.Length
                  && pattern5[endoffset_begin] = '#' do
                endoffset_begin <- endoffset_begin - 1
        with
        | :? System.IndexOutOfRangeException ->
            debug.printfn "end = %A" endoffset_begin
            failwith "eeerrr"
        // Look ahead past the length of the pattern
        for index in endoffset_begin..endoffset_end do
            let mutable endoffset = index

            // if endoffset > pattern5.Length - 1 then
            //     endoffset <- pattern5.Length - 1

            let mutable _pattern = pattern5[offset..endoffset]

            if endoffset <> pattern5.Length - 1
               && pattern5[endoffset + 1] = '#'
               && Seq.last _pattern = '#' then
                _pattern <- ""

            if _pattern <> ""
               && endoffset <> pattern5.Length - 1
               && pattern5[endoffset + 1] = '#'
               && Seq.last _pattern = '?' then
                let a = _pattern.ToCharArray()
                a[a.Length - 1] <- '.'
                _pattern <- a.ToString()

            let count, cache = count_all_matches _pattern damaged

            // if count > 0 && index = endoffset_begin then
            //     debug.printfn "ODD : %A %A %A damaged=%A off=%A pattern=%A" _pattern count cache damaged offset pattern5
            // Add offset
            let cache = offset_key cache offset
            append_dictionary result cache

        result

    let rec sum_arena_tree (arena: (Option<Dictionary<_, _>> array)) : int64 = // (pattern_length: int) (offset: int) (depth: int) : int64 =
        assert (arena[0] <> None)
        // Run a DFS to enumerate all paths and sum the products
        let mutable result = 0L
        let stack = new Stack<path_component>()
        stack.Push path_component.Default // root

        while stack.Count > 0 do
            let node = stack.Pop()

            match node with
            | _ when node.path.Length = 5 ->
                result <-
                    result
                    + (node.path
                       |> List.map int64
                       |> List.fold (*) 1L
                       |> int64)
            | _ ->
                match arena[node.offset] with
                | None -> ()
                | Some children ->
                    for kv in children do
                        let newNode =
                            { offset = kv.Key
                              count = kv.Value
                              path = node.path @ [ kv.Value ] }

                        stack.Push newNode

        result

    //  Arena stores the tree of results flattened in an arena. [0] is the root
    let arena: (Option<Dictionary<_, _>> array) =
        Array.create (5 * pattern.Length + 4 + 2) None

    let mutable queue = [ 0 ]

    for part in 4..-1..0 do
        let mutable new_queue = []

        for offset in queue do
            let results = count_matches_from_offset pattern damaged offset part

            new_queue <-
                new_queue @ [ for kv in results -> kv.Key ]
                |> List.distinct

            arena[offset] <- Some(results)

        queue <- new_queue

    let result = sum_arena_tree arena
    result

let parse_data line =
    let tokens = fileio.tokenize line "\x20"
    assert (tokens.Length = 2)
    let pattern, numbers = tokens[0], tokens[1]
    let damaged = fileio.tokenize numbers "," |> List.map (int)
    pattern, damaged

let SolvePart1 data =
    let solution =
        data
        |> List.map parse_data
        |> List.map (fun (x, y) -> kount_all_matches x y)
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
    assert (1107070048593L = solution) // x1089788869141 1090262810196L x1090262810196 x1107070048593 z1102775081297 / 1058706942005 1056954621633 1058695783385 too low

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
    let ``Test Part1`` () =
        let ret, _ = kount_all_matches "#.#.###" [ 1; 1; 3 ]
        Assert.Equal(1L, ret)
        let ret, _ = kount_all_matches "???.###" [ 1; 1; 3 ]
        Assert.Equal(1L, ret)
        let ret, _ = kount_all_matches ".??..??...?##." [ 1; 1; 3 ]
        Assert.Equal(4L, ret)
        let ret, _ = kount_all_matches "?###????????" [ 3; 2; 1 ]
        Assert.Equal(10L, ret)

        let data = fileio.linesFromString data |> List.map parse_data

        let expected_results = [ 1L; 4L; 1L; 1L; 4L; 10L ]

        for index, expected in List.indexed expected_results do
            let pattern, damaged = data[index]
            let result, _ = kount_all_matches pattern damaged
            Assert.Equal(expected, result)

        let solution =
            data
            |> List.map (fun (x, y) -> kount_all_matches x y)
            |> List.map fst
            |> List.sum

        Assert.Equal(21L, solution)

    [<Fact>]
    let ``Test Part2`` () =
        let data = fileio.linesFromString data

        let data =
            data
            |> List.map parse_data
            |> List.map (fun d -> fst d |> List.replicate 5 |> String.concat "?", snd d |> List.replicate 5 |> List.concat)

        let expected_results = [ 1L; 16384L; 1L; 16L; 2500L; 506250L ]

        for index, expected in List.indexed expected_results do
            let pattern, damaged = data[index]
            let result = fst (kount_all_matches pattern damaged)
            Assert.Equal(expected, result)

        let solution =
            data
            |> List.map (fun (x, y) -> fst (kount_all_matches x y))
            |> List.sum

        Assert.Equal(525152L, solution)

    [<Fact>]
    let ``Test Solution2`` () =
        let data = fileio.linesFromString data

        let result = SolvePart2 data
        Assert.Equal(525152L, result)

    [<Fact>]
    let ``Test Slow Pattern`` () =
        let data = "??????.??????????. 3,3"

        let data = fileio.linesFromString data |> List.map parse_data

        let expected_results = [ 3034988402L ]

        for index, expected in List.indexed expected_results do
            let pattern, damaged = data[index]
            let result = count_all_matches5 pattern damaged
            Assert.Equal(expected, result)

        let pattern5 =
            fst data[0]
            |> List.replicate 5
            |> String.concat "?"

        let damaged5 = snd data[0] |> List.replicate 5 |> List.concat

        kount_all_matches (fst data[0]) (snd data[0])
        |> ignore

        kount_all_matches pattern5 damaged5 |> ignore
