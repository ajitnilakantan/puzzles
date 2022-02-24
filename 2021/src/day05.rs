use regex::Regex;
use std::cmp;
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

#[derive(Debug, PartialEq)]
struct Segment {
    x1: isize,
    y1: isize,
    x2: isize,
    y2: isize,
}

fn read_segment(line: &str) -> Segment {
    let numbers: Vec<isize> = line
        .split([',', ' ', '\r', '\n', '-', '>'].as_ref())
        .filter(|&x| !x.is_empty())
        .map(|x| x.to_string().parse::<isize>().unwrap())
        .collect();

    return Segment {
        x1: numbers[0],
        y1: numbers[1],
        x2: numbers[2],
        y2: numbers[3],
    };
}

fn part1(lines: &Vec<String>) {
    let mut segments: Vec<Segment> = Vec::new();
    for line in lines.iter() {
        segments.push(read_segment(line));
    }
    // Filter horz + vert segments
    segments.retain(|x| x.x1 == x.x2 || x.y1 == x.y2);
    // println!("SEGMENTS = {:#?}", segments);
    // Grid
    let mut grid: HashMap<(isize, isize), isize> = HashMap::new();
    for segment in segments.iter() {
        if segment.x1 == segment.x2 {
            let y1 = cmp::min(segment.y1, segment.y2);
            let y2 = cmp::max(segment.y1, segment.y2);
            for y in y1..=y2 {
                if grid.contains_key(&(segment.x1, y)) {
                    let val = grid.get(&(segment.x1, y)).unwrap();
                    *grid.get_mut(&(segment.x1, y)).unwrap() = val + 1;
                } else {
                    grid.insert((segment.x1, y), 1);
                }
            }
        } else {
            let x1 = cmp::min(segment.x1, segment.x2);
            let x2 = cmp::max(segment.x1, segment.x2);
            for x in x1..=x2 {
                if grid.contains_key(&(x, segment.y1)) {
                    let val = grid.get(&(x, segment.y1)).unwrap();
                    *grid.get_mut(&(x, segment.y1)).unwrap() = val + 1;
                } else {
                    grid.insert((x, segment.y1), 1);
                }
            }
        }
    }
    // Check grid
    let mut count = 0;
    for (_k, v) in grid.iter() {
        if v > &1 {
            count += 1;
        }
    }
    println!("Result1 = {:?}", count); // 6687
}

fn part2(lines: &Vec<String>) {
    let mut segments: Vec<Segment> = Vec::new();
    for line in lines.iter() {
        segments.push(read_segment(line));
    }
    // Filter horz + vert + diagonal segments
    segments.retain(|x| x.x1 == x.x2 || x.y1 == x.y2 || (x.x1 - x.x2).abs() == (x.y1 - x.y2).abs());
    // println!("SEGMENTS = {:#?}", segments);
    // Grid
    let mut grid: HashMap<(isize, isize), isize> = HashMap::new();
    for segment in segments.iter() {
        if segment.x1 == segment.x2 {
            let y1 = cmp::min(segment.y1, segment.y2);
            let y2 = cmp::max(segment.y1, segment.y2);
            for y in y1..=y2 {
                if grid.contains_key(&(segment.x1, y)) {
                    let val = grid.get(&(segment.x1, y)).unwrap();
                    *grid.get_mut(&(segment.x1, y)).unwrap() = val + 1;
                } else {
                    grid.insert((segment.x1, y), 1);
                }
            }
        } else if segment.y1 == segment.y2 {
            let x1 = cmp::min(segment.x1, segment.x2);
            let x2 = cmp::max(segment.x1, segment.x2);
            for x in x1..=x2 {
                if grid.contains_key(&(x, segment.y1)) {
                    let val = grid.get(&(x, segment.y1)).unwrap();
                    *grid.get_mut(&(x, segment.y1)).unwrap() = val + 1;
                } else {
                    grid.insert((x, segment.y1), 1);
                }
            }
        } else {
            // Diagonal
            let p1;
            let p2;
            if segment.x1 > segment.x2 {
                p1 = (segment.x2, segment.y2);
                p2 = (segment.x1, segment.y1);
            } else {
                p1 = (segment.x1, segment.y1);
                p2 = (segment.x2, segment.y2);
            }
            let ysign = if p2.1 > p1.1 { 1 } else { -1 };
            for x in p1.0..=p2.0 {
                let pos = (x, p1.1 + ysign * (x - p1.0));
                if grid.contains_key(&pos) {
                    let val = grid.get(&pos).unwrap();
                    *grid.get_mut(&pos).unwrap() = val + 1;
                } else {
                    grid.insert(pos, 1);
                }
            }
        }
    }
    // Check grid
    let mut count = 0;
    for (_k, v) in grid.iter() {
        if v > &1 {
            count += 1;
        }
    }
    println!("Result2 = {:?}", count); // 19851
}

pub fn main() -> Result<(), Box<dyn std::error::Error>> {
    let lines = read_lines("input5.txt")?;

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
