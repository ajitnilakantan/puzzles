import { describe, expect, test } from "bun:test";
import { debug, fileio } from "../lib/prelude";
import { __test__ } from "./day23";

describe("day23", () => {
  const data = `kh-tc
                qp-kh
                de-cg
                ka-co
                yn-aq
                qp-ub
                cg-tb
                vc-aq
                tb-ka
                wh-tc
                yn-cg
                kh-ub
                ta-co
                de-co
                tc-td
                tb-wq
                wh-td
                ta-ka
                td-qp
                aq-cg
                wq-ub
                ub-vc
                de-ta
                wq-aq
                wq-vc
                wh-yn
                ka-de
                kh-ta
                co-tc
                wh-qp
                tb-vc
                td-yn`;

  const lines = fileio.linesFromString(data);

  test("part1", async () => {
    const [nodes, neighbours] = __test__.parseData(lines);
    const getNeighbours =
      (neighbours: Map<string, Set<string>>) =>
      (node: string): string[] => {
        const n = neighbours.get(node);
        return n ? [...n] : [];
      };
    const triangles = __test__.findCliquesOfSize3<string>(nodes, getNeighbours(neighbours));
    expect(triangles.length).toBe(12);
    const solution = triangles.reduce(
      (acc, [t1, t2, t3]) => ([t1, t2, t3].some((s) => s.startsWith("t")) ? acc + 1 : acc),
      0
    );
    expect(solution).toBe(7);
  });

  test("part2", async () => {
    const [nodes, neighbours] = __test__.parseData(lines);
    const solution = __test__.findLargestSubgraph(nodes, neighbours);
    expect(solution).toBe("co,de,ka,ta");
  });
});
