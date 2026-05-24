module aoc2023.day22

type internal Marker = interface end
type Range = { from: int; too: int }

type Bar =
    // The [from..to] ranges are inclusive
    | XAligned of id: int * x: Range * y: int * z: int
    | YAligned of id: int * x: int * y: Range * z: int
    | ZAligned of id: int * x: int * y: int * z: Range

// Orthogonal Segment Intersection: Sweep Line Algorithm
// BEGIN Intersection result including coordinates and both segment IDs
// Faster than an O(n^2) brute force. This is  O(NlogN + R), where
// R is the number of intersections found
type Point<'t when 't: comparison> = { X: 't; Y: 't }

type Segment<'t when 't: comparison> =

    | Horizontal of id: int * y: 't * x1: 't * x2: 't
    | Vertical of id: int * x: 't * y1: 't * y2: 't

type Overlap<'t when 't: comparison> =
    { Id1: int
      Id2: int
      Start: Point<'t>
      End: Point<'t> }

type Result<'t when 't: comparison> =

    | Intersection of point: Point<'t> * hId: int * vId: int
    | CollinearOverlap of overlap: Overlap<'t>

// Priority: Left (0) < Vert (1) < Right (2) to catch endpoints
type EventType =
    | Left = 0
    | Vert = 1
    | Right = 2

type Event<'t when 't: comparison> =
    { X: 't
      Type: EventType
      Y1: 't
      Y2: 't
      Id: int }

let findInteractions<'t when 't: comparison> (segments: Segment<'t> list) =

    // 1. Efficient Collinear Overlap Check (O(N log N))
    let findCollinear (items: (int * 't * 't * 't) list) isHorizontal =
        items

        |> List.groupBy (fun (_, axis, _, _) -> axis)
        |> List.collect (fun (axis, group) ->
            let sorted = group |> List.sortBy (fun (_, _, s, _) -> s)

            [ for i in 0 .. sorted.Length - 1 do
                  let (id1, _, s1, e1) = sorted.[i]
                  let mutable j = i + 1

                  while j < sorted.Length && (let (_, _, s2, _) = sorted.[j] in s2 <= e1) do
                      let (id2, _, s2, e2) = sorted.[j]
                      let overlapStart = s2
                      let overlapEnd = if e1 < e2 then e1 else e2

                      let pStart, pEnd =
                          if isHorizontal then
                              ({ X = overlapStart; Y = axis }, { X = overlapEnd; Y = axis })
                          else
                              ({ X = axis; Y = overlapStart }, { X = axis; Y = overlapEnd })

                      yield
                          CollinearOverlap
                              { Id1 = id1
                                Id2 = id2
                                Start = pStart
                                End = pEnd }

                      j <- j + 1 ])

    let hData =
        segments
        |> List.choose (function
            | Horizontal(id, y, x1, x2) -> Some(id, y, min x1 x2, max x1 x2)
            | _ -> None)

    let vData =
        segments
        |> List.choose (function
            | Vertical(id, x, y1, y2) -> Some(id, x, min y1 y2, max y1 y2)
            | _ -> None)

    let overlaps = (findCollinear hData true) @ (findCollinear vData false)

    // 2. Perpendicular Intersection Sweep-Line
    let events =
        segments

        |> List.collect (function
            | Horizontal(id, y, x1, x2) ->
                // Explicitly typing the record to avoid field name ambiguity with Point
                [ ({ X = min x1 x2
                     Type = EventType.Left
                     Y1 = y
                     Y2 = y
                     Id = id }
                  : Event<'t>)
                  ({ X = max x1 x2
                     Type = EventType.Right
                     Y1 = y
                     Y2 = y
                     Id = id }
                  : Event<'t>) ]

            | Vertical(id, x, y1, y2) ->
                [ ({ X = x
                     Type = EventType.Vert
                     Y1 = min y1 y2
                     Y2 = max y1 y2
                     Id = id }
                  : Event<'t>) ])
        |> List.sortBy (fun e -> e.X, e.Type)

    let folder (activeY: Set<'t * int>, res: Result<'t> list) (e: Event<'t>) =
        match e.Type with

        | EventType.Left -> (activeY.Add(e.Y1, e.Id), res)
        | EventType.Right -> (activeY.Remove(e.Y1, e.Id), res)

        | EventType.Vert ->
            let found =
                activeY
                |> Set.filter (fun (y, _) -> y >= e.Y1 && y <= e.Y2)
                |> List.ofSeq

                |> List.map (fun (y, hId) -> Intersection({ X = e.X; Y = y }, hId, e.Id))

            (activeY, res @ found)
        | _ -> (activeY, res)

    let (_, crossIntersections) = List.fold folder (Set.empty, []) events

    overlaps @ crossIntersections

// END Intersection result including coordinates and both segment IDs

let drop_bars bars =
    let segments: List<Segment<int>> =
        bars
        |> List.map (function
            | XAligned(id, x, y, z) -> Horizontal(id = id, y = y, x1 = x.from, x2 = x.too)
            | YAligned(id, x, y, z) -> Vertical(id = id, x = x, y1 = y.from, y2 = y.too)
            | ZAligned(id, x, y, z) -> Vertical(id = id, x = x, y1 = y, y2 = y))

    // Extract intersecting IDs
    let pairs =
        findInteractions segments
        |> List.map (function
            | Intersection(p, h, v) -> h, v
            | CollinearOverlap(o) -> o.Id1, o.Id2)

    let z_top bar =
        match bar with
        | XAligned(_, _, _, z) -> z
        | YAligned(_, _, _, z) -> z
        | ZAligned(_, _, _, z) -> z.too

    let z_bottom bar =
        match bar with
        | XAligned(_, _, _, z) -> z
        | YAligned(_, _, _, z) -> z
        | ZAligned(_, _, _, z) -> z.from

    let minmax (bars: Bar list) ((a, b): int * int) =
        assert (z_top (bars[a]) <> z_top (bars[b])) // We never have overlapping bars zzz
        if z_top (bars[a]) < z_top (bars[b]) then (a, b) else (b, a)

    // Find all the bars below each.
    let mutable below: array<int list> = Array.create (bars |> List.length) []

    for pair in pairs do
        let bottom, top = minmax bars pair // bottom has a lower z-value
        below[top] <- bottom :: below[top]

    let ids = [ 0 .. (bars |> List.length) - 1 ] // bar ids
    let mutable droppedBars = Set.empty // all the bars that have dropped
    let mutable undroppedBars = Set.ofList ids // all the undropped bars - todo list
    let mutable dropAmount = Array.zeroCreate (ids |> List.length) // amount each bar has dropped along the z-axis

    // Loop and drop the bottommost bars at each iteration
    while undroppedBars |> Set.count > 0 do
        // list |> forall returns true for an empty list - i.e. bottommost bars which are not over anything
        let mutable toDrop =
            undroppedBars
            |> Set.filter (fun id -> below[id] |> List.forall (fun idd -> droppedBars.Contains idd))

        // Find max in a a list, default to 0 for empty list
        let findMax list =
            list
            |> List.fold
                (fun maxSoFar x ->
                    assert (x >= 0)
                    if x > maxSoFar then x else maxSoFar)
                0

        toDrop
        |> Seq.iter (fun id ->
            let bot = z_bottom bars[id]
            // Find the highest bar dropped below, that this bar will rest over
            let top =
                below[id]
                // |> List.filter (fun bid -> droppedBars |> Set.contains bid)
                |> List.map (fun bid -> z_top bars[bid] - dropAmount[bid])
                |> findMax

            dropAmount[id] <- bot - top - 1)

        droppedBars <- Set.union droppedBars toDrop
        undroppedBars <- Set.difference undroppedBars toDrop

    // After dropping everything, calculate which bars each bar is resting on, adjusting
    // for the drop amounts
    let mutable directlyBelow: Map<int, int list> = Map.empty

    for id in ids do
        let directlyBelowId =
            below[id]
            |> List.filter (fun bid -> z_bottom bars[id] - dropAmount[id] - 1 = z_top bars[bid] - dropAmount[bid])

        directlyBelow <- directlyBelow |> Map.add id directlyBelowId


    // Now count all the multiple-supports (any one of them can be removed)
    let multipleSupports =
        directlyBelow
        |> Map.values // Gets a sequence of the lists
        |> Seq.filter (fun bel -> bel |> List.length > 1) // Multiple support
        |> Seq.concat // Flattens into one sequence
        |> Set.ofSeq // Collects into a Set (removes duplicates)

    // Exclude single supports. They cannot be removed, even if in one of the multiple supports above.
    let singleSupports =
        directlyBelow
        |> Map.values // Gets a sequence of the lists
        |> Seq.filter (fun bel -> bel |> List.length = 1) // Single support
        |> Seq.concat // Flattens into one sequence
        |> Set.ofSeq // Collects into a Set (removes duplicates)

    // Get the "parents" of the singleSupports. They would also fall if a single support is removed.
    // This is needed for Part 2
    let singleParents =
        directlyBelow
        |> Map.toSeq // Convert to seq of (parentId, [belowIds])
        |> Seq.collect (fun (parentId, belowIds) ->
            belowIds
            |> List.filter (fun bid -> Set.contains bid singleSupports) // Filter those where belowId is a singleSupport
            |> List.map (fun bids -> (bids, parentId))) // Invert the mapping to (belowId, parentId), where belowId is a singleSupport

    // Convert the seq (belowId, parentId: int*int) to a map<int, int list> since the key "belowId" can repeat
    let singleParents = singleParents |> Seq.fold collections.multiListToMap Map.empty

    // Now count the topmost bars - they don't support anything
    let topmost =
        Set.difference (ids |> Set.ofList) (Set.union multipleSupports singleSupports)

    directlyBelow, topmost, multipleSupports, singleSupports, singleParents

let parse_line id (tokens: int list) =
    let x0, y0, z0, x1, y1, z1 =
        tokens[0], tokens[1], tokens[2], tokens[3], tokens[4], tokens[5]

    let bar: Bar =
        if x0 <> x1 then
            assert (x0 <= x1)
            assert (z0 > 0)
            XAligned(id = id, x = { from = x0; too = x1 }, y = y0, z = z0)
        elif y0 <> y1 then
            assert (y0 <= y1)
            assert (z0 > 0)
            YAligned(id = id, x = x0, y = { from = y0; too = y1 }, z = z0)
        else
            assert (z0 <= z1)
            assert (z0 > 0)
            assert (z0 > 0 && z1 > 0)
            ZAligned(id = id, x = x0, y = y0, z = { from = z0; too = z1 })

    bar

let parse_data data =
    data
    |> List.map (fun s -> fileio.tokenize s ",~")
    |>
    // Nested map to convert each string in every inner list
    List.map (List.map int)
    |> List.mapi (fun index tokens -> parse_line index tokens)

let SolvePart1 data =
    let bars = parse_data data

    let _directlyBelow, topmost, multipleSupports, singleSupports, _singleParents =
        drop_bars bars

    let redundant = Set.union (Set.difference multipleSupports singleSupports) topmost
    let solution = redundant |> Set.count
    solution

let SolvePart2 data =
    let bars = parse_data data

    let directlyBelow, topmost, multipleSupports, singleSupports, singleParents =
        drop_bars bars

    let result =
        singleSupports
        |> Set.toSeq
        |> Seq.map (fun id ->
            let mutable toRemove = Set.singleton id // Initially removed the single supports
            let mutable removedBars = Set.empty // Accumulate all the removed bars (which include the single supports that don't fall)
            let mutable fallenBars = Set.empty // Accumulated the bars that "fall" because all supports have been removed

            while toRemove |> Set.count > 0 do
                removedBars <- Set.union removedBars toRemove

                toRemove <-
                    directlyBelow
                    |> Map.toSeq // Convert into Seq of (id*list_of_ids_below)
                    |> Seq.filter (fun (id, bids) -> not (removedBars |> Set.contains id)) // Those already removed
                    |> Seq.filter (fun (id, bids) -> bids <> [] && bids |> List.forall (fun bid -> removedBars |> Set.contains bid)) // Filter out those without any support
                    |> Seq.map (fun (id, _bids) -> id)
                    |> Set.ofSeq

                removedBars <- Set.union removedBars toRemove
                fallenBars <- Set.union fallenBars toRemove

            id, fallenBars)

    let solution = result |> Seq.sumBy (fun (_, fallen) -> fallen |> Set.count)
    solution

let public Solve () =
    printfn $"Solve from {typeof<Marker>.DeclaringType}"
    let data = fileio.linesFromFile "day22.txt"

    let stopWatch = System.Diagnostics.Stopwatch.StartNew()
    let solution = SolvePart1 data
    printfn "Part1 = %A" solution
    stopWatch.Stop()
    printfn "Elapsed time %ims" stopWatch.ElapsedMilliseconds
    assert (451 = solution)

    let stopWatch = System.Diagnostics.Stopwatch.StartNew()
    let solution = SolvePart2 data
    printfn "Part2 = %A" solution
    stopWatch.Stop()
    printfn "Elapsed time %ims" stopWatch.ElapsedMilliseconds
    assert (66530 = solution)

// #################################### //
open Xunit

type Tests() =
    let data =
        "1,0,1~1,2,1\n\
         0,0,2~2,0,2\n\
         0,2,3~2,2,3\n\
         0,0,4~0,2,4\n\
         2,0,5~2,2,5\n\
         0,1,6~2,1,6\n\
         1,1,8~1,1,9"

    [<Fact>]
    let ``Test Part1`` () =
        let data = fileio.linesFromString data
        let bars = parse_data data

        let segments: List<Segment<int>> =
            bars
            |> List.map (function
                | XAligned(id, x, y, z) -> Horizontal(id = id, y = y, x1 = x.from, x2 = x.too)
                | YAligned(id, x, y, z) -> Vertical(id = id, x = x, y1 = y.from, y2 = y.too)
                | ZAligned(id, x, y, z) -> Horizontal(id = id, y = y, x1 = x, x2 = x))

        // Extract intersecting IDs
        let pairs =
            findInteractions segments
            |> List.map (function
                | Intersection(p, h, v) -> h, v
                | CollinearOverlap(o) -> o.Id1, o.Id2)

        let expected =
            [ (5, 6); (1, 3); (5, 3); (2, 3); (1, 0); (5, 0); (6, 0); (2, 0); (1, 4); (5, 4); (2, 4) ]

        Assert.Equivalent(expected, pairs)

        let directlyBelow, topmost, multipleSupports, singleSupports, singleParents =
            drop_bars bars

        Assert.Equivalent(Map.ofList [ (0, []); (1, [ 0 ]); (2, [ 0 ]); (3, [ 2; 1 ]); (4, [ 2; 1 ]); (5, [ 4; 3 ]); (6, [ 5 ]) ], directlyBelow)
        Assert.Equivalent([ 0; 5 ], singleSupports |> Set.toList)
        Assert.Equivalent([ 1; 2; 3; 4 ], multipleSupports |> Set.toList)
        Assert.Equivalent(Map.ofList [ (0, [ 2; 1 ]); (5, [ 6 ]) ], singleParents)

        assert (Set.empty = Set.intersect (Set.union multipleSupports singleSupports) topmost)

        assert
            (directlyBelow
             |> Map.values
             |> Seq.concat
             |> Set.ofSeq
             |> Set.forall (fun bid -> not (topmost |> Set.contains bid)))

        let redundant = Set.union (Set.difference multipleSupports singleSupports) topmost

        Assert.Equivalent([ 1; 2; 3; 4; 6 ], redundant |> Set.toList)
        Assert.Equal(5, redundant |> Set.count)

    [<Fact>]
    let ``Test findIntersections`` () =
        // Example usage:
        let segments =
            [ Horizontal(id = 101, y = 2, x1 = 1, x2 = 5) // ID 101
              Vertical(id = 202, x = 3, y1 = 1, y2 = 4) // ID 202
              Vertical(id = 203, x = 1, y1 = 2, y2 = 2) // ID 203
              Horizontal(id = 204, y = 4, x1 = 2, x2 = 3) // ID 204
              Horizontal(id = 205, y = 2, x1 = 5, x2 = 7) ] // ID 205

        let results = findInteractions segments
        // Extract intersecting IDs
        let results =
            results
            |> List.map (function
                | Intersection(p, h, v) -> h, v
                | CollinearOverlap(o) -> o.Id1, o.Id2)

        let expected = [ (101, 205); (101, 203); (101, 202); (204, 202) ]
        Assert.Equivalent(expected, results)

        // Example Usage
        let segments =
            [ Horizontal(1, 10, 0, 100)
              Vertical(2, 50, 0, 20) // Normal intersection
              Horizontal(3, 10, 50, 150) ] // Collinear overlap with Segment 1

        let results = findInteractions segments

        let expected =
            [ CollinearOverlap
                  { Id1 = 1
                    Id2 = 3
                    Start = { X = 50; Y = 10 }
                    End = { X = 100; Y = 10 } }
              Intersection({ X = 50; Y = 10 }, 1, 2)
              Intersection({ X = 50; Y = 10 }, 3, 2) ]

        Assert.Equivalent(expected, results)

        // Overlap end
        let segments =
            [ Horizontal(id = 101, y = 2, x1 = 1, x2 = 5) // ID 101
              Horizontal(id = 205, y = 2, x1 = 5, x2 = 7) ] // ID 205

        let results = findInteractions segments

        let expected =
            [ CollinearOverlap
                  { Id1 = 101
                    Id2 = 205
                    Start = { X = 5; Y = 2 }
                    End = { X = 5; Y = 2 } } ]

        Assert.Equivalent(expected, results)

        // Example Usage
        let segments =
            [ Horizontal(id = 1, y = 10, x1 = 0, x2 = 10)
              Horizontal(id = 2, y = 10, x1 = 10, x2 = 10) // point
              Horizontal(id = 3, y = 10, x1 = 10, x2 = 20)
              Horizontal(id = 4, y = 10, x1 = 25, x2 = 25) // point
              Horizontal(id = 5, y = 10, x1 = 25, x2 = 25) ] // point

        let results = findInteractions segments

        let expected =
            [ CollinearOverlap
                  { Id1 = 1
                    Id2 = 2
                    Start = { X = 10; Y = 10 }
                    End = { X = 10; Y = 10 } }
              CollinearOverlap
                  { Id1 = 2
                    Id2 = 3
                    Start = { X = 10; Y = 10 }
                    End = { X = 10; Y = 10 } }
              CollinearOverlap
                  { Id1 = 1
                    Id2 = 3
                    Start = { X = 10; Y = 10 }
                    End = { X = 10; Y = 10 } }
              CollinearOverlap
                  { Id1 = 4
                    Id2 = 5
                    Start = { X = 25; Y = 10 }
                    End = { X = 25; Y = 10 } } ]

        Assert.Equivalent(expected, results)

        // Example Usage
        let segments =
            [ Vertical(id = 1, x = 10, y1 = 0, y2 = 10)
              Vertical(id = 2, x = 10, y1 = 10, y2 = 10) // point
              Vertical(id = 3, x = 10, y1 = 10, y2 = 20)
              Vertical(id = 4, x = 10, y1 = 25, y2 = 25) // point
              Vertical(id = 5, x = 10, y1 = 25, y2 = 25) ] // point

        let results = findInteractions segments

        let expected =
            [ CollinearOverlap
                  { Id1 = 1
                    Id2 = 2
                    Start = { X = 10; Y = 10 }
                    End = { X = 10; Y = 10 } }
              CollinearOverlap
                  { Id1 = 2
                    Id2 = 3
                    Start = { X = 10; Y = 10 }
                    End = { X = 10; Y = 10 } }
              CollinearOverlap
                  { Id1 = 1
                    Id2 = 3
                    Start = { X = 10; Y = 10 }
                    End = { X = 10; Y = 10 } }
              CollinearOverlap
                  { Id1 = 4
                    Id2 = 5
                    Start = { X = 10; Y = 25 }
                    End = { X = 10; Y = 25 } } ]

        Assert.Equivalent(expected, results)

        // Example Usage
        let segments =
            [ Vertical(id = 1, x = 10, y1 = 0, y2 = 20)
              Horizontal(id = 2, y = 10, x1 = 0, x2 = 20)
              Vertical(id = 3, x = 50, y1 = 10, y2 = 20)
              Horizontal(id = 4, y = 15, x1 = 50, x2 = 50) // point
              Horizontal(id = 5, y = 20, x1 = 50, x2 = 50) ] // point

        let results = findInteractions segments

        let expected =
            [ Intersection({ X = 10; Y = 10 }, 2, 1)
              Intersection({ X = 50; Y = 15 }, 4, 3)
              Intersection({ X = 50; Y = 20 }, 5, 3) ]

        Assert.Equivalent(expected, results)

    [<Fact>]
    let ``Test Part2`` () =
        let data = fileio.linesFromString data
        let bars = parse_data data

        let directlyBelow, topmost, multipleSupports, singleSupports, singleParents =
            drop_bars bars

        let result =
            singleSupports
            |> Set.toSeq
            |> Seq.map (fun id ->
                let mutable toRemove = Set.singleton id // Initially removed the single supports
                let mutable removedBars = Set.empty // Accumulate all the removed bars (which include the single supports that don't fall)
                let mutable fallenBars = Set.empty // Accumulated the bars that "fall" because all supports have been removed

                while toRemove |> Set.count > 0 do
                    removedBars <- Set.union removedBars toRemove

                    toRemove <-
                        directlyBelow
                        |> Map.toSeq // Convert into Seq of (id*list_of_ids_below)
                        |> Seq.filter (fun (id, bids) -> not (removedBars |> Set.contains id)) // Those already removed
                        |> Seq.filter (fun (id, bids) -> bids <> [] && bids |> List.forall (fun bid -> removedBars |> Set.contains bid)) // Filter out those without any support
                        |> Seq.map (fun (id, _bids) -> id)
                        |> Set.ofSeq

                    removedBars <- Set.union removedBars toRemove
                    fallenBars <- Set.union fallenBars toRemove

                id, fallenBars)

        Assert.Equivalent(
            seq {
                (0, set [ 1; 2; 3; 4; 5; 6 ])
                (5, set [ 6 ])
            },
            result
        )

        let solution = result |> Seq.sumBy (fun (_, fallen) -> fallen |> Set.count)
        Assert.Equal(7, solution)
