use regex::Regex;
use std::collections::HashMap;
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

type Grid = HashMap<(isize, isize), bool>;

fn get_grid_dims(grid: &Grid) -> (usize, usize) {
    let mut width = 0;
    let mut height = 0;
    for (k, _v) in grid {
        if k.0 > width {
            width = k.0;
        }
        if k.1 > height {
            height = k.1;
        }
    }
    (width as usize, height as usize)
}
fn create_grid(lines: &String) -> Grid {
    let mut grid = HashMap::new();
    let numbers = read_numbers(lines);
    let len = numbers.len();
    assert_eq!(len % 2, 0);
    for n in (0..len).step_by(2) {
        grid.insert((numbers[n], numbers[n + 1]), true);
    }
    // println!("grid = {:#?}", grid);
    grid
}

fn fold_by_y(grid: &Grid, y: usize) -> Grid {
    let mut newgrid = HashMap::new();
    // let (width, height) = get_grid_dims(grid);
    let y = y as isize;
    // Fold bottom up
    for (k, _v) in grid {
        if k.1 <= y {
            // top
            newgrid.insert((k.0, k.1), true);
        } else {
            // bottom folded over top
            newgrid.insert((k.0, y - (k.1 - y)), true);
        }
    }

    newgrid
}

fn fold_by_x(grid: &Grid, x: usize) -> Grid {
    let mut newgrid = HashMap::new();
    // let (width, height) = get_grid_dims(grid);
    let x = x as isize;
    // Fold left
    for (k, _v) in grid {
        if k.0 <= x {
            // top
            newgrid.insert((k.0, k.1), true);
        } else {
            // bottom folded over top
            newgrid.insert((x - (k.0 - x), k.1), true);
        }
    }

    newgrid
}

fn count_grid(grid: &Grid) -> usize {
    let mut count = 0;
    for (_k, v) in grid {
        if *v {
            count += 1;
        }
    }
    count
}

#[allow(dead_code)]
fn print_grid(grid: &Grid) {
    let (width, height) = get_grid_dims(&grid);
    for j in 0..=height {
        for i in 0..=width {
            if let Some(_x) = grid.get(&(i as isize, j as isize)) {
                print!("#");
            } else {
                print!(".");
            }
        }
        println!("");
    }
}
fn part1(lines: &Vec<String>) {
    let grid = create_grid(&lines[0]);
    /*
    let (width, height) = get_grid_dims(&grid);
    print_grid(&grid);
    println!("Grid w={} h={} count={}", width, height, count_grid(&grid));
    let newgrid = fold_by_y(&grid, 7);
    print_grid(&newgrid);
    println!(
        "Grid w={} h={} count={}",
        width,
        height,
        count_grid(&newgrid)
    );
    let newgrid = fold_by_x(&newgrid, 5);
    print_grid(&newgrid);
    println!(
        "Grid w={} h={} count={}",
        width,
        height,
        count_grid(&newgrid)
    );
     */
    let newgrid = fold_by_x(&grid, 655);
    let result = count_grid(&newgrid);
    println!("Result1 = {}", result); // 755
}

fn part2(lines: &Vec<String>) {
    let mut grid = create_grid(&lines[0]);

    let lines: Vec<String> = lines[1]
        .split(['\r', '\n'].as_ref())
        .filter(|&x| !x.is_empty())
        .map(|x| x.to_string())
        .collect();

    for line in &lines {
        let pair: Vec<String> = line
            .split(['='].as_ref())
            .filter(|&x| !x.is_empty())
            .map(|x| x.to_string())
            .collect();

        let fold = pair[1].parse::<usize>().unwrap();
        if pair[0] == "fold along x" {
            grid = fold_by_x(&grid, fold);
        } else {
            grid = fold_by_y(&grid, fold);
        }
    }
    print_grid(&grid);

    println!("Result2 = {}", "BLKJRBAG"); // BLKJRBAG
}

pub fn main() -> Result<(), Box<dyn std::error::Error>> {
    // let lines = read_lines("input.txt")?;
    let chunks = read_chunks("input13.txt")?;

    part1(&chunks);
    part2(&chunks);
    Ok(())
}

#[cfg(test)]
mod tests {
    use super::*;
    #[test]
    fn it_works() {}
}
