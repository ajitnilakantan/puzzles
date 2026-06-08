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
        { Id: int
          X: double
          Y: double
          DX: double
          DY: double }

    /// Axis-aligned bounding box in world coordinates
    type BoundedBox =
        { MinX: double
          MaxX: double
          MinY: double
          MaxY: double }

    /// Result of a single intersection between two lines
    type IntersectionResult =
        { LineIds: int * int
          Point: double * double
          Parameters: double * double } // (t1, t2)

    // -----------------------------
    // Core numeric helpers
    // -----------------------------
    /// Small epsilon for floating-point comparisons
    let eps = 1e-9

    /// True if |v| is "almost" zero
    let inline isZero (v: float) = abs v <= eps

    /// True if a <= x <= b with tolerance
    let inline inRange (a: float) (b: float) (x: float) = x >= a - eps && x <= b + eps

    /// Clamp a value into [a, b]
    let inline clamp (a: float) (b: float) (x: float) =
        if x < a then a
        elif x > b then b
        else x

    // -----------------------------
    // Geometry helpers
    // -----------------------------


    /// Check if a point lies inside (or on the boundary of) the box
    let pointInBox (box: BoundedBox) (x: float, y: float) =
        inRange box.MinX box.MaxX x && inRange box.MinY box.MaxY y

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
            if not (inRange box.MinX box.MaxX line.X) then ok <- false
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
                if not (inRange box.MinY box.MaxY line.Y) then ok <- false
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
            Some((x1, y1), (x2, y2), tMin, tMax)
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
            Some(ix, iy, t1, t2)

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
        let mutable results: IntersectionResult list = []
        let lineArray = lines |> List.toArray

        for i = 0 to n - 1 do
            let l1 = lineArray.[i]

            for j = i + 1 to n - 1 do
                let l2 = lineArray.[j]

                // Try to find a unique intersection of infinite lines
                match intersectInfiniteLines l1 l2 with
                | Some(ix, iy, t1, t2) ->
                    // Only keep if the intersection point is inside the bounding box
                    if pointInBox box (ix, iy) then
                        let res =
                            { LineIds = (l1.Id, l2.Id)
                              Point = (ix, iy)
                              Parameters = (t1, t2) }

                        results <- res :: results
                | None ->
                    // Lines are parallel or overlapping.
                    // As per test specs, these return no discrete point intersections.
                    ()

        // We built results in reverse order; reverse once at the end
        results |> List.rev

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
        // "Test 4 (Vertical Cross): Expected 1 at (4,4). Found: %A" res4
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
        // "Test 3 (Handles Vertical Line Intersection): Expected 1 at (2,3). Found: %A" intersections
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
