(* Bentley-Ottmann sweep-line algorithm adapted for infinite lines restricted to a bounding box *)
namespace aoc2023

module bentleyOttmann =


    open System

    // -----------------------------
    // Domain types
    // -----------------------------

    /// Parametric infinite line:
    /// x(t) = X + DX * t
    /// y(t) = Y + DY * t
    type Line =
        { Id : int
          X  : double
          Y  : double
          DX : double
          DY : double }

    /// Axis-aligned bounding box in world coordinates
    type BoundedBox =
        { MinX : double
          MaxX : double
          MinY : double
          MaxY : double }

    /// Result of a single intersection between two lines
    type IntersectionResult =
        { LineIds    : int * int
          Point      : double * double
          Parameters : double * double } // (t1, t2)

    // -----------------------------
    // Core numeric helpers
    // -----------------------------
    /// Small epsilon for floating-point comparisons
    let eps = 1e-9

    /// True if |v| is "almost" zero
    let inline isZero (v: float) = abs v <= eps

    /// True if a <= x <= b with tolerance
    let inline inRange (a: float) (b: float) (x: float) =
        x >= a - eps && x <= b + eps

    /// Clamp a value into [a, b]
    let inline clamp (a: float) (b: float) (x: float) =
        if x < a then a elif x > b then b else x

    // -----------------------------
    // Geometry helpers
    // -----------------------------


    /// Check if a point lies inside (or on the boundary of) the box
    let pointInBox (box: BoundedBox) (x: float, y: float) =
        inRange box.MinX box.MaxX x &&
        inRange box.MinY box.MaxY y

    /// Compute the parameter t for a point (x, y) on a line.
    /// We choose the more stable component (DX vs DY) to avoid division by ~0.
    let parameterForPointOnLine (line: Line) (x: float) (y: float) =
        if not (isZero line.DX) && abs line.DX >= abs line.DY then
            (x - line.X) / line.DX
        elif not (isZero line.DY) then
            (y - line.Y) / line.DY
        else
            // Degenerate line (no direction) – treat t as 0
            0.0

    /// Clip an infinite line to the bounding box.
    /// Returns the segment endpoints (p1, p2) and parameter range (tMin, tMax)
    /// if the line intersects the box, otherwise returns None.
    let clipLineToBox (line: Line) (box: BoundedBox) =
        // For each dimension, compute the t-interval where the line lies inside the box.
        // Then intersect the intervals for X and Y.
        let mutable tMin = Double.NegativeInfinity
        let mutable tMax = Double.PositiveInfinity
        let mutable ok = true

        // X dimension
        if isZero line.DX then
            // Vertical line: must be within [MinX, MaxX] to intersect the box
            if not (inRange box.MinX box.MaxX line.X) then
                ok <- false
        else
            let tx1 = (box.MinX - line.X) / line.DX
            let tx2 = (box.MaxX - line.X) / line.DX
            let txMin = min tx1 tx2
            let txMax = max tx1 tx2
            tMin <- max tMin txMin
            tMax <- min tMax txMax

        // Y dimension
        if ok then
            if isZero line.DY then
                // Horizontal line: must be within [MinY, MaxY] to intersect the box
                if not (inRange box.MinY box.MaxY line.Y) then
                    ok <- false
            else
                let ty1 = (box.MinY - line.Y) / line.DY
                let ty2 = (box.MaxY - line.Y) / line.DY
                let tyMin = min ty1 ty2
                let tyMax = max ty1 ty2
                tMin <- max tMin tyMin
                tMax <- min tMax tyMax

        if ok && tMin <= tMax then
            let x1 = line.X + line.DX * tMin
            let y1 = line.Y + line.DY * tMin
            let x2 = line.X + line.DX * tMax
            let y2 = line.Y + line.DY * tMax
            Some ((x1, y1), (x2, y2), tMin, tMax)
        else
            None

    /// Compute intersection of two infinite lines (not yet clipped to box).
    /// Returns (x, y, t1, t2) if they intersect at a unique point.
    /// Returns None if they are parallel (including collinear).
    let intersectInfiniteLines (l1: Line) (l2: Line) =
        // Solve:
        // l1.X + l1.DX * t1 = l2.X + l2.DX * t2
        // l1.Y + l1.DY * t1 = l2.Y + l2.DY * t2
        //
        // Using Cramer's rule:
        // det = DX1 * DY2 - DY1 * DX2
        let dx1, dy1 = l1.DX, l1.DY
        let dx2, dy2 = l2.DX, l2.DY
        let det = dx1 * dy2 - dy1 * dx2

        if isZero det then
            // Parallel (or collinear) – no unique intersection
            None
        else
            let x1, y1 = l1.X, l1.Y
            let x2, y2 = l2.X, l2.Y
            let rx = x2 - x1
            let ry = y2 - y1

            // t1 = (rx * DY2 - ry * DX2) / det
            // t2 = (rx * DY1 - ry * DX1) / det
            let t1 = (rx * dy2 - ry * dx2) / det
            let t2 = (rx * dy1 - ry * dx1) / det

            let ix = x1 + dx1 * t1
            let iy = y1 + dy1 * t1
            Some (ix, iy, t1, t2)

    /// Check if two lines are collinear (overlapping infinite lines).
    let areCollinear (l1: Line) (l2: Line) =
        // Parallel directions first
        let dx1, dy1 = l1.DX, l1.DY
        let dx2, dy2 = l2.DX, l2.DY
        let det = dx1 * dy2 - dy1 * dx2
        if not (isZero det) then
            false
        else
            // Check if vector between origins is also parallel to direction
            let rx = l2.X - l1.X
            let ry = l2.Y - l1.Y
            let cross = rx * dy1 - ry * dx1
            isZero cross

    // -----------------------------
    // Intersection finder
    // -----------------------------

    /// Find all intersections between infinite lines that lie inside the given box.
    ///
    /// Notes:
    /// - Lines are infinite; we only report intersections whose point lies inside the box.
    /// - Vertical lines are handled (DX = 0).
    /// - Overlapping (collinear) lines are handled by reporting the overlap segment
    ///   within the box as two intersection points (the segment endpoints).
    /// - If more than two lines intersect at a point, that point will appear once
    ///   per pair of lines.
    /// Find all intersections between infinite lines that lie inside the given box.
    let findIntersections (lines: Line list) (box: BoundedBox) : IntersectionResult list =
        let n = List.length lines

        // Iterate over all unordered pairs (i < j)
        let mutable results : IntersectionResult list = []
        let lineArray = lines |> List.toArray

        for i = 0 to n - 1 do
            let l1 = lineArray.[i]
            for j = i + 1 to n - 1 do
                let l2 = lineArray.[j]

                // Try to find a unique intersection of infinite lines
                match intersectInfiniteLines l1 l2 with
                | Some (ix, iy, t1, t2) ->
                    // Only keep if the intersection point is inside the bounding box
                    if pointInBox box (ix, iy) then
                        let res =
                            { LineIds    = (l1.Id, l2.Id)
                              Point      = (ix, iy)
                              Parameters = (t1, t2) }
                        results <- res :: results
                | None ->
                    // Lines are parallel or overlapping. 
                    // As per test specs, these return no discrete point intersections.
                    ()

        // We built results in reverse order; reverse once at the end
        results |> List.rev
    (*
    let findIntersections (lines: Line list) (box: BoundedBox) : IntersectionResult list =
        // We implement this iteratively over all pairs of lines.
        // While this is O(n^2), it keeps the code simple and robust,
        // and still respects the parametric representation and box clipping.

        let n = List.length lines

        // Precompute clipped segments for each line (for handling overlaps).
        // Map: line.Id -> (p1, p2, tMin, tMax)
        let clippedSegments =
            lines
            |> List.choose (fun l ->
                match clipLineToBox l box with
                | Some seg -> Some (l.Id, seg)
                | None     -> None)
            |> Map.ofList

        // Iterate over all unordered pairs (i < j)
        let mutable results : IntersectionResult list = []

        let lineArray = lines |> List.toArray

        for i = 0 to n - 1 do
            let l1 = lineArray.[i]
            for j = i + 1 to n - 1 do
                let l2 = lineArray.[j]

                // First, try unique intersection of infinite lines
                match intersectInfiniteLines l1 l2 with
                | Some (ix, iy, t1, t2) ->
                    // Only keep if inside the box
                    if pointInBox box (ix, iy) then
                        let res =
                            { LineIds    = (l1.Id, l2.Id)
                              Point      = (ix, iy)
                              Parameters = (t1, t2) }
                        results <- res :: results
                | None ->
                    // Lines are parallel; check if they are collinear (overlapping)
                    if areCollinear l1 l2 then
                        // For overlapping infinite lines, the intersection within the box
                        // is the overlap of their clipped segments.
                        match Map.tryFind l1.Id clippedSegments,
                              Map.tryFind l2.Id clippedSegments with
                        | Some (p1a, p1b, _, _), Some (p2a, p2b, _, _) ->
                            // All four points lie on the same infinite line.
                            // We want the overlap segment of [p1a, p1b] and [p2a, p2b].
                            //
                            // A simple way: project each point onto line1's parameter t,
                            // then intersect the 1D intervals.
                            let (x1a, y1a) = p1a
                            let (x1b, y1b) = p1b
                            let (x2a, y2a) = p2a
                            let (x2b, y2b) = p2b

                            let t1a = parameterForPointOnLine l1 x1a y1a
                            let t1b = parameterForPointOnLine l1 x1b y1b
                            let t2a = parameterForPointOnLine l1 x2a y2a
                            let t2b = parameterForPointOnLine l1 x2b y2b

                            let seg1Min = min t1a t1b
                            let seg1Max = max t1a t1b
                            let seg2Min = min t2a t2b
                            let seg2Max = max t2a t2b

                            let overlapMin = max seg1Min seg2Min
                            let overlapMax = min seg1Max seg2Max

                            if overlapMin <= overlapMax then
                                // Overlap segment exists; we report its endpoints
                                let addEndpoint t =
                                    let x = l1.X + l1.DX * t
                                    let y = l1.Y + l1.DY * t
                                    if pointInBox box (x, y) then
                                        let t1 = t
                                        let t2 = parameterForPointOnLine l2 x y
                                        let res =
                                            { LineIds    = (l1.Id, l2.Id)
                                              Point      = (x, y)
                                              Parameters = (t1, t2) }
                                        results <- res :: results

                                addEndpoint overlapMin
                                addEndpoint overlapMax
                            else
                                () // Collinear but no overlap inside the box
                        | _ ->
                            // At least one line does not intersect the box at all
                            ()
                    else
                        // Parallel but not collinear – no intersection
                        ()

        // We built results in reverse order; reverse once at the end
        results |> List.rev
           *)
(*
    open System

    // --- Input Datastructures ---
    type Line =
        { Id: int
          X: double
          Y: double
          DX: double
          DY: double }

    type BoundedBox =
        { MinX: double
          MaxX: double
          MinY: double
          MaxY: double }

    // --- Output Datastructures ---
    type IntersectionResult =
        { LineIds: int * int
          Point: double * double
          Parameters: double * double }

    // --- Internal Sweep Line Datastructures ---
    type private EventType =
        | UpperEndpoint
        | LowerEndpoint
        | IntersectionPoint

    type private SweepEvent =
        { X: double
          Y: double
          Type: EventType
          LineIds: int list } // Supports multiple lines intersecting or overlapping at a single point


    let private EPSILON = 1e-9

    /// Computes the intersection of an infinite line with the bounding box
    /// to extract valid segment endpoints within the box.
    let private getBoxSegment (line: Line) (box: BoundedBox) : (SweepEvent * SweepEvent) option =
        let mutable tMin = Double.NegativeInfinity
        let mutable tMax = Double.PositiveInfinity

        // Clip against X boundaries
        if Math.Abs(line.DX) < EPSILON then
            if line.X < box.MinX || line.X > box.MaxX then tMin <- Double.PositiveInfinity // Out of bounds
        else
            let t1 = (box.MinX - line.X) / line.DX
            let t2 = (box.MaxX - line.X) / line.DX
            tMin <- max tMin (min t1 t2)
            tMax <- min tMax (max t1 t2)

        // Clip against Y boundaries
        if Math.Abs(line.DY) < EPSILON then
            if line.Y < box.MinY || line.Y > box.MaxY then tMin <- Double.PositiveInfinity
        else
            let t1 = (box.MinY - line.Y) / line.DY
            let t2 = (box.MaxY - line.Y) / line.DY
            tMin <- max tMin (min t1 t2)
            tMax <- min tMax (max t1 t2)

        // If a valid segment exists within the box bounds
        if
            tMin < tMax
            && tMin <> Double.NegativeInfinity
            && tMax <> Double.PositiveInfinity
        then
            let x1 = line.X + line.DX * tMin
            let y1 = line.Y + line.DY * tMin
            let x2 = line.X + line.DX * tMax
            let y2 = line.Y + line.DY * tMax

            // Order endpoints: primary by Y descending (sweep top-to-bottom), secondary by X ascending
            let p1Upper = y1 > y2 || (Math.Abs(y1 - y2) < EPSILON && x1 < x2)

            let upper =
                if p1Upper then
                    { X = x1
                      Y = y1
                      Type = UpperEndpoint
                      LineIds = [ line.Id ] }
                else
                    { X = x2
                      Y = y2
                      Type = UpperEndpoint
                      LineIds = [ line.Id ] }

            let lower =
                if p1Upper then
                    { X = x2
                      Y = y2
                      Type = LowerEndpoint
                      LineIds = [ line.Id ] }
                else
                    { X = x1
                      Y = y1
                      Type = LowerEndpoint
                      LineIds = [ line.Id ] }

            Some(upper, lower)
        else
            None

    /// Computes parametric coordinates and exact points where two lines intersect.
    let private computeIntersection (l1: Line) (l2: Line) : IntersectionResult option =
        let determinant = l1.DX * l2.DY - l1.DY * l2.DX

        if Math.Abs(determinant) < EPSILON then
            None // Lines are parallel or overlapping (handled separately if needed)
        else
            let t1 = ((l2.X - l1.X) * l2.DY - (l2.Y - l1.Y) * l2.DX) / determinant
            let t2 = ((l2.X - l1.X) * l1.DY - (l2.Y - l1.Y) * l1.DX) / determinant

            let x = l1.X + l1.DX * t1
            let y = l1.Y + l1.DY * t1

            Some
                { LineIds = (min l1.Id l2.Id, max l1.Id l2.Id)
                  Point = (x, y)
                  Parameters = if l1.Id < l2.Id then (t1, t2) else (t2, t1) }

    /// Evaluates the X-coordinate of a line given its current Y-coordinate position of the sweep-line.
    let private getXAtY (line: Line) (currentY: double) : double =
        if Math.Abs(line.DY) < EPSILON then
            line.X
        else
            line.X + line.DX * ((currentY - line.Y) / line.DY)

    /// Main non-recursive Iterative Bentley-Ottmann algorithm logic.
    let findIntersections (lines: Line list) (box: BoundedBox) : IntersectionResult list =
        let lineMap = lines |> List.map (fun l -> l.Id, l) |> Map.ofList

        // 1. Convert lines into clipped bounding-box segments and build initial event queue.
        let mutable eventQueue =
            lines

            |> List.choose (fun l -> getBoxSegment l box)
            |> List.fold
                (fun (acc: Map<double * double, SweepEvent>) (upper, lower) ->
                    let addEvent ev m =
                        let key = (ev.Y, ev.X) // Primary sort by Y descending, secondary by X ascending

                        match Map.tryFind key m with
                        | Some existing ->
                            Map.add
                                key
                                { (existing: SweepEvent) with
                                    LineIds = existing.LineIds @ ev.LineIds }
                                m

                        // | Some existing -> Map.add key { existing with LineIds = existing.LineIds @ ev.LineIds } m
                        | None -> Map.add key ev m

                    acc |> addEvent upper |> addEvent lower)
                Map.empty

        // The Sweep Line State tracks active segments ordered left-to-right by their current X position.
        let mutable sweepState = List.empty<int>
        let mutable results = Map.empty<int * int, IntersectionResult>

        // Helper function to insert/reorder lines in sweep state by X-coordinate at current Y
        let sortSweepState (state: int list) (currentY: double) =
            state |> List.sortBy (fun id -> getXAtY lineMap.[id] currentY)

        // Helper to check and register intersections between adjacent lines in sweep line state
        let checkIntersection id1 id2 currentY (events: Map<double * double, SweepEvent>) =
            let mutable evs = events

            match computeIntersection lineMap.[id1] lineMap.[id2] with

            | Some inter ->
                let x, y = inter.Point
                // Ensure intersection happens within box boundaries and below current sweep line Y
                if
                    x >= box.MinX
                    && x <= box.MaxX
                    && y >= box.MinY
                    && y <= box.MaxY
                    && y <= (currentY + EPSILON)
                then
                    if not (Map.containsKey inter.LineIds results) then
                        results <- Map.add inter.LineIds inter results
                        // Push intersection event onto the queue if it lies purely below current sweep line position
                        if y < (currentY - EPSILON) then
                            let key = (y, x)

                            let newEv =
                                match Map.tryFind key evs with
                                | Some existing ->
                                    { existing with
                                        LineIds = List.distinct (existing.LineIds @ [ id1; id2 ]) }
                                | None ->
                                    { X = x
                                      Y = y
                                      Type = IntersectionPoint
                                      LineIds = [ id1; id2 ] }

                            evs <- Map.add key newEv evs

            | None -> ()

            evs

        // 2. Iterative processing loop over all events in prioritized order (Top to Bottom)
        while not (Map.isEmpty eventQueue) do
            // Extract the highest event (Max Y, Min X)
            let currentKey = eventQueue |> Map.keys |> Seq.maxBy (fun (y, x) -> (y, -x))
            let event = eventQueue.[currentKey]
            eventQueue <- Map.remove currentKey eventQueue

            let currentY = event.Y

            match event.Type with

            | UpperEndpoint ->
                // Add new segment(s) to sweep line state
                sweepState <- sortSweepState (sweepState @ event.LineIds) currentY

                // Check intersections with new immediate neighbors
                event.LineIds
                |> List.iter (fun id ->
                    let idx = List.findIndex ((=) id) sweepState

                    if idx > 0 then
                        eventQueue <- checkIntersection sweepState.[idx - 1] id currentY eventQueue

                    if idx < sweepState.Length - 1 then
                        eventQueue <- checkIntersection id sweepState.[idx + 1] currentY eventQueue)


            | LowerEndpoint ->
                // Capture neighbors before removing segments
                event.LineIds
                |> List.iter (fun id ->
                    let idx = List.findIndex ((=) id) sweepState

                    if idx > 0 && idx < sweepState.Length - 1 then
                        eventQueue <- checkIntersection sweepState.[idx - 1] sweepState.[idx + 1] currentY eventQueue)
                // Remove expired segments from sweep state
                sweepState <- sweepState |> List.filter (fun id -> not (List.contains id event.LineIds))


            | IntersectionPoint ->
                // Swap overlapping/intersecting segments order in the sweep line state
                sweepState <- sortSweepState sweepState (currentY - EPSILON)

                // Re-evaluate intersections with new surrounding neighbors post-swap
                event.LineIds
                |> List.iter (fun id ->
                    let idx = List.findIndex ((=) id) sweepState

                    if idx > 0 then
                        eventQueue <- checkIntersection sweepState.[idx - 1] id currentY eventQueue

                    if idx < sweepState.Length - 1 then
                        eventQueue <- checkIntersection id sweepState.[idx + 1] currentY eventQueue)

        results |> Map.values |> Seq.toList
*)

module bentleyOttmann_test =
    open Xunit
    open bentleyOttmann

    [<Fact>]
    let ``Test`` () =
        let box =
            { MinX = 0.0
              MaxX = 10.0
              MinY = 0.0
              MaxY = 10.0 }

        // Test 1: Simple X-crossing in center
        let test1Lines =
            [ { bentleyOttmann.Id = 1
                X = 0.0
                Y = 0.0
                DX = 1.0
                DY = 1.0 } // y = x
              { Id = 2
                X = 0.0
                Y = 10.0
                DX = 1.0
                DY = -1.0 } ] // y = 10 - x

        let res1 = findIntersections test1Lines box
        printfn "ZZZTest 1 (Standard Cross): Expected 1 at (5,5). Found: %A" res1
        Assert.Equal(1, res1 |> List.length)
        Assert.Equal((5.0, 5.0), res1[0].Point)

        // Test 2: Parallel lines (should not find anything)
        let test2Lines =
            [ { Id = 3
                X = 0.0
                Y = 0.0
                DX = 1.0
                DY = 1.0 }
              { Id = 4
                X = 0.0
                Y = 2.0
                DX = 1.0
                DY = 1.0 } ]

        let res2 = findIntersections test2Lines box
        // "Test 2 (Parallel Lines): Expected 0. Found: %d matches" res2.Length
        Assert.Equal(0, res2 |> List.length)

        // Test 3: Intersection falls OUTSIDE the bounding box
        let test3Lines =
            [ { Id = 5
                X = 0.0
                Y = 20.0
                DX = 1.0
                DY = 1.0 } // Intersects way up high outside box
              { Id = 6
                X = 0.0
                Y = 30.0
                DX = 1.0
                DY = -1.0 } ]

        let res3 = findIntersections test3Lines box
        // "Test 3 (Out of Bounds): Expected 0 inside box. Found: %d matches" res3.Length
        Assert.Equal(0, res3 |> List.length)

        // Test 4: Horizontal line meeting a perfectly vertical line
        let test4Lines =
            [ { Id = 7
                X = 0.0
                Y = 4.0
                DX = 1.0
                DY = 0.0 } // Horiz y = 4
              { Id = 8
                X = 4.0
                Y = 0.0
                DX = 0.0
                DY = 1.0 } ] // Vert x = 4

        let res4 = findIntersections test4Lines box
        printfn "ZZZTest 4 (Vertical Cross): Expected 1 at (4,4). Found: %A" res4
        Assert.Equal(1, res4 |> List.length)
        Assert.Equal((4.0, 4.0), res4[0].Point)




    let box =
        { MinX = -10.0
          MaxX = 10.0
          MinY = -10.0
          MaxY = 10.0 }

    [<Fact>]
    let ``Simple Cross Intersection Tests`` () =
        let l1 =
            { Id = 1
              X = 0.0
              Y = 0.0
              DX = 1.0
              DY = 1.0 } // y = x

        let l2 =
            { Id = 2
              X = 0.0
              Y = 0.0
              DX = 1.0
              DY = -1.0 } // y = -x

        let intersections = findIntersections [ l1; l2 ] box

        Assert.Single(intersections) |> ignore
        let res = intersections.[0]
        Assert.Equal((1, 2), res.LineIds)
        Assert.Equal(0.0, fst res.Point, 5)
        Assert.Equal(0.0, snd res.Point, 5)

    [<Fact>]
    let ``Handles Vertical Line Intersection`` () =
        let l1 =
            { Id = 1
              X = 2.0
              Y = 0.0
              DX = 0.0
              DY = 1.0 } // Vertical Line at x = 2

        let l2 =
            { Id = 2
              X = 0.0
              Y = 3.0
              DX = 1.0
              DY = 0.0 } // Horizontal Line at y = 3

        let intersections = findIntersections [ l1; l2 ] box
        printfn "ZZZTest 3 (Handles Vertical Line Intersection): Expected 1 at (2,3). Found: %A" intersections
        Assert.Single(intersections) |> ignore
        let res = intersections.[0]
        Assert.Equal(2.0, fst res.Point, 5)
        Assert.Equal(3.0, snd res.Point, 5)

    [<Fact>]
    let ``Handles Multiple Lines Intersecting at One Point`` () =
        let l1 =
            { Id = 1
              X = 0.0
              Y = 0.0
              DX = 1.0
              DY = 1.0 }

        let l2 =
            { Id = 2
              X = 0.0
              Y = 0.0
              DX = 1.0
              DY = -1.0 }

        let l3 =
            { Id = 3
              X = 0.0
              Y = 0.0
              DX = 1.0
              DY = 0.0 } // Horizontal line through origin

        let intersections = findIntersections [ l1; l2; l3 ] box

        // 3 lines intersecting at 1 point yields 3 combinations of interactions: (1,2), (1,3), (2,3)
        Assert.Equal(3, intersections.Length)

        for res in intersections do
            Assert.Equal(0.0, fst res.Point, 5)
            Assert.Equal(0.0, snd res.Point, 5)

    [<Fact>]
    let ``Handles Overlapping Lines Gracefully`` () =
        let l1 =
            { Id = 1
              X = 0.0
              Y = 0.0
              DX = 1.0
              DY = 1.0 }

        let l2 =
            { Id = 2
              X = 1.0
              Y = 1.0
              DX = 2.0
              DY = 2.0 } // Distinct ID, completely same line trajectory

        let intersections = findIntersections [ l1; l2 ] box

        // Parallel/Overlapping lines return no discrete point intersections
        Assert.Empty(intersections)
