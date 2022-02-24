//use z3::ast::{Ast, Bool};
use regex::Regex;
use std::cell::RefCell;
use std::collections::HashMap;
use std::fs;
use std::fs::File;
use std::io::{self, BufRead, Error};
use std::ops::AddAssign;
use std::ops::Deref;
use std::ops::DivAssign;
use std::ops::MulAssign;
use std::ops::RemAssign;
use std::path::Path;
use z3::ast::Ast;
//use z3::*;

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
fn read_lines<P>(filename: P) -> Result<Vec<String>, Error>
where
    P: AsRef<Path>,
{
    let file = File::open(filename)?;
    let mut result: Vec<String> = Vec::new();
    let lines = io::BufReader::new(file).lines();
    for line in lines.flatten() {
        result.push(line)
    }
    Ok(result)
}

// The output is wrapped in a Result to allow matching on errors
// Returns list of strings separated by blank lines
#[allow(dead_code)]
fn read_chunks<P>(filename: P) -> Result<Vec<String>, Error>
where
    P: AsRef<Path>,
{
    let text = fs::read_to_string(filename)?;
    let chunks: Vec<String> = Regex::new(r"\r\n\r\n")
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
    w: RefCell<z3::ast::Int<'a>>,
    x: RefCell<z3::ast::Int<'a>>,
    y: RefCell<z3::ast::Int<'a>>,
    z: RefCell<z3::ast::Int<'a>>,
    inputs: Vec<RefCell<z3::ast::Int<'a>>>,
    constants: HashMap<i64, RefCell<z3::ast::Int<'a>>>,
    zero: z3::ast::Int<'a>,
    one: z3::ast::Int<'a>,
    ten: z3::ast::Int<'a>,
    ten_power: Vec<z3::ast::Int<'a>>,
}

impl<'a> Z3Solver<'a> {
    // See: https://users.rust-lang.org/t/having-a-struct-where-one-member-refers-to-another/51380/5
    fn new(ctx: &'a z3::Context) -> Self {
        Self {
            w: RefCell::new(z3::ast::Int::from_i64(ctx, 0)),
            x: RefCell::new(z3::ast::Int::from_i64(ctx, 0)),
            y: RefCell::new(z3::ast::Int::from_i64(ctx, 0)),
            z: RefCell::new(z3::ast::Int::from_i64(ctx, 0)),
            inputs: vec![],
            constants: HashMap::new(),
            zero: z3::ast::Int::from_u64(ctx, 0),
            one: z3::ast::Int::from_u64(ctx, 1),
            ten: z3::ast::Int::from_u64(ctx, 10),
            ten_power: (0u32..15u32)
                .map(|x| z3::ast::Int::from_i64(ctx, i64::pow(10i64, x)))
                .collect(),
        }
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

    fn lookup(&mut self, ctx: &'a z3::Context, variable: &str) -> RefCell<z3::ast::Int<'a>> {
        match variable {
            "w" => self.w.clone(),
            "x" => self.x.clone(),
            "y" => self.y.clone(),
            "z" => self.z.clone(),
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

    // Return tuple: (solution, w, x, y, z)
    fn solve(&mut self, ctx: &'a z3::Context, solver: &'a z3::Solver) -> i64 {
        /*
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
        */
        // z must be zero for a valid serial number
        solver.assert(&self.z.borrow()._eq(&self.zero));
        // solver.push();
        //solver.assert(&serial_no.gt(&z3::ast::Int::from_u64(&ctx, guess)));
        println!("solver = \n{:#?}", solver);
        let sat = solver.check();
        let mut serial_no_v = 0;
        if sat == z3::SatResult::Sat {
            let model = solver.get_model().unwrap();
            //serial_no_v = model.eval(&serial_no, true).unwrap().as_i64().unwrap();
            println!("model=\n{:?}", model);
            // solver.pop(1);
        }
        //solver.assert(&self.lookup(ctx, "w").borrow().clone());
        //solver.assert(&self.lookup(ctx, "x").borrow().clone());
        //solver.assert(&self.lookup(ctx, "y").borrow().clone());

        // Normally would call:
        //   let solver = z3::Optimize::new(&ctx);
        //   ...
        //   solver.maximize(&serial_no);
        //   assert_eq!(solver.check(&[]), z3::SatResult::Sat);
        //   ...but the Optimize algo is super slow. Use a binary search instead.
        /*
        let mut min = 0;
        let mut max = 99999999999999;
        let mut guess = 1; //(min + max) / 2;
        let (mut serial_no_v, mut w, mut x, mut y, mut z) = (0, 0, 0, 0, 0);

        while guess != min && guess != max {
            solver.push();
            solver.assert(&serial_no.gt(&z3::ast::Int::from_u64(&ctx, guess)));
            let sat = solver.check();
            if sat == z3::SatResult::Sat {
                let model = solver.get_model().unwrap();
                serial_no_v = model.eval(&serial_no, true).unwrap().as_i64().unwrap();
                w = model
                    .eval(&self.w.borrow().clone(), true)
                    .unwrap()
                    .as_i64()
                    .unwrap();
                x = model
                    .eval(&self.x.borrow().clone(), true)
                    .unwrap()
                    .as_i64()
                    .unwrap();
                y = model
                    .eval(&self.y.borrow().clone(), true)
                    .unwrap()
                    .as_i64()
                    .unwrap();
                z = model
                    .eval(&self.z.borrow().clone(), true)
                    .unwrap()
                    .as_i64()
                    .unwrap();

                println!("sat = {:?} serial_no={}", sat, serial_no_v);
                min = guess;
            } else {
                println!("sat = {:?}", sat);
                max = guess;
            }
            solver.pop(1);

            guess = (max + min) / 2;
        }
        */

        // maximize the serial number
        println!(
            "Solving: input_len={}, len()={:?}",
            self.inputs.len(),
            (self.constants.len())
        );

        println!("Getting serial_no: {:?}", serial_no_v);

        serial_no_v
    }

    fn inp(&mut self, ctx: &'a z3::Context, solver: &'a z3::Solver, var: &str) {
        // variable <- input
        let input = self.new_input(ctx, solver);
        match var {
            "w" => self.w.replace(z3::ast::Int::add(ctx, &[&input.borrow()])),
            "x" => self.x.replace(z3::ast::Int::add(ctx, &[&input.borrow()])),
            "y" => self.y.replace(z3::ast::Int::add(ctx, &[&input.borrow()])),
            "z" => self.z.replace(z3::ast::Int::add(ctx, &[&input.borrow()])),
            _ => panic!(),
        };
    }
    fn add(&mut self, ctx: &'a z3::Context, var: &str, arg: &str) {
        // variable += arg
        let arg1 = &self.lookup(ctx, arg);
        let arg = arg1.borrow();

        match var {
            "w" => self.w.borrow_mut().add_assign(arg.deref()),
            "x" => self.x.borrow_mut().add_assign(arg.deref()),
            "y" => self.y.borrow_mut().add_assign(arg.deref()),
            "z" => self.z.borrow_mut().add_assign(arg.deref()),
            _ => panic!(),
        };
    }
    fn mul(&mut self, ctx: &'a z3::Context, var: &str, arg: &str) {
        // variable *= arg
        let arg = &self.lookup(ctx, arg).borrow().clone();

        match var {
            "w" => self.w.borrow_mut().mul_assign(arg),
            "x" => self.x.borrow_mut().mul_assign(arg),
            "y" => self.y.borrow_mut().mul_assign(arg),
            "z" => self.z.borrow_mut().mul_assign(arg),
            _ => panic!(),
        };
    }
    fn div(&mut self, ctx: &'a z3::Context, var: &str, arg: &str) {
        // var =  var // arg (divistion with truncation)
        let arg = &self.lookup(ctx, arg).borrow().clone();

        match var {
            "w" => self.w.borrow_mut().div_assign(arg),
            "x" => self.x.borrow_mut().div_assign(arg),
            "y" => self.y.borrow_mut().div_assign(arg),
            "z" => self.z.borrow_mut().div_assign(arg),
            _ => panic!(),
        };
    }
    fn modulo(&mut self, ctx: &'a z3::Context, var: &str, arg: &str) {
        // Modulo operation var = var mod b
        let arg = &self.lookup(ctx, arg).borrow().clone();

        match var {
            "w" => self.w.borrow_mut().rem_assign(arg),
            "x" => self.x.borrow_mut().rem_assign(arg),
            "y" => self.y.borrow_mut().rem_assign(arg),
            "z" => self.z.borrow_mut().rem_assign(arg),
            _ => panic!(),
        };
    }
    fn eql(&mut self, ctx: &'a z3::Context, var: &str, arg: &str) {
        // If var == arg then var = 1 else var = 0
        let arg1 = &self.lookup(ctx, var).borrow().clone();
        let arg2 = &self.lookup(ctx, arg).borrow().clone();
        let op = (arg1)._eq(arg2).ite(&self.one, &self.zero).clone();
        match var {
            "w" => self.w.replace(op),
            "x" => self.x.replace(op),
            "y" => self.y.replace(op),
            "z" => self.z.replace(op),
            _ => panic!(),
        };
    }
}

fn process_lines(lines: &[&str]) {
    let cfg = z3::Config::new();
    let ctx = z3::Context::new(&cfg);
    let solver = z3::Solver::new(&ctx);
    let mut z3solver = Z3Solver::new(&ctx);

    for line in lines {
        let words: Vec<String> = line
            .split([' ', '\r', '\n'].as_ref())
            .filter(|&x| !x.is_empty())
            .map(|x| x.to_string())
            .collect();
        match words[0].as_str() {
            "inp" => z3solver.inp(&ctx, &solver, &words[1]),
            "add" => z3solver.add(&ctx, &words[1], &words[2]),
            "mul" => z3solver.mul(&ctx, &words[1], &words[2]),
            "div" => z3solver.div(&ctx, &words[1], &words[2]),
            "mod" => z3solver.modulo(&ctx, &words[1], &words[2]),
            "eql" => z3solver.eql(&ctx, &words[1], &words[2]),
            _ => panic!(),
        }
    }
    let solution = z3solver.solve(&ctx, &solver);
    println!("part1 solution={}", solution);
}

fn part1(lines: &[String]) {
    // https://stackoverflow.com/questions/33216514/how-do-i-convert-a-vecstring-to-vecstr
    let lines: Vec<&str> = lines.iter().map(AsRef::as_ref).collect();
    process_lines(&lines);
    let result1 = 0;
    println!("Result1 = {}", result1); //
}

fn part2<S>(lines: &[S])
where
    S: AsRef<str>,
{
    // let lines: Vec<&str> = lines.iter().map(AsRef::as_ref).collect();
    let result2 = 0;
    println!("Result2 = {}", result2); //
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
    part2(&lines);

    Ok(())
}

#[cfg(test)]
mod tests {
    use super::*;
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
        let solver = z3::Optimize::new(&ctx);
        let mut z3solver = Z3Solver::new(&ctx);

        z3solver.inp(&ctx, &solver, "w");
        z3solver.inp(&ctx, &solver, "x");
        z3solver.add(&ctx, "x", "2");
        z3solver.add(&ctx, "y", "7");
        //z3solver.add(&ctx, "x", "x");
        //z3solver.add(&ctx, "y", "x");
        //z3solver.modulo(&ctx, "y", "3");
        z3solver.add(&ctx, "z", "x");
        z3solver.add(&ctx, "z", "y");
        //z3solver.div(&ctx, "z", "500");

        let (solution, w, x, y, z) = z3solver.solve(&ctx, &solver);
        println!(
            "{} solution={} w={} x={} y={} z={}",
            function_name!(),
            solution,
            w,
            x,
            y,
            z
        );
        assert_eq!(solution, 99);
        assert_eq!(w, 9);
        assert_eq!(x, 22);
        assert_eq!(y, 1);
        assert_eq!(z, 0);
    }

    #[test]
    fn part2_works() {}
}
