use regex::Regex;
use std::cmp;
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

fn read_numbers(line: &str) -> Vec<isize> {
    let numbers: Vec<isize> = line
        .split([',', ' ', '\r', '\n'].as_ref())
        .filter(|&x| !x.is_empty())
        .map(|x| x.to_string().parse::<isize>().unwrap())
        .collect();

    numbers
}

fn part1(lines: &Vec<String>) {
    let mut numbers = read_numbers(&lines[0]);
    numbers.sort();
    let median = numbers[numbers.len() / 2];
    let median_abs_diff = numbers.iter().map(|x| (x - median).abs()).sum::<isize>();
    println!("Result1 = {:?}", median_abs_diff); // 349357
}

fn part2(lines: &Vec<String>) {
    let mut numbers = read_numbers(&lines[0]);
    numbers.sort();

    let cost = |p1: &isize, p2: &isize| -> isize {
        let n = (p1 - p2).abs();
        n * (n + 1) / 2
    };
    let total_cost =
        |numbers: &Vec<isize>, pos: &isize| -> isize { numbers.iter().map(|x| cost(x, pos)).sum() };

    // Initial guess: mean
    let mean: isize = numbers.iter().map(|x| x).sum::<isize>() / (numbers.len() as isize);
    /*
    let mean_pos = numbers.binary_search_by(|x| x.cmp(&mean));
    let mean_pos: usize = match mean_pos {
        Ok(v) => v,
        Err(v) => v,
    };
     */
    let mean_abs_diff: isize = total_cost(&numbers, &mean);

    for i in numbers[0]..numbers[numbers.len() - 1] {
        println!("i={} cost = {}", i, total_cost(&numbers, &i));
    }
    // look left
    let mut left_pos = mean;
    let mut left_mean_abs_diff: isize = mean_abs_diff;
    while left_pos > 0 {
        let next_left_mean_abs_diff = total_cost(&numbers, &(left_pos - 1));
        if next_left_mean_abs_diff > mean_abs_diff {
            break;
        }
        left_mean_abs_diff = next_left_mean_abs_diff;
        left_pos -= 1;
    }

    // look right
    let mut right_pos = mean;
    let mut right_mean_abs_diff: isize = mean_abs_diff;
    while right_pos < numbers.len() as isize {
        let next_right_mean_abs_diff = total_cost(&numbers, &(right_pos + 1));
        if next_right_mean_abs_diff > mean_abs_diff {
            break;
        }
        right_mean_abs_diff = next_right_mean_abs_diff;
        right_pos += 1;
    }
    println!(
        "mean = {:?} {:?} left = {} {}  right = {} {}",
        mean, mean_abs_diff, left_pos, left_mean_abs_diff, right_pos, right_mean_abs_diff
    );
    let result = cmp::min(
        cmp::min(mean_abs_diff, left_mean_abs_diff),
        right_mean_abs_diff,
    );
    println!("Result2 = {}", result); // 96708205
}

pub fn main() -> Result<(), Box<dyn std::error::Error>> {
    let lines = read_lines("input7.txt")?;

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
