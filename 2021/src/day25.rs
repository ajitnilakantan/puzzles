use itertools::iproduct;
use ndarray::Array2;
use std::collections::hash_map::DefaultHasher;
use std::hash::Hash;
use std::hash::Hasher;

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
fn read_lines(line: &str) -> Vec<String> {
    let lines: Vec<String> = regex::Regex::new(r"(\r\n)|(\n)")
        .unwrap()
        .split(line)
        .map(|x| x.to_string())
        .filter(|x| !x.is_empty())
        .collect::<Vec<String>>();

    lines
}

// The output is wrapped in a Result to allow matching on errors
// Returns list of strings separated by blank lines
#[allow(dead_code)]
fn read_chunks(line: &str) -> Vec<String> {
    let chunks: Vec<String> = regex::Regex::new(r"(\r\n\r\n)|(\n\n)")
        .unwrap()
        .split(line)
        .map(|x| x.to_string())
        .collect::<Vec<String>>();

    chunks
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

fn read_image<T: AsRef<str>>(lines: &[T]) -> Array2<char> {
    // Create grid
    let width = lines[0].as_ref().len();
    let height = lines.len();

    // Grid addressed [y, x]
    let mut grid = Array2::<char>::from_elem((height, width), '.');

    // Fill grid
    for j in 0..height {
        let chars: Vec<char> = lines[j].as_ref().chars().collect();
        for i in 0..width {
            let val = chars[i];
            grid[[j, i]] = val;
        }
    }

    grid
}
fn iterate_image(grid: &Array2<char>) -> Array2<char> {
    let (height, width) = grid.dim();
    // Across
    let mut grid2 = grid.clone();
    for (j, i) in itertools::iproduct!(0..height, 0..width) {
        let n = (i + 1) % width;
        if grid[[j, i]] == '>' && grid[[j, n]] == '.' {
            grid2[[j, i]] = '.';
            grid2[[j, n]] = '>';
        }
    }
    // Down
    let mut grid3 = grid2.clone();
    for (j, i) in itertools::iproduct!(0..height, 0..width) {
        let n = (j + 1) % height;
        if grid2[[j, i]] == 'v' && grid2[[n, i]] == '.' {
            grid3[[j, i]] = '.';
            grid3[[n, i]] = 'v';
        }
    }

    grid3
}

fn print_image(grid: &Array2<char>) {
    let (height, width) = grid.dim();
    for j in 0..height {
        for i in 0..width {
            print!("{}", grid[[j, i]]);
        }
        println!();
    }
    println!("\n");
}

fn hash_image(grid: &Array2<char>) -> u64 {
    let mut hasher = DefaultHasher::new();
    grid.hash(&mut hasher);
    hasher.finish()
}

fn process_lines1(lines: &[&str]) -> isize {
    let mut grid = read_image(lines);
    let mut hash = hash_image(&grid);
    let mut iteration = 0;
    loop {
        iteration += 1;
        grid = iterate_image(&grid);
        let new_hash = hash_image(&grid);
        if new_hash == hash {
            break;
        }
        hash = new_hash;
    }
    // println!("Iteration={iteration}");
    iteration
}
fn process_lines2(lines: &[&str]) -> isize {
    0
}

fn part1(lines: &[String]) {
    // https://stackoverflow.com/questions/33216514/how-do-i-convert-a-vecstring-to-vecstr
    let lines: Vec<&str> = lines.iter().map(AsRef::as_ref).collect();
    let result1 = process_lines1(&lines);
    println!("Result1 = {}", result1); // 489
}

fn part2<S: AsRef<str>>(lines: &[S]) {
    let result2 = 0;
    println!("Result2 = {}", result2); // Nothing to do!
}

pub fn main() -> Result<(), Box<dyn std::error::Error>> {
    let args: Vec<String> = std::env::args().collect();
    let text = if args.len() > 1 {
        std::fs::read_to_string(&args[1])?
    } else {
        include_str!("input25.txt").to_string()
    };

    let lines = read_lines(&text);
    //let lines = read_chunks(&text);

    part1(&lines);
    part2(&lines[..]);

    Ok(())
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn part1_works() {
        let input = indoc::indoc! {"
                    v...>>.vv>
                    .vv>>.vv..
                    >>.>v>...v
                    >>v>>.>.v.
                    v>v.vv.v..
                    >.>>..v...
                    .vv..>.>v.
                    v.v..>>v.v
                    ....v..v.>"};
        // println!("Input = '{input}'");
        let mut grid = read_image(&read_lines(input));
        let mut hash = hash_image(&grid);
        let mut iteration = 0;
        loop {
            iteration += 1;
            grid = iterate_image(&grid);
            let new_hash = hash_image(&grid);
            if new_hash == hash {
                break;
            }
            hash = new_hash;
        }
        println!("Iteration={iteration}");
        assert_eq!(iteration, 58);
    }

    #[test]
    fn part2_works() {}
}
