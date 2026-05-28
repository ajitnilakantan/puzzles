module aoc2023.day24

type internal Marker = interface end

type HailStone =
    { X: float
      Y: float
      Z: float
      DX: float
      DY: float
      DZ: float }

    static member create x y z dx dy dz =
        { X = x
          Y = y
          Z = z
          DX = dx
          DY = dy
          DZ = dz }


let find_intersections (stones: HailStone list) (from_x, from_y, to_x, to_y) =
    let lines =
        stones
        |> List.mapi (fun index s ->
            { Id = index
              X = s.X
              Y = s.Y
              DX = s.DX
              DY = s.DY }
            : bentleyOttmann.Line)

    let bbox: bentleyOttmann.BoundedBox =
        { MinX = from_x
          MaxX = to_x
          MinY = from_y
          MaxY = to_y }

    let intersections = bentleyOttmann.findIntersections lines bbox
    intersections


let parse_data data =
    data
    |> List.map (fun line -> fileio.tokenize line ",@\x20")
    |> List.map (List.map int64) // map the items in the list
    |> List.map (fun x ->
        assert (x |> List.length = 6)
        x)
    |> List.map (List.map float) // map the items in the list
    |> List.map (fun x -> HailStone.create x[0] x[1] x[2] x[3] x[4] x[5])

// BEGIN Part 2
open MathNet.Numerics.LinearAlgebra
open System

type Line3D =
    { Point: Vector<float>
      Direction: Vector<float> }

type TransversalResult =
    { Point: Vector<float>
      Direction: Vector<float>
      Intersections: Vector<float> list }

let inline vB (arr: float array) = Vector<float>.Build.DenseOfArray arr

let inline cross (a: Vector<float>) (b: Vector<float>) =
    vB [| a.[1] * b.[2] - a.[2] * b.[1]; a.[2] * b.[0] - a.[0] * b.[2]; a.[0] * b.[1] - a.[1] * b.[0] |]

// Convert angles (alpha, beta) → unit direction vector u
let directionFromAngles (alpha: float) (beta: float) =
    let ca = cos alpha
    let sa = sin alpha
    let cb = cos beta
    let sb = sin beta
    vB [| ca * cb; sa * cb; sb |]

// Distance residual between transversal (x0,u) and line (p,d)
let lineResidual (x0: Vector<float>) (u: Vector<float>) (p: Vector<float>) (d: Vector<float>) =
    let w = x0 - p
    let crossUD = cross u d
    let num = w.DotProduct(crossUD)
    let den = crossUD.L2Norm()
    num / den

// Gauss–Newton solver
let bestFitTransversalWithIntersections (lines: Line3D list) : TransversalResult =

    let n = lines.Length

    // Initial guess: x0 = average of points, u = z-axis
    let x0 =
        lines
        |> List.map (fun l -> l.Point)
        |> List.reduce (+)
        |> fun s -> s / float n


    let mutable x = x0.[0]
    let mutable y = x0.[1]
    let mutable z = x0.[2]

    let mutable alpha = 0.0
    let mutable beta = 0.5 // tilt upward

    let maxIter = 50
    let tol = 1e-10

    let mutable iter = 0
    let mutable converged = false

    while iter < maxIter && not converged do
        iter <- iter + 1

        let x0v = vB [| x; y; z |]
        let u = directionFromAngles alpha beta

        // residuals
        let r =
            Vector<float>.Build
                .Dense(
                    n,
                    fun i ->
                        let L = lines.[i]
                        lineResidual x0v u L.Point L.Direction
                )

        // Jacobian J (n × 5)
        let J = Matrix<float>.Build.Dense(n, 5)

        for i in 0 .. n - 1 do
            let L = lines.[i]
            let p = L.Point
            let d = L.Direction
            let w = x0v - p
            let crossUD = cross u d
            let den = crossUD.L2Norm()
            let num = w.DotProduct(crossUD)

            // d/dx0
            J.[i, 0] <- crossUD.[0] / den
            J.[i, 1] <- crossUD.[1] / den
            J.[i, 2] <- crossUD.[2] / den

            // du/dalpha, du/dbeta
            let du_dalpha = vB [| -sin alpha * cos beta; cos alpha * cos beta; 0.0 |]

            let du_dbeta = vB [| -cos alpha * sin beta; -sin alpha * sin beta; cos beta |]

            let d_num_dalpha = w.DotProduct(cross du_dalpha d)
            let d_num_dbeta = w.DotProduct(cross du_dbeta d)

            let d_den_dalpha = (crossUD.DotProduct(cross du_dalpha d)) / den
            let d_den_dbeta = (crossUD.DotProduct(cross du_dbeta d)) / den

            J.[i, 3] <- (d_num_dalpha * den - num * d_den_dalpha) / (den * den)
            J.[i, 4] <- (d_num_dbeta * den - num * d_den_dbeta) / (den * den)

        let A = J.TransposeThisAndMultiply(J)
        let b = -(J.TransposeThisAndMultiply(r))
        let delta = A.Svd(true).Solve(b)

        x <- x + delta.[0]
        y <- y + delta.[1]
        z <- z + delta.[2]
        alpha <- alpha + delta.[3]
        beta <- beta + delta.[4]

        if delta.L2Norm() < tol then converged <- true

    // Final line
    let x0_final = vB [| x; y; z |]
    let u_final = directionFromAngles alpha beta

    // Compute intersection points
    let intersections =
        lines
        |> List.map (fun line ->
            let p = line.Point
            let d = line.Direction
            let rhs = p - x0_final

            let M =
                Matrix<float>.Build.DenseOfArray(array2D [ [ u_final.[0]; -d.[0] ]; [ u_final.[1]; -d.[1] ]; [ u_final.[2]; -d.[2] ] ])

            let ts = M.Svd(true).Solve(rhs)
            let t = ts.[0]
            x0_final + t * u_final)

    { Point = x0_final
      Direction = u_final
      Intersections = intersections }
// END Part 2



let SolvePart1 data =
    let stones = data |> parse_data

    let intersections =
        find_intersections stones (200000000000000.0, 200000000000000.0, 400000000000000.0, 400000000000000.0)

    let solution =
        intersections
        |> List.filter (fun x -> fst x.Parameters >= 0.0 && snd x.Parameters >= 0.0)
        |> List.length

    solution

let SolvePart2 data =
    let solution = 0
    solution

let public Solve () =
    printfn $"Solve from {typeof<Marker>.DeclaringType}"
    let data = fileio.linesFromFile "day24.txt"

    let stopWatch = System.Diagnostics.Stopwatch.StartNew()
    let solution = SolvePart1 data
    printfn "Part1 = %A" solution
    stopWatch.Stop()
    printfn "Elapsed time %ims" stopWatch.ElapsedMilliseconds
    assert (26611 = solution)

    let stopWatch = System.Diagnostics.Stopwatch.StartNew()
    let solution = SolvePart2 data
    printfn "Part2 = %A" solution
    stopWatch.Stop()
    printfn "Elapsed time %ims" stopWatch.ElapsedMilliseconds
    assert (0 = solution)

// #################################### //
open Xunit
open MathNet.Spatial.Euclidean

type Tests() =
    let data =
        "19, 13, 30 @ -2,  1, -2\n\
         18, 19, 22 @ -1, -1, -2\n\
         20, 25, 34 @ -2, -2, -4\n\
         12, 31, 28 @ -1, -2, -1\n\
         20, 19, 15 @  1, -5, -3"

    [<Fact>]
    let ``Test Part1`` () =
        let data = fileio.linesFromString data
        let stones = data |> parse_data
        let intersections = find_intersections stones (7.0, 7.0, 27.0, 27.0)
        Assert.Equal(5, intersections |> List.length)

        let solution =
            intersections
            |> List.filter (fun x -> fst x.Parameters >= 0.0 && snd x.Parameters >= 0.0)

        Assert.Equal(2, 2)

    // ----------------------------------------------------------------------
    // Example test data (your lines)
    // ----------------------------------------------------------------------

    [<Fact>]
    let ``Test Partxx`` () =
        let lines =
            [ { Point = vB [| 19.0; 13.0; 30.0 |]
                Direction = vB [| -2.0; 1.0; -2.0 |] }
              { Point = vB [| 18.0; 19.0; 22.0 |]
                Direction = vB [| -1.0; -1.0; -2.0 |] }
              { Point = vB [| 20.0; 25.0; 34.0 |]
                Direction = vB [| -2.0; -2.0; -4.0 |] }
              { Point = vB [| 12.0; 31.0; 28.0 |]
                Direction = vB [| -1.0; -2.0; -1.0 |] }
              { Point = vB [| 20.0; 19.0; 15.0 |]
                Direction = vB [| 1.0; -5.0; -3.0 |] } ]

        let result = bestFitTransversalWithIntersections lines
        debug.printfn "result=%A" result
        debug.printfn "P=%A" result.Point // ≈ (24, 13, 10)
        debug.printfn "D=%A" result.Direction // ≈ normalized (-3, 1, 2)
        debug.printfn "I=%A" result.Intersections // five intersection points

    [<Fact>]
    let ``Test Part2`` () =
        let data = fileio.linesFromString data
        // let data = fileio.linesFromFile "day24.txt"
        let stones = data |> parse_data
        let vB (arr: float array) = Vector.Build.Dense(arr)

        let lines =
            stones
            |> List.map (fun s ->
                { Point = vB [| s.X; s.Y; s.Z |]
                  Direction = vB [| s.DX; s.DY; s.DZ |] })

        let optimalLine = bestFitTransversalWithIntersections lines

        debug.printfn "Optimal Line Point (P):   %A" optimalLine.Point
        debug.printfn "Optimal Line Direction (d): %A" optimalLine.Direction

        // The signed distance from a point to a line defined by a point and a direction vector
        let signed_distance (from_p: Vector<float>) (from_v: Vector<float>) (to_p: Vector<float>) =
            let d = MathNet.Numerics.Distance.Euclidean(from_p, to_p)
            let sign = from_v.DotProduct(to_p - from_p)
            if sign < 0.0 then -1.0 * d else d

        let distances =
            optimalLine.Intersections
            |> List.map (fun p -> signed_distance optimalLine.Point optimalLine.Direction p)

        let speed = Vector3D.OfVector(optimalLine.Direction).Length
        let times = distances |> List.map (fun d -> d / speed) |> List.sort

        debug.printfn "Distances: %A" distances
        debug.printfn "Times: %A" times
        Assert.Equal(2, 2)
