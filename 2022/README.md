## AoC 2022

```
mkdir 2022
cd 2022
go version
  # go version go1.19.3 windows/amd64
go mod init aoc/ajitn/2022
```

## Running
type  
`go run .\cmd\aoc\main.go  DayNN [-verbose]`  
e.g.  
`go run .\cmd\aoc\main.go  Day01`  


reformat code
`gofmt -s -w .`

run tests
`go test -v -run "^TestDayNN" ./...`
e.g.
`go test -v -run "^TestDay01" ./...`

run all tests
`go test -v ./...`

## References
- GoLang structure based on
https://qqq.ninja/blog/post/go-structure/

- Functional wrappers
https://bitfieldconsulting.com/golang/functional

- Iterators
https://bbengfort.github.io/2016/12/yielding-functions-for-iteration-golang/

## Notes/Improvements

### Day 16 Part B
The graph search takes too long (~15min).  Must be a better way to prune/direct the search.
Maybe use strongly connected graph decomposition to split the problem (i.e. not explore
already explored regions)?
Maybe remove nodes with zero flow to reduce search space?
Maybe use bitmask instead of Set to keep track of opened valves?
Clean solution: https://www.reddit.com/r/adventofcode/comments/zn6k1l/comment/j2xhog7

### Day 17 Part B
Look for looping repetitions in pattern

### Day 18 Part B
Flood-fill outer volume to know which faces are not "holes"

### Day 19
Part A is slow...takes ~5min.  Part B takes ~10 min.  The B tests take too long, and are commented out.
Speed up search with
- Don't accumulate stuff, it it is possible to purchase robots
- Have a visited cache. need to clear periodically to avoid oom probs
- Added heuristic to check if a better path was found. i.e. at a given time, more geodes were mined in another path. if so, prune current search.  It works, but I doubt it is mathematically sound.

Other posts show these additional heuristics:
- Don't create more robots of a particular type that the max spend rate of that robot type
- Except, if you can build a geode robot, do so
- If you have more of a type than you could spend in the remaining time, delete the excess (except for geode). This reduces search space
- Good walkthrough: https://www.youtube.com/watch?v=H3PSODv4nf0

### Day 20
Overly complicated solution using an indexed skip list. Would have been simpler to use a simple circular linked list. Also took forever to figure out need to do mod size-1 to account for the moving node.

### Day 21
Ternary search

### Day 22
Hexomino,  https://en.m.wikipedia.org/wiki/Net_(polyhedron)
Should be able to generalize by iteratively matching up edges with common vertex.

### Day 24
Treat a 3D graph, with time-axis. Small optmization to only compute local neighbours

<!--
https://go.dev/play/p/tUF6yAolTMb
https://go.dev/play/p/-LZo0ydrrLK
-->
