module aoc2023.day19

type internal Marker =
    interface
    end

type Category =
    | X
    | M
    | A
    | S

type State =
    | Accept
    | Reject
    | Label of string
    static member NA = Label ""

    static member FromString(s: string) =
        match s with
        | "A" -> Accept
        | "R" -> Reject
        | _ -> Label s


type Rule =
    | LessThan of Category * int * State
    | GreaterThan of Category * int * State
    | SetState of State
    static member FromString(s) =
        match s with
        | "A" -> SetState Accept
        | "R" -> SetState Reject
        | _ when s.Length > 1 && (s[1] = '<' || s[1] = '>') ->
            let tokens = fileio.tokenize s ":"
            assert (tokens.Length = 2)
            let prefix = tokens[0][0..1]
            let value = int (tokens[0][2..])
            let state = State.FromString tokens[1]

            match prefix with
            | "x<" -> LessThan(X, int value, state)
            | "x>" -> GreaterThan(X, int value, state)
            | "m<" -> LessThan(M, int value, state)
            | "m>" -> GreaterThan(M, int value, state)
            | "a<" -> LessThan(A, int value, state)
            | "a>" -> GreaterThan(A, int value, state)
            | "s<" -> LessThan(S, int value, state)
            | "s>" -> GreaterThan(S, int value, state)
            | _ -> failwithf "Invalid prefix %A" prefix
        | _ -> SetState(Label s)


    member this.get_state() =
        match this with
        | LessThan (_, _, state) -> state
        | GreaterThan (_, _, state) -> state
        | SetState state -> state

    /// Invert the rule for the else case
    member this.invert =
        match this with
        | LessThan (category, value, state) -> GreaterThan(category, value - 1, State.NA)
        | GreaterThan (category, value, state) -> LessThan(category, value + 1, State.NA)
        | SetState state -> SetState state


type Part =
    { x: int
      m: int
      a: int
      s: int }
    static member FromString(s) =
        let tokens = fileio.tokenize s "{},="
        assert (tokens.Length = 8)

        assert
            (tokens[0] = "x"
             && tokens[2] = "m"
             && tokens[4] = "a"
             && tokens[6] = "s")

        { x = int tokens[1]
          m = int tokens[3]
          a = int tokens[5]
          s = int tokens[7] }


type AppliedRule =
    { x_lt: Option<int>
      x_gt: Option<int>
      m_lt: Option<int>
      m_gt: Option<int>
      a_lt: Option<int>
      a_gt: Option<int>
      s_lt: Option<int>
      s_gt: Option<int> }
    static member Default =
        { x_lt = None
          x_gt = None
          m_lt = None
          m_gt = None
          a_lt = None
          a_gt = None
          s_lt = None
          s_gt = None }

    // "And" the given rule with the current state
    member this.merge(rule: Rule) =
        let max_val a b =
            match a with
            | Some a -> Some(max a b)
            | None -> Some b

        let min_val a b =
            match a with
            | Some a -> Some(min a b)
            | None -> Some b

        match rule with
        | LessThan (X, value, state) -> { this with x_lt = min_val this.x_lt value }
        | LessThan (M, value, state) -> { this with m_lt = min_val this.m_lt value }
        | LessThan (A, value, state) -> { this with a_lt = min_val this.a_lt value }
        | LessThan (S, value, state) -> { this with s_lt = min_val this.s_lt value }
        | GreaterThan (X, value, state) -> { this with x_gt = max_val this.x_gt value }
        | GreaterThan (M, value, state) -> { this with m_gt = max_val this.m_gt value }
        | GreaterThan (A, value, state) -> { this with a_gt = max_val this.a_gt value }
        | GreaterThan (S, value, state) -> { this with s_gt = max_val this.s_gt value }
        | _ -> this

    member this.get_coords =
        let ltX = this.x_lt |> Option.defaultValue 4001 |> int64
        let gtX = this.x_gt |> Option.defaultValue 0 |> int64
        let ltM = this.m_lt |> Option.defaultValue 4001 |> int64
        let gtM = this.m_gt |> Option.defaultValue 0 |> int64
        let ltA = this.a_lt |> Option.defaultValue 4001 |> int64
        let gtA = this.a_gt |> Option.defaultValue 0 |> int64
        let ltS = this.s_lt |> Option.defaultValue 4001 |> int64
        let gtS = this.s_gt |> Option.defaultValue 0 |> int64

        // Return intervals as [min, max]
        [ gtX, ltX
          gtM, ltM
          gtA, ltA
          gtS, ltS ]


let parse_command_definitions (data: string list) =

    data
    |> List.map (fun s ->
        let tokens = fileio.tokenize s "{}"
        assert (tokens.Length = 2)
        let key = tokens[0]
        let rule = tokens[1]
        let rules = fileio.tokenize rule ","

        let values = rules |> List.map (fun s -> Rule.FromString s)

        key, values)
    |> Map.ofList


let parse_part_definitions (data: string list) =
    data |> List.map (fun s -> Part.FromString s)

let process_rule (part: Part) (commands: Map<string, Rule list>) (label: string) =
    let apply_rule (rule: Rule) (part: Part) =
        match rule with
        | SetState state -> Some state
        | LessThan (category, value, state) ->
            match category with
            | X when part.x < value -> Some state
            | M when part.m < value -> Some state
            | A when part.a < value -> Some state
            | S when part.s < value -> Some state
            | _ -> None
        | GreaterThan (category, value, state) ->
            match category with
            | X when part.x > value -> Some state
            | M when part.m > value -> Some state
            | A when part.a > value -> Some state
            | S when part.s > value -> Some state
            | _ -> None

    let iterateUntil condition (list: Rule list) (part: Part) =
        let rec loop remainingList part =
            match remainingList with
            | [] -> failwith "Error: Ran out of rules" // () // Base case: list is empty, stop
            | head :: tail ->
                let state = apply_rule head part

                if condition state then
                    // Condition met: stop iteration (return unit)
                    state // ()
                else
                    // Condition not met: perform action and continue with the tail
                    loop tail part

        loop list part

    let rules = commands |> Map.find label

    let result = iterateUntil (fun x -> x |> Option.isSome) rules part

    result

let process_rules (part: Part) (commands: Map<string, Rule list>) =
    let mutable endstate = Some(Label "in")

    while endstate <> Some Accept
          && endstate <> Some Reject
          && endstate <> None do
        match endstate with
        | Some (Label label) -> endstate <- process_rule part commands label
        | _ -> failwith "Error: Unexpected endstate"

    endstate

type Node = State // Label of workflow, e.g. "in", "A", "R", ...
type Edge = Node * List<Rule> // Edge's Target node * the rule to get to it.
type Graph = Map<Node, List<Edge>>

let addEdge (graph: Graph) (node: Node) (edge: Edge) : Graph =
    let edges = graph.TryFind node |> Option.defaultValue []

    graph |> Map.add node ([ edge ] @ edges)


/// Create a directed graph of the workflow
let create_workflow_graph (commands: Map<string, Rule list>) =
    let mutable graph: Map<Node, List<Edge>> = Map.empty
    let mutable count = 0
    graph <- graph.Add(State.FromString "A", [])
    graph <- graph.Add(State.FromString "R", [])

    for kv in commands do
        let node_name, rules = kv.Key, kv.Value
        let node = State.FromString node_name

        graph <- graph.Add(node, [])
        let mutable edgerules: List<Rule> = []

        for rule in rules do
            let target = rule.get_state ()

            graph <- addEdge graph node (target, edgerules @ [ rule ])
            // Invert for the else case
            edgerules <- edgerules @ [ rule.invert ]

    graph

let SolvePart1 data =
    let chunks = fileio.chunkLines data
    let commands = parse_command_definitions chunks[0]
    let parts = parse_part_definitions chunks[1]

    let accepted_parts =
        parts
        |> List.filter (fun part -> process_rules part commands = Some Accept)

    let solution =
        accepted_parts
        |> List.map (fun part -> part.x + part.m + part.a + part.s)
        |> List.sum

    solution

let SolvePart2 data =
    let chunks = fileio.chunkLines data
    let commands = parse_command_definitions chunks[0]
    let parts = parse_part_definitions chunks[1]

    let get_neighbours (g: Graph) (node: Node) = g |> Map.tryFind node
    let graph = create_workflow_graph commands

    let paths =
        graphsearch.findAllPaths_multigraph (State.Label "in") (State.Accept) (graph |> get_neighbours)

    let boxes =
        paths
        |> Seq.toList
        |> List.map (fun path ->
            let len = path |> List.length
            assert (fst path[len - 1] = State.Accept)
            let mutable applied = AppliedRule.Default

            for p in path do
                for rule in snd p do
                    applied <- applied.merge rule

            applied)

    let intervals: klee.HyperBox list =
        boxes
        |> List.map (fun applied -> applied.get_coords)
        |> List.map (fun coords ->
            coords
            |> List.map (fun (x, y) -> float x, float y - 1.0))

    let solution = int64 (klee.computeVolume intervals) // 23214101440000

    solution

let public Solve () =
    printfn $"Solve from {typeof<Marker>.DeclaringType}"
    let data = fileio.linesFromFile "day19.txt"

    let stopWatch = System.Diagnostics.Stopwatch.StartNew()
    let solution = SolvePart1 data
    printfn "Part1 = %A" solution
    stopWatch.Stop()
    printfn "Elapsed time %ims" stopWatch.ElapsedMilliseconds
    assert (382440 = solution)

    let stopWatch = System.Diagnostics.Stopwatch.StartNew()
    let solution = SolvePart2 data
    printfn "Part2 = %A" solution
    stopWatch.Stop()
    printfn "Elapsed time %ims" stopWatch.ElapsedMilliseconds
    assert (136394217540123L = solution)

// #################################### //
open Xunit

type Tests() =
    let data =
        "px{a<2006:qkq,m>2090:A,rfg}\n\
         pv{a>1716:R,A}\n\
         lnx{m>1548:A,A}\n\
         rfg{s<537:gd,x>2440:R,A}\n\
         qs{s>3448:A,lnx}\n\
         qkq{x<1416:A,crn}\n\
         crn{x>2662:A,R}\n\
         in{s<1351:px,qqz}\n\
         qqz{s>2770:qs,m<1801:hdj,R}\n\
         gd{a>3333:R,R}\n\
         hdj{m>838:A,pv}\n\
         \n\
         {x=787,m=2655,a=1222,s=2876}\n\
         {x=1679,m=44,a=2067,s=496}\n\
         {x=2036,m=264,a=79,s=2244}\n\
         {x=2461,m=1339,a=466,s=291}\n\
         {x=2127,m=1623,a=2188,s=1013}"

    [<Fact>]
    let ``Test Part1`` () =
        let data = fileio.linesFromString data
        let chunks = fileio.chunkLines data
        let commands = parse_command_definitions chunks[0]
        let parts = parse_part_definitions chunks[1]

        let accepted_parts =
            parts
            |> List.filter (fun part -> process_rules part commands = Some Accept)


        let solution =
            accepted_parts
            |> List.map (fun part -> part.x + part.m + part.a + part.s)
            |> List.sum

        Assert.Equal(19114, solution)

    [<Fact>]
    let ``Test Part2`` () =
        let data = fileio.linesFromString data
        let chunks = fileio.chunkLines data
        let commands = parse_command_definitions chunks[0]

        let get_neighbours (g: Graph) (node: Node) = g |> Map.tryFind node
        let graph = create_workflow_graph commands

        let paths =
            graphsearch.findAllPaths_multigraph (State.Label "in") (State.Accept) (graph |> get_neighbours)

        let boxes =
            paths
            |> Seq.toList
            |> List.map (fun path ->
                let len = path |> List.length
                assert (fst path[len - 1] = State.Accept)
                let mutable applied = AppliedRule.Default

                for p in path do
                    for rule in snd p do
                        applied <- applied.merge rule

                applied)

        let intervals: klee.HyperBox list =
            boxes
            |> List.map (fun applied -> applied.get_coords)
            |> List.map (fun coords ->
                coords
                |> List.map (fun (x, y) -> float x, float y - 1.0))

        let solution = int64 (klee.computeVolume intervals) // 23214101440000
        Assert.Equal(167409079868000L, solution)
