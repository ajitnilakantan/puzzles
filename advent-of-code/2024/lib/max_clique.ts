export type GetNeighbours<N> = (node: N) => Iterable<N> | N[];

/**
 * Finds all maximal cliques in an undirected graph using the Bron-Kerbosch algorithm with pivoting.
 * Works with any node type N and custom neighbor lookup logic.
 *
 * @param nodes An iterable list or array of all nodes in the graph.
 * @param getNeighbours A function that returns the neighbors of a given node.
 * @returns An array of maximal cliques, where each clique is an array of nodes.
 */

export function findMaximalCliques<N>(nodes: Iterable<N> | N[], getNeighbours: GetNeighbours<N>): N[][] {
  const cliques: N[][] = [];
  const p = toSet(nodes);

  // Return early if the input graph contains zero nodes
  if (p.size === 0) {
    return cliques;
  }

  // Helper to safely convert any Iterable/Array to a Set
  function toSet<T>(iter: Iterable<T> | T[]): Set<T> {
    return new Set(iter);
  }

  // Cache neighbors as Sets for O(1) lookups during recursion
  const neighborCache = new Map<N, Set<N>>();
  const getCachedNeighbors = (node: N): Set<N> => {
    if (!neighborCache.has(node)) {
      neighborCache.set(node, toSet(getNeighbours(node)));
    }
    return neighborCache.get(node)!;
  };

  const r = new Set<N>();
  const x = new Set<N>();

  function bronKerbosch(R: Set<N>, P: Set<N>, X: Set<N>): void {
    if (P.size === 0 && X.size === 0) {
      cliques.push(Array.from(R));
      return;
    }

    // Pivot selection from P U X
    const pivot = getPivot(P, X, getCachedNeighbors);
    const pivotNeighbors = getCachedNeighbors(pivot);

    // Candidates are vertices in P that are NOT neighbors of the chosen pivot
    const candidates = Array.from(P).filter((v) => !pivotNeighbors.has(v));

    for (const v of candidates) {
      const neighbors = getCachedNeighbors(v);

      // Recurse with:
      // R ⋃ {v}
      // P ⋂ N(v)
      // X ⋂ N(v)
      bronKerbosch(
        new Set([...R, v]),
        new Set(Array.from(P).filter((node) => neighbors.has(node))),
        new Set(Array.from(X).filter((node) => neighbors.has(node)))
      );

      // Move v from P to X
      P.delete(v);
      X.add(v);
    }
  }

  bronKerbosch(r, p, x);
  return cliques;
}

/**
 * Selects a pivot node from P U X that maximizes neighbor intersections with P.
 */
function getPivot<N>(P: Set<N>, X: Set<N>, getNeighbors: (node: N) => Set<N>): N {
  let maxCount = -1;
  // Initialize with an arbitrary fallback node from P (guaranteed to exist if we reach here)
  let chosenPivot: N | undefined = undefined;

  // We check P first. Since P U X is guaranteed to have at least one element
  // when this function is called, chosenPivot will always be assigned.
  for (const vertex of P) {
    const neighbors = getNeighbors(vertex);
    const intersectionSize = Array.from(neighbors).filter((n) => P.has(n)).length;
    if (intersectionSize > maxCount) {
      maxCount = intersectionSize;
      chosenPivot = vertex;
    }
  }

  for (const vertex of X) {
    const neighbors = getNeighbors(vertex);
    const intersectionSize = Array.from(neighbors).filter((n) => P.has(n)).length;
    if (intersectionSize > maxCount) {
      maxCount = intersectionSize;
      chosenPivot = vertex;
    }
  }

  // Cast back to N because logic guarantees P U X is non-empty here
  return chosenPivot as N;
}
