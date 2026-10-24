import { describe, expect, test } from "bun:test";
import { debug, fileio } from "../lib/prelude";
import { __test__ } from "./day24";

describe("day24", () => {
  const data1 = `x00: 1
                 x01: 1
                 x02: 1
                 y00: 0
                 y01: 1
                 y02: 0

                 x00 AND y00 -> z00
                 x01 XOR y01 -> z01
                 x02 OR y02 -> z02`;

  const data2 = `x00: 1
                 x01: 0
                 x02: 1
                 x03: 1
                 x04: 0
                 y00: 1
                 y01: 1
                 y02: 1
                 y03: 1
                 y04: 1

                 ntg XOR fgs -> mjb
                 y02 OR x01 -> tnw
                 kwq OR kpj -> z05
                 x00 OR x03 -> fst
                 tgd XOR rvg -> z01
                 vdt OR tnw -> bfw
                 bfw AND frj -> z10
                 ffh OR nrd -> bqk
                 y00 AND y03 -> djm
                 y03 OR y00 -> psh
                 bqk OR frj -> z08
                 tnw OR fst -> frj
                 gnj AND tgd -> z11
                 bfw XOR mjb -> z00
                 x03 OR x00 -> vdt
                 gnj AND wpb -> z02
                 x04 AND y00 -> kjc
                 djm OR pbm -> qhw
                 nrd AND vdt -> hwm
                 kjc AND fst -> rvg
                 y04 OR y02 -> fgs
                 y01 AND x02 -> pbm
                 ntg OR kjc -> kwq
                 psh XOR fgs -> tgd
                 qhw XOR tgd -> z09
                 pbm OR djm -> kpj
                 x03 XOR y03 -> ffh
                 x00 XOR y04 -> ntg
                 bfw OR bqk -> z06
                 nrd XOR fgs -> wpb
                 frj XOR qhw -> z04
                 bqk OR frj -> z07
                 y03 OR x01 -> nrd
                 hwm AND bqk -> z03
                 tgd XOR rvg -> z12
                 tnw OR pbm -> gnj`;

  const expected1 = [4, 2024];

  test("part1", async () => {
    for (const [index, data] of [data1, data2].entries()) {
      const lines = fileio.linesFromString(data);
      const [inputs, gates, gateInputs, _gateOutputs] = __test__.parseData(lines);
      const wires = __test__.runCircuit(inputs, gates, gateInputs);
      const solution = __test__.get_register(wires, "z");
      expect(solution).toBe(expected1[index]);
    }
  });

  test("part2", async () => {
    // No test case for part2.  Solution is in day24
  });
});
