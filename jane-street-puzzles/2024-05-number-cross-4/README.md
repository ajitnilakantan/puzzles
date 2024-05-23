## May 2024: Number Cross 4

https://www.janestreet.com/puzzles/archive/



Doing a brute force search takes too long.
Use the following optimizations:

* Subdivide each row of the grid into possible subpatterns
for possible numbers.  E.g. for the example grid, the fifth row is

  [1, 2, 2, 3, 3],  so the possible (normalized) subpatterns are, per position:<br/>
  0: (1,2),(1,2,2),(1,2,2,3),(1,2,2,3,3) -> (1, 2), (1,2,2), (1,2,2,3), (1,2,2,3,3)<br/>
  1: (2,2),(2,2,3),(2,2,3,3) -> (1,1),(1,1,2)(1,1,2,2)<br/>
  2: (2,3),(2,3,3) -> (1,2).(1,2,2)<br/>
  3: (3,3) -> (1,1)<br/>
  4: none possible<br/>


* For sequences that are sparse (e.g. fibonacci) compute the sequence and check if it matches a possible subpattern

* For non-sparse (e.g. multiples of n) instead of checking all `10**width` combinations of numbers, we know the
list of valid numbers (from the subpatterns above) and just check them. This greatly reduces the search space.

* Pre-filter rows - adjacent regions should not have same digit

* When flood filling (when a filled square breaks a region) optimize to only flood file squares around the filled square.


After all this still takes ~4hrs to run. The search is easily parallelizable. Maybe that can be tried next.

## RYE
```
rye init
rye tools install mypy
rye add typing-extensions
rye sync

rye lint
rye run number-cross-4.py
```
