## AoC 2023

## Project setup

```
dotnet new sln -o aoc2023
dotnet new classlib -lang "F#" -o src/aoc2023
dotnet sln add src/aoc2023/aoc2023.fsproj
dotnet new console -lang "F#" -o src/main
dotnet add src/main/main.fsproj reference src/aoc2023/aoc2023.fsproj
dotnet sln add src/main/main.fsproj

# Test framework
dotnet add src/aoc2023/aoc2023.fsproj package Microsoft.NET.Test.Sdk
dotnet add src/aoc2023/aoc2023.fsproj package xunit
dotnet add src/aoc2023/aoc2023.fsproj package xunit.runner.visualstudio
```

Versions installed: `dotnet --version; dotnet fsi --version`

```
9.0.100
Microsoft (R) F# Interactive version 12.9.100.0 for F# 9.0
```

## Run project:

Type:  
 `dotnet run --project .\src\main\main.fsproj daynn`  
E.g.  
 `dotnet run --project .\src\main\main.fsproj day01`

Run tests:  
 `dotnet test [--logger:"console;verbosity=detailed"] --filter "aoc2023.dayxx"`  
E.g.  
 `dotnet test --logger:"console;verbosity=detailed" --filter "aoc2023.day01"`

Interactive REPL:  
 `dotnet fsi --strict-indentation-`

## Notes

### General

The main entrypoint in main.fs uses reflection to find the entry point.
All solutions are in the aoc2023 namespace, but in separate modules

### Day notes

#### Day01

The simple backtracking regex `xxx(?!.*(xxx))` to find the last match doesn't handle overlapping strings like "twoneight".
Instead return the last match and use positive lookahead `(?=(xxx))` to find all matches.

#### Day02

Ran into an F# issue. https://github.com/dotnet/fsharp/issues/18206
The following incorrectly mutates all three variables (since the ref is shared):

```
type P = {mutable x: int; mutable y: int}
let defaultP = {x = 0; y=0}
let v1 = defaultP
v1.x <- 1
let v2 = defaultP
v2.y <- 2
printfn "default=%A v1=%A v2=%A" defaultP v1 v2;;
     >>> default={x=1; y=2} v1={x=1; y=2} v2={x=1; y=2}

```

Fix by using the following construct:

```
type P = {mutable x: int; mutable y: int} with static member Default = {x=0; y=0;}
let v1 = P.Default
v1.x <- 1
let v2 = P.Default
v2.y <- 2
printfn "default=%A v1=%A v2=%A" P.Default v1 v2;;
     >>> default={x=0; y=0} v1={x=1; y=0} v2={x=0; y=2}
```

#### Day02

Found out F# support list comprehension:

```
let s = [ for i in 0 .. 10 -> i * i ]
```

#### Day05

Apply series of functions:

```
let identity = fun x -> x
let square = fun x -> x * x
let cube = fun x -> x * x * x
let ff = [identity; square; cube]
[ 2; 3; 4] |> List.map( fun v -> List.fold(fun acc f -> f acc) v ff);;
```

Solve part2 by working with seed ranges

#### Day06

Solve inequality $ x(a-x) < b$ where $x$ is time spent depressing button (what we solve for) and $a$ is time and $b$ is the distance.
[See](https://www.wolframalpha.com/input?i=x%5Cleft%28a-x%5Cright%29%3Eb)

Assert($b<a^2/4$)
$$ a/2 - 1/2*sqrt(a^2-4b) < x < a/2 + 1/2*sqrt(a^2-4b) $$

#### Day08

For part 2 - keep track of when each "ghost" hits a goal position. The take the LCM of the results.

#### Day12

The naive solution of placing one tile at a time is too slow for part 2. Instead place 1/5 of the tiles (i.e. the original problem) and keep track of the end position. Recursively place 1/5 at each end position and accumulate the count. This effectively creates a tree of all possible run-lengths (of depth 5). Use a DFS to find all paths and find the sum of products.
Sorry to say, finding an efficient solution to part 2 took ~2 weeks.

### F# Annoyances

- No early return/continue/break. Forces you to have nested if/else/match or artifically introduce awkward recursive solution. See https://tomasp.net/blog/imperative-i-return.aspx/ for a workaround

- "if let" like in Rust would be a useful addition

- Default int size is 32 bits. Nice to have variable size like Python, or at least 64bits

### Useful links

- Converting multiline lambdas to one-liners:  
  https://stackoverflow.com/questions/62726587/f-multiple-expressions-on-one-line

- Oneline sum:  
  https://www.reddit.com/r/fsharp/comments/5nmtwk/can_operations_be_performed_on_int_option_types/  
  let optionSum a b = match (a,b) with | (None,_) -> None | (_,None) -> None | (Some a, Some b) ->Some( a+ b);;

- Regex:  
  https://stackoverflow.com/questions/41870124/regex-to-find-last-occurrence-of-pattern-in-a-string
  https://www.regular-expressions.info/lookaround.html

- List to Map/dict:
  https://stackoverflow.com/questions/15258834/list-of-objects-to-collections-map

- Use MemberData to pass complex type to Theory for unit tests
  https://draptik.github.io/posts/2022/01/12/fsharp-writing-parameterized-xunit-tests/
  https://stackoverflow.com/questions/35026735/in-f-how-do-you-pass-a-collection-to-xunits-inlinedata-attribute

- Printf wrapper:
  `let mywrite format = Printf.ksprintf (fun (s: string) -> printfn "%s" ("DEBUG: " + s)) format`

- Memoization: https://stackoverflow.com/questions/833180/handy-f-snippets/851449#851449

- F# Stack: https://stackoverflow.com/questions/78202439/f-stack-interface

- F# Queue: https://stackoverflow.com/questions/33464319/implement-a-queue-type-in-f

- F#/functional data structures: http://lepensemoi.free.fr/index.php/tag/data-structure
