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

fn read_bingo_numbers(line: &str) -> Vec<isize> {
    let numbers: Vec<isize> = line
        .split([',', ' ', '\r', '\n'].as_ref())
        .map(|x| x.to_string().parse::<isize>().unwrap())
        .collect();

    numbers
}

#[derive(Debug)]
struct BingoBoard {
    board: Vec<isize>,
    bingo_hits: Vec<bool>,
}

impl Default for BingoBoard {
    fn default() -> BingoBoard {
        BingoBoard {
            board: vec![0; 5 * 5],
            bingo_hits: vec![false; 5 * 5],
        }
    }
}
impl BingoBoard {
    fn read_string(&mut self, line: &str) {
        /*
        let zz: Vec<String> = line
            .split_terminator([',', ' ', '\r', '\n'].as_ref())
            .filter(|&x| !x.is_empty())
            .map(|x| x.to_string())
            .collect();
        println!("bingo {:#?}", zz);
         */
        self.board = line
            .split_terminator([',', ' ', '\r', '\n'].as_ref())
            .filter(|&x| !x.is_empty())
            .map(|x| x.to_string().parse::<isize>().unwrap())
            .collect();

        assert!(self.board.len() == 25);
    }
    fn update_bingo(&mut self, number: &isize) {
        let index = self.board.iter().position(|x| x == number);
        if let Some(index) = index {
            self.bingo_hits[index] = true;
        }
    }
    fn is_bingo(&self) -> bool {
        // Horz
        for i in 0..5 {
            let mut found: bool = true;
            for j in 0..5 {
                if !self.bingo_hits[i * 5 + j] {
                    found = false;
                    break;
                }
            }
            if found {
                return true;
            }
        }
        // Vert
        for i in 0..5 {
            let mut found: bool = true;
            for j in 0..5 {
                if !self.bingo_hits[i + j * 5] {
                    found = false;
                    break;
                }
            }
            if found {
                return true;
            }
        }
        false
    }
    fn score(&self, last_number: &isize) -> isize {
        let mut score: isize = 0;
        for (i, num) in self.board.iter().enumerate() {
            if !self.bingo_hits[i] {
                score += num;
            }
        }
        score * last_number
    }
}

fn part1(lines: &Vec<String>) {
    // println!("CHUNKS = {:#?}", chunks);
    let num = read_bingo_numbers(&lines[0]);
    // println!("NUMS = {:#?}", num);
    // Read boards
    let mut boards: Vec<BingoBoard> = Vec::new();
    for i in 1..lines.len() {
        let mut bingo = BingoBoard::default();
        bingo.read_string(&lines[i]);
        boards.push(bingo);
    }
    for val in num.iter() {
        for board in boards.iter_mut() {
            board.update_bingo(&val);
            if board.is_bingo() {
                // println!("BINGO: {:#?}", board);
                println!("Result1: {}", board.score(&val)); // 31424
                return;
            }
        }
    }
}

fn part2(lines: &Vec<String>) {
    let num = read_bingo_numbers(&lines[0]);
    // Read boards
    let mut boards: Vec<BingoBoard> = Vec::new();
    for i in 1..lines.len() {
        let mut bingo = BingoBoard::default();
        bingo.read_string(&lines[i]);
        boards.push(bingo);
    }
    for val in num.iter() {
        let boards_len = boards.len();
        for board in boards.iter_mut() {
            board.update_bingo(&val);
            if board.is_bingo() {
                if boards_len == 1 {
                    // println!("BINGO: {:#?}", board);
                    println!("Result2: {}", board.score(&val)); // 23042
                    return;
                }
            }
        }
        // remove bingo boards
        boards.retain(|x| !x.is_bingo());
    }
}

pub fn main() -> Result<(), Box<dyn std::error::Error>> {
    // let lines = read_lines(&"input4ex.txt")?;
    // let chunks = read_chunks(&"input4ex.txt")?;
    let chunks = read_chunks("input4.txt")?;

    part1(&chunks);
    part2(&chunks);
    Ok(())
}

#[cfg(test)]
mod tests {
    use super::*;
    #[test]
    fn it_works() {
        let mut bingo = BingoBoard::default();
        let bingo_string = "22 13 17 11  0\r\n 8  2 23  4 24\r\n21  9 14 16  7\r\n 6 10  3 18  5\r\n 1 12 20 15 19";
        bingo.read_string(bingo_string);
        let mut bingo_hits = vec![false; 5 * 5];
        assert_eq!(bingo.bingo_hits, bingo_hits);
        bingo.update_bingo(&99);
        assert_eq!(bingo.bingo_hits, bingo_hits);
        bingo.update_bingo(&11);
        bingo_hits[3] = true;
        assert_eq!(bingo.bingo_hits, bingo_hits);
    }
}
