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

fn parse_line(line: &String) -> (usize, String) {
    let mut tokens: Vec<char> = Vec::new();
    for ch in line.chars() {
        if ch == '(' || ch == '[' || ch == '{' || ch == '<' {
            tokens.push(ch);
        } else {
            let last = match tokens.pop() {
                Some(top) => top,
                None => '.',
            };
            if last == '.' {
                // Error
                return (0, "".to_string());
            }
            if ch == ')' && last != '(' {
                return (3, "".to_string());
            } else if ch == ']' && last != '[' {
                return (57, "".to_string());
            } else if ch == '}' && last != '{' {
                return (1197, "".to_string());
            } else if ch == '>' && last != '<' {
                return (25137, "".to_string());
            }
        }
    }
    return (0, tokens.into_iter().collect());
}

fn part1(lines: &Vec<String>) {
    let mut result = 0;
    for line in lines {
        let (r, _) = parse_line(&line);
        result += r;
    }
    println!("Result1 = {}", result); // 266301
}

fn score(line: &String) -> usize {
    let mut score = 0;
    for ch in line.chars().rev() {
        score = score * 5;
        score += match ch {
            '(' => 1,
            '[' => 2,
            '{' => 3,
            '<' => 4,
            _ => 0,
        };
    }
    score
}

fn part2(lines: &Vec<String>) {
    let mut scores: Vec<usize> = Vec::new();
    for line in lines {
        let (_, remain) = parse_line(&line);
        if remain != "" {
            // println!("remain = {}", remain);
            let score = score(&remain);
            scores.push(score);
            // println!("Score = {}", score);
        }
    }
    scores.sort();
    let result = scores[scores.len() / 2];
    println!("Result2 = {}", result); // 3404870164
}

pub fn main() -> Result<(), Box<dyn std::error::Error>> {
    let lines = read_lines("input10.txt")?;
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
