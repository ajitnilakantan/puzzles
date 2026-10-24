import assert from "node:assert";
import { init } from "z3-solver";
import { debug, fileio } from "../lib/prelude";
import type { DayPlugin } from "../src/plugin";

// export private functions for testing
export const __test__ = { parseData, run };

export interface Registers {
  A: number;
  B: number;
  C: number;
  PC: number; // Program counter
}

// opcodes
export enum OP {
  adv = 0,
  bxl = 1,
  bst = 2,
  jnz = 3,
  bcx = 4,
  out = 5,
  bdv = 6,
  cdv = 7,
}

function getLiteral(reg: Registers, program: number[]): number {
  return program[reg.PC + 1];
}

function getCombo(reg: Registers, program: number[]): number {
  const val = program[reg.PC + 1];
  switch (val) {
    case 0:
    case 1:
    case 2:
    case 3:
      return val;
    case 4:
      return reg.A;
    case 5:
      return reg.B;
    case 6:
      return reg.C;
    default: // Exhaustiveness check
      throw new Error(`Unhandled fallback`);
  }
}

function run(
  reg: Registers,
  program: number[],
  outFn: ((outVal: number) => void) | null = null
): [Registers, number[]] {
  const outdata: number[] = [];
  let o: number;
  while (reg.PC < program.length) {
    // The program is a list of plain numbers. Treat the value as an opcode.
    // eslint-disable-next-line @typescript-eslint/no-unsafe-enum-assignment
    const opcode: OP = program[reg.PC];
    switch (opcode) {
      case OP.adv: // 0
        reg.A = Math.trunc(reg.A / 2 ** getCombo(reg, program));
        break;
      case OP.bxl: // 1
        reg.B = reg.B ^ getLiteral(reg, program);
        break;
      case OP.bst: // 2
        reg.B = getCombo(reg, program) % 8;
        break;
      case OP.jnz: // 3
        if (reg.A != 0) {
          reg.PC = getLiteral(reg, program) - 2;
        }
        break;
      case OP.bcx: // 4
        reg.B = reg.B ^ reg.C;
        break;
      case OP.out: // 5
        o = getCombo(reg, program) % 8;
        outdata.push(o);
        if (outFn) {
          outFn(o);
        }
        break;
      case OP.bdv: // 6
        reg.B = Math.trunc(reg.A / 2 ** getCombo(reg, program));
        break;
      case OP.cdv: // 7
        reg.C = Math.trunc(reg.A / 2 ** getCombo(reg, program));
        break;
      default: // Exhaustiveness check
        throw new Error(`Unhandled fallback`);
    }
    reg.PC += 2;
  }
  return [reg, outdata];
}

function parseData(lines: string[]): [reg: Registers, program: number[]] {
  const rhs = (line: string): string => {
    const toks = fileio.tokenize(line, ":");
    assert(toks.length == 2);
    return toks[1];
  };
  const [A, B, C] = [Number(rhs(lines[0])), Number(rhs(lines[1])), Number(rhs(lines[2]))];
  const reg: Registers = { A: A, B: B, C: C, PC: 0 };
  const program = fileio.tokenize(rhs(lines[4]), "\x20,").map((s) => Number(s));
  return [reg, program];
}

async function part2Solver(): Promise<bigint> {
  const { Context } = await init();
  const ctx = new Context("main");
  const { Optimize, BitVec } = ctx;
  const optimizer = new Optimize();
  // A,B,C registers
  type BitVecExpr = ReturnType<typeof BitVec.const>;
  const a: BitVecExpr[] = [];
  const b: BitVecExpr[] = [];
  const c: BitVecExpr[] = [];
  // Remainders
  const r: BitVecExpr[] = [];

  // declare 64bit variables
  a.push(BitVec.const(`a`, 64));
  b.push(BitVec.const(`b`, 64));
  c.push(BitVec.const(`c`, 64));

  const bv = (val: number): BitVecExpr => ctx.BitVec.val(val, 64); // convert number to BitVec
  const last = (x: BitVecExpr[]): BitVecExpr => x[x.length - 1]; // Last element
  const lastm1 = (x: BitVecExpr[]): BitVecExpr => x[x.length - 2]; // second to last element
  const val0 = bv(0);
  const val3 = bv(3);
  const val5 = bv(5);
  const mask8 = bv(7); // x % 8  is equivalent to  x & 7
  const program: number[] = [2, 4, 1, 3, 7, 5, 4, 7, 0, 3, 1, 5, 5, 5, 3, 0];
  optimizer.add(last(b).eq(val0));
  optimizer.add(last(c).eq(val0));
  // Loop through one iteration of the program
  /*
   * # Program: 2,4,1,3,7,5,4,7,0,3,1,5,5,5,3,0
   *
   * a1 = a
   * b1 = b
   * c1 = c
   * # 2 4
   * b1 = a1 % 8
   *
   * # 1 3
   * b2 = b1 ^ 3
   *
   * #7 5
   * c2 = trunc(a1 / 2 ** b2)
   *
   * #4 7
   * b3 = b2 ^ c2
   *
   * # 0 3
   * a2 = trunc(a1 / 2 ** 3)
   *
   * # 1 5
   * b4 = b3 ^ 5
   *
   * # 5 5
   * o1 = b4 % 8
   * o1 = 2
   *
   * # 3 0
   * a2 != 0
   */
  for (let k = 0; k < program.length; k++) {
    // 2 4: bst 4: b = a % 8 => b = a & 0x7
    b.push(BitVec.const(`b1_${k}`, 64));
    optimizer.add(last(b).eq(last(a).and(mask8)));
    // 1 3: blx 3: b = b ^ 3
    b.push(BitVec.const(`b2_${k}`, 64));
    optimizer.add(last(b).eq(lastm1(b).xor(val3)));
    // 7 5: cdv 5: c = trunc(a / 2 ** b) => c = a >> b
    c.push(BitVec.const(`c1_${k}`, 64));
    optimizer.add(last(c).eq(last(a).lshr(last(b))));
    // 4 7: bcx  : b = b ^ c
    b.push(BitVec.const(`b3_${k}`, 64));
    optimizer.add(last(b).eq(lastm1(b).xor(last(c))));
    // 0 3: adv 3: a = trunc(a / 2**3) => a = a >> 3
    a.push(BitVec.const(`a_${k}`, 64));
    optimizer.add(last(a).eq(lastm1(a).lshr(val3)));
    // 1 5: bxl 5: b = b ^ 5
    b.push(BitVec.const(`b4_${k}`, 64));
    optimizer.add(last(b).eq(lastm1(b).xor(val5)));
    // 5 5: out 5: r = b % 8 => r = b & 0x7; r == program[i]
    r.push(BitVec.const(`r${k}`, 64));
    optimizer.add(last(r).eq(last(b).and(mask8)));
    optimizer.add(last(r).eq(bv(program[k])));
    // 3 0: jnz 0: a <> 0; loop
    if (k < program.length - 1) {
      optimizer.add(last(a).eq(val0).not()); // jnz
    } else {
      optimizer.add(last(a).eq(val0)); // final jnz
    }
  }

  optimizer.minimize(a[0]);

  let solution = 0n;
  if ((await optimizer.check()) === "sat") {
    const model = optimizer.model();
    const val = model.eval(a[0]);
    const result = BigInt(val.asString());
    // console.log(`x: ${model.eval(a[0])} = ${result}`);
    solution = result;
  } else {
    // console.log(`UNSAT`);
  }
  return solution;
}

function solvePart1(lines: string[]): string {
  const [reg, program] = parseData(lines);
  const outdata: number[] = [];
  const outFn = (outval: number): void => {
    outdata.push(outval);
  };
  run(reg, program, outFn);
  const solution = outdata.map((s) => String(s)).join(",");

  return solution;
}

async function solvePart2(_lines: string[]): Promise<bigint> {
  const solution = await part2Solver();
  return solution;
}

const plugin: DayPlugin = {
  name: "Day 17",
  async run({ verbose }) {
    console.log(`Running ${this.name} verbose=${verbose}`);
    const data = await fileio.linesFromFile(`${import.meta.dir}/day17.txt`);

    const start1 = performance.now();
    const solution1 = solvePart1(data);
    const duration1 = (performance.now() - start1).toFixed(3);
    console.log(`${this.name} verbose=${verbose}.Part1 = ${solution1} : Elapsed time: ${duration1}ms`);
    assert.strictEqual(solution1, "1,3,5,1,7,2,5,1,6", `${this.name} verbose=${verbose}.Part1`);

    const start2 = performance.now();
    const solution2 = await solvePart2(data);
    const duration2 = (performance.now() - start2).toFixed(3);
    console.log(`${this.name} verbose=${verbose}.Part2 = ${solution2} : Elapsed time: ${duration2}ms`);
    assert.strictEqual(solution2, 236555997372013n, `${this.name} verbose=${verbose}.Part2`);
  },
};

export default plugin;
