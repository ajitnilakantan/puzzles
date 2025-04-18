namespace aoc2023

open Xunit

module Marker =
    let public marker () = 0

module fileio =
    open System.Text.RegularExpressions
    let linebreakRegex = Regex(@"\r\n?|\n", RegexOptions.Compiled)

    let public linesFromString (str: string) =
        linebreakRegex.Split(str) |> Array.toList

    let public linesFromFile (filePath: string) =
        System.IO.File.ReadAllLines(System.IO.Path.Join(__SOURCE_DIRECTORY__, filePath))
        |> Array.toList

    let tokenize (data: string) seps =
        let splitopts =
            System.StringSplitOptions.TrimEntries
            ||| System.StringSplitOptions.RemoveEmptyEntries

        let seps: char array = seps |> Seq.toArray
        data.Split(seps, splitopts) |> Array.toList

module debug =
    let enable_highlight = true 
    let highlight_on = if enable_highlight then "\x1B[0;33m" else "" 
    let highlight_off = if enable_highlight then "\x1B[39;49m" else ""
    let printfn format =
        Printf.ksprintf (fun (s: string) -> printfn "%s%s%s" highlight_on s highlight_off) format

    let printf format =
        Printf.ksprintf (fun (s: string) -> printf "%s%s%s" highlight_on s highlight_off) format

module gridio =
    let read_grid (lines: string list) isPadded default_value : char [,] =
        // If isPadded, add extra rows/columns around the grid. Makes loops easier, avoiding boundary conditions.
        assert (lines.Length > 0)
        assert (lines[0].Length > 0)
        let width, height = lines[0].Length, lines.Length

        // Make sure it is rectanglular
        lines
        |> List.iter (fun l -> assert(l.Length = width))

        let grid =
            if isPadded then
                let initfn (y: int) (x: int) =
                    match (x, y) with
                    | (0, _) -> default_value
                    | (w, _) when w = width + 1 -> default_value
                    | (_, 0) -> default_value
                    | (_, h) when h = height + 1 -> default_value
                    | _ ->
                        // -1 for padding
                        lines[y - 1][x - 1]

                Array2D.initBased 0 0 (height + 2) (width + 2) initfn
            else
                let initfn (y: int) (x: int) = lines[y][x]
                Array2D.initBased 0 0 height width initfn

        grid

    let print_grid (grid: 'T [,]) print_cell =
        // E.g. gridio.print_grid grid (fun cell -> printf "%c" cell)
        let width, height = grid.GetLength(1), grid.GetLength(0)

        for y in 0 .. height - 1 do
            for x in 0 .. width - 1 do
                print_cell grid.[y, x]

            printfn ""

    // Return a list of (y, x) tuple coordinates between [starty,endy]x[startx,endx]
    // inclusive of start/end. Optionally skip (0,0) in enumeration
    let enumerate_coordinates starty endy startx endx =
        seq {
            for y in starty..endy do
                for x in startx..endx do
                    yield (y, x)
        }
    // Same as enumerate_coordinates origin±window_size, except we skip origin
    let enumerate_neighbours (y,x) window_size  =
        assert(window_size > 0)
        enumerate_coordinates  (y - window_size) (y + window_size) (x - window_size) (x + window_size) |> Seq.filter (fun x -> x <> (0,0))

module math =
    let rec gcd<'T when 'T :> System.Numerics.INumber<'T> and 'T: equality> (a: 'T) (b: 'T) : 'T =
        match (a, b) with
        | (x, z) when z = LanguagePrimitives.GenericZero<'T> -> x
        | (z, y) when z = LanguagePrimitives.GenericZero<'T> -> y
        | (a, b) -> gcd b (a % b)

    // let rec lcm<'T when 'T :> System.Numerics.INumber<'T> and 'T: equality> (a: 'T) (b: 'T) : 'T = a * b / (gcd a b)
    let rec inline lcm<'T when ^T :> System.Numerics.INumber<^T> and ^T:equality > (a: 'T) (b: 'T) : 'T = a * b / (gcd a b)

    let rec lcmList<'T when 'T :> System.Numerics.INumber<'T> and 'T: equality> (data: 'T list) : 'T =
        match data with
        | a :: b :: [] -> lcm a b
        | a :: b -> lcm a (lcmList b)
        | [] -> LanguagePrimitives.GenericOne<'T>

module mathtest =
    [<Fact>]
    let ``test gcd and lcm`` () =

        Assert.Equal(12, (math.lcm 4 6))
        Assert.Equal(6L, math.lcm 2L 3L)
        Assert.Equal(60L, math.lcmList [ 2L; 3L; 4L; 5L; 6L ])

module collections =
    /// Bi directional map.
    /// It stores correspondences of two values.
    /// It yields correspond value from another value of the pair.
    type BiMap<'a,'b when 'a : comparison and 'b : comparison>(item1s:'a list, item2s:'b list) =
        // reusing standard F# library's map to implement find functions
        let item1IsKey = List.zip item1s item2s |> Map.ofList
        let item2IsKey = List.zip item2s item1s |> Map.ofList
        member __.findBy1    key = Map.find    key item1IsKey
        member __.tryFindBy1 key = Map.tryFind key item1IsKey 
        member __.findBy2    key = Map.find    key item2IsKey 
        member __.tryFindBy2 key = Map.tryFind key item2IsKey 
        member __.Length () = item1s.Length 

    // all_pairs [1;2;3;4] |> Seq.toList;;
    let rec all_pairs l = seq {  
        match l with 
        | h::t ->
            for e in t do
                yield h, e
            yield! all_pairs t
        | _ -> ()
    } 

    /// C# Dictionary to F# Map
    let toMap dictionary =
        (dictionary :> seq<_>)
        |> Seq.map (|KeyValue|)
        |> Map.ofSeq



module collectionstest = 
    [<Fact>]
    let ``test bimap`` () =
        let keys = [0; 1; 2; 3; 4]
        let vals = ["zero"; "one"; "two"; "three"; "four"]
        let bm = collections.BiMap(keys, vals)
        Assert.Equal(Some(1),  bm.tryFindBy2 "one")
        Assert.Equal(None,  bm.tryFindBy2 "five")
        Assert.Equal(Some("two"),  bm.tryFindBy1 2)
        Assert.Equal(None,  bm.tryFindBy1 5)
        Assert.Throws<System.Collections.Generic.KeyNotFoundException>(fun () -> bm.findBy1(5) :> obj) |> ignore
        Assert.Throws<System.Collections.Generic.KeyNotFoundException>(fun () -> bm.findBy2("five") :> obj) |> ignore
        Assert.Equal(Some(4), bm.tryFindBy2 "four")
        Assert.Equal(Some("four"), bm.tryFindBy1 4)
        Assert.Equal(4, bm.findBy2 "four")
        Assert.Equal("four", bm.findBy1 4)

    [<Fact>]
    let ``test all_pairs`` () =
        let data = [1; 2; 3; 4]
        let expected = [(1, 2); (1, 3); (1, 4); (2, 3); (2, 4); (3, 4)]
        Assert.Equivalent(expected, collections.all_pairs data)

module dayxxtest =
    type Marker =
        interface
        end

    let public Solve () = printfn "Solver from dayxxtest"
