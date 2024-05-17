use std::cell::RefCell;
use std::collections::HashMap;
use z3::ast::Ast;

#[allow(unused_macros)]
macro_rules! function_name {
    () => {{
        fn f() {}
        fn type_name_of<T>(_: T) -> &'static str {
            std::any::type_name::<T>()
        }
        let name = type_name_of(f);

        // Find and cut the rest of the path
        match &name[..name.len() - 3].rfind(':') {
            Some(pos) => &name[pos + 1..name.len() - 3],
            None => &name[..name.len() - 3],
        }
    }};
}
// The output is wrapped in a Result to allow matching on errors
// Returns an Iterator to the Reader of the lines of the file.
#[allow(dead_code)]
fn read_lines<P>(filename: P) -> Result<Vec<String>, std::io::Error>
where
    P: AsRef<std::path::Path>,
{
    use std::io::BufRead;
    let file = std::fs::File::open(filename)?;
    let mut result: Vec<String> = Vec::new();
    let lines = std::io::BufReader::new(file).lines();
    for line in lines.flatten() {
        result.push(line)
    }
    Ok(result)
}

// The output is wrapped in a Result to allow matching on errors
// Returns list of strings separated by blank lines
#[allow(dead_code)]
fn read_chunks<P>(filename: P) -> Result<Vec<String>, std::io::Error>
where
    P: AsRef<std::path::Path>,
{
    let text = std::fs::read_to_string(filename)?;
    let chunks: Vec<String> = regex::Regex::new(r"\r\n\r\n")
        .unwrap()
        .split(&text)
        .map(|x| x.to_string())
        .collect::<Vec<String>>();

    Ok(chunks)
}

#[allow(dead_code)]
fn read_numbers<T>(line: &str) -> Vec<T>
where
    T: std::str::FromStr,
    <T as std::str::FromStr>::Err: std::fmt::Debug,
{
    let numbers: Vec<T> = line
        .split([',', ' ', '\r', '\n'].as_ref())
        .filter(|&x| !x.is_empty())
        .map(|x| x.to_string().parse::<T>().unwrap())
        .collect();

    numbers
}

struct Z3Solver<'a> {
    w: Vec<RefCell<z3::ast::Int<'a>>>,
    x: Vec<RefCell<z3::ast::Int<'a>>>,
    y: Vec<RefCell<z3::ast::Int<'a>>>,
    z: Vec<RefCell<z3::ast::Int<'a>>>,
    inputs: Vec<RefCell<z3::ast::Int<'a>>>,
    constants: HashMap<i64, RefCell<z3::ast::Int<'a>>>,
    zero: z3::ast::Int<'a>,
    one: z3::ast::Int<'a>,
    ten: z3::ast::Int<'a>,
    ten_power: Vec<z3::ast::Int<'a>>,
}

impl<'a> Z3Solver<'a> {
    // See: https://users.rust-lang.org/t/having-a-struct-where-one-member-refers-to-another/51380/5
    fn new(ctx: &'a z3::Context, solver: &'a z3::Solver) -> Self {
        let val = Self {
            w: vec![RefCell::new(z3::ast::Int::new_const(ctx, "w0"))],
            x: vec![RefCell::new(z3::ast::Int::new_const(ctx, "x0"))],
            y: vec![RefCell::new(z3::ast::Int::new_const(ctx, "y0"))],
            z: vec![RefCell::new(z3::ast::Int::new_const(ctx, "z0"))],
            inputs: vec![],
            constants: HashMap::new(),
            zero: z3::ast::Int::from_u64(ctx, 0),
            one: z3::ast::Int::from_u64(ctx, 1),
            ten: z3::ast::Int::from_u64(ctx, 10),
            ten_power: (0u32..15u32)
                .map(|x| z3::ast::Int::from_i64(ctx, i64::pow(10i64, x)))
                .collect(),
        };

        solver.assert(&val.w[0].borrow()._eq(&val.zero));
        solver.assert(&val.x[0].borrow()._eq(&val.zero));
        solver.assert(&val.y[0].borrow()._eq(&val.zero));
        solver.assert(&val.z[0].borrow()._eq(&val.zero));

        val
    }
    fn new_input(
        &mut self,
        ctx: &'a z3::Context,
        solver: &'a z3::Solver,
    ) -> RefCell<z3::ast::Int<'a>> {
        let name = format!("i{}", self.inputs.len());
        let input = z3::ast::Int::new_const(ctx, name);

        solver.assert(&input.gt(&self.zero));
        solver.assert(&input.lt(&self.ten));

        self.inputs.push(RefCell::new(input));
        self.inputs.last().unwrap().clone()
    }

    fn new_var(
        &mut self,
        ctx: &'a z3::Context,
        _solver: &'a z3::Solver,
        variable: &str,
    ) -> RefCell<z3::ast::Int<'a>> {
        let name = match variable {
            "w" => format!("w{}", self.w.len()),
            "x" => format!("x{}", self.x.len()),
            "y" => format!("y{}", self.y.len()),
            "z" => format!("z{}", self.z.len()),
            _ => panic!(),
        };
        let var = RefCell::new(z3::ast::Int::new_const(ctx, name));
        match variable {
            "w" => self.w.push(var.clone()),
            "x" => self.x.push(var.clone()),
            "y" => self.y.push(var.clone()),
            "z" => self.z.push(var.clone()),
            _ => panic!(),
        };

        var.clone()
    }

    fn lookup(&mut self, ctx: &'a z3::Context, variable: &str) -> RefCell<z3::ast::Int<'a>> {
        match variable {
            "w" => self.w.last().unwrap().clone(),
            "x" => self.x.last().unwrap().clone(),
            "y" => self.y.last().unwrap().clone(),
            "z" => self.z.last().unwrap().clone(),
            _ => {
                let number = variable.parse::<i64>().unwrap();
                let ret = self
                    .constants
                    .entry(number)
                    .or_insert_with(|| RefCell::new(z3::ast::Int::from_i64(ctx, number)));
                ret.clone()
            }
        }
    }

    // Return: max solution
    fn solve(&mut self, ctx: &'a z3::Context, solver: &'a z3::Solver) -> i64 {
        // Convert the inputs to a decimal number
        let serial_no_len = self.inputs.len();
        let serial_no_digits: Vec<z3::ast::Int> = (0..serial_no_len)
            .map(|x| {
                z3::ast::Int::mul(
                    ctx,
                    &[
                        &self.ten_power[serial_no_len - x - 1],
                        &self.inputs[x].borrow(),
                    ],
                )
            })
            .collect();

        // Convert vec of objects to vec of references:
        let serial_no = z3::ast::Int::add(ctx, &serial_no_digits.iter().collect::<Vec<_>>()[..]);

        // z must be zero for a valid serial number
        solver.assert(&self.z.last().unwrap().borrow()._eq(&self.zero));

        /*
        // Normally would call:
        Solver: z3::Optimize ...;
        solver.maximize(&serial_no);
        let sat = solver.check(&[]);
        // ...but the Optimize algo is super slow. Use a binary search instead.
         */

        let mut min = 0;
        let mut max = 99999999999999;
        let mut guess = (min + max) / 2;
        let mut serial_no_v = 0;

        while guess != min && guess != max {
            println!("min={} guess={} max={}", min, guess, max);
            solver.push();
            solver.assert(&serial_no.le(&z3::ast::Int::from_u64(&ctx, max)));
            solver.assert(&serial_no.ge(&z3::ast::Int::from_u64(&ctx, guess)));
            let sat = solver.check();
            if sat == z3::SatResult::Sat {
                let model = solver.get_model().unwrap();
                serial_no_v = model.eval(&serial_no, true).unwrap().as_i64().unwrap();
                println!("sat = {:?} serial_no={}", sat, serial_no_v);

                min = if serial_no_v as u64 > guess {
                    serial_no_v as u64
                } else {
                    guess
                };
            } else {
                println!("sat = {:?}", sat);
                max = guess;
            }
            solver.pop(1);

            guess = (max + min) / 2;
            println!("==> min={} guess={} max={}", min, guess, max);
        }

        println!("Getting serial_no: {:?}", serial_no_v);

        serial_no_v
    }

    // Return: solution
    // solve() is too slow. Try an alternate approach to reduce one digit at at time.
    fn solve1(&mut self, ctx: &'a z3::Context, solver: &'a z3::Solver) -> i64 {
        // z must be zero for a valid serial number
        solver.assert(&self.z.last().unwrap().borrow()._eq(&self.zero));

        println!("solver=\n{:#?}", solver);
        let sat = solver.check();
        assert_eq!(sat, z3::SatResult::Sat);
        let model = solver.get_model().unwrap();

        assert_eq!(self.inputs.len(), 14);

        let mut guess: Vec<i64> = self
            .inputs
            .iter()
            .map(|x| {
                model
                    .eval(&x.borrow().clone(), true)
                    .unwrap()
                    .as_i64()
                    .unwrap()
            })
            .collect();
        println!("Guess         = {:?}", guess);

        for digit in 0..self.inputs.len() {
            loop {
                if guess[digit] == 9 {
                    break;
                }
                solver.push();
                for d in 0..self.inputs.len() {
                    if d < digit {
                        println!("  assert input{d} == {}", guess[d]);
                        solver.assert(
                            &self.inputs[d]
                                .borrow()
                                .clone()
                                ._eq(&z3::ast::Int::from_i64(&ctx, guess[d])),
                        );
                    } else if d == digit {
                        println!("  assert input{d} >  {}", guess[d]);
                        solver.assert(
                            &self.inputs[d]
                                .borrow()
                                .clone()
                                .gt(&z3::ast::Int::from_i64(&ctx, guess[d])),
                        );
                    }
                }
                let sat = solver.check();
                if sat != z3::SatResult::Sat {
                    println!("Unsat for digit={digit} {sat:?}");
                    solver.pop(1);
                    break;
                }
                let model = solver.get_model().unwrap();
                guess = self
                    .inputs
                    .iter()
                    .map(|x| {
                        model
                            .eval(&x.borrow().clone(), true)
                            .unwrap()
                            .as_i64()
                            .unwrap()
                    })
                    .collect();

                println!("Guess digit={digit} = {:?}", guess);
                solver.pop(1);
            }
        }
        guess.iter().fold(0, |acc, x| 10 * acc + x)
    }

    // Return: solution
    // solve() is too slow. Try an alternate approach to reduce one digit at at time.
    fn solve2(&mut self, ctx: &'a z3::Context, solver: &'a z3::Solver) -> i64 {
        // z must be zero for a valid serial number
        solver.assert(&self.z.last().unwrap().borrow()._eq(&self.zero));

        let sat = solver.check();
        assert_eq!(sat, z3::SatResult::Sat);
        let model = solver.get_model().unwrap();

        assert_eq!(self.inputs.len(), 14);

        let mut guess: Vec<i64> = self
            .inputs
            .iter()
            .map(|x| {
                model
                    .eval(&x.borrow().clone(), true)
                    .unwrap()
                    .as_i64()
                    .unwrap()
            })
            .collect();
        println!("Guess         = {:?}", guess);

        for digit in 0..self.inputs.len() {
            loop {
                if guess[digit] == 1 {
                    break;
                }
                solver.push();
                for d in 0..self.inputs.len() {
                    if d < digit {
                        solver.assert(
                            &self.inputs[d]
                                .borrow()
                                ._eq(&z3::ast::Int::from_i64(&ctx, guess[d])),
                        );
                    } else if d == digit {
                        solver.assert(
                            &self.inputs[d]
                                .borrow()
                                .lt(&z3::ast::Int::from_i64(&ctx, guess[d])),
                        );
                    }
                }
                let sat = solver.check();
                if sat != z3::SatResult::Sat {
                    println!("Unsat for digit={digit}");
                    solver.pop(1);
                    break;
                }
                let model = solver.get_model().unwrap();
                guess = self
                    .inputs
                    .iter()
                    .map(|x| {
                        model
                            .eval(&x.borrow().clone(), true)
                            .unwrap()
                            .as_i64()
                            .unwrap()
                    })
                    .collect();

                println!("Guess digit={digit} = {:?}", guess);
                solver.pop(1);
            }
        }
        guess.iter().fold(0, |acc, x| 10 * acc + x)
    }

    fn inp(&mut self, ctx: &'a z3::Context, solver: &'a z3::Solver, var: &str) {
        // variable <- input
        let variable = self.new_var(ctx, solver, var);
        let input = self.new_input(ctx, solver);
        solver.assert(&variable.borrow()._eq(&input.borrow()));
    }
    fn add(&mut self, ctx: &'a z3::Context, solver: &'a z3::Solver, var: &str, arg: &str) {
        // variable += arg
        let old_variable1 = self.lookup(ctx, var);
        let old_variable = &old_variable1.borrow();
        let variable1 = self.new_var(ctx, solver, var);
        let variable = &variable1.borrow();
        let arg1 = self.lookup(ctx, arg);
        let arg = &arg1.borrow();

        solver.assert(&variable._eq(&z3::ast::Int::add(ctx, &[old_variable, arg])));
    }
    fn mul(&mut self, ctx: &'a z3::Context, solver: &'a z3::Solver, var: &str, arg: &str) {
        // variable *= arg
        let old_variable1 = self.lookup(ctx, var);
        let old_variable = &old_variable1.borrow();
        let variable1 = self.new_var(ctx, solver, var);
        let variable = &variable1.borrow();
        let arg1 = self.lookup(ctx, arg);
        let arg = &arg1.borrow();

        solver.assert(&variable._eq(&z3::ast::Int::mul(ctx, &[old_variable, arg])));
    }
    fn div(&mut self, ctx: &'a z3::Context, solver: &'a z3::Solver, var: &str, arg: &str) {
        // var =  var // arg (division with truncation)
        let old_variable1 = self.lookup(ctx, var);
        let old_variable = &old_variable1.borrow();
        let variable1 = self.new_var(ctx, solver, var);
        let variable = &variable1.borrow();
        let arg1 = self.lookup(ctx, arg);
        let arg = &arg1.borrow();

        solver.assert(&variable._eq(&old_variable.div(arg)));
    }
    fn modulo(&mut self, ctx: &'a z3::Context, solver: &'a z3::Solver, var: &str, arg: &str) {
        // Modulo operation var = var mod b
        let old_variable1 = self.lookup(ctx, var);
        let old_variable = &old_variable1.borrow();
        let variable1 = self.new_var(ctx, solver, var);
        let variable = &variable1.borrow();
        let arg1 = self.lookup(ctx, arg);
        let arg = &arg1.borrow();

        solver.assert(&variable._eq(&old_variable.rem(arg)));
    }
    fn eql(&mut self, ctx: &'a z3::Context, solver: &'a z3::Solver, var: &str, arg: &str) {
        // If var == arg then var = 1 else var = 0
        let old_variable1 = self.lookup(ctx, var);
        let old_variable = &old_variable1.borrow();
        let variable1 = self.new_var(ctx, solver, var);
        let variable = &variable1.borrow();
        let arg1 = self.lookup(ctx, arg);
        let arg = &arg1.borrow();

        let op = (old_variable)._eq(arg).ite(&self.one, &self.zero).clone();

        solver.assert(&variable._eq(&op));
    }
}

fn process_lines1(lines: &[&str]) -> i64 {
    let cfg = z3::Config::new();
    let ctx = z3::Context::new(&cfg);
    let solver = z3::Solver::new(&ctx);
    //let solver = z3::Optimize::new(&ctx);
    let mut z3solver = Z3Solver::new(&ctx, &solver);

    for line in lines {
        let words: Vec<String> = line
            .split([' ', '\r', '\n'].as_ref())
            .filter(|&x| !x.is_empty())
            .map(|x| x.to_string())
            .collect();
        match words[0].as_str() {
            "inp" => z3solver.inp(&ctx, &solver, &words[1]),
            "add" => z3solver.add(&ctx, &solver, &words[1], &words[2]),
            "mul" => z3solver.mul(&ctx, &solver, &words[1], &words[2]),
            "div" => z3solver.div(&ctx, &solver, &words[1], &words[2]),
            "mod" => z3solver.modulo(&ctx, &solver, &words[1], &words[2]),
            "eql" => z3solver.eql(&ctx, &solver, &words[1], &words[2]),
            _ => panic!(),
        }
    }
    let solution = z3solver.solve1(&ctx, &solver);
    println!("part1 solution={}", solution);
    solution
}

fn process_lines2(lines: &[&str]) -> i64 {
    let cfg = z3::Config::new();
    let ctx = z3::Context::new(&cfg);
    let solver = z3::Solver::new(&ctx);
    //let solver = z3::Optimize::new(&ctx);
    let mut z3solver = Z3Solver::new(&ctx, &solver);

    for line in lines {
        let words: Vec<String> = line
            .split([' ', '\r', '\n'].as_ref())
            .filter(|&x| !x.is_empty())
            .map(|x| x.to_string())
            .collect();
        match words[0].as_str() {
            "inp" => z3solver.inp(&ctx, &solver, &words[1]),
            "add" => z3solver.add(&ctx, &solver, &words[1], &words[2]),
            "mul" => z3solver.mul(&ctx, &solver, &words[1], &words[2]),
            "div" => z3solver.div(&ctx, &solver, &words[1], &words[2]),
            "mod" => z3solver.modulo(&ctx, &solver, &words[1], &words[2]),
            "eql" => z3solver.eql(&ctx, &solver, &words[1], &words[2]),
            _ => panic!(),
        }
    }
    let solution = z3solver.solve2(&ctx, &solver);
    println!("part1 solution={}", solution);
    solution
}

fn part1(lines: &[String]) {
    // https://stackoverflow.com/questions/33216514/how-do-i-convert-a-vecstring-to-vecstr
    let lines: Vec<&str> = lines.iter().map(AsRef::as_ref).collect();
    let result1 = process_lines1(&lines);
    println!("Result1 = {}", result1); // 89959794919939 takes a very long time
}

fn part2<S: AsRef<str>>(lines: &[S]) {
    let lines: Vec<&str> = lines.iter().map(AsRef::as_ref).collect();
    let result2 = process_lines2(&lines);
    println!("Result2 = {}", result2); // 17115131916112 takes a very long time
}

pub fn main() -> Result<(), Box<dyn std::error::Error>> {
    let args: Vec<String> = std::env::args().collect();
    let file_name = if args.len() > 1 {
        &args[1]
    } else {
        "input24.txt"
    };

    let lines = match read_lines(file_name) {
        Ok(v) => v,
        Err(e) => {
            println!("Error reading file: '{}' error: '{:?}'", file_name, e);
            std::panic::set_hook(Box::new(|_info| {
                // do nothing
            }));
            panic!();
        }
    };
    //let chunks = read_chunks(file_name)?;

    part1(&lines);
    part2(&lines[..]);

    Ok(())
}

#[cfg(test)]
mod tests {
    use super::*;
    use std::ops::AddAssign;
    use std::ops::SubAssign;

    #[test]
    fn test_z3_solver() {
        let cfg = z3::Config::new();
        let ctx = z3::Context::new(&cfg);
        let x = z3::ast::Int::new_const(&ctx, "x");
        let y = z3::ast::Int::new_const(&ctx, "y");
        let solver = z3::Solver::new(&ctx);
        let opt = z3::Optimize::new(&ctx);
        solver.assert(&x.gt(&y));
        solver.assert(&x.gt(&z3::ast::Int::from_i64(&ctx, 0)));
        solver.assert(&y.gt(&z3::ast::Int::from_i64(&ctx, 0)));
        solver.assert(&x.lt(&z3::ast::Int::from_i64(&ctx, 10)));
        solver.assert(&y.lt(&z3::ast::Int::from_i64(&ctx, 10)));

        opt.maximize(&z3::ast::Int::add(&ctx, &[&x, &y]));

        assert_eq!(solver.check(), z3::SatResult::Sat);

        let model = solver.get_model().unwrap();
        let xv = model.eval(&x, true).unwrap().as_i64().unwrap();
        let yv = model.eval(&y, true).unwrap().as_i64().unwrap();
        println!("{} x: {} y: {}", function_name!(), xv, yv);
    }
    #[test]
    fn test_z3_optimizer() {
        let cfg = z3::Config::new();
        let ctx = z3::Context::new(&cfg);
        let x = z3::ast::Int::new_const(&ctx, "x");
        let y = z3::ast::Int::new_const(&ctx, "y");
        let opt = z3::Optimize::new(&ctx);

        opt.assert(&x.gt(&y));
        opt.assert(&x.gt(&z3::ast::Int::from_i64(&ctx, 0)));
        opt.assert(&y.gt(&z3::ast::Int::from_i64(&ctx, 0)));
        opt.assert(&x.lt(&z3::ast::Int::from_i64(&ctx, 10)));
        opt.assert(&y.lt(&z3::ast::Int::from_i64(&ctx, 10)));

        let mut x_plus_y = z3::ast::Int::add(&ctx, &[&x, &y]);
        x_plus_y.add_assign(&z3::ast::Int::from_i64(&ctx, 11));

        let mut x_plus_10 = z3::ast::Int::from_i64(&ctx, 5);
        x_plus_10.add_assign(&z3::ast::Int::from_i64(&ctx, 10));

        opt.maximize(&z3::ast::Int::add(&ctx, &[&x, &y]));

        assert_eq!(opt.check(&[]), z3::SatResult::Sat);

        let model = opt.get_model().unwrap();
        let xv = model.eval(&x, true).unwrap().as_i64().unwrap();
        let yv = model.eval(&y, true).unwrap().as_i64().unwrap();
        let x_plus_yv = model.eval(&x_plus_y, true).unwrap().as_i64().unwrap();
        let x_plus_10v = model.eval(&x_plus_10, true).unwrap().as_i64().unwrap();
        println!(
            "{} x: {}  y: {}: x_plus_y: {} x_plus_10: {}",
            function_name!(),
            xv,
            yv,
            x_plus_yv,
            x_plus_10v,
        );
        assert_eq!(xv, 9);
        assert_eq!(yv, 8);
    }

    #[test]
    fn test_z3_optimizer2() {
        let cfg = z3::Config::new();
        let ctx = z3::Context::new(&cfg);
        let solver = z3::Optimize::new(&ctx);

        let i1 = z3::ast::Int::new_const(&ctx, "i1");
        let i2 = z3::ast::Int::new_const(&ctx, "i2");
        let x2 = z3::ast::Int::new_const(&ctx, "x2");
        //let mut x = z3::ast::Int::new_const(&ctx, "x");
        let mut x = z3::ast::Int::from_u64(&ctx, 0);

        let y = z3::ast::Int::new_const(&ctx, "y");
        let zero = z3::ast::Int::from_u64(&ctx, 0);
        let one = z3::ast::Int::from_u64(&ctx, 1);
        let nine = z3::ast::Int::from_u64(&ctx, 9);
        let ten = z3::ast::Int::from_u64(&ctx, 10);

        solver.assert(&i1.gt(&zero));
        solver.assert(&i1.lt(&ten));
        solver.assert(&i2.gt(&zero));
        solver.assert(&i2.lt(&ten));

        let y = z3::ast::Int::add(&ctx, &[&nine, &y]);
        // let mut x = z3::ast::Int::add(&ctx, &[&nine, &x]);
        x.add_assign(&nine);
        // let mut x = z3::ast::Int::sub(&ctx, &[&x, &ten]);
        x.sub_assign(&ten);
        // let mut x = z3::ast::Int::add(&ctx, &[&x, &i1]);
        x.add_assign(&i1);
        solver.assert(&x2._eq(&x));
        let x = z3::ast::Int::add(&ctx, &[&one, &x]);
        solver.assert(&x._eq(&one));
        //solver.assert(&y);

        let i1i2 = z3::ast::Int::add(&ctx, &[&z3::ast::Int::mul(&ctx, &[&i1, &ten]), &i2]);

        let mut z = z3::ast::Int::from_u64(&ctx, 0);
        z.add_assign(&one);
        z = z3::ast::Int::add(&ctx, &[&one, &x]);

        solver.maximize(&i1i2);

        assert_eq!(solver.check(&[]), z3::SatResult::Sat);

        let model = solver.get_model().unwrap();
        let xv = model.eval(&x, true).unwrap().as_i64().unwrap();
        let yv = model.eval(&y, true).unwrap().as_i64().unwrap();
        let i1i2v = model.eval(&i1i2, true).unwrap().as_i64().unwrap();
        let x2 = model.eval(&x2, true).unwrap().as_i64().unwrap();
        let z = model.eval(&z, true).unwrap().as_i64().unwrap();

        println!(
            "{} x: {}  y: {}: i1i2: {} x2={} z={}",
            function_name!(),
            xv,
            yv,
            i1i2v,
            x2,
            z
        );
        assert_eq!(xv, 1);
        assert_eq!(yv, 9);
        assert_eq!(i1i2v, 19);
        assert_eq!(z, xv + 1);
    }

    #[test]
    fn test_z3_optimizer3() {
        let cfg = z3::Config::new();
        let ctx = z3::Context::new(&cfg);
        let solver = z3::Solver::new(&ctx);

        let i1 = z3::ast::Int::new_const(&ctx, "i1");
        let i2 = z3::ast::Int::new_const(&ctx, "i2");
        let x2 = z3::ast::Int::new_const(&ctx, "x2");
        let mut x = z3::ast::Int::from_u64(&ctx, 0);

        let y = z3::ast::Int::new_const(&ctx, "y");
        let zero = z3::ast::Int::from_u64(&ctx, 0);
        let one = z3::ast::Int::from_u64(&ctx, 1);
        let nine = z3::ast::Int::from_u64(&ctx, 9);
        let ten = z3::ast::Int::from_u64(&ctx, 10);

        solver.assert(&i1.gt(&zero));
        solver.assert(&i1.lt(&ten));
        solver.assert(&i2.gt(&zero));
        solver.assert(&i2.lt(&ten));

        let y = z3::ast::Int::add(&ctx, &[&nine, &y]);
        x.add_assign(&nine);
        x.sub_assign(&ten);
        x.add_assign(&i1);
        solver.assert(&x2._eq(&x));
        let x = z3::ast::Int::add(&ctx, &[&one, &x]);
        solver.assert(&x._eq(&one));

        let i1i2 = z3::ast::Int::add(&ctx, &[&z3::ast::Int::mul(&ctx, &[&i1, &ten]), &i2]);

        let mut z = z3::ast::Int::from_u64(&ctx, 0);
        z.add_assign(&one);
        z = z3::ast::Int::add(&ctx, &[&one, &x]);

        // solver.maximize(&i1i2);
        let mut min = 0;
        let mut max = 99;
        let mut guess = (min + max) / 2;
        let mut model: Option<z3::Model> = None;

        loop {
            solver.push();
            solver.assert(&i1i2.gt(&z3::ast::Int::from_u64(&ctx, guess)));
            let sat = solver.check();
            if sat == z3::SatResult::Sat {
                model = Some(solver.get_model().unwrap());
                let i1i2v = model
                    .as_ref()
                    .unwrap()
                    .eval(&i1i2, true)
                    .unwrap()
                    .as_i64()
                    .unwrap();
                println!("sat = {:?} i1i2v={}", sat, i1i2v);
                min = guess;
            } else {
                println!("sat = {:?}", sat);
                max = guess;
            }
            solver.pop(1);
            println!("sat={:?} min={} max={} guess={}", sat, min, max, guess);
            guess = (max + min) / 2;
            if guess == min || guess == max {
                break;
            }
        }

        let model = model.unwrap();
        let xv = model.eval(&x, true).unwrap().as_i64().unwrap();
        let yv = model.eval(&y, true).unwrap().as_i64().unwrap();
        let i1i2v = model.eval(&i1i2, true).unwrap().as_i64().unwrap();
        let x2 = model.eval(&x2, true).unwrap().as_i64().unwrap();
        let z = model.eval(&z, true).unwrap().as_i64().unwrap();

        println!(
            "{} x: {}  y: {}: i1i2: {} x2={} z={}",
            function_name!(),
            xv,
            yv,
            i1i2v,
            x2,
            z
        );
        assert_eq!(xv, 1);
        assert_eq!(yv, 9);
        assert_eq!(i1i2v, 19);
        assert_eq!(z, xv + 1);
    }

    #[test]
    fn part1_works() {
        let cfg = z3::Config::new();
        let ctx = z3::Context::new(&cfg);
        let solver = z3::Solver::new(&ctx);
        let mut z3solver = Z3Solver::new(&ctx, &solver);

        z3solver.inp(&ctx, &solver, "w");
        z3solver.inp(&ctx, &solver, "x");
        z3solver.add(&ctx, &solver, "x", "2");
        z3solver.add(&ctx, &solver, "y", "7");
        //z3solver.add(&ctx, &solver, "x", "x");
        //z3solver.add(&ctx, &solver, "y", "x");
        //z3solver.modulo(&ctx, &solver, "y", "3");
        z3solver.add(&ctx, &solver, "z", "x");
        z3solver.add(&ctx, &solver, "z", "y");
        //z3solver.div(&ctx, &solver, "z", "500");

        let solution = z3solver.solve(&ctx, &solver);
        println!("{} solution={}", function_name!(), solution,);
        assert_eq!(solution, 0);
    }

    #[test]
    fn part2_works() {}
}
