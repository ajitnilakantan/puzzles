implement a non-recursive Tarjan graph partition algorithm in F#. As inputs take 
- a seq of nodes of type Seq<'N>
- a function that gets the neighbors of a given node

Return 
- name the function partitionGraph
- a list of bridges of type List<'N*'N>
- a map node to island mappings of type Map<'N, int>


The graph can have bidirectional edges and cycles.
Can you use F# datastructures instead of C# generics where possible?
Use the F# List and Map datastructures everywhere and not mutable .NET generic collections.
Don't use recursive calls anywhere.
Keep the code as simple as possible without custom data structures.

Provide test graphs for a linear graph and a "Y" shaped graph. The tests should be compatible with Xunit. Don't use FsUnit.Xunit and FsUnit.


====


Write a non-recursive F# function the enumerate all paths in a graph with backtracking. Can you use F# datastructures instead of C# generics where possible?
Use the F# List and Map datastructures everywhere and not mutable .NET generic collections. Inputs are a function that gets the neighbors of a given node, start and goal nodes. Have it return a Seq of lists to be lazy.
Signature of function is:
let findAllPaths (neighbors : 'a -> 'a seq) (isGoal: 'a -> bool) (start: 'a): seq<'a list> =
The graph can be bidirectional and contain cycles.

====
Implement the Longest Path algorithm in F#. Make it iterative, and non-recursive.
Can you use F# datastructures instead of C# generics where possible?
Use the F# List and Map datastructures everywhere and not mutable .NET generic collections. Inputs are 
- a function that gets the neighbors and edge weights of a given node
- is_goal function
- start node
Signature of function is:
let findLongestPathIterative (neighbors : 'a -> ('a*int) seq) (isGoal: 'a -> bool) (start: 'a): seq<'a list> 
Make sure to use dynamic programming and memoization to help simplify the search.

====
Implement the Kernighan–Lin algorithm F#. Make it iterative, and non-recursive.
Can you use F# datastructures instead of C# generics where possible?
Use the F# List and Map datastructures everywhere and not mutable .NET generic collections. Inputs are 
- list of nodes
- a function that gets the neighbors of a given node.
- A target "n" to split into "n" subgraphs
Output
- The partitions
- List of edges crossing partitions

Signature of function is:
    /// Returns a tuple of (Partitions, Cut Edges crossing between partitions)
    let partitionGraph (nodes: 'a list) (getNeighbors: 'a -> 'a list) (n: int) : ('a list list * ('a * 'a) list) when 'a : comparison


====
Simplify a graph that contains a lot of unit paths into weighted edges with fewer nodes
Can you implement this in F#. 
Make it iterative, and non-recursive.
Can you use F# datastructures instead of C# generics where possible?
Use the F# List and Map datastructures everywhere and not mutable .NET generic collections. 
Inputs are 
- list of nodes
- a function that gets the neighbors of a given node.
Output
- The new nodes and weighted edges
- Map of old nodes to new "compacted" node
====
Implement the Bentley-Ottmann sweep-line algorithm in F# with test cases.
Can you include infinite lines, not just segments. They must intersect within a specified box.
- Use a parametrized equation of a line:
  Unique ID
  x(t) = x0 + x't
  y(t) = y0 + y't
It should handle 
 - vertical lines.
 - Overlapping lines
 - More that 2 lines intersecting at a point
E.g. Input datastructures:
 type Line = { Id: int;X: double;Y: double;DX: double;DY: double }
 type BoundedBox = { MinX: double; MaxX: double; MinY: double;MaxY: double }

Return list of:
 - Intersecting line IDs as a tuple (id1, id2)
 - The (x, y) intersection point
 - The (t1, t2) parameters for each line
 type IntersectionResult = { LineIds: int * int; Point: double * double; Parameters: double * double }

The function signature is:
let findIntersections (lines: Line list) (box: BoundedBox) : IntersectionResult list =

Can you thoroughly comment the code?
Can you implement this in F#. 
Make it iterative, and non-recursive.
Can you use F# datastructures instead of C# generics where possible?
Use the F# List and Map datastructures everywhere and not mutable .NET generic collections. 
Add F# Xunit tests. Don't use FsUnit.Xunit and FsUnit.

===
- I want the algorithm to find a 3D line that intersects (or comes as close as possible) to a given set of N 3D lines
- Show the code in F#
- You can use the MathNet.Numerics and MathNet.Spatial assemblies
- E.g. with the input:
  open MathNet.Numerics.LinearAlgebra
  open MathNet.Spatial.Euclidean
 
  type MyLine3D = { Point: Vector<float>;  Direction: Vector<float> }
  let vB (arr: float array) = Vector.Build.Dense(arr)
  let lines =
    [ { Point = vB.DenseOfArray [| 19.0; 13.0; 30.0 |];  Direction = vB.DenseOfArray [| -2.0;  1.0; -2.0 |] }
      { Point = vB.DenseOfArray [| 18.0; 19.0; 22.0 |];  Direction = vB.DenseOfArray [| -1.0; -1.0; -2.0 |] }
      { Point = vB.DenseOfArray [| 20.0; 25.0; 34.0 |];  Direction = vB.DenseOfArray [| -2.0; -2.0; -4.0 |] } 
      { Point = vB.DenseOfArray [| 12.0; 31.0; 28.0 |];  Direction = vB.DenseOfArray [| -1.0; -2.0; -1.0 |] } 
      { Point = vB.DenseOfArray [| 20.0; 19.0; 15.0 |];  Direction = vB.DenseOfArray [|  1.0; -5.0; -3.0 |] } 
    ]
   I should get the output similar to:
    { Point = vB.DenseOfArray [| 24.0; 13.0; 10.0 |];  Direction = vB.DenseOfArray [| -3.0;  1.0;  2.0 |] } 


