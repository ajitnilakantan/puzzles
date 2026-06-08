module aoc2023.day24

open System.Numerics
open MathNet.Numerics // Requires MathNet.Numerics and MathNet.Numerics.FSharp packages
open Microsoft.FSharp.Core.Operators.Checked

type internal Marker = interface end

[<StructuredFormatDisplay("X:{X} Y:{Y} Z:{Z} D:({DX},{DY},{DZ})")>]
type HailStone<'T> =
    { X: 'T
      Y: 'T
      Z: 'T
      DX: 'T
      DY: 'T
      DZ: 'T }

    static member create x y z dx dy dz =
        { X = x
          Y = y
          Z = z
          DX = dx
          DY = dy
          DZ = dz }


// Part 1
let find_intersections (stones: HailStone<float> list) (from_x, from_y, to_x, to_y) =
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



// Converts any type 'Src to any type 'Dst, provided an explicit conversion exists.
// Clear, safe, and completely native to modern .NET
let inline fromInt64Generic<'Dst when 'Dst :> INumber<'Dst>> (value: int64) : 'Dst = 'Dst.CreateChecked value

let parse_data<'T when 'T :> INumber<'T>> data =
    data
    |> List.map (fun line -> fileio.tokenize line ",@\x20")
    |> List.map (List.map int64) // map the items in the list
    |> List.map (fun x ->
        assert (x |> List.length = 6)
        x)
    |> List.map (List.map (fun x -> fromInt64Generic<'T> x))
    |> List.map (fun x -> HailStone<'T>.create x[0] x[1] x[2] x[3] x[4] x[5])



let parse_data2 data =
    data
    |> List.map (fun line -> fileio.tokenize line ",@\x20")
    |> List.map (List.map int64) // map the items in the list
    |> List.map (List.map (fun x -> BigRational.FromBigInt(bigint x)))
    |> List.map (fun x -> HailStone<BigRational>.create x[0] x[1] x[2] x[3] x[4] x[5])


// BEGIN PART2

// Represents a 3D Vector using arbitrary precision BigRational
// type BVec3 = { X: BigRational; Y: BigRational; Z: BigRational }
type BVec3 =
    val X: BigRational
    val Y: BigRational
    val Z: BigRational
    // Constructor to initialize fields
    new(x, y, z) = { X = x; Y = y; Z = z }

    override v.ToString() =
        sprintf "(%s, %s, %s)" (v.X.ToString()) (v.Y.ToString()) (v.Z.ToString())

    override this.Equals(obj) =
        match obj with
        | :? BVec3 as other -> this.X = other.X && this.Y = other.Y && this.Z = other.Z
        | _ -> false
    // Override GetHashCode using a standard hash combining algorithm
    override this.GetHashCode() =
        let inline hashCombine h1 h2 = (h1 * 397) ^^^ h2
        let hX = hash this.X
        let hY = hash this.Y
        let hZ = hash this.Z
        hashCombine (hashCombine hX hY) hZ

module BVec3 =
    let zero = BVec3(BigRational.Zero, BigRational.Zero, BigRational.Zero)

    let create (x: int64) (y: int64) (z: int64) =
        BVec3(BigRational.FromBigInt(bigint x), BigRational.FromBigInt(bigint y), BigRational.FromBigInt(bigint z))

    let add (a: BVec3) (b: BVec3) = BVec3(a.X + b.X, a.Y + b.Y, a.Z + b.Z)
    let sub (a: BVec3) (b: BVec3) = BVec3(a.X - b.X, a.Y - b.Y, a.Z - b.Z)
    let scale (s: BigRational) (v: BVec3) = BVec3(s * v.X, s * v.Y, s * v.Z)
    let dot (a: BVec3) (b: BVec3) = a.X * b.X + a.Y * b.Y + a.Z * b.Z

    let cross (a: BVec3) (b: BVec3) =
        BVec3(a.Y * b.Z - a.Z * b.Y, a.Z * b.X - a.X * b.Z, a.X * b.Y - a.Y * b.X)

    let areParallel (d1: BVec3) (d2: BVec3) =
        let c = cross d1 d2
        c.X.IsZero && c.Y.IsZero && c.Z.IsZero

    let lengthSquared v = dot v v

    let isInteger (v: BVec3) =
        v.X.IsInteger && v.Y.IsInteger && v.Z.IsInteger

// -------------------------------------------------
// Line type
// -------------------------------------------------
type Line3D = { P: BVec3; V: BVec3 }

// -------------------------------------------------
// Helper: exact square root of a BigRational (returns Some if perfect square)
// -------------------------------------------------
module BigRationalHelpers =
    /// Newton‑Raphson square root for BigInteger
    let sqrt (n: BigInteger) =
        if n < 0I then
            invalidArg "n" "square root of negative number"
        elif n = 0I then
            0I
        else
            let rec loop x =
                let x' = (x + n / x) >>> 1
                if x' >= x then x else loop x'

            loop (n >>> 1)

    let trySqrt (x: BigRational) =
        if x.Numerator < 0I then
            None
        else
            let sqrtNum = sqrt (x.Numerator)
            let sqrtDen = sqrt (x.Denominator)

            if sqrtNum * sqrtNum = x.Numerator && sqrtDen * sqrtDen = x.Denominator then
                Some(BigRational.FromBigIntFraction(sqrtNum, sqrtDen))
            else
                None

/// Converts a BigRational direction vector to integer by finding the smallest integer representation
let toIntegerDirection (v: BVec3) =
    let xn, xd = v.X.Numerator, v.X.Denominator
    let yn, yd = v.Y.Numerator, v.Y.Denominator
    let zn, zd = v.Z.Numerator, v.Z.Denominator

    // Find common denominator for all three components
    let lcm_xy = xd * yd / BigInteger.GreatestCommonDivisor(xd, yd)
    let commonDenom = lcm_xy * zd / BigInteger.GreatestCommonDivisor(lcm_xy, zd)

    // Scale all components to have common denominator
    let xNum = xn * (commonDenom / xd)
    let yNum = yn * (commonDenom / yd)
    let zNum = zn * (commonDenom / zd)

    // Find GCD of numerators to reduce
    let g =
        BigInteger.GreatestCommonDivisor(xNum, BigInteger.GreatestCommonDivisor(yNum, zNum))

    let finalX = xNum / g
    let finalY = yNum / g
    let finalZ = zNum / g

    let finalDenominator = commonDenom / g

    BVec3(BigRational.FromBigInt finalX, BigRational.FromBigInt finalY, BigRational.FromBigInt finalZ), finalDenominator

// -------------------------------------------------
// Null‑space computation via Gaussian elimination
// -------------------------------------------------
module LinearAlgebra =
    /// Find a basis for the null space of a matrix (rows x cols).
    /// Returns a list of basis vectors (arrays of BigRational).
    let nullspace (matrix: BigRational[][]) =
        let rows = matrix.Length
        let cols = matrix.[0].Length
        let m = matrix |> Array.map (fun row -> Array.copy row)
        let mutable pivotCols = []
        let mutable r = 0
        let mutable c = 0

        while r < rows && c < cols do
            // Find pivot row
            let mutable pivot = None

            for i = r to rows - 1 do
                if not (m.[i].[c].IsZero) && pivot.IsNone then pivot <- Some i

            match pivot with
            | None -> c <- c + 1
            | Some pr ->
                // Swap rows
                let tmp = m.[r]
                m.[r] <- m.[pr]
                m.[pr] <- tmp
                // Normalize pivot row
                let pivVal = m.[r].[c]

                for j in 0 .. cols - 1 do
                    m.[r].[j] <- m.[r].[j] / pivVal
                // Eliminate other rows
                for i in 0 .. rows - 1 do
                    if i <> r && not (m.[i].[c].IsZero) then
                        let factor = m.[i].[c]

                        for j in 0 .. cols - 1 do
                            m.[i].[j] <- m.[i].[j] - factor * m.[r].[j]

                pivotCols <- c :: pivotCols
                r <- r + 1
                c <- c + 1

        let pivotSet = set pivotCols

        let freeCols =
            [ 0 .. cols - 1 ] |> List.filter (fun c -> not (Set.contains c pivotSet))
        // Build basis vectors for each free column
        freeCols
        |> List.map (fun freeCol ->
            let vec = Array.create cols BigRational.Zero
            vec.[freeCol] <- BigRational.One

            for r in 0 .. rows - 1 do
                let pivotCol = m.[r] |> Array.tryFindIndex (fun x -> not x.IsZero)

                match pivotCol with
                | Some pc when Set.contains pc pivotSet ->
                    let mutable sum = BigRational.Zero

                    for j in 0 .. cols - 1 do
                        if j <> pc then sum <- sum + m.[r].[j] * vec.[j]

                    vec.[pc] <- -sum
                | _ -> ()

            vec)
// -------------------------------------------------
// Core function: find all integer‑intersecting transversals
// -------------------------------------------------
let findIntegerTransversals (lines: Line3D[]) : (BigRational[] * BVec3 * BVec3) list =
    if lines.Length <> 4 then invalidArg "lines" "Exactly four lines required"
    let moments = lines |> Array.map (fun l -> BVec3.cross l.P l.V)

    let eqMatrix =
        Array.init 4 (fun i ->
            let m = moments.[i]
            let v = lines.[i].V
            [| m.X; m.Y; m.Z; v.X; v.Y; v.Z |])

    let basis = LinearAlgebra.nullspace eqMatrix
    let candidates = ResizeArray()

    let extract3 (arr: BigRational array) off =
        BVec3(arr.[off], arr.[off + 1], arr.[off + 2])

    match basis with
    | [ u; v ] ->
        let uDir = extract3 u 0
        let uMom = extract3 u 3
        let vDir = extract3 v 0
        let vMom = extract3 v 3
        let A = BVec3.dot uDir uMom
        let B = BVec3.dot uDir vMom + BVec3.dot vDir uMom
        let C = BVec3.dot vDir vMom

        let solveQuadratic (a: BigRational) (b: BigRational) (c: BigRational) =
            if a.IsZero then
                if b.IsZero then
                    if c.IsZero then [] else []
                else
                    [ -c / b ]
            else
                let disc = b * b - BigRational.FromInt(4) * a * c

                match BigRationalHelpers.trySqrt disc with
                | None -> []
                | Some sqrtDisc ->
                    let twoA = BigRational.FromInt(2) * a
                    [ (-b + sqrtDisc) / twoA; (-b - sqrtDisc) / twoA ]

        let alphaCandidates =
            solveQuadratic A B C
            @ (if A.IsZero && C.IsZero then [ BigRational.One ] else [])

        let alphaCandidates = List.distinct alphaCandidates

        for alpha in alphaCandidates do
            let beta =
                if A.IsZero && C.IsZero && alpha = BigRational.One then
                    BigRational.Zero
                else
                    BigRational.One

            let dir = BVec3.add (BVec3.scale alpha uDir) (BVec3.scale beta vDir)
            let mmt = BVec3.add (BVec3.scale alpha uMom) (BVec3.scale beta vMom)

            if dir = BVec3.zero then
                ()
            else
                let dirLenSq = BVec3.lengthSquared dir

                if dirLenSq.IsZero then
                    ()
                else
                    let qPoint = BVec3.scale (BigRational.One / dirLenSq) (BVec3.cross dir mmt)

                    let us =
                        [| for line in lines do
                               let rhs = BVec3.sub line.P qPoint
                               let mutable uVal = BigRational.Zero
                               let mutable found = false

                               for i in 0..2 do
                                   if not found then
                                       let a11 = [| dir.X; dir.Y; dir.Z |].[i]
                                       let a12 = [| -line.V.X; -line.V.Y; -line.V.Z |].[i]
                                       let a21 = [| dir.X; dir.Y; dir.Z |].[(i + 1) % 3]
                                       let a22 = [| -line.V.X; -line.V.Y; -line.V.Z |].[(i + 1) % 3]
                                       let det = a11 * a22 - a12 * a21

                                       if not det.IsZero then
                                           let b1 = [| rhs.X; rhs.Y; rhs.Z |].[i]
                                           let b2 = [| rhs.X; rhs.Y; rhs.Z |].[(i + 1) % 3]
                                           uVal <- (b1 * a21 - b2 * a11) / det
                                           found <- true

                               if not found then failwith "no intersection computed"
                               uVal |]

                    let allInt =
                        Array.forall2
                            (fun line u ->
                                let pt = BVec3.add line.P (BVec3.scale u line.V)
                                BVec3.isInteger pt)
                            lines
                            us

                    if allInt then candidates.Add(us, qPoint, dir)
    | _ -> ()

    List.ofSeq candidates


/// Computes the intersection of two lines in 3D, if it exists.
/// Returns the intersection point and the parameters along each line.
/// Returns None if the lines are parallel, skew, or collinear (no unique intersection).
let findIntersectionBigRational ((p1, d1): BVec3 * BVec3) ((p2, d2): BVec3 * BVec3) : Option<BVec3 * BigRational * BigRational> =
    // Extract values for cleaner formulas
    let x1, y1, z1 = p1.X, p1.Y, p1.Z
    let a1, b1, c1 = d1.X, d1.Y, d1.Z

    let x2, y2, z2 = p2.X, p2.Y, p2.Z
    let a2, b2, c2 = d2.X, d2.Y, d2.Z

    // We need to solve a 2x2 system from X and Y components:
    // a1*t - a2*s = x2 - x1
    // b1*t - b2*s = y2 - y1
    let det = (-a1 * b2) - (-a2 * b1)

    // Check if the lines are parallel in the XY projection
    if det.IsZero then
        None
    else
        let dx = x2 - x1
        let dy = y2 - y1

        // Use Cramer's Rule to solve for t and s
        let t = (dx * -b2 - -a2 * dy) / det
        let s = (a1 * dy - dx * b1) / det

        // Check if the calculated t and s satisfy the Z component (Verification step)
        let zL1 = z1 + c1 * t
        let zL2 = z2 + c2 * s

        // Use a small tolerance for floating-point comparisons
        if (zL1 - zL2).IsZero then
            // Calculate and return the final intersection point
            let intersection = BVec3(x1 + a1 * t, y1 + b1 * t, zL1)
            Some(intersection, t, s)
        else
            None // The lines are skew

// Pick any 3 stones that do not point in the same direction
let pick_three_stones (stones: HailStone<BigRational> list) (stone: HailStone<BigRational>) =
    let sv = BVec3(stone.DX, stone.DY, stone.DZ)

    let stone1 =
        stones
        |> List.find (fun s -> s <> stone && not (BVec3.areParallel sv (BVec3(s.DX, s.DY, s.DZ))))

    let s1v = BVec3(stone1.DX, stone1.DY, stone1.DZ)

    let stone2 =
        stones
        |> List.find (fun s ->
            s <> stone
            && s <> stone1
            && not (BVec3.areParallel sv (BVec3(s.DX, s.DY, s.DZ)))
            && not (BVec3.areParallel s1v (BVec3(s.DX, s.DY, s.DZ))))

    let s2v = BVec3(stone2.DX, stone2.DY, stone2.DZ)

    let stone3 =
        stones
        |> List.find (fun s ->
            s <> stone
            && s <> stone1
            && not (BVec3.areParallel sv (BVec3(s.DX, s.DY, s.DZ)))
            && not (BVec3.areParallel s1v (BVec3(s.DX, s.DY, s.DZ)))
            && not (BVec3.areParallel s2v (BVec3(s.DX, s.DY, s.DZ))))

    stone1, stone2, stone3


// Checks whether the lines  L1 = p1 + t * d1  and  L2 = p2 + s * d2  intersect
// at a point that lies on both rays (t >= 0, s >= 0).
//  Scalar triple product
// Returns true if such an intersection exists, false otherwise.
let doLinesIntersectPositive (p1: BVec3) (d1: BVec3) (p2: BVec3) (d2: BVec3) =
    match findIntersectionBigRational (p1, d1) (p2, d2) with
    | None -> false
    | Some(_intersection, _u1, u2) -> u2 > 0N


let is_valid_solution lines stones stone =
    lines
    |> List.tryPick (fun (_u, p, v) ->
        if
            stones
            |> List.forall (fun s ->
                s = stone
                || doLinesIntersectPositive p v (BVec3(s.X, s.Y, s.Z)) (BVec3(s.DX, s.DY, s.DZ)))
        then
            Some(p, fst (toIntegerDirection v))
        else
            None)

let find_thrown_point p v stones =
    let u =
        stones
        |> List.map (fun s ->
            let intersection =
                findIntersectionBigRational (p, v) ((BVec3(s.X, s.Y, s.Z)), (BVec3(s.DX, s.DY, s.DZ)))

            match intersection with
            | None -> failwith "Error: expected to find intersection"
            | Some(pos, u1, u2) -> pos, u1, u2)

    let minPos =
        u
        |> List.mapi (fun idx value -> idx, value) // Pair each item with its index
        |> List.minBy (fun (_idx, (_pos, _u1, u2)) -> u2) // Find the minimum based on the value (u2 item)
        |> fst // Extract the index

    let maxPos =
        u
        |> List.mapi (fun idx value -> idx, value) // Pair each item with its index
        |> List.minBy (fun (_idx, (_pos, _u1, u2)) -> u2) // Find the minimum based on the value (u2 item)
        |> fst // Extract the index

    // The first hailstone to be hit is at "u" offset u[minPos].
    let intersection, minU1, minU2 = u[minPos]
    let _, maxU1, _ = u[maxPos]
    let pos1 = BVec3(p.X + minU1 * v.X, p.Y + minU1 * v.Y, p.Z + minU1 * v.Z)

    let pos2 =
        BVec3(stones[minPos].X + minU2 * stones[minPos].DX, stones[minPos].Y + minU2 * stones[minPos].DY, stones[minPos].Z + minU2 * stones[minPos].DZ)

    assert (pos1 = pos2) // Intersections should match
    assert (pos1 = intersection) // Intersections should match
    // offset this by -v * minU (time delay to first hit) depending on which side we are approaching the first line from
    let p =
        if maxU1 > minU1 then
            BVec3(intersection.X - minU2 * v.X, intersection.Y - minU2 * v.Y, intersection.Z - minU2 * v.Z)
        else
            BVec3(intersection.X + minU2 * v.X, intersection.Y + minU2 * v.Y, intersection.Z + minU2 * v.Z)

    p, v
// END PART2B

let SolvePart1 data =
    let stones = data |> parse_data<float>

    let intersections =
        find_intersections stones (200000000000000.0, 200000000000000.0, 400000000000000.0, 400000000000000.0)

    let solution =
        intersections
        |> List.filter (fun x -> fst x.Parameters >= 0.0 && snd x.Parameters >= 0.0)
        |> List.length

    solution

let SolvePart2 data =
    let stones = data |> parse_data2

    let result =
        stones
        // Pick a stone, one at a time
        |> List.tryPick (fun stone ->
            // Pick three stones that are not parallel
            let stone1, stone2, stone3 = pick_three_stones stones stone
            // Find the line, starting at the the point after 1ns on the picked stone that goes through the two stones picked above
            let fourLines =
                [| { P = BVec3(stone.X, stone.Y, stone.Z)
                     V = BVec3(stone.DX, stone.DY, stone.DZ) }
                   { P = BVec3(stone1.X, stone1.Y, stone1.Z)
                     V = BVec3(stone1.DX, stone1.DY, stone1.DZ) }
                   { P = BVec3(stone2.X, stone2.Y, stone2.Z)
                     V = BVec3(stone2.DX, stone2.DY, stone2.DZ) }
                   { P = BVec3(stone3.X, stone3.Y, stone3.Z)
                     V = BVec3(stone3.DX, stone3.DY, stone3.DZ) } |]

            let lines = findIntegerTransversals fourLines

            is_valid_solution lines stones stone)

    let p, v =
        match result with
        | None -> failwith "Error: No solution found"
        | Some(p, v) -> find_thrown_point p v stones

    let solution = int64 p.X.Numerator + int64 p.Y.Numerator + int64 p.Z.Numerator
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
    assert (684195328708898L = solution)

// #################################### //
open Xunit

type Tests() =
    let data =
        "19, 13, 30 @ -2, 1, -2\n\
         18, 19, 22 @ -1, -1, -2\n\
         20, 25, 34 @ -2, -2, -4\n\
         12, 31, 28 @ -1, -2, -1\n\
         20, 19, 15 @ 1, -5, -3"


    [<Fact>]
    let ``Test Part1`` () =
        let data = fileio.linesFromString data
        let stones = data |> parse_data<float>
        let intersections = find_intersections stones (7.0, 7.0, 27.0, 27.0)
        Assert.Equal(5, intersections |> List.length)

        let solution =
            intersections
            |> List.filter (fun x -> fst x.Parameters >= 0.0 && snd x.Parameters >= 0.0)

        Assert.Equal(2, solution |> List.length)


    [<Fact>]
    let ``Test Part2`` () =
        let data = fileio.linesFromString data
        // let data = fileio.linesFromFile "day24.txt"
        let stones = data |> parse_data2

        let result =
            stones
            // Pick a stone, one at a time
            |> List.tryPick (fun stone ->
                // Pick three stones that are not parallel
                let stone1, stone2, stone3 = pick_three_stones stones stone
                // Find the line, starting at the the point after 1ns on the picked stone that goes through the two stones picked above
                let fourLines =
                    [| { P = BVec3(stone.X, stone.Y, stone.Z)
                         V = BVec3(stone.DX, stone.DY, stone.DZ) }
                       { P = BVec3(stone1.X, stone1.Y, stone1.Z)
                         V = BVec3(stone1.DX, stone1.DY, stone1.DZ) }
                       { P = BVec3(stone2.X, stone2.Y, stone2.Z)
                         V = BVec3(stone2.DX, stone2.DY, stone2.DZ) }
                       { P = BVec3(stone3.X, stone3.Y, stone3.Z)
                         V = BVec3(stone3.DX, stone3.DY, stone3.DZ) } |]

                let lines = findIntegerTransversals fourLines

                is_valid_solution lines stones stone)

        let p, v =
            match result with
            | None -> failwith "Error: No solution found"
            | Some(p, v) -> find_thrown_point p v stones

        // Resulting coordinate is a whole number
        Assert.True(
            p.Z.Denominator = BigInteger.One
            && p.Y.Denominator = BigInteger.One
            && p.Z.Denominator = BigInteger.One
        )

        let solution = int64 p.X.Numerator + int64 p.Y.Numerator + int64 p.Z.Numerator
        Assert.Equal(47L, solution)


    [<Fact>]
    let ``Test Part22`` () =

        // Helper to easily construct BigRational vectors from integer literals
        let createVec x y z =
            BVec3(BigRational.FromInt x, BigRational.FromInt y, BigRational.FromInt z)

        // --- Test Data Initialization ---
        let line1 =
            { P = createVec 19 13 30
              V = createVec -2 1 -2 }

        let line2 =
            { P = createVec 18 19 22
              V = createVec -1 -1 -2 }

        let line3 =
            { P = createVec 12 31 28
              V = createVec -1 -2 -1 }

        let line4 =
            { P = createVec 20 19 15
              V = createVec 1 -5 -3 }

        // Execute the solver
        let results = findIntegerTransversals [| line1; line2; line3; line4 |]
        // Print the verified output
        if results.IsEmpty then
            Assert.Fail("No pure integer transversal lines found.")
        else
            Assert.Equal(1, results.Length)
            let uParams, pt, dir = results[0]
            Assert.Equivalent([| -5N; -3N; -6N; -1N |], uParams)

            Assert.Equal(
                BVec3(BigRational.FromBigIntFraction(219I, 14I), BigRational.FromBigIntFraction(221I, 14I), BigRational.FromBigIntFraction(109I, 7I)),
                pt
            )

            Assert.Equal(BVec3(BigRational.FromInt(-3), BigRational.FromInt(1), BigRational.FromInt(2)), fst (toIntegerDirection dir))
