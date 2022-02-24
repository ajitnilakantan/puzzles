use regex::Regex;
use itertools::Itertools;
use std::collections::HashSet;
use std::fs;
use std::fs::File;
use std::io::{self, BufRead, Error};
use std::iter::FromIterator;
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

fn part1(lines: &Vec<String>) {
    let mut count = 0;
    for line in lines {
        let parts: Vec<String> = line
            .split(['|'].as_ref())
            .filter(|&x| !x.is_empty())
            .map(|x| x.trim().to_string())
            .collect();
        let _lhs = parts[0].to_string();
        let rhs = parts[1].to_string();
        // println!("lhs = {} rhs = {}", lhs, rhs);
        let words: Vec<String> = rhs
            .split([' '].as_ref())
            .filter(|&x| !x.is_empty())
            .map(|x| x.trim().to_string())
            .collect();
        for w in words {
            if w.len() == 2 || w.len() == 4 || w.len() == 3 || w.len() == 7 {
                // 1,4,7,8
                count += 1;
            }
        }
    }
    println!("Result1 = {}", count); // 288
}

fn solve(line: &String) -> usize {
    let valid_numbers_vec = vec![
        "abcefg", "cf", "acdeg", "acdfg", "bcdf", "abdfg", "abdefg", "acf", "abcdefg", "abcdfg",
    ];
    let valid_numbers: HashSet<&str> = HashSet::from_iter(valid_numbers_vec.iter().cloned());
    // Loop through the different permutations of mappings
    let items = vec!['a', 'b', 'c', 'd', 'e', 'f', 'g'];
    let sort_str = |x: &String| x.chars().sorted().collect::<String>();

    let parts: Vec<String> = line
        .split(['|'].as_ref())
        .filter(|&x| !x.is_empty())
        .map(|x| x.trim().to_string())
        .collect();
    let lhs = parts[0].to_string();
    let rhs = parts[1].to_string();
    // println!("lhs = {} rhs = {}", lhs, rhs);
    let words: Vec<String> = lhs
        .split([' '].as_ref())
        .filter(|&x| !x.is_empty())
        .map(|x| x.trim().to_string())
        .collect();

    for perm in items.iter().permutations(items.len()).unique() {
        // println!("{:?}", perm);
        let mut found: bool = true;
        for w in words.iter() {
            let w2 = w
                .chars()
                .map(|c| perm[c as usize - 'a' as usize])
                .collect::<String>();
            let w2 = sort_str(&w2);
            if !valid_numbers.contains(w2.as_str()) {
                // println!("BAD {} -> {}", w, w2);
                found = false;
                break;
            }
        }
        if found {
            let digit = |x: &String| valid_numbers_vec.iter().position(|&r| r == x).unwrap();
            let rhs_words: Vec<String> = rhs
                .split([' '].as_ref())
                .filter(|&x| !x.is_empty())
                .map(|x| x.trim().to_string())
                .collect();
            // println!("FOUND! {:?} ", perm);
            let shuffle = |x: &String| {
                x.chars()
                    .map(|c| perm[c as usize - 'a' as usize])
                    .collect::<String>()
            };
            let (r0, r1, r2, r3) = (
                shuffle(&rhs_words[0]),
                shuffle(&rhs_words[1]),
                shuffle(&rhs_words[2]),
                shuffle(&rhs_words[3]),
            );
            //println!(
            //    "{} {}   {} {}    {} {}     {} {}",
            //    rhs_words[0], r0, rhs_words[1], r1, rhs_words[2], r2, rhs_words[3], r3
            //);
            let result = digit(&sort_str(&r0)) * 1000
                + digit(&sort_str(&r1)) * 100
                + digit(&sort_str(&r2)) * 10
                + digit(&sort_str(&r3));
            return result;
        }
    }
    0
}

fn part2(lines: &Vec<String>) {
    let mut result = 0;
    for line in lines {
        result += solve(line);
    }
    println!("Result2 = {}", result); // 940724
}

pub fn main() -> Result<(), Box<dyn std::error::Error>> {
    let lines = read_lines("input8.txt")?;
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
