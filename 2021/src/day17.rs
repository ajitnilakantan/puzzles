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

fn get_target(line: &String) -> Vec<isize> {
    // target area: x=20..30, y=-10..-5
    let coords = &line[13..];
    let numbers: Vec<isize> = coords
        .split([',', ' ', '.', '=', 'x', 'y'].as_ref())
        .filter(|&x| !x.is_empty())
        .map(|x| x.to_string().parse::<isize>().unwrap())
        .collect();
    vec![numbers[0], numbers[1], numbers[2], numbers[3]]
}

fn get_xspeed(x: isize) -> (isize, isize) {
    // x speed decreases by 1 each step.  For maximum time, speed ends up at 0.
    // sum(0..v) = (v)(v+1)/2 --> solve x = v*(v+1)/2 for v, given x.
    let x = x as f64;
    let v = ((-1.0 + (1.0 + 4.0 * x).sqrt()) / 2.0) as f64;
    (v as isize, (v * (v + 1.0) / 2.0) as isize)
}

fn get_yspeed(y: isize, time: isize) -> isize {
    // y = 1/2 a*t^2 + v_0*t + y_0
    let y = y as f64;
    let time = time as f64;
    let val = y / time + time / 2.0;
    val as isize
}

fn plot_course(
    xspeed: isize,
    yspeed: isize,
    max_time: isize,
    target: &Vec<isize>,
) -> Option<Vec<(isize, isize)>> {
    let mut pos = (0, 0);
    let mut xspeed = xspeed;
    let mut yspeed = yspeed;
    let mut result: Vec<(isize, isize)> = Vec::new();
    result.push(pos);
    for _ in 0..max_time {
        pos.0 = pos.0 + xspeed;
        if xspeed > 0 {
            xspeed -= 1;
        }
        pos.1 = pos.1 + yspeed;
        yspeed -= 1;
        result.push(pos);
        if pos.0 >= target[0] && pos.0 <= target[1] && pos.1 >= target[2] && pos.1 <= target[3] {
            return Some(result);
        }
        if pos.1 < target[2] {
            break;
        }
    }
    return None;
}

fn get_max_y(target: &Vec<isize>) -> (isize, isize) {
    let (mut xspeed0, _) = get_xspeed(target[0]);
    xspeed0 -= 1;
    let (mut xspeed, _) = get_xspeed(target[1]);
    xspeed += 1;
    let time = xspeed * (xspeed + 1) / 2;
    let yspeed = get_yspeed(target[3], time);

    let mut max_y = 0;
    let mut max_yspeed = 0;
    for y in (yspeed - 20..yspeed + 200).rev() {
        for x in xspeed0 - 12..xspeed + 12 {
            if let Some(vec) = plot_course(x, y, time + 500, &target) {
                let max = vec.iter().map(|x| x.1).max().unwrap();
                if max_y < max {
                    max_y = max;
                    max_yspeed = y;
                }
            }
        }
    }
    (max_y, max_yspeed)
}
fn get_count(target: &Vec<isize>) -> isize {
    let (mut xspeed_min, _) = get_xspeed(target[0]);
    xspeed_min -= 2;
    let xspeed_max = target[1] + 2;
    let (_, yspeed_max) = get_max_y(target);
    let yspeed_min = target[2];

    let mut count = 0;
    for y in (yspeed_min - 1..yspeed_max + 1).rev() {
        for x in xspeed_min..xspeed_max {
            if let Some(_vec) = plot_course(x, y, 50000, &target) {
                count += 1;
            }
        }
    }
    count
}
fn part1(lines: &Vec<String>) {
    let target = get_target(&lines[0]);
    let (result, _) = get_max_y(&target);
    println!("Result1 = {}", result); // 4278
}

fn part2(lines: &Vec<String>) {
    let target = get_target(&lines[0]);
    let result = get_count(&target);
    println!("Result2 = {}", result); // 1994
}

pub fn main() -> Result<(), Box<dyn std::error::Error>> {
    let lines = read_lines("input17.txt")?;
    // let chunks = read_chunks("input.txt")?;

    part1(&lines);
    part2(&lines);
    Ok(())
}

#[cfg(test)]
mod tests {
    use super::*;
    #[test]
    fn part1_works() {
        let target = get_target(&"target area: x=20..30, y=-10..-5".to_string());
        let target = get_target(&"target area: x=195..238, y=-93..-67".to_string());
        println!("target = {:?}", target);
        let (vx0, t0) = get_xspeed(target[0]);
        let (vx1, t1) = get_xspeed(target[1]);
        println!("xspeed = {:?} {:?} time = {} {}", vx0, vx1, t0, t1);
        let yv0 = get_yspeed(target[2], t0);
        let yv1 = get_yspeed(target[2], t1);
        println!("yspeed = {:?} {:?}", yv0, yv1);
        let yv0 = get_yspeed(target[3], t0);
        let yv1 = get_yspeed(target[3], t1);
        println!("yspeed = {:?} {:?}", yv0, yv1);

        let (mut xspeed0, _) = get_xspeed(target[0]);
        xspeed0 -= 1;
        let (mut xspeed, _) = get_xspeed(target[1]);
        xspeed += 1;
        let time = xspeed * (xspeed + 1) / 2;
        let yspeed = get_yspeed(target[3], time);
        println!("x, y = {:?}, {:?}  time={}", xspeed, yspeed, time);
        let mut max_y = 0;
        for y in (yspeed - 20..yspeed + 200).rev() {
            // 4278
            for x in xspeed0 - 12..xspeed + 12 {
                if let Some(vec) = plot_course(x, y, time + 500, &target) {
                    let max = vec.iter().map(|x| x.1).max().unwrap();
                    if max_y < max {
                        max_y = max;
                    }
                }
            }
        }
        println!("max_y = {}", max_y);
    }
    #[test]
    fn part2_works() {}
}
