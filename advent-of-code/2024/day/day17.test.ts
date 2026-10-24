import { describe, expect, test } from "bun:test";
import { init } from "z3-solver";
import { debug, fileio } from "../lib/prelude";
import { __test__ } from "./day17";

describe("day17", () => {
  const data = `Register A: 729
                Register B: 0
                Register C: 0

                Program: 0,1,5,4,3,0`;

  const lines = fileio.linesFromString(data);

  test("part1", async () => {
    {
      const [reg, program] = __test__.parseData(lines);
      const outFnData: number[] = [];
      const outFn = (outval: number): void => {
        outFnData.push(outval);
      };
      const [_outReg, outData] = __test__.run(reg, program, outFn);
      const solution = outFnData.map((s) => String(s)).join(",");
      expect(outData.map((s) => String(s)).join(",")).toEqual("4,6,3,5,6,3,5,2,1,0");
      expect(solution).toBe("4,6,3,5,6,3,5,2,1,0");
    }
    {
      const [reg, program] = [{ A: 0, B: 0, C: 9, PC: 0 }, [2, 6]];
      const [outReg, _] = __test__.run(reg, program);
      expect(outReg.B).toEqual(1);
    }
    {
      const [reg, program] = [{ A: 10, B: 0, C: 0, PC: 0 }, [5, 0, 5, 1, 5, 4]];
      const [_outReg, outData] = __test__.run(reg, program);
      expect(outData).toEqual([0, 1, 2]);
    }

    {
      const [reg, program] = [{ A: 2024, B: 0, C: 0, PC: 0 }, [0, 1, 5, 4, 3, 0]];
      const [outReg, outData] = __test__.run(reg, program);
      expect(outData).toEqual([4, 2, 5, 6, 7, 7, 7, 7, 3, 1, 0]);
      expect(outReg.A).toEqual(0);
    }
    {
      const [reg, program] = [{ A: 0, B: 29, C: 0, PC: 0 }, [1, 7]];
      const [outReg, _outData] = __test__.run(reg, program);
      expect(outReg.B).toEqual(26);
    }

    {
      const [reg, program] = [{ A: 0, B: 2024, C: 43690, PC: 0 }, [4, 0]];
      const [outReg, _outData] = __test__.run(reg, program);
      expect(outReg.B).toEqual(44354);
    }
  });

  test("part2 manual test", async () => {
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
    a.push(BitVec.const(`a0`, 64));
    b.push(BitVec.const(`b0`, 64));
    c.push(BitVec.const(`c0`, 64));
    r.push(BitVec.const(`r0`, 64));

    const bv = (val: number): BitVecExpr => ctx.BitVec.val(val, 64);
    const mask8 = ctx.BitVec.val(7, 64); // x % 8  is equivalent to  x & 7
    const program: number[] = [0, 3, 5, 4, 3, 0];
    for (let k = 0; k < program.length; k++) {
      a.push(BitVec.const(`a${k + 1}`, 64));
      r.push(BitVec.const(`r${k + 1}`, 64));
      optimizer.add(a[k + 1].eq(a[k].lshr(bv(3)))); // a = a / 2^^combo
      optimizer.add(r[k + 1].eq(a[k + 1].and(mask8))); // out combo(4=a)
      optimizer.add(r[k + 1].eq(bv(program[k])));
      if (k < program.length - 1) {
        optimizer.add(a[k + 1].eq(bv(0)).not()); // jnz
      } else {
        optimizer.add(a[k + 1].eq(bv(0))); // final jnz
      }
    }

    optimizer.minimize(a[0]);

    // optimizer.add(a[program.length].eq(bv(0))); // final jnz
    // optimizer.add(a[a.length-1].eq(bv(0))); // final jnz
    let solution = 0;
    if ((await optimizer.check()) === "sat") {
      const model = optimizer.model();
      const val = model.eval(a[0]);
      const result = Number(val.asString());
      // console.log(`x: ${model.eval(a[0])} = ${result}`)
      solution = result;
    } else {
      // console.log(`UNSAT`);
    }
    expect(solution).toBe(117440);
  });

  test("part2", async () => {
    const solution = 0;
    expect(solution).toBe(0);
  });
});
