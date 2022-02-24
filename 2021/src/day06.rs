use regex::Regex;
//use std::cmp;
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

fn read_numbers(line: &str) -> Vec<usize> {
    let numbers: Vec<usize> = line
        .split([',', ' ', '\r', '\n'].as_ref())
        .filter(|&x| !x.is_empty())
        .map(|x| x.to_string().parse::<usize>().unwrap())
        .collect();

    numbers
}

fn simulate(seed: usize, count: usize) -> usize {
    let mut school = vec![seed; 1]; // size one vector
    for _i in 0..count {
        let mut new_fish: Vec<usize> = Vec::new();
        let len = school.len();
        for i in 0..len {
            if school[i] == 0 {
                school[i] = 6;
                new_fish.push(8);
            } else {
                school[i] -= 1;
            }
        }
        school.append(&mut new_fish);
    }
    school.len()
}

fn simulate2(seed: usize, count: usize) -> usize {
    let mut school = vec![0; 9]; // vector 0..=8
    school[seed] = 1;
    for _i in 0..count {
        let len = school.len();
        let new8 = school[0];
        for i in 1..len {
            school[i - 1] = school[i];
        }
        school[8] = new8;
        school[6] += new8;
    }
    let mut count = 0;
    for s in school.iter() {
        count += s;
    }
    count
}

fn part1(lines: &Vec<String>) {
    let numbers = read_numbers(&lines[0]);
    let mut answers: HashMap<usize, usize> = HashMap::new();
    let generations = 80;
    for i in 0..=8 {
        answers.insert(i, simulate(i, generations));
    }
    let mut result = 0;
    for n in numbers {
        result += answers.get(&n).unwrap();
    }
    println!("Result1 = {:?}", result); // 374994
}

fn part2(lines: &Vec<String>) {
    let numbers = read_numbers(&lines[0]);
    let mut answers: HashMap<usize, usize> = HashMap::new();
    let generations = 256;
    for i in 0..=8 {
        let i: usize = i;
        answers.insert(i, simulate2(i, generations));
    }
    let mut result = 0;
    for n in numbers {
        result += answers.get(&n).unwrap();
    }
    println!("Result2 = {:?}", result); // 1686252324092
}

pub fn main() -> Result<(), Box<dyn std::error::Error>> {
    let lines = read_lines("input6.txt")?;
    // let chunks = read_chunks("input4.txt")?;

    part1(&lines);
    part2(&lines);
    Ok(())
}

#[cfg(test)]
mod tests {
    use super::*;
    #[test]
    fn it_works() {
        let segment = read_segment("0,9 -> 5,9");
        let expected = Segment {
            x1: 0,
            y1: 9,
            x2: 5,
            y2: 9,
        };
        println!("Segment = {:#?}", segment);
        assert_eq!(segment, expected);
    }
}
