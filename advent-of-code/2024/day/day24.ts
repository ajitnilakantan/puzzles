import assert from "node:assert";
import { type CombinationDecision, type CombinationsConfig, getRecursiveCombinations } from "../lib/combinations";
import { debug, fileio } from "../lib/prelude";
import type { DayPlugin } from "../src/plugin";

// export private functions for testing
export const __test__ = { parseData, runCircuit, get_register, get_predecessors, simulateSwaps };

interface Wire {
  name: string;
  status: "0" | "1" | "?";
}

type WiresState = Map<string, Wire["status"]>;

interface Gate {
  type: "AND" | "OR" | "XOR";
  input: [string, string]; // Name of Wire
  output: string; // Name of Wire
}

// Swapped pair of output wires
type Pair = [string, string];

function parseData(
  lines: string[]
): [inputs: Wire[], gates: Gate[], gateInputs: Map<string, Set<[Gate, number]>>, gateOutputs: Map<string, Gate>] {
  const chunks = fileio.chunkLines(lines);
  assert(chunks.length === 2);

  // Parse input settings
  const inputs: Wire[] = [];
  chunks[0].forEach((s) => {
    const tokens = fileio.tokenize(s, ":\x20");
    assert(tokens.length === 2);
    const name = tokens[0];
    assert(["0", "1"].some((s) => s === tokens[1]));
    const value = tokens[1] as Wire["status"];
    inputs.push({ name: name, status: value });
  });

  // Parse network
  const gates: Gate[] = [];

  // Map input to the gates they enter (0 or 1)
  const gateInputs = new Map<string, Set<[Gate, number]>>();
  chunks[1].forEach((s) => {
    const tokens = fileio.tokenize(s, "->\x20");
    assert(tokens.length === 4);
    assert(["AND", "OR", "XOR"].some((s) => s === tokens[1]));
    const gate: Gate = {
      type: tokens[1] as Gate["type"],
      input: [tokens[0], tokens[2]],
      output: tokens[3],
    };
    gates.push(gate);

    // Map of wire name to the gates they input (on input0 or input1)
    // Used to quickly look up "wire name" => Gate (input 0 or 1)
    for (let k = 0; k <= 1; k++) {
      let current = gateInputs.get(gate.input[k]);
      if (current === undefined) {
        gateInputs.set(gate.input[k], (current = new Set<[Gate, number]>()));
      }
      current.add([gate, k]);
    }
  });

  // Map output wire name to the gate that outputs it.
  // Used to backtrace a wire to the gates that feed into it
  const gateOutputs = new Map<string, Gate>();
  gates.forEach((g) => gateOutputs.set(g.output, g));

  return [inputs, gates, gateInputs, gateOutputs];
}

// Run the circuit similation for the given inputs and gate definitions.
// Returns the status of all the wires.
function runCircuit(inputs: Wire[], gates: Gate[], gateInputs: Map<string, Set<[Gate, number]>>): WiresState {
  let signals = new Set<Wire>(inputs);
  const wires: WiresState = new Map(
    gates.flatMap((g) => [
      [g.input[0], "?"],
      [g.input[1], "?"],
      [g.output, "?"],
    ])
  );
  while (wires.entries().some(([_k, v]) => v === "?")) {
    // Loop while we have unknown signals
    const newSignals = new Set<Wire>();
    for (const signal of signals) {
      // Update the "wires" maps of name to status
      wires.set(signal.name, signal.status);
      // Look up which gates get input from "signal"
      for (const [gate, _inputNumber] of gateInputs.get(signal.name) ?? []) {
        assert(_inputNumber === 0 || _inputNumber === 1);
        if (wires.get(gate.input[0]) !== "?" && wires.get(gate.input[1]) !== "?" && wires.get(gate.output) === "?") {
          // perform the operation
          const gateOutput: Wire = { name: gate.output, status: "?" };
          assert(wires.get(gate.output) === "?");
          assert(wires.get(gate.input[0]) !== "?");
          assert(wires.get(gate.input[1]) !== "?");
          switch (gate.type) {
            case "AND":
              gateOutput.status = wires.get(gate.input[0]) === "1" && wires.get(gate.input[1]) === "1" ? "1" : "0";
              break;
            case "OR":
              gateOutput.status = wires.get(gate.input[0]) === "1" || wires.get(gate.input[1]) === "1" ? "1" : "0";
              break;
            case "XOR":
              gateOutput.status = wires.get(gate.input[0]) !== wires.get(gate.input[1]) ? "1" : "0";
              break;
            default:
              throw new Error(`Unhandled fallback`); // Exhaustiveness check
          }
          newSignals.add(gateOutput);
          wires.set(gateOutput.name, gateOutput.status);
        }
      }
    }
    // Feed forward the new signals
    const areSetsEqual = <T>(a: Set<T>, b: Set<T>): boolean =>
      a.size === b.size && [...a].every((value) => b.has(value));
    if (areSetsEqual(signals, newSignals)) {
      return wires;
    }
    signals = newSignals;
  }
  return wires;
}

// get the value of the "x", "y", or "z" wires concatenated as a number
function get_register(wires: WiresState, register: "x" | "y" | "z"): number {
  const r_wires = [...wires.entries().filter(([k, _v]) => k.startsWith(register))].toSorted(
    (a, b) => -a[0].localeCompare(b[0])
  );
  const r_str = r_wires.map(([_k, v]) => v).join("");
  return parseInt(r_str, 2);
}

// Get the gates that feed into "input"
function get_predecessors(input: string, _gates: Gate[], gateOutputs: Map<string, Gate>): Set<string> {
  const predecessors = new Set<string>();
  const queue: string[] = [input];
  while (queue.length > 0) {
    const current = queue.pop()!;
    const parentGate = gateOutputs.get(current);
    if (parentGate !== undefined) {
      predecessors.add(current);
      queue.push(parentGate.input[0]);
      queue.push(parentGate.input[1]);
    }
  }
  return predecessors;
}

// An x and y input setting, and the z (= x + y) the circuit should produce for it
interface TestCase {
  inputs: Wire[];
  expected: bigint;
}
interface TestSuite {
  cases: TestCase[];
  zBits: number;
}

const bitName = (letter: "x" | "y" | "z", bit: number): string => letter + bit.toString(10).padStart(2, "0");

// Read the x or y register from the input wires
function readInputRegister(inputs: Wire[], letter: "x" | "y"): bigint {
  return inputs
    .filter((w) => w.name.startsWith(letter))
    .reduce((acc, w) => (w.status === "1" ? acc | (1n << BigInt(parseInt(w.name.slice(1), 10))) : acc), 0n);
}

// Make the input wires of "template" hold the numbers x and y
function makeTestCase(template: Wire[], x: bigint, y: bigint): TestCase {
  const inputs = template.map((w): Wire => {
    const value = w.name.startsWith("x") ? x : y;
    const bit = BigInt(parseInt(w.name.slice(1), 10));
    return { name: w.name, status: ((value >> bit) & 1n) === 1n ? "1" : "0" };
  });
  return { inputs, expected: x + y };
}

// A single input can pass by luck, so check several: the given input, each bit on its own and together,
// carries rippling up through the lower bits, and some fixed pseudo-random numbers.
function buildTestSuite(inputs: Wire[], gates: Gate[]): TestSuite {
  const xBits = inputs.filter((w) => w.name.startsWith("x")).length;
  const zBits = gates.filter((g) => /^z\d+$/.test(g.output)).length;
  const numbers: [bigint, bigint][] = [[readInputRegister(inputs, "x"), readInputRegister(inputs, "y")]];
  for (let i = 0; i < xBits; i++) {
    const bit = 1n << BigInt(i);
    numbers.push([bit, 0n], [0n, bit], [bit, bit]);
    if (i > 0) numbers.push([bit - 1n, 1n]);
  }
  numbers.push([(1n << BigInt(xBits)) - 1n, 1n]);
  const mask = (1n << BigInt(xBits)) - 1n;
  let seed = 12345n;
  const nextRandom = (): bigint => {
    seed = (seed * 6364136223846793005n + 1442695040888963407n) & ((1n << 64n) - 1n);
    return (seed >> 8n) & mask;
  };
  for (let k = 0; k < 16; k++) numbers.push([nextRandom(), nextRandom()]);
  return { cases: numbers.map(([x, y]) => makeTestCase(inputs, x, y)), zBits };
}

// Return the index of the first bad z bit over all the test cases, starting at the least significant.
// Return -1 on no differences. A wire that never got a value (e.g. a swap made a loop) counts as bad.
// Gives up early with the bad bit found so far once it is <= giveUpAt, as the caller only needs to know it is not above that.
function getBadBit(
  suite: TestSuite,
  gates: Gate[],
  gateInputs: Map<string, Set<[Gate, number]>>,
  giveUpAt = -1
): number {
  let firstBad = Infinity;
  for (const testCase of suite.cases) {
    const wires = runCircuit(testCase.inputs, gates, gateInputs);
    for (let i = 0; i < Math.min(firstBad, suite.zBits); i++) {
      const expectedBit = ((testCase.expected >> BigInt(i)) & 1n) === 1n ? "1" : "0";
      if (wires.get(bitName("z", i)) !== expectedBit) {
        firstBad = i;
        break;
      }
    }
    if (firstBad <= giveUpAt) return firstBad;
  }
  return firstBad === Infinity ? -1 : firstBad;
}

// Swap the pairs of output gates in-place. Applying the same swaps in reverse order undoes them.
function applySwaps(_gates: Gate[], gateOutputs: Map<string, Gate>, swaps: Pair[]): void {
  for (const pair of swaps) {
    const [g0, g1] = [gateOutputs.get(pair[0])!, gateOutputs.get(pair[1])!];
    g0.output = pair[1];
    g1.output = pair[0];
    // Keep the lookup in step with the gates so later swaps and predecessor searches see the swapped wiring
    gateOutputs.set(pair[0], g1);
    gateOutputs.set(pair[1], g0);
  }
}

interface SwapContext {
  currentBadBit: number;
}

// Candidate pairs to swap for the bad bit. One wire of the pair feeds the bad bit (but no lower bit);
// the other can be any wire that no lower bit uses, as a swapped wire often belongs to the next bit's logic.
// Wires already swapped are left out of both sides.
function getCandidatePairs(badBit: number, currentPath: Pair[], gates: Gate[], gateOutputs: Map<string, Gate>): Pair[] {
  const alreadySwapped = new Set(currentPath.flat());
  // Wires that feed the lower significant bits
  let lowerPredecessors = new Set<string>();
  for (let k = 0; k < badBit; k++) {
    lowerPredecessors = lowerPredecessors.union(get_predecessors(bitName("z", k), gates, gateOutputs));
  }
  const cone = get_predecessors(bitName("z", badBit), gates, gateOutputs)
    .difference(lowerPredecessors)
    .difference(alreadySwapped);
  const partners = new Set(gateOutputs.keys()).difference(lowerPredecessors).difference(alreadySwapped);

  const pairs: Pair[] = [];
  for (const a of cone) {
    for (const b of partners) {
      // Don't list a pair twice when both wires are in the cone
      if (a === b || (cone.has(b) && b < a)) continue;
      pairs.push([a, b]);
    }
  }
  return pairs;
}

// Look at the gate-predecessors of the bad z-bit and try swapping 1pair, 2pairs, 3pairs, 4pairs to see if the bad bit gets fixed.
// Returns the winning pairs, or null if no 4 swaps fix the circuit.
function simulateSwaps(
  inputs: Wire[],
  gates: Gate[],
  gateInputs: Map<string, Set<[Gate, number]>>,
  gateOutputs: Map<string, Gate>
): Pair[] | null {
  // Start from the first bad bit of the unswapped circuit. Each swap must move the first bad bit higher.
  const suite = buildTestSuite(inputs, gates);
  const baseline = getBadBit(suite, gates, gateInputs);

  const config: CombinationsConfig<Pair, SwapContext> = {
    getChoicesForLevel: (currentPath: Pair[], _levelIndex: number, context: SwapContext): Pair[] => {
      const pairsLeft = 4 - currentPath.length;
      assert(pairsLeft >= 1 && pairsLeft <= 4);

      // onStep already checked this path and recorded its first bad bit in the context
      const pairs = getCandidatePairs(context.currentBadBit, currentPath, gates, gateOutputs);

      return pairs;
    },
    onStep: (currentPath: Pair[], _levelIndex: number, context: SwapContext): CombinationDecision => {
      // Skip/Prune any when we exceed 4 swaps
      if (currentPath.length > 4) {
        return "skip";
      }
      // Find the incorrect bit
      applySwaps(gates, gateOutputs, currentPath); // Apply
      const badBit = getBadBit(suite, gates, gateInputs, context.currentBadBit);
      applySwaps(gates, gateOutputs, [...currentPath].reverse()); // Undo apply, newest swap first
      if (badBit === -1) {
        // The circuit is correct. The puzzle asks for exactly 4 swaps, so a shorter fix is not a solution
        return currentPath.length === 4 ? "success" : "skip";
      }
      if (currentPath.length === 4) {
        // Failure, try something else
        return "skip";
      }
      if (badBit <= context.currentBadBit) {
        // No progress: this swap did not fix the bad bit, or it broke a lower significant bit
        return "skip";
      }
      if (getCandidatePairs(badBit, currentPath, gates, gateOutputs).length === 0) {
        // Dead end: nothing left to swap for this bit
        return "skip";
      }
      // Record progress so the next level's choices start from this bad bit
      context.currentBadBit = badBit;
      return "continue";
    },
  };

  const outcome = getRecursiveCombinations<Pair, SwapContext>(config, { currentBadBit: baseline });

  // Only a success sets done. Dead ends also end up in results, so the answer is never read from results in general.
  // A success is always the last path appended.
  return outcome.done ? outcome.results[outcome.results.length - 1] : null;
}

function solvePart1(lines: string[]): number {
  const [inputs, gates, gateInputs, _gateOutputs] = parseData(lines);
  const wires = runCircuit(inputs, gates, gateInputs);
  const solution = get_register(wires, "z");

  return solution;
}

function solvePart2(lines: string[]): string {
  const [inputs, gates, gateInputs, gateOutputs] = parseData(lines);

  const result = __test__.simulateSwaps(inputs, gates, gateInputs, gateOutputs)!;
  const solution = result
    .flatMap((m) => m)
    .toSorted((a, b) => a[0].localeCompare(b[0]))
    .join(",");

  return solution;
}

const plugin: DayPlugin = {
  name: "Day 24",
  async run({ verbose }) {
    console.log(`Running ${this.name} verbose=${verbose}`);
    const data = await fileio.linesFromFile(`${import.meta.dir}/day24.txt`);

    const start1 = performance.now();
    const solution1 = solvePart1(data);
    const duration1 = (performance.now() - start1).toFixed(3);
    console.log(`${this.name} verbose=${verbose}.Part1 = ${solution1} : Elapsed time: ${duration1}ms`);
    assert.strictEqual(solution1, 41324968993486, `${this.name} verbose=${verbose}.Part1`);

    const start2 = performance.now();
    const solution2 = solvePart2(data);
    const duration2 = (performance.now() - start2).toFixed(3);
    console.log(`${this.name} verbose=${verbose}.Part2 = ${solution2} : Elapsed time: ${duration2}ms`);
    assert.strictEqual(solution2, "bmn,jss,mvb,rds,wss,z08,z18,z23", `${this.name} verbose=${verbose}.Part2`);
  },
};

export default plugin;
