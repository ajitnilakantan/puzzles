/**
 * A compare function representing the partial order relation.
 * Returns true if a <= b. Returns false if a > b OR if they are incomparable.
 */
export type PartialOrderRelation<T> = (a: T, b: T) => boolean;

export class Poset<T> {
  private elements = new Set<T>();
  // Maps an element to the set of elements it directly precedes (adjacency list)
  private adjacencyList = new Map<T, Set<T>>();
  private customRelation?: PartialOrderRelation<T>;

  constructor(relation?: PartialOrderRelation<T>) {
    this.customRelation = relation;
  }

  /**
   * Adds an element to the poset.
   */
  add(element: T): void {
    this.elements.add(element);
    if (!this.adjacencyList.has(element)) {
      this.adjacencyList.set(element, new Set());
    }
  }

  /**
   * Defines an explicit ordering constraint: "fromElement <= toElement".
   * Ignored if a functional relation is driving the Poset instead.
   */
  addConstraint(fromElement: T, toElement: T): void {
    this.add(fromElement);
    this.add(toElement);

    if (fromElement === toElement) return; // Reflexivity is implicit

    // Prevent cycles to maintain strict DAG properties (antisymmetry)
    if (this.isLessThanOrEqual(toElement, fromElement)) {
      // console.log(`Fail at ${fromElement} ${toElement}`);
      throw new Error("Cannot add constraint: Would violate antisymmetry (creates a cycle).");
    }

    this.adjacencyList.get(fromElement)!.add(toElement);
  }

  /**
   * Core partial order check: returns true if a <= b.
   */
  isLessThanOrEqual(a: T, b: T): boolean {
    // Reflexivity
    if (a === b) return true;

    if (!this.elements.has(a) || !this.elements.has(b)) return false;

    // If a functional rule was given (e.g., divisibility), evaluate it directly
    if (this.customRelation) {
      return this.customRelation(a, b);
    }

    // Otherwise, check via graph reachability (Axiom 3: Transitivity via DFS)
    return this.hasPath(a, b);
  }

  /**
   * Determines if two elements are directly adjacent
   */
  areAdjacent(a: T, b: T): boolean {
    if (a === b) return true;

    if (!this.elements.has(a) || !this.elements.has(b)) return false;
    const neighbors = this.adjacencyList.get(a) ?? new Set();
    return neighbors.has(b);
  }
  /**
   * Get list of adjacent nodes a < b, c, d, ...
   */
  getAdjacencies(a: T): Set<T> {
    return this.adjacencyList.get(a) ?? new Set();
  }
  /**
   * Determines if two elements can be compared at all.
   */
  areComparable(a: T, b: T): boolean {
    return this.isLessThanOrEqual(a, b) || this.isLessThanOrEqual(b, a);
  }

  /**
   * Standard Depth First Search to evaluate transitive relationships.
   */
  private hasPath(start: T, target: T): boolean {
    const visited = new Set<T>();
    const queue: T[] = [start];

    while (queue.length > 0) {
      const current = queue.shift()!;
      if (current === target) return true;

      if (!visited.has(current)) {
        visited.add(current);
        const neighbors = this.adjacencyList.get(current) ?? new Set();
        for (const neighbor of neighbors) {
          queue.push(neighbor);
        }
      }
    }
    return false;
  }

  /**
   * Produces a valid Total Order using Topological Sorting.
   */
  getLinearExtension(): T[] {
    if (this.customRelation) {
      // Fallback naive sorting for functional relation posets
      return Array.from(this.elements).sort((a, b) => {
        if (this.customRelation!(a, b)) return -1;
        if (this.customRelation!(b, a)) return 1;
        return 0;
      });
    }

    const visited = new Set<T>();
    const result: T[] = [];

    const visit = (node: T): void => {
      if (visited.has(node)) return;
      visited.add(node);

      const neighbors = this.adjacencyList.get(node) ?? new Set();
      for (const neighbor of neighbors) {
        visit(neighbor);
      }
      result.unshift(node);
    };

    for (const element of this.elements) {
      visit(element);
    }
    return result;
  }

  /**
   * Provides a valid linear extension (topological sort) restricted to a subset of elements.
   */
  sort(toSort: T[]): T[] {
    const subset = new Set(toSort);

    // 1. Get a valid total order of all elements
    const fullOrder = this.getLinearExtension();

    // 2. Filter the total order to only keep elements present in the requested subset
    return fullOrder.filter((element) => subset.has(element));
  }
}
