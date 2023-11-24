## Running
type  
`cargo run --bin dayNN`  
e.g.  
`cargo run --bin day01`  
from the *src* directory

## Improvements
These take a long time to run

### Day 12
A lot of string comparisons + check if lowercase. Should be replaced by table index lookups.

### Day 15
Apparently the Rust BinaryHeap is a bit slow. Should use an alternative implementation

### Day 18
Too many string operations (splitting, regex, splicing, atoi, etc.).
Probably would be better to convert the initial string to an array of (value, depth) and work with integer tuples.

### Day 22
Approach was too complicated (decomposing the prisms into interior solid+faces+vertices).\
Proper approach is to use $|A \cup B| = |A| + |B| - |A \cap B|$

### Day 24
Use the Z3 solver, but it takes several hours. Should use BitVec instead of Int! \
Some solutions looked at the actual structure of the code to simplify it. \
A clever "set based" solution is given in https://www.mattkeeter.com/blog/2021-12-27-brute/

<!--
cargo new day00 --bin --vcs none

https://stackoverflow.com/questions/32723794/how-do-i-write-a-function-that-takes-both-owned-and-non-owned-string-collections
-->

