module aoc2023.day20

open System

type internal Marker = interface end


type State =
    | On
    | Off

type Pulse =
    | High
    | Low

type ModuleType =
    | Broadcaster
    | FlipFlop of State
    | Conjunction of Map<string, Pulse> // map of source and last pulse sent

type Module =
    { name: string
      mutable mtype: ModuleType
      mutable outputs: string list
      mutable inputs: Set<string> }

    static member Default =
        { name = ""
          mtype = Broadcaster
          outputs = []
          inputs = Set.empty }

type Modules = Map<string, Module>


let parse_data (data: string list) =
    let mutable modules: Modules = Map.empty

    // Add default module for button
    [ "button" ]
    |> List.iter (fun name ->
        let modul = { Module.Default with name = name }

        modules <- modules |> Map.add modul.name modul)


    // Add the modules and their outputs
    for line in data do
        let tokens = fileio.tokenize line "->"

        let name, mtype =
            match tokens[0] with
            | s when s.StartsWith "%" -> s[1..], ModuleType.FlipFlop Off
            | s when s.StartsWith "&" -> s[1..], ModuleType.Conjunction Map.empty
            | s -> s, ModuleType.Broadcaster

        let modul =
            { Module.Default with
                name = name
                mtype = mtype }

        let tokens = fileio.tokenize tokens[1] ", "
        modul.outputs <- tokens

        modules <- modules |> Map.add modul.name modul

    // Add any missing outputs, i.e. modules that have no definition on the lhs. E.g. the "output" module in example 2.
    let mutable missing: Set<string> = Set.empty

    for KeyValue(name, modul) in modules do
        modul.outputs
        |> List.iter (fun c -> if not (modules |> Map.containsKey c) then missing <- missing.Add c)

    for c in missing do
        modules <- modules |> Map.add c { Module.Default with name = c }

    // Add the inputs
    for KeyValue(name, modul) in modules do
        modul.outputs
        |> List.iter (fun c -> let input = modules |> Map.find c in input.inputs <- input.inputs.Add(name))

    // For Conjunction types, populate the state map
    for KeyValue(_, modul) in modules do
        match modul.mtype with
        | ModuleType.Conjunction x ->
            let state = [ for inp in modul.inputs -> (inp, Low) ]
            modul.mtype <- ModuleType.Conjunction(Map.ofList state)
        | _ -> ()

    modules

let update_count (low_pulse_count: byref<int>) (high_pulse_count: byref<int>) (pulse: Pulse) (count: int) =
    if pulse = High then
        high_pulse_count <- high_pulse_count + count
    else
        low_pulse_count <- low_pulse_count + count


/// Process a pulse on the button. Return, new module states + low/high pulse counts + conjunctions that sent high pulse
let process_pulse (modules: Modules) =
    // Queue of (source, destination, pulse)
    let mutable queue = queuelib.MutableQueue<string * string * Pulse>()
    let newqueue = queuelib.MutableQueue<string * string * Pulse>()

    let mutable low_pulse_count = 0
    let mutable high_pulse_count = 0
    let mutable high_pulse_count_conjunctions = Set.empty

    queue.Enqueue("button", "broadcaster", Low)
    low_pulse_count <- low_pulse_count + 1

    // Have nested while because queue is replaced by newqueue
    while not queue.IsEmpty do
        while not queue.IsEmpty do
            let (source, destination, pulse) = queue.Dequeue()
            let modul_src = modules |> Map.find source
            let modul_dest = modules |> Map.find destination

            match modul_dest.mtype with
            | ModuleType.Broadcaster ->
                // Forward pulse to all outputs
                if modul_dest.name = "rx" && pulse = Low then
                    debug.printfn "Broadcast %A from %A" pulse modul_dest
                    failwithf "Found"

                modul_dest.outputs
                |> List.iter (fun c -> newqueue.Enqueue(modul_dest.name, c, pulse))

                update_count &low_pulse_count &high_pulse_count pulse modul_dest.outputs.Length
            | ModuleType.FlipFlop state ->
                if pulse = High then
                    // no-op
                    ()
                else
                    // Toggle state and forward toggled pulse to all outputs
                    let new_state = if state = On then Off else On
                    modul_dest.mtype <- ModuleType.FlipFlop(new_state)

                    let output_pulse = if new_state = On then High else Low

                    modul_dest.outputs
                    |> List.iter (fun c -> newqueue.Enqueue(modul_dest.name, c, output_pulse))

                    update_count &low_pulse_count &high_pulse_count output_pulse modul_dest.outputs.Length

            | ModuleType.Conjunction state ->
                assert (state |> Map.containsKey modul_src.name)
                modul_dest.mtype <- ModuleType.Conjunction(state |> Map.add modul_src.name pulse)
                // If all high inputs, send low, otherwise send high
                let output_pulse =
                    match modul_dest.mtype with
                    | ModuleType.Conjunction state when state |> Map.values |> Seq.forall (fun x -> x = High) ->
                        assert (pulse <> Low)
                        Low
                    | _ ->
                        high_pulse_count_conjunctions <- high_pulse_count_conjunctions |> Set.add modul_dest.name

                        High

                modul_dest.outputs
                |> List.iter (fun c -> newqueue.Enqueue(modul_dest.name, c, output_pulse))

                update_count &low_pulse_count &high_pulse_count output_pulse modul_dest.outputs.Length

        queue <- newqueue

    modules, low_pulse_count, high_pulse_count, high_pulse_count_conjunctions

/// Find the cycle length and start index of a pattern
/// E.g. pattern = [| 0; 1; 0; 1; 0; 1; 0; 1 |] -> 2
/// E.g. pattern = [| 0; 0; 0; 0; 1; 1; 1; 1; 0; 0; 0; 0; 1; 1; 1; 1 |] -> 8
let find_cycle (pattern: int list) =
    let cycle =
        [ 1 .. pattern.Length / 2 ]
        |> List.tryPick (fun len ->
            if
                [ 1 .. pattern.Length / len - 1 ]
                |> List.forall (fun i -> pattern.[0 .. len - 1] = pattern.[i * len .. (i + 1) * len - 1])
            then
                Some len
            else
                None)

    cycle

let rec backtrack (modules: Modules) (node: string) (output: Pulse) (assignments: Map<string, State>) : Map<string, State> seq =
    seq {
        let modul = modules |> Map.find node
        let mutable assignments = assignments

        match modul.mtype with
        | ModuleType.Broadcaster ->
            // Must get same input
            if modul.inputs |> Set.count <> 1 then
                failwithf "Broadcaster %A has %i inputs = %A" node (modul.inputs |> Set.count) modul.inputs

            assert (modul.inputs |> Set.count = 1) // single input

            debug.printfn "Process Broadcaster %A inputs=%A" node modul.inputs

            for c in modul.inputs do
                yield! backtrack modules c output assignments
        | ModuleType.FlipFlop _ ->
            let ff_state = if output = High then Off else On
            let next_output = Low // Low gets flipped
            assignments <- assignments |> Map.add node ff_state

            assert (modul.inputs |> Set.count = 1) // single input

            debug.printfn "Process flipper %A inputs=%A set node=%A=%A" node modul.inputs node ff_state

            for c in modul.inputs do
                yield! backtrack modules c next_output assignments

        | ModuleType.Conjunction _ ->
            if output = High then
                // One of the inputs must be low
                debug.printfn "Process Conjunction %A at least one inputs low=%A " node modul.inputs
                ()
            else
                // All inputs must be high
                debug.printfn "Process Conjunction  %A all inputs high=%A " node modul.inputs

                for c in modul.inputs do
                    yield! backtrack modules c High assignments

        yield assignments
    }



let SolvePart1 data =
    // 1000 rounds
    let mutable modules = parse_data data
    let mutable low_pulse_count = 0
    let mutable high_pulse_count = 0

    for _ in [ 1..1000 ] do
        let m, lp, hp, _ = process_pulse modules
        modules <- m
        low_pulse_count <- low_pulse_count + lp
        high_pulse_count <- high_pulse_count + hp

    let solution = low_pulse_count * high_pulse_count
    solution



let SolvePart2 data =
    let mutable modules = parse_data data
    // Get inputs to "rx"
    // -> This input must get a high pulse from all its inputs to send a low pulse to "rx"
    let rx_inputs = modules |> Map.find "rx" |> (fun x -> x.inputs)
    // Inputs to the inputs of "rx"
    // -> All the inputs must send a high pulse to the input to "rx" found above
    let rx_inputs =
        rx_inputs
        |> Set.toList
        |> List.collect (fun x -> modules[x].inputs |> Set.toList)
        |> Set.ofList
    // Index the inputs
    let rx_inputs_index =
        rx_inputs |> Set.toList |> List.mapi (fun i x -> x, i) |> Map.ofList

    // Look at the behaviour of the high pulses
    let num_runs = 1024 * 16 + 1
    let width, height = rx_inputs |> Set.count, num_runs + 1
    let states = Array2D.init height width (fun _ _ -> 0)
    // Run simulation
    for c in [ 1..num_runs ] do
        let m, _, _, high_pulse_count_conjunctions = process_pulse modules
        modules <- m

        high_pulse_count_conjunctions
        |> Set.toList
        |> List.iter (fun x ->
            match rx_inputs_index |> Map.tryFind x with
            | Some i -> states[c, i] <- 1
            | None -> ())


    let cycles =
        [ 0 .. width - 1 ]
        |> List.map (fun col -> states[*, col][1..] |> Array.toList |> find_cycle)
        |> List.choose id
        |> List.map int64

    let solution = math.lcmList cycles

    solution

let public Solve () =
    printfn $"Solve from {typeof<Marker>.DeclaringType}"
    let data = fileio.linesFromFile "day20.txt"

    let stopWatch = System.Diagnostics.Stopwatch.StartNew()
    let solution = SolvePart1 data
    printfn "Part1 = %A" solution
    stopWatch.Stop()
    printfn "Elapsed time %ims" stopWatch.ElapsedMilliseconds
    assert (949764474 = solution)

    let stopWatch = System.Diagnostics.Stopwatch.StartNew()
    let solution = SolvePart2 data
    printfn "Part2 = %A" solution
    stopWatch.Stop()
    printfn "Elapsed time %ims" stopWatch.ElapsedMilliseconds
    assert (243221023462303L = solution)

// #################################### //
open Xunit

type Tests() =
    let data1 =
        "broadcaster -> a, b, c\n\
         %a -> b\n\
         %b -> c\n\
         %c -> inv\n\
         &inv -> a"

    let data2 =
        "broadcaster -> a\n\
         %a -> inv, con\n\
         &inv -> b\n\
         %b -> con\n\
         &con -> output"

    // Convert a binary array to an integer
    let binaryArrayToInt (bits: int seq) : int64 =
        // The initial state (accumulator) is 0.
        // The function 'f' takes the accumulator 'acc' and the current 'bit'.
        Seq.fold
            (fun acc bit ->
                // Shift the accumulator one position to the left (multiply by 2)
                // and add the current bit using bitwise OR.
                acc <<< 1 ||| int64 bit)
            0L
            bits

    // This method iterates through the array once, tracking the current value and the count of consecutive occurrences.
    let countConsecutive (arr: 'a[]) =
        if Array.isEmpty arr then
            []
        else
            let folder (acc, currentCount) x =
                if List.isEmpty acc then
                    ([ x, 1 ], 1)
                else
                    let (lastVal, lastCount) = List.head acc

                    if lastVal = x then
                        // Update count for the same value
                        ((x, lastCount + 1) :: List.tail acc, lastCount + 1)
                    else
                        // New value found
                        ((x, 1) :: acc, 1)

            let (result, _) = Array.fold folder ([], 0) arr
            List.rev result // Reverses to maintain original order



    [<Fact>]
    let ``Test countConsecutive`` () =
        // Usage:
        let data = [| 1; 1; 2; 3; 3; 3; 4; 2; 2 |]
        let counts = countConsecutive data
        // Output: [(1, 2); (2, 1); (3, 3); (4, 1); (2, 2)]
        Assert.Equivalent([ (1, 2); (2, 1); (3, 3); (4, 1); (2, 2) ], counts)


    [<Fact>]
    let ``Test Part1`` () =
        // First example
        let data = fileio.linesFromString data1
        let modules = parse_data data
        // 1 round
        let modules, lp, hp, _ = process_pulse modules

        let a, b, c =
            modules |> Map.find "a", modules |> Map.find "b", modules |> Map.find "c"

        Assert.Equal((FlipFlop Off, FlipFlop Off, FlipFlop Off), (a.mtype, b.mtype, c.mtype))
        Assert.Equal((8, 4), (lp, hp))

        // Second example
        let data = fileio.linesFromString data2
        let modules = parse_data data
        // 1 round
        let modules, _, _, _ = process_pulse modules
        let a, b = modules |> Map.find "a", modules |> Map.find "b"
        Assert.Equal((FlipFlop On, FlipFlop On), (a.mtype, b.mtype))
        // 2 round
        let modules, _, _, _ = process_pulse modules
        let a, b = modules |> Map.find "a", modules |> Map.find "b"
        Assert.Equal((FlipFlop Off, FlipFlop On), (a.mtype, b.mtype))
        // 3 round
        let modules, _, _, _ = process_pulse modules
        let a, b = modules |> Map.find "a", modules |> Map.find "b"
        Assert.Equal((FlipFlop On, FlipFlop Off), (a.mtype, b.mtype))
        // 4 round
        let modules, _, _, _ = process_pulse modules
        let a, b = modules |> Map.find "a", modules |> Map.find "b"
        Assert.Equal((FlipFlop Off, FlipFlop Off), (a.mtype, b.mtype))

        // 1000 rounds
        let data = fileio.linesFromString data2
        let mutable modules = parse_data data
        let mutable low_pulse_count = 0
        let mutable high_pulse_count = 0

        for _ in [ 1..1000 ] do
            let m, lp, hp, _ = process_pulse modules
            modules <- m
            low_pulse_count <- low_pulse_count + lp
            high_pulse_count <- high_pulse_count + hp

        Assert.Equal(4250, low_pulse_count)
        Assert.Equal(2750, high_pulse_count)
        Assert.Equal(11687500, low_pulse_count * high_pulse_count)



    [<Fact>]
    let ``Test Part2`` () =
        let data = fileio.linesFromFile "day20.txt"
        let mutable modules = parse_data data

        let is_conjunction (x: string) =
            match modules[x].mtype with
            | ModuleType.Conjunction _ -> true
            | _ -> false

        // Get inputs to "rx"
        // -> This input must get a high pulse from all its inputs to send a low pulse to "rx"
        let rx_inputs = modules |> Map.find "rx" |> (fun x -> x.inputs)

        Assert.True(rx_inputs |> Set.forall (fun x -> is_conjunction x))

        Assert.Equivalent(Set.ofList [ "kj" ], rx_inputs)

        // Inputs to the inputs of "rx"
        // -> All the inputs must send a high pulse to the input to "rx" found above
        let rx_inputs =
            rx_inputs
            |> Set.toList
            |> List.collect (fun x -> modules[x].inputs |> Set.toList)
            |> Set.ofList

        Assert.True(rx_inputs |> Set.forall (fun x -> is_conjunction x))

        Assert.Equivalent(Set.ofList [ "dr"; "ln"; "vn"; "zx" ], rx_inputs)

        let rx_inputs_index =
            rx_inputs |> Set.toList |> List.mapi (fun i x -> x, i) |> Map.ofList

        // Look at the behaviour of the high pulses
        let num_runs = 1024 * 16 + 1
        let width, height = rx_inputs |> Set.count, num_runs + 1
        let states = Array2D.init height width (fun _ _ -> 0)
        // Run simulation
        for c in [ 1..num_runs ] do
            let m, _, _, high_pulse_count_conjunctions = process_pulse modules
            modules <- m

            high_pulse_count_conjunctions
            |> Set.toList
            |> List.iter (fun x ->
                match rx_inputs_index |> Map.tryFind x with
                | Some i -> states[c, i] <- 1
                | None -> ())

        (*
        // Print the states
        for c in rx_inputs_index |> Map.keys do
            debug.printf "%3s" c

        debug.printfn ""
        gridio.print_grid states (fun cell -> printf "%3i" cell)
        *)

        let cycles =
            [ 0 .. width - 1 ]
            |> List.map (fun col -> states[*, col][1..] |> Array.toList |> find_cycle)
            |> List.choose id

        Assert.Equivalent([ 3863; 4003; 3943; 3989 ], cycles)

        let consecutive_counts =
            [ 0 .. width - 1 ]
            |> List.map (fun col -> states[*, col][1..] |> countConsecutive |> List.take 8)

        for c in consecutive_counts do
            let evens, odds =
                c
                |> List.mapi (fun i el -> i, el)
                |> List.partition (fun pair -> fst pair % 2 = 0)

            let evens = evens |> List.map (fun pair -> snd pair)
            let odds = odds |> List.map (fun pair -> snd pair)
            // Runs of zeros
            evens |> List.iter (fun el -> Assert.Equal(0, fst el))
            // Runs of ones
            odds |> List.iter (fun el -> Assert.Equal(1, fst el))
            // All the zero runs have the same size
            evens |> List.iter (fun el -> Assert.Equal(evens[0], el))
            // Size of one run is one
            odds |> List.iter (fun el -> Assert.Equal(1, snd el))


// for col in [ 0 .. width - 1 ] do
//     let column = states[*, col][1..]
//     let cycle = find_cycle (column |> Array.toList)
//     let counts = countConsecutive column
//     debug.printfn "Cycle = %A Column = %A" cycle counts
