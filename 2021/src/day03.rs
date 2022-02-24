use std::fs::File;
use std::io::{self, BufRead, Error};
use std::path::Path;

// The output is wrapped in a Result to allow matching on errors
// Returns an Iterator to the Reader of the lines of the file.
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

fn count_zeros(lines: &Vec<String>) -> Vec<usize> {
    let num_bits = lines[0].len();
    let mut zero_count = vec![0; num_bits];
    for line in lines {
        let chars: Vec<char> = line.chars().collect();
        for (i, ch) in chars.iter().enumerate() {
            if ch == &'0' {
                zero_count[i] += 1;
            }
        }
    }
    zero_count
}
fn part1(lines: &Vec<String>) {
    let num_lines = lines.len();
    let num_bits = lines[0].len();
    let mut zero_count = vec![0; num_bits];
    for line in lines {
        let chars: Vec<char> = line.chars().collect();
        for (i, ch) in chars.iter().enumerate() {
            if ch == &'0' {
                zero_count[i] += 1;
            }
        }
    }
    let mut gamma: i64 = 0;
    let mut epsilon: i64 = 0;
    for (i, _n) in zero_count.iter().rev().enumerate() {
        gamma = gamma << 1;
        epsilon = epsilon << 1;
        if zero_count[i] > num_lines / 2 {
            epsilon |= 1;
        } else {
            gamma |= 1;
        }
    }
    println!("gamma = {} epsilon= {}", gamma, epsilon);
    println!("Result = {}", gamma * epsilon);
}

fn part2(lines: &Vec<String>) {
    let num_bits = lines[0].len();
    let mut oxygen = lines.clone();
    for i in 0..num_bits {
        // Keep 1s
        let zero_count = count_zeros(&oxygen);
        let num_lines = oxygen.len();
        if num_lines == 1 {
            break;
        }
        if zero_count[i] > num_lines / 2 {
            oxygen = oxygen
                .into_iter()
                .filter(|x| x.chars().collect::<Vec<char>>()[i] == '0')
                .collect();
        } else {
            oxygen = oxygen
                .into_iter()
                .filter(|x| x.chars().collect::<Vec<char>>()[i] == '1')
                .collect();
        }
    }
    let mut co2 = lines.clone();
    for i in 0..num_bits {
        // Keep 0s
        let zero_count = count_zeros(&co2);
        let num_lines = co2.len();
        if num_lines == 1 {
            break;
        }
        if zero_count[i] <= num_lines / 2 {
            co2 = co2
                .into_iter()
                .filter(|x| x.chars().collect::<Vec<char>>()[i] == '0')
                .collect();
        } else {
            co2 = co2
                .into_iter()
                .filter(|x| x.chars().collect::<Vec<char>>()[i] == '1')
                .collect();
        }
    }

    println!("oxygen = {:#?} co2= {:#?}", oxygen, co2);
    let oxygen_val = isize::from_str_radix(&oxygen[0], 2).unwrap();
    let co2_val = isize::from_str_radix(&co2[0], 2).unwrap();
    println!("Result = {}", oxygen_val * co2_val);
}

pub fn main() -> Result<(), Box<dyn std::error::Error>> {
    // let lines = read_lines("input3ex.txt")?;
    let lines = read_lines("input3.txt")?;

    part1(&lines);
    part2(&lines);
    Ok(())
}
