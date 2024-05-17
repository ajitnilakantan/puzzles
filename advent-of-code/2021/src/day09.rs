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
fn read_numbers(line: &str) -> Vec<isize> {
    let numbers: Vec<isize> = line
        .split([',', ' ', '\r', '\n'].as_ref())
        .filter(|&x| !x.is_empty())
        .map(|x| x.to_string().parse::<isize>().unwrap())
        .collect();

    numbers
}

fn create_grid(lines: &Vec<String>) -> Array2<isize> {
    // Create grid
    let width = lines[0].len();
    let height = lines.len();
    // println!("w = {} h = {}", width, height);
    // Grid addressed [y, x]
    let mut grid = Array2::<isize>::zeros((height + 2, width + 2)); // pad all around

    for i in 0..width + 2 {
        grid[[0, i]] = 10;
        grid[[height + 1, i]] = 11;
    }
    for j in 0..height + 2 {
        grid[[j, 0]] = 12;
        grid[[j, width + 1]] = 13;
    }

    // Fill grid
    for j in 0..height {
        let chars: Vec<char> = lines[j].chars().collect();
        for i in 0..width {
            let val = chars[i] as isize - '0' as isize;
            grid[[j + 1, i + 1]] = val;
        }
    }
    // println!("grid = {:#?}", grid);
    grid
}
fn part1(lines: &Vec<String>) {
    let grid = create_grid(lines);

    // Find minima
    let width = lines[0].len();
    let height = lines.len();
    let mut minima: Vec<isize> = Vec::new();
    for j in 1..height + 1 {
        for i in 1..width + 1 {
            let v = grid[[j, i]];
            if v < grid[[j - 1, i - 1]]
                && v < grid[[j - 1, i]]
                && v < grid[[j - 1, i + 1]]
                && v < grid[[j, i - 1]]
                && v < grid[[j, i + 1]]
                && v < grid[[j + 1, i - 1]]
                && v < grid[[j + 1, i]]
                && v < grid[[j + 1, i + 1]]
            {
                minima.push(v);
            }
        }
    }
    // println!("minima = {:#?}", minima);
    let result: isize = minima.iter().map(|x| x + 1).sum::<isize>();
    println!("Result1 = {}", result); // 607
}

fn flood_fill(grid: &mut Array2<isize>, width: usize, height: usize, x: usize, y: usize) -> usize {
    if grid[[y, x]] < 9 {
        grid[[y, x]] = 20;
        let mut count = 1;
        if y > 0 {
            count += flood_fill(grid, width, height, x, y - 1);
        }
        if x > 0 {
            count += flood_fill(grid, width, height, x - 1, y);
        }
        if y < height - 1 {
            count += flood_fill(grid, width, height, x, y + 1);
        }
        if x < width - 1 {
            count += flood_fill(grid, width, height, x + 1, y);
        }
        return count;
    } else {
        return 0;
    }
}

fn part2(lines: &Vec<String>) {
    let mut grid = create_grid(lines);

    // Flood fill
    let width = lines[0].len();
    let height = lines.len();
    // Find basins
    let mut flood_count: Vec<usize> = Vec::new();
    // println!("grid = {:#?}", grid);
    for j in 1..height + 1 {
        for i in 1..width + 1 {
            let v = grid[[j, i]];
            if v < 9 {
                let count = flood_fill(&mut grid, width + 2, height + 2, i, j);
                // println!("Floodfill = {}", count);
                flood_count.push(count);
                // println!("grid = {:#?}", grid);
            }
        }
    }
    flood_count.sort();
    flood_count.reverse();
    let result: usize = flood_count[0] * flood_count[1] * flood_count[2];
    println!("Result2 = {}", result); // 900864
}

pub fn main() -> Result<(), Box<dyn std::error::Error>> {
    let lines = read_lines("input9.txt")?;
    // let chunks = read_chunks("input4.txt")?;

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
