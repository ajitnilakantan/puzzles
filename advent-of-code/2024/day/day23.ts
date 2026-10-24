import assert from "node:assert";
import { findMaximalCliques } from "../lib/max_clique";
import { debug, fileio } from "../lib/prelude";
// Adjust path to your source file
import type { DayPlugin } from "../src/plugin";

// export private functions for testing
export const __test__ = { parseData, findCliquesOfSize3, findLargestSubgraph };

export type GetNeighbours<N> = (node: N) => Iterable<N> | N[];

/**
 * Finds all cliques of size 3 (triangles) using a neighbor discovery function.
 * @param nodes All unique nodes present in the graph.
 * @param getNeighbours A functional dependency that returns a node's neighbors.
 * @returns An array of tuples, each containing 3 nodes forming a clique.
 */
export function findCliquesOfSize3<N>(nodes: Iterable<N> | N[], getNeighbours: GetNeighbours<N>): [N, N, N][] {
  const triangles: [N, N, N][] = [];

  // We use a Map to assign a unique, comparable ID to each node reference.
  // This allows the code to work natively with objects, arrays, strings, or numbers.
  const nodeIds = new Map<N, number>();
  let idCounter = 0;

  const getId = (node: N): number => {
    if (!nodeIds.has(node)) {
      nodeIds.set(node, idCounter++);
    }
    return nodeIds.get(node)!;
  };

  // Pre-populate IDs for all primary nodes
  for (const node of nodes) {
    getId(node);
  }

  // Process each node 'u'
  for (const u of nodes) {
    const uId = getId(u);
    const uNeighbours = getNeighbours(u);

    // Turn neighbors into an array to cleanly iterate over pairings
    const uNeighboursArr = Array.isArray(uNeighbours) ? uNeighbours : Array.from(uNeighbours);

    // To find triangles relative to 'u', look at all pairs of its neighbors (v, w)
    for (let i = 0; i < uNeighboursArr.length; i++) {
      for (let j = i + 1; j < uNeighboursArr.length; j++) {
        const v = uNeighboursArr[i];
        const w = uNeighboursArr[j];

        const vId = getId(v);
        const wId = getId(w);

        // Enforce a strict structural order (uId < vId < wId)
        // This eliminates duplicate permutations and ensures each triangle is caught exactly once
        if (uId < vId && vId < wId) {
          // Check if v and w are connected to complete the triangle
          const vNeighbours = getNeighbours(v);
          const vNeighboursSet = vNeighbours instanceof Set ? vNeighbours : new Set(vNeighbours);

          if (vNeighboursSet.has(w)) {
            triangles.push([u, v, w]);
          }
        } else if (uId < wId && wId < vId) {
          // Alternate valid structural ordering permutation
          const wNeighbours = getNeighbours(w);
          const wNeighboursSet = wNeighbours instanceof Set ? wNeighbours : new Set(wNeighbours);

          if (wNeighboursSet.has(v)) {
            triangles.push([u, w, v]);
          }
        }
      }
    }
  }

  return triangles;
}

function parseData(lines: string[]): [nodes: Set<string>, neighbours: Map<string, Set<string>>] {
  const nodes = new Set<string>();
  const neighbours = new Map<string, Set<string>>();
  const addEdge = (from: string, to: string): void => {
    const n1 = neighbours.get(from);
    if (n1 === undefined) {
      neighbours.set(from, new Set<string>([to]));
    } else {
      n1.add(to);
    }
    const n2 = neighbours.get(to);
    if (n2 === undefined) {
      neighbours.set(to, new Set<string>([from]));
    } else {
      n2.add(from);
    }
  };

  for (const line of lines) {
    const [n1, n2] = fileio.tokenize(line, "-\x20");
    [n1, n2].forEach((n) => nodes.add(n));
    addEdge(n1, n2);
  }
  return [nodes, neighbours];
}

function findLargestSubgraph(nodes: Set<string>, neighbours: Map<string, Set<string>>): string {
  // Find largest clique
  const getNeighbours =
    (neighbours: Map<string, Set<string>>) =>
    (node: string): string[] => {
      const n = neighbours.get(node);
      return n ? [...n] : [];
    };
  const result = findMaximalCliques<string>(nodes, getNeighbours(neighbours));
  // Find largest, sort, concatenate
  const largest = result
    .reduce((longest, current) => (current.length > longest.length ? current : longest))
    .toSorted()
    .join(",");

  return largest;
}

function findTriangles(nodes: Set<string>, neighbours: Map<string, Set<string>>): [string, string, string][] {
  const getNeighbours =
    (neighbours: Map<string, Set<string>>) =>
    (node: string): string[] => {
      const n = neighbours.get(node);
      return n ? [...n] : [];
    };
  return findCliquesOfSize3<string>(nodes, getNeighbours(neighbours));
}

function solvePart1(lines: string[]): number {
  const [nodes, neighbours] = parseData(lines);
  const triangles = findTriangles(nodes, neighbours);
  const solution = triangles.reduce(
    (acc, [t1, t2, t3]) => ([t1, t2, t3].some((s) => s.startsWith("t")) ? acc + 1 : acc),
    0
  );

  return solution;
}

function solvePart2(lines: string[]): string {
  const [nodes, neighbours] = __test__.parseData(lines);
  const solution = __test__.findLargestSubgraph(nodes, neighbours);

  return solution;
}

const plugin: DayPlugin = {
  name: "Day 23",
  async run({ verbose }) {
    console.log(`Running ${this.name} verbose=${verbose}`);
    const data = await fileio.linesFromFile(`${import.meta.dir}/day23.txt`);

    const start1 = performance.now();
    const solution1 = solvePart1(data);
    const duration1 = (performance.now() - start1).toFixed(3);
    console.log(`${this.name}.Part1 = ${solution1} : Elapsed time: ${duration1}ms`);
    assert.strictEqual(solution1, 1411, `${this.name}.Part1`);

    const start2 = performance.now();
    const solution2 = solvePart2(data);
    const duration2 = (performance.now() - start2).toFixed(3);
    console.log(`${this.name}.Part2 = ${solution2} : Elapsed time: ${duration2}ms`);
    assert.strictEqual(solution2, "aq,bn,ch,dt,gu,ow,pk,qy,tv,us,yx,zg,zu", `${this.name}.Part2`);
  },
};

export default plugin;
