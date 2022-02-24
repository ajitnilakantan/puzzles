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

pub fn main() -> Result<(), Error> {
    // let lines = read_lines("input.txt")?;
    let lines = read_lines("input1.txt")?;

    let nums: Vec<i64> = lines.iter().map(|n| n.parse().unwrap()).collect();

    let mut max = i64::MAX;
    let mut count = 0;
    for num in &nums {
        if *num > max {
            count += 1;
        }
        max = *num;
    }
    println!("Result1 = '{}'", count);

    let mut nums3: Vec<i64> = Vec::new();
    for (pos, _e) in nums.iter().enumerate() {
        if pos > nums.len() - 3 {
            break;
        }
        nums3.push(nums[pos] + nums[pos + 1] + nums[pos + 2]);
    }
    let mut max3 = i64::MAX;
    let mut count3 = 0;
    for num3 in nums3 {
        if num3 > max3 {
            count3 += 1;
        }
        max3 = num3;
    }
    println!("Result2 = '{}'", count3);

    Ok(())
}
