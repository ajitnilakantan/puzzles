use ndarray::Array2;
use regex::Regex;
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

fn read_bits(lines: &String) -> Vec<usize> {
    let mut lines = lines.clone();
    lines.retain(|c| !c.is_whitespace()); // remove all whitespace
    let mut bits: Vec<usize> = Vec::new();
    let chars: Vec<char> = lines.chars().collect();
    for c in &chars {
        bits.push(if *c == '#' { 1 } else { 0 });
    }
    assert_eq!(bits.len(), 512);
    bits
}
fn read_image(lines: &String, padding: usize) -> Array2<usize> {
    let lines: Vec<String> = lines
        .split_terminator([' ', '\r', '\n'].as_ref())
        .filter(|&x| !x.is_empty())
        .map(|x| x.to_string())
        .collect();
    // Create grid
    let width = lines[0].len();
    let height = lines.len();
    // Grid addressed [y, x]
    let mut grid = Array2::<usize>::zeros((height + 2 * padding, width + 2 * padding)); // pad all around

    // Fill grid
    for j in 0..height {
        let chars: Vec<char> = lines[j].chars().collect();
        for i in 0..width {
            let val = if chars[i] == '#' { 1 } else { 0 };
            grid[[j + padding, i + padding]] = val;
        }
    }
    // println!("grid = {:#?}", grid);
    grid
}

fn get_3x3_value(grid: &Array2<usize>, col: usize, row: usize) -> usize {
    let (col, row): (isize, isize) = (col as isize, row as isize);
    // Get the "9-bit" number around the specified coord (x-offset, y-offset, bitshift)
    let vals: Vec<(isize, isize, usize)> = vec![
        (-1, -1, 8),
        (0, -1, 7),
        (1, -1, 6),
        (-1, 0, 5),
        (0, 0, 4),
        (1, 0, 3),
        (-1, 1, 2),
        (0, 1, 1),
        (1, 1, 0),
    ];
    let mut val: usize = 0;
    for v in vals {
        val += grid[[(row + v.1) as usize, (col + v.0) as usize]] << v.2
    }
    val
}

fn count_pixels(grid: &Array2<usize>, offset: usize) -> usize {
    let (height, width) = grid.dim();
    let mut count = 0;
    for i in offset..width - offset {
        for j in offset..height - offset {
            if grid[[j, i]] != 0 {
                count += 1;
            }
        }
    }

    count
}
fn convolve_grid(grid: &Array2<usize>, bits: &Vec<usize>, _padding: usize, passes: usize) -> usize {
    let mut grid = grid.clone();
    let (height, width) = grid.dim();
    // println!("{:#?}\n", grid);
    for pass in 0..passes {
        let mut newgrid = grid.clone();
        for i in pass + 1..width - 1 - pass {
            for j in pass + 1..height - 1 - pass {
                let val = get_3x3_value(&grid, i, j);
                newgrid[[j, i]] = bits[val];
            }
        }
        grid = newgrid.clone();
        // println!("{:#?}\n", grid);
    }
    // sum up, excluding 3x3 buffer
    let count = count_pixels(&grid, passes + 1);

    count
}

fn part1(lines: &Vec<String>) {
    let passes = 2;
    // Add  1 border + passes to ignore around border + passes to expand image on each step
    let padding = 2 * passes + 1;
    let bits = read_bits(&lines[0]);
    let image = read_image(&lines[1], padding);
    let result1 = convolve_grid(&image, &bits, padding, passes);

    println!("Result1 = {}", result1); // 5395
}

fn part2(lines: &Vec<String>) {
    let passes = 50;
    // Add  1 border + passes to ignore around border + passes to expand image on each step
    let padding = 2 * passes + 1;
    let bits = read_bits(&lines[0]);
    let image = read_image(&lines[1], padding);
    let result2 = convolve_grid(&image, &bits, padding, passes);

    println!("Result2 = {}", result2); // 17584
}

pub fn main() -> Result<(), Box<dyn std::error::Error>> {
    //let lines = read_lines("input.txt")?;
    let chunks = read_chunks("input20.txt")?;

    part1(&chunks);
    part2(&chunks);

    Ok(())
}

#[cfg(test)]
mod tests {
    use super::*;
    use std::path::PathBuf;
    #[test]
    fn part1_works() {
        let mut gg = Array2::<usize>::zeros((2, 2)); // pad all around
        gg[[0, 0]] = 0;
        gg[[0, 1]] = 1;
        gg[[1, 0]] = 2;
        gg[[1, 1]] = 3;
        println!("gg=\n{:#?}\n", gg);
    }
    #[test]
    fn part2_works() {
        let chunks = read_chunks(
            [env!("CARGO_MANIFEST_DIR"), "src", "input20.txt"]
                .iter()
                .collect::<PathBuf>(),
        )
        .unwrap();
        let bits = read_bits(&chunks[0]);
        let gg = Array2::<usize>::zeros((10, 10)); // pad all around
        convolve_grid(&gg, &bits, 1, 4);
    }
}
