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

fn is_regular_pair(chars: &Vec<char>, offset: usize) -> bool {
    for i in offset + 1..chars.len() {
        if chars[i] == '[' {
            return false;
        }
        if chars[i] == ']' {
            return true;
        }
    }
    false
}

fn explode(line: &String, re_lhs: &Regex, re_inner: &Regex, re_rhs: &Regex) -> Option<String> {
    let mut current_level = 0;
    let mut result: String;
    let chars: Vec<char> = line.chars().collect();

    for (i, c) in chars.iter().enumerate() {
        if c == &'[' {
            current_level += 1;
        } else if c == &']' {
            current_level -= 1;
        }
        if current_level >= 5 && is_regular_pair(&chars, i) {
            let right_bracket = &line[i..].find("]").unwrap();
            let left = &line[0..i];
            let mid = &line[i..i + right_bracket + 1];
            let right = &line[i + right_bracket + 1..];

            // Inner Pair
            let re = re_inner;
            let captures = re.captures(mid).unwrap();
            let pair_left: usize = captures[1].parse().unwrap();
            let pair_right: usize = captures[2].parse().unwrap();

            // last on lhs
            let re = re_lhs;
            if let Some(captures) = re.captures(left) {
                let val: usize = captures[1].parse().unwrap();
                let mach = captures.get(1).unwrap();
                result = left[0..mach.start()].to_string();
                result.push_str(&(val + pair_left).to_string());
                result.push_str(&left[mach.end()..]);
            } else {
                result = left.to_string();
            }

            // mid
            result.push_str("0");

            // First on rhs
            let re = re_rhs;
            if let Some(captures) = re.captures(right) {
                let val: usize = captures[1].parse().unwrap();
                let mach = captures.get(1).unwrap();
                result.push_str(&right[0..mach.start()]);
                result.push_str(&(val + pair_right).to_string());
                result.push_str(&right[mach.end()..]);
            } else {
                result.push_str(right);
            }

            return Some(result);
        }
    }
    None
}

fn split(line: &String, re_split: &Regex) -> Option<String> {
    let re = re_split;
    if let Some(captures) = re.captures(line) {
        let val: usize = captures[1].parse().unwrap();
        let mach = captures.get(1).unwrap();
        let l = val / 2;
        let r = val - l;
        let mut result = line[0..mach.start()].to_string();
        result.push_str("[");
        result.push_str(&l.to_string());
        result.push_str(",");
        result.push_str(&r.to_string());
        result.push_str("]");
        result.push_str(&line[mach.end()..]);

        return Some(result);
    }
    None
}

fn simplify(line: &String) -> String {
    let mut result = line.clone();
    let mut dirty = true;
    let re_lhs = Regex::new(r"(\d+)\D*\z").unwrap();
    let re_inner = Regex::new(r".*?(\d+),(\d+).*?").unwrap();
    let re_rhs = Regex::new(r".*?(\d+).*").unwrap();
    let re_split = Regex::new(r".*?(\d\d+).*").unwrap();

    while dirty {
        dirty = false;
        while let Some(val) = explode(&result, &re_lhs, &re_inner, &re_rhs) {
            dirty = true;
            result = val.clone();
        }
        if let Some(val) = split(&result, &re_split) {
            dirty = true;
            result = val.clone();
        }
    }
    result
}

fn evaluate(line: &String, re: &Regex) -> usize {
    if line.matches("[").count() == 1 {
        // [a,b]
        let captures = re.captures(line).unwrap();
        let pair_left: usize = captures[1].parse().unwrap();
        let pair_right: usize = captures[2].parse().unwrap();
        return 3 * pair_left + 2 * pair_right;
    } else if line.matches("[").count() == 0 {
        // a
        let val: usize = line.parse().unwrap();
        return val;
    } else {
        // [[a,b],[[a,b],c]] split at first level
        let mut current_level = 0;
        let chars = line.chars();
        for (i, c) in chars.enumerate() {
            if c == '[' {
                current_level += 1;
            } else if c == ']' {
                current_level -= 1;
            } else if c == ',' && current_level == 1 {
                let left = &line[1..i].to_string();
                let right = &line[i + 1..line.len() - 1].to_string();
                return 3 * evaluate(&left, re) + 2 * evaluate(&right, re);
            }
        }
    }
    0
}
fn part1(lines: &Vec<String>) {
    let mut line = lines[0].clone();
    for i in 1..lines.len() {
        line = format!("[{},{}]", line, lines[i]);
        line = simplify(&line);
    }
    let re_evaluate = Regex::new(r".*?(\d+),(\d+).*?").unwrap();
    let result = evaluate(&line, &re_evaluate);
    println!("Result1 = {}", result); // 4235 (takes a long time)
}

fn part2(lines: &Vec<String>) {
    let mut max = 0;
    let re_evaluate = Regex::new(r".*?(\d+),(\d+).*?").unwrap();
    for i in 0..lines.len() - 1 {
        for j in i + 1..lines.len() {
            let line = format!("[{},{}]", lines[i], lines[j]);
            let line = simplify(&line);
            let val = evaluate(&line, &re_evaluate);
            if max < val {
                max = val;
            }
            let line = format!("[{},{}]", lines[j], lines[i]);
            let line = simplify(&line);
            let val = evaluate(&line, &re_evaluate);
            if max < val {
                max = val;
            }
        }
    }
    println!("Result2 = {}", max); // 4659 (takes a very long time)
}

pub fn main() -> Result<(), Box<dyn std::error::Error>> {
    let lines = read_lines("input18.txt")?;
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
        //let mut reader: VecDeque<char> = VecDeque::from("[1,2]".chars().iter());
        //parse(&"[1,2]".to_string());
        //parse(&"[[1,2],[[3,4],5]]".to_string());
        let re_lhs = Regex::new(r"(\d+)\D*\z").unwrap();
        let re_inner = Regex::new(r".*?(\d+),(\d+).*?").unwrap();
        let re_rhs = Regex::new(r".*?(\d+).*").unwrap();

        let exp = explode(
            &"[[[[[9,8],1],2],3],4]".to_string(),
            &re_lhs,
            &re_inner,
            &re_rhs,
        )
        .unwrap();
        assert_eq!(exp, "[[[[0,9],2],3],4]");
        let exp = explode(
            &"[7,[6,[5,[4,[3,2]]]]]".to_string(),
            &re_lhs,
            &re_inner,
            &re_rhs,
        )
        .unwrap();
        assert_eq!(exp, "[7,[6,[5,[7,0]]]]");
        let exp = explode(
            &"[[6,[5,[4,[3,2]]]],1]".to_string(),
            &re_lhs,
            &re_inner,
            &re_rhs,
        )
        .unwrap();
        assert_eq!(exp, "[[6,[5,[7,0]]],3]");
        let exp = explode(
            &"[[3,[2,[1,[7,3]]]],[6,[5,[4,[3,2]]]]]".to_string(),
            &re_lhs,
            &re_inner,
            &re_rhs,
        )
        .unwrap();
        assert_eq!(exp, "[[3,[2,[8,0]]],[9,[5,[4,[3,2]]]]]");
        let exp = explode(
            &"[[3,[2,[8,0]]],[9,[5,[4,[3,2]]]]]".to_string(),
            &re_lhs,
            &re_inner,
            &re_rhs,
        )
        .unwrap();
        assert_eq!(exp, "[[3,[2,[8,0]]],[9,[5,[7,0]]]]");
        let exp = explode(
            &"[[3,[2,[8,0]]],[9,[5,[7,0]]]]".to_string(),
            &re_lhs,
            &re_inner,
            &re_rhs,
        );
        assert_eq!(exp, None);

        let re_split = Regex::new(r".*?(\d\d+).*").unwrap();
        let exp = split(&"[[[[0,7],4],[15,[0,13]]],[1,1]]".to_string(), &re_split).unwrap();
        assert_eq!(exp, "[[[[0,7],4],[[7,8],[0,13]]],[1,1]]");
        let exp = split(&"[[[[0,7],4],[[7,8],[0,13]]],[1,1]]".to_string(), &re_split).unwrap();
        assert_eq!(exp, "[[[[0,7],4],[[7,8],[0,[6,7]]]],[1,1]]");
        let exp = split(
            &"[[[[0,7],4],[[7,8],[0,[6,7]]]],[1,1]]".to,
            &re_split_string(),
        );
        assert_eq!(exp, None);

        let exp = simplify(&"[[[[[4,3],4],4],[7,[[8,4],9]]],[1,1]]".to_string());
        assert_eq!(exp, "[[[[0,7],4],[[7,8],[6,0]]],[8,1]]");

        let re_evaluate = Regex::new(r".*?(\d+),(\d+).*?").unwrap();

        let val = evaluate(&"[9,1]".to_string(), &re_evaluate);
        assert_eq!(val, 29);
        let val = evaluate(&"[[9,1],[1,9]]".to_string(), &re_evaluate);
        assert_eq!(val, 129);

        let val = evaluate(
            &"[[[[0,7],4],[[7,8],[6,0]]],[8,1]]".to_string(),
            &re_evaluate,
        );
        assert_eq!(val, 1384);
        let val = evaluate(
            &"[[[[8,7],[7,7]],[[8,6],[7,7]]],[[[0,7],[6,6]],[8,7]]]".to_string(),
            &re_evaluate,
        );
        assert_eq!(val, 3488);
    }
    #[test]
    fn part2_works() {
        //let exp = simplify(&"[[[[[1,1],[2,2]],[3,3]],[4,4]],[5,5]]".to_string());
        //assert_eq!(exp, "[[[[3,0],[5,3]],[4,4]],[5,5]]");
        //let exp = simplify(&"[[[[[[1,1],[2,2]],[3,3]],[4,4]],[5,5]],[6,6]]".to_string());
        //assert_eq!(exp, "[[[[5,0],[7,4]],[5,5]],[6,6]]");
        let exp = simplify(
            &"[[[[[6,6],[6,6]],[[6,0],[6,7]]],[[[7,7],[8,9]],[8,[8,1]]]],[2,9]]".to_string(),
        );
        assert_eq!(exp, "[[[[6,6],[7,7]],[[0,7],[7,7]]],[[[5,5],[5,6]],9]]");
    }
}
