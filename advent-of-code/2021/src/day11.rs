use ndarray::Array2;
use regex::Regex;
use std::collections::HashSet;
use std::fs;
use std::fs::File;
use std::io::{self, BufRead, Error};
use std::path::Path;

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
    for line in lines {
        if let Ok(l) = line {
            result.push(l)
        }
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
fn read_numbers(line: &str) -> Vec<isize> {
    let numbers: Vec<isize> = line
        .split([',', ' ', '\r', '\n'].as_ref())
        .filter(|&x| !x.is_empty())
        .map(|x| x.to_string().parse::<isize>().unwrap())
        .collect();

    numbers
}

fn create_grid(lines: &Vec<String>) -> Array2<usize> {
    // Create grid
    let width = lines[0].len();
    let height = lines.len();
    // println!("w = {} h = {}", width, height);
    // Grid addressed [y, x]
    let mut grid = Array2::<usize>::zeros((height, width));

    // Fill grid
    for j in 0..height {
        let chars: Vec<char> = lines[j].chars().collect();
        for i in 0..width {
            let val = chars[i] as usize - '0' as usize;
            grid[[j, i]] = val;
        }
    }
    // println!("grid = {:#?}", grid);
    grid
}

fn process_grid(grid: &Array2<usize>, width: usize, height: usize) -> (Array2<usize>, usize) {
    let mut newgrid = grid.clone();

    // Bump everything by 1
    for j in 0..height {
        for i in 0..width {
            newgrid[[j, i]] += 1;
        }
    }

    // Flash neighbours
    let dirs: Vec<(isize, isize)> = vec![
        (-1, -1),
        (0, -1),
        (1, -1),
        (-1, 0),
        (1, 0),
        (-1, 1),
        (0, 1),
        (1, 1),
    ];
    let mut already_flashed: HashSet<(usize, usize)> = HashSet::new();
    let mut done = false;
    while !done {
        done = true;
        for j in 0..height {
            for i in 0..width {
                if newgrid[[j, i]] > 9 && !already_flashed.contains(&(i, j)) {
                    already_flashed.insert((i, j));
                    // print!("FLASH {:?} ", (i, j));
                    done = false;
                    for d in &dirs {
                        let ii: isize = i as isize + d.0;
                        let jj: isize = j as isize + d.1;
                        if ii >= 0 && ii < width as isize && jj >= 0 && jj < height as isize {
                            newgrid[[jj as usize, ii as usize]] += 1;
                            // print!("   {:?} ", (ii, jj));
                        }
                    }
                    // println!("");
                }
            }
        }
    }

    // Zero out overflows
    let mut score = 0;
    for j in 0..height {
        for i in 0..width {
            if newgrid[[j, i]] > 9 {
                score += 1;
                newgrid[[j, i]] = 0;
            }
        }
    }

    (newgrid, score)
}

fn part1(lines: &Vec<String>) {
    let mut grid = create_grid(lines);
    let width = lines[0].len();
    let height = lines.len();
    let iterations = 100;
    // println!("{:#?}", grid);
    let mut result = 0;
    for _ in 0..iterations {
        // let (newgrid, score) = process_grid(&grid, width, height);
        let (newgrid, score) = process_grid(&grid, width, height);
        // grid = newgrid.clone();
        grid = newgrid;
        // println!("{:#?} SCORE= {}", grid, score);
        result += score;
    }

    println!("Result1 = {}", result); // 1620
}

fn part2(lines: &Vec<String>) {
    let mut grid = create_grid(lines);
    let width = lines[0].len();
    let height = lines.len();
    let iterations = 10000;
    // println!("{:#?}", grid);
    let mut result = 0;
    for iteration in 0..iterations {
        // let (newgrid, score) = process_grid(&grid, width, height);
        let (newgrid, score) = process_grid(&grid, width, height);
        // grid = newgrid.clone();
        grid = newgrid;
        // println!("{:#?} SCORE= {}", grid, score);
        if score == width * height {
            result = iteration + 1; // zero index
            break;
        }
    }

    println!("Result2 = {}", result); // 371
}

pub fn main() -> Result<(), Box<dyn std::error::Error>> {
    let lines = read_lines("input11.txt")?;
    // let chunks = read_chunks("input.txt")?;

    part1(&lines);
    part2(&lines);
    Ok(())
}

#[cfg(test)]
mod tests {
    use super::*;
    #[test]
    fn it_works() {}
}
