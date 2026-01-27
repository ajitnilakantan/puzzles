namespace aoc2023

open Xunit

module klee =
    type Interval = float * float
    type HyperBox = Interval list

    // Base Case: 1D Union Length (Klee's original 1977 algorithm)
    let private unionLength1D (intervals: Interval list) =
        intervals
        |> List.collect (fun (lo, hi) -> [ (lo, 1); (hi, -1) ])
        |> List.sortBy fst
        |> List.fold
            (fun (accLen, count, prevX) (x, delta) ->
                let newLen = if count > 0 then accLen + (x - prevX) else accLen
                (newLen, count + delta, x))
            (0.0, 0, 0.0)
        |> fun (len, _, _) -> len

    // Recursive N-Dimensional Volume
    let rec computeVolume (boxes: HyperBox list) : float =
        match boxes with
        | [] -> 0.0
        | box :: _ when box.Length = 1 ->
            // Base case: If dimension is 1, use the optimized 1D length algorithm
            boxes |> List.map List.head |> unionLength1D
        | box :: _ ->
            // Recursive case: Sweep through the first dimension (index 0)
            let coords =
                boxes
                |> List.collect (fun b -> [ fst b.[0]; snd b.[0] ])
                |> List.distinct
                |> List.sort

            coords
            |> List.pairwise
            |> List.sumBy (fun (lo, hi) ->
                let active =
                    boxes
                    |> List.filter (fun b -> fst b.[0] <= lo && snd b.[0] >= hi)
                    |> List.map List.tail // Reduce dimensionality for the next recursion

                (hi - lo) * computeVolume active)
    // | _ -> 0L


    // Generalized Surface Area: Sum of (N-1) volumes of all exposed faces
    let computeSurfaceArea (boxes: HyperBox list) : float =
        if boxes.IsEmpty then
            0.0
        else
            let dim = boxes.[0].Length

            [ 0 .. dim - 1 ]
            |> List.sumBy (fun axis ->
                let coords =
                    boxes
                    |> List.collect (fun b -> [ fst b.[axis]; snd b.[axis] ])
                    |> List.distinct
                    |> List.sort

                coords
                |> List.sumBy (fun x ->
                    let eps = 1e-9 // Infinitesimal offset to measure boundary change

                    let sliceAt v =
                        boxes
                        |> List.filter (fun b -> fst b.[axis] <= v && snd b.[axis] >= v)
                        |> List.map (fun b ->
                            b
                            |> List.indexed
                            |> List.filter (fun (i, _) -> i <> axis)
                            |> List.map snd)

                    abs (
                        computeVolume (sliceAt (x + eps))
                        - computeVolume (sliceAt (x - eps))
                    )))

module klee_test =
    [<Fact>]
    let ``test klee`` () =
        // --- 2026 Example: 3D Overlapping Cubes ---
        let cube1 = [ (0.0, 2.0); (0.0, 2.0); (0.0, 2.0) ] // 2x2x2 cube
        let cube2 = [ (1.0, 3.0); (1.0, 3.0); (1.0, 3.0) ] // Overlapping 2x2x2 cube
        let hyperboxes = [ cube1; cube2 ]

        // printfn "N-Dim Volume: %A" (klee.computeVolume hyperboxes)
        // printfn "N-Dim Surface Area: %A" (klee.computeSurfaceArea hyperboxes)
        Assert.Equal(42.0, klee.computeSurfaceArea hyperboxes)
        Assert.Equal(15.0, klee.computeVolume hyperboxes)
