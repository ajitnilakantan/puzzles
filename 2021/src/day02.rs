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

pub fn main() -> Result<(), Box<dyn std::error::Error>> {
    // let lines = read_lines("input2ex.txt")?;
    let lines = read_lines("input2.txt")?;

    #[derive(Debug)]
    struct Heading(String, i64);
    let mut pos_horz = 0;
    let mut pos_vert = 0;
    for line in &lines {
        let parts: Vec<&str> = line.split(' ').collect();
        let dir = parts[0].parse::<String>()?;
        let val = parts[1].parse::<i64>()?;

        let my_tuple = Heading(dir, val);
        // print!("my = {:#?}", my_tuple);
        match my_tuple.0.as_str() {
            "forward" => pos_horz += my_tuple.1,
            "up" => pos_vert -= my_tuple.1,
            "down" => pos_vert += my_tuple.1,
            _ => (),
        }
    }
    println!("pos = {} {}", pos_horz, pos_vert);
    println!("result = {}", pos_horz * pos_vert);

    // Part 2
    pos_horz = 0;
    pos_vert = 0;
    let mut aim = 0;

    for line in &lines {
        let parts: Vec<&str> = line.split(' ').collect();
        let dir = parts[0].parse::<String>()?;
        let val = parts[1].parse::<i64>()?;

        let my_tuple = Heading(dir, val);
        match my_tuple.0.as_str() {
            "forward" => {
                pos_horz += my_tuple.1;
                pos_vert += aim * my_tuple.1
            }
            "up" => aim -= my_tuple.1,
            "down" => aim += my_tuple.1,
            _ => (),
        }
    }
    println!("pos = {} {}", pos_horz, pos_vert);
    println!("result = {}", pos_horz * pos_vert);

    Ok(())
}
