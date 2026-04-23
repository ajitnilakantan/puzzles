module aoc2023.day15

type internal Marker = interface end

type Lens =
    { label: string; value: int; hash: int }

type Remove = { label: string; hash: int }

type Token =
    | Lens of Lens
    | Remove of Remove

let hash (s: string) =
    let mutable hash = 0

    for c in s do
        hash <- (hash + int c) * 17 % 256

    hash

let add_token (token: Lens) (box: list<Lens>) : list<Lens> =
    if box |> List.exists (fun item -> item.label = token.label) then
        box
        |> List.map (fun item -> if item.label = token.label then { item with value = token.value } else item)
    else
        box @ [ token ]

let remove_token (token: Remove) (box: list<Lens>) : list<Lens> =
    box |> List.filter (fun item -> item.label <> token.label)

let parse_tokens (tokens: list<string>) : list<Token> =
    tokens
    |> List.map (fun s ->
        match s with
        | _ when s.EndsWith("-") ->
            let label = s.Substring(0, s.Length - 1)
            Remove({ label = label; hash = hash label })
        | _ when s.Contains("=") ->
            let parts = s.Split('=')
            let label = parts[0]
            let value = int parts[1]

            Lens(
                { label = label
                  value = value
                  hash = hash label }
            )
        | _ -> failwithf "Invalid token '%A'" s)

let process_tokens (tokens: list<Token>) =
    let boxes: Lens list array = [| for _ in 0..255 -> [] |]

    tokens
    |> List.iter (fun token ->
        match token with
        | Lens lens -> boxes.[lens.hash] <- add_token lens boxes.[lens.hash]
        | Remove remove -> boxes.[remove.hash] <- remove_token remove boxes.[remove.hash])

    boxes

let score_boxes (boxes: Lens list array) =
    boxes
    |> Array.mapi (fun boxindex box ->
        box
        |> List.mapi (fun lensindex lens -> (boxindex + 1) * (lensindex + 1) * lens.value)
        |> List.sum)
    |> Array.sum

let SolvePart1 (data: list<string>) =
    let data = data[0]
    let tokens = fileio.tokenize data ","
    let solution = tokens |> List.map (fun s -> hash s) |> List.sum
    solution

let SolvePart2 (data: list<string>) =
    let data = data[0]
    let tokens = fileio.tokenize data ","
    let tokens = tokens |> parse_tokens
    let boxes = tokens |> process_tokens
    let solution = score_boxes boxes

    solution

let public Solve () =
    printfn $"Solve from {typeof<Marker>.DeclaringType}"
    let data = fileio.linesFromFile "day15.txt"

    let stopWatch = System.Diagnostics.Stopwatch.StartNew()
    let solution = SolvePart1 data
    printfn "Part1 = %A" solution
    stopWatch.Stop()
    printfn "Elapsed time %ims" stopWatch.ElapsedMilliseconds
    assert (522547 = solution)

    let stopWatch = System.Diagnostics.Stopwatch.StartNew()
    let solution = SolvePart2 data
    printfn "Part2 = %A" solution
    stopWatch.Stop()
    printfn "Elapsed time %ims" stopWatch.ElapsedMilliseconds
    assert (229271 = solution)

// #################################### //
open Xunit

type Tests() =
    let data = "rn=1,cm-,qp=3,cm=2,qp-,pc=4,ot=9,ab=5,pc-,pc=6,ot=7"

    [<Fact>]
    let ``Test Part1`` () =
        let data = (fileio.linesFromString data)[0]
        let tokens = fileio.tokenize data ","

        let ret = tokens |> List.map (fun s -> hash s) |> List.sum

        Assert.Equal(1320, ret)

    [<Fact>]
    let ``Test Part2`` () =
        let data = (fileio.linesFromString data)[0]
        let tokens = fileio.tokenize data ","
        let tokens = tokens |> parse_tokens
        let boxes = tokens |> process_tokens
        let ret = score_boxes boxes

        Assert.Equal(145, ret)
