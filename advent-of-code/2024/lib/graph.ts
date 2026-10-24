import assert from "node:assert";

// Types
export type GetNeighbours<N> = (node: N) => Iterable<N> | N[];
export type IsGoal<N> = (node: N) => boolean;
export type HashKey<N> = (node: N) => string;

// BFS
export function bfs<N>(startNode: N, getNeighbours: GetNeighbours<N>): N[] {
  const visited = new Set<N>([startNode]);
  const queue: N[] = [startNode];
  const orderOfVisitation: N[] = [];

  while (queue.length > 0) {
    const currentNode = queue.shift()!;
    orderOfVisitation.push(currentNode);

    const neighbours = getNeighbours(currentNode);
    for (const neighbour of neighbours) {
      if (!visited.has(neighbour)) {
        visited.add(neighbour);
        queue.push(neighbour);
      }
    }
  }
  return orderOfVisitation;
}

// Returns an Iterator of Paths (each path is an array of N)
export function* getAllPaths<N>(
  startNode: N,
  isGoal: IsGoal<N>,
  getNeighbours: GetNeighbours<N>
): IterableIterator<N[]> {
  // We use a DFS-based queue/stack tracking both the current node and the path to reach it
  // A Set is kept per-path to prevent cycles
  const stack: { node: N; path: N[]; visitedInPath: Set<N> }[] = [
    { node: startNode, path: [startNode], visitedInPath: new Set([startNode]) },
  ];

  while (stack.length > 0) {
    const { node, path, visitedInPath } = stack.pop()!;

    if (isGoal(node)) {
      yield path; // Yield the valid path to the caller lazily
    }

    const neighbours = getNeighbours(node);
    for (const neighbour of neighbours) {
      // Prevent visiting a node already in the current path branch (avoids infinite loops)
      if (!visitedInPath.has(neighbour)) {
        const nextVisited = new Set(visitedInPath);
        nextVisited.add(neighbour);

        stack.push({
          node: neighbour,
          path: [...path, neighbour],
          visitedInPath: nextVisited,
        });
      }
    }
  }
}

// Deterministic key hashing for true structural value comparison
export function hashKey(key: unknown): string {
  if (key === null || typeof key !== "object") {
    // Not an object, so a primitive (or undefined)
    return String(key);
  }

  if (!Array.isArray(key)) {
    const record = key as Record<string, unknown>;
    const sortedObj: Record<string, unknown> = {};
    Object.keys(record)
      .sort()
      .forEach((k) => {
        sortedObj[k] = record[k];
      });
    return JSON.stringify(sortedObj);
  }

  return JSON.stringify(key.map(hashKey));
}

export class StructuralMap<K, V> {
  private internalMap = new Map<string, { key: K; value: V }>();
  private hashKey: HashKey<K> = hashKey;

  constructor(entries?: [K, V][], customHashKey?: HashKey<K>) {
    if (customHashKey) {
      this.hashKey = customHashKey;
    }
    if (entries) {
      for (const [k, v] of entries) {
        this.set(k, v);
      }
    }
  }
  //
  // Executes a provided function once per map entry
  forEach(callbackfn: (value: V, key: K, map: StructuralMap<K, V>) => void): void {
    for (const entry of this.internalMap.values()) {
      callbackfn(entry.value, entry.key, this);
    }
  }
  set(key: K, value: V): this {
    this.internalMap.set(this.hashKey(key), { key, value });
    return this;
  }

  get(key: K): V | undefined {
    return this.internalMap.get(this.hashKey(key))?.value;
  }

  has(key: K): boolean {
    return this.internalMap.has(this.hashKey(key));
  }

  delete(key: K): boolean {
    return this.internalMap.delete(this.hashKey(key));
  }

  get size(): number {
    return this.internalMap.size;
  }

  clone(): StructuralMap<K, V> {
    const newMap = new StructuralMap<K, V>();
    for (const [hKey, entry] of this.internalMap.entries()) {
      newMap.internalMap.set(hKey, { ...entry });
    }
    return newMap;
  }
}
export class StructuralSet<T> {
  // Retain access to original un-hashed object reference for safe execution inside forEach loop iterations
  private internalMap = new Map<string, T>();
  private hashKey: HashKey<T> = hashKey;

  constructor(elements?: T[], customHashKey?: HashKey<T>) {
    if (customHashKey) {
      this.hashKey = customHashKey;
    }
    if (elements) {
      for (const el of elements) {
        this.add(el);
      }
    }
  }

  // Executes a provided function once per set element
  forEach(callbackfn: (value: T, value2: T, set: StructuralSet<T>) => void): void {
    for (const value of this.internalMap.values()) {
      callbackfn(value, value, this);
    }
  }

  add(element: T): this {
    this.internalMap.set(this.hashKey(element), element);
    return this;
  }

  has(element: T): boolean {
    return this.internalMap.has(this.hashKey(element));
  }

  delete(element: T): boolean {
    return this.internalMap.delete(this.hashKey(element));
  }

  get size(): number {
    return this.internalMap.size;
  }

  clone(): StructuralSet<T> {
    const newSet = new StructuralSet<T>();
    for (const [hKey, value] of this.internalMap.entries()) {
      newSet.internalMap.set(hKey, value);
    }
    return newSet;
  }
}

class PriorityQueue<T, P extends number> {
  private heap: { element: T; priority: P }[] = [];

  get size(): number {
    return this.heap.length;
  }

  enqueue(element: T, priority: P): void {
    this.heap.push({ element, priority });
    this.bubbleUp(this.heap.length - 1);
  }

  dequeue(): T | undefined {
    if (this.heap.length === 0) return undefined;
    const top = this.heap[0].element;
    const bottom = this.heap.pop();
    if (this.heap.length > 0 && bottom !== undefined) {
      this.heap[0] = bottom;
      this.sinkDown(0);
    }
    return top;
  }

  private bubbleUp(index: number): void {
    const node = this.heap[index];
    while (index > 0) {
      const parentIndex = Math.floor((index - 1) / 2);
      const parent = this.heap[parentIndex];
      if (node.priority >= parent.priority) break;
      this.heap[index] = parent;
      index = parentIndex;
    }
    this.heap[index] = node;
  }

  private sinkDown(index: number): void {
    const length = this.heap.length;
    const node = this.heap[index];
    for (;;) {
      const leftChildIndex = 2 * index + 1;
      const rightChildIndex = 2 * index + 2;
      let swapIndex = -1;

      if (leftChildIndex < length && this.heap[leftChildIndex].priority < node.priority) {
        swapIndex = leftChildIndex;
      }
      if (rightChildIndex < length) {
        if (
          (swapIndex === -1 && this.heap[rightChildIndex].priority < node.priority) ||
          (swapIndex !== -1 && this.heap[rightChildIndex].priority < this.heap[swapIndex].priority)
        ) {
          swapIndex = rightChildIndex;
        }
      }
      if (swapIndex === -1) break;
      this.heap[index] = this.heap[swapIndex];
      index = swapIndex;
    }
    this.heap[index] = node;
  }
}

export function aStar<T>(
  start: T,
  goal: T,
  getNeighbors: (current: T, cameFrom: StructuralMap<T, T>) => Iterable<T>, // T[],
  cost: (from: T, to: T) => number,
  heuristic: (node: T) => number,
  isGoal: (node: T, goal: T) => boolean,
  customHashKey?: HashKey<T>
): T[] | null {
  function reconstructPath(current: T, cameFrom: StructuralMap<T, T>, acc: T[]): T[] {
    const parent = cameFrom.get(current);
    if (parent !== undefined) {
      return reconstructPath(parent, cameFrom, [current, ...acc]);
    }
    return [current, ...acc];
  }

  function search(
    openSet: PriorityQueue<T, number>,
    gScores: StructuralMap<T, number>,
    cameFrom: StructuralMap<T, T>
  ): T[] | null {
    if (openSet.size === 0) return null;

    const currentPos = openSet.dequeue()!;

    if (isGoal(currentPos, goal)) {
      return reconstructPath(currentPos, cameFrom, []);
    }

    const neighbors = getNeighbors(currentPos, cameFrom);
    const nextGScores = gScores.clone();
    const nextCameFrom = cameFrom.clone();

    for (const neighbor of neighbors) {
      const tentativeG = (gScores.get(currentPos) ?? 0) + cost(currentPos, neighbor);
      const currentNeighborG = gScores.get(neighbor) ?? Infinity;

      if (tentativeG < currentNeighborG) {
        const fScore = tentativeG + heuristic(neighbor);
        openSet.enqueue(neighbor, fScore);
        nextGScores.set(neighbor, tentativeG);
        nextCameFrom.set(neighbor, currentPos);
      }
    }

    return search(openSet, nextGScores, nextCameFrom);
  }

  const initialGScores = new StructuralMap<T, number>([[start, 0]], customHashKey);
  const initialOpen = new PriorityQueue<T, number>();
  initialOpen.enqueue(start, heuristic(start));

  return search(initialOpen, initialGScores, new StructuralMap<T, T>(undefined, customHashKey));
}

export function dijkstra<T>(
  getNeighbors: (node: T) => T[],
  getCost: (from: T, to: T) => number,
  source: T
): StructuralMap<T, number> {
  const distances = new StructuralMap<T, number>();
  const pq = new PriorityQueue<T, number>();

  distances.set(source, 0);
  pq.enqueue(source, 0);

  while (pq.size > 0) {
    const u = pq.dequeue()!;
    const distU = distances.get(u) ?? Infinity;

    for (const v of getNeighbors(u)) {
      const weight = getCost(u, v);
      const newDist = distU + weight;

      if (!distances.has(v) || newDist < distances.get(v)!) {
        distances.set(v, newDist);
        pq.enqueue(v, newDist);
      }
    }
  }
  return distances;
}

export function dijkstraPath<T>(
  // getNeighbors: (node: T) => Iterable<T>, // T[],
  getNeighbors: (current: T, cameFrom: StructuralMap<T, T>) => Iterable<T>, // T[],
  getCost: (from: T, to: T) => number,
  source: T,
  isGoal: (node: T) => boolean,
  customHashKey?: HashKey<T>
): [number, T[]] | null {
  const distances = new StructuralMap<T, number>(undefined, customHashKey);
  const predecessors = new StructuralMap<T, T>(undefined, customHashKey);
  const pq = new PriorityQueue<T, number>();

  distances.set(source, 0);
  pq.enqueue(source, 0);
  let found = false;
  let target: T | undefined = undefined;

  while (pq.size > 0 && !found) {
    const u = pq.dequeue()!;
    const distU = distances.get(u) ?? Infinity;

    if (isGoal(u)) {
      found = true;
      target = u;
    } else {
      for (const v of getNeighbors(u, predecessors)) {
        const weight = getCost(u, v);
        const newDist = distU + weight;

        if (!distances.has(v) || newDist < distances.get(v)!) {
          distances.set(v, newDist);
          predecessors.set(v, u);
          pq.enqueue(v, newDist);
        }
      }
    }
  }

  if (found) {
    const path: T[] = [];
    assert(target !== undefined);
    let curr: T = target;
    path.push(curr);
    while (hashKey(curr) !== hashKey(source)) {
      curr = predecessors.get(curr)!;
      path.push(curr);
    }
    return [distances.get(target)!, path.reverse()];
  }
  return null;
}

/**
 * Finds all paths that achieve the absolute minimum cost from a source node to any goal node.
 * Like Dijkstra, but returns all shortest paths.
 */
export function dijkstraAllPaths<T>(
  // getNeighbors: (node: T) => Iterable<T>,
  getNeighbors: (current: T) => Iterable<T>, // T[],
  getCost: (from: T, to: T) => number,
  source: T,
  isGoal: (node: T) => boolean,
  customHashKey?: HashKey<T>
): [number, T[][]] | null {
  const dist = new StructuralMap<T, number>(undefined, customHashKey);
  const prev = new StructuralMap<T, T[]>(undefined, customHashKey);

  dist.set(source, 0);

  const pq = new PriorityQueue<T, number>();
  pq.enqueue(source, 0);

  let optimalGoalCost = Infinity;
  const reachedGoals = new StructuralSet<T>(undefined, customHashKey);

  // --- 1. GRAPH SEARCH / TRAVERSAL PHASE ---
  while (pq.size > 0) {
    const u = pq.dequeue()!;
    const currentDist = dist.get(u) ?? Infinity;

    if (currentDist > optimalGoalCost) {
      break;
    }

    if (isGoal(u)) {
      optimalGoalCost = currentDist;
      reachedGoals.add(u);
    }

    for (const v of getNeighbors(u)) {
      const edgeCost = getCost(u, v);
      const alt = currentDist + edgeCost;
      const existingDist = dist.get(v) ?? Infinity;

      if (alt < existingDist) {
        dist.set(v, alt);
        prev.set(v, [u]);
        pq.enqueue(v, alt);
      } else if (alt === existingDist) {
        const predecessors = prev.get(v);
        if (predecessors) {
          predecessors.push(u);
        } else {
          prev.set(v, [u]);
        }
      }
    }
  }

  if (reachedGoals.size === 0 || optimalGoalCost === Infinity) {
    return null;
  }

  // --- 2. MEMOIZED BACKTRACKING PHASE ---
  const memo = new StructuralMap<T, T[][]>();

  function backtrack(node: T): T[][] {
    if (dist.get(node) === 0) {
      return [[node]];
    }
    if (memo.has(node)) {
      return memo.get(node)!;
    }

    const paths: T[][] = [];
    const parents = prev.get(node) ?? [];

    for (const p of parents) {
      for (const path of backtrack(p)) {
        paths.push([...path, node]);
      }
    }

    memo.set(node, paths);
    return paths;
  }

  // --- 3. PATH AGGREGATION VIA SAFELY TYPED FOREACH ---
  const allOptimalPaths: T[][] = [];

  reachedGoals.forEach((goalNode) => {
    if (dist.get(goalNode) === optimalGoalCost) {
      allOptimalPaths.push(...backtrack(goalNode));
    }
  });

  return [optimalGoalCost, allOptimalPaths];
}

export function* findAllPathsRecursive<T>(
  getNeighbors: (node: T) => T[],
  isTarget: (node: T) => boolean,
  pathSoFar: T[]
): Generator<T[], void, unknown> {
  const currentNode = pathSoFar[0];
  if (isTarget(currentNode)) {
    yield [...pathSoFar].reverse();
  } else {
    const neighbors = getNeighbors(currentNode);
    for (const neighbor of neighbors) {
      if (!pathSoFar.map(hashKey).includes(hashKey(neighbor))) {
        yield* findAllPathsRecursive(getNeighbors, isTarget, [neighbor, ...pathSoFar]);
      }
    }
  }
}

// If maxPath is specified, prune paths of length > maxPath
export function* findAllPaths<T>(
  neighbors: (node: T) => Iterable<T>, // T[],
  isGoal: (node: T) => boolean,
  start: T,
  maxPath?: number
): Generator<T[], void, unknown> {
  if (isGoal(start)) {
    yield [start];
  }

  interface StackFrame {
    curr: T;
    path: T[];
    remainingNeighbors: T[];
    visited: StructuralSet<T>;
  }

  const stack: StackFrame[] = [
    {
      curr: start,
      path: [start],
      remainingNeighbors: [...neighbors(start)],
      visited: new StructuralSet([start]),
    },
  ];

  while (stack.length > 0) {
    const frame = stack.pop()!;

    // prune if path too long
    if (maxPath !== undefined && frame.path.length > maxPath) {
      continue;
    }
    if (isGoal(frame.curr)) {
      yield [...frame.path].reverse();
      continue;
    }

    if (frame.remainingNeighbors.length === 0) {
      continue;
    }

    const next = frame.remainingNeighbors[0];
    const restNeighbors = frame.remainingNeighbors.slice(1);

    stack.push({ ...frame, remainingNeighbors: restNeighbors });

    if (!frame.visited.has(next)) {
      const nextVisited = frame.visited.clone().add(next);
      const nextPath = [next, ...frame.path];

      // prune before pushing
      if (maxPath === undefined || nextPath.length <= maxPath) {
        stack.push({
          curr: next,
          path: nextPath,
          remainingNeighbors: [...neighbors(next)],
          visited: nextVisited,
        });
      }
    }
  }
}
