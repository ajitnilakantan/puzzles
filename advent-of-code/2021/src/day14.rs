use regex::Regex;
use std::collections::HashMap;
use std::collections::HashSet;
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

fn read_substitutions(lines: &String) -> HashMap<(char, char), char> {
    let lines: Vec<String> = lines
        .split(['\r', '\n'].as_ref())
        .filter(|&x| !x.is_empty())
        .map(|x| x.to_string())
        .collect();

    let mut substitutions: HashMap<(char, char), char> = HashMap::new();
    for line in &lines {
        let pair: Vec<String> = line
            .split(['-', '>', ' '].as_ref())
            .filter(|&x| !x.is_empty())
            .map(|x| x.to_string())
            .collect();
        let chars: Vec<char> = pair[0].chars().collect();
        assert_eq!(chars.len(), 2);
        assert_eq!(pair[1].len(), 1);
        // first char of string
        let subst_char = pair[1].chars().next().unwrap();
        substitutions.insert((chars[0], chars[1]), subst_char);
    }
    substitutions
}
fn process_substitution(
    template: &Vec<char>,
    substitutions: &HashMap<(char, char), char>,
) -> Vec<char> {
    let mut result: Vec<char> = Vec::new();
    for i in 0..template.len() - 1 {
        result.push(template[i]);
        let key = (template[i], template[i + 1]);
        if let Some(subst) = substitutions.get(&key) {
            result.push(*subst);
        }
    }
    result.push(template[template.len() - 1]);
    result
}
fn get_alphabet(
    template: &Vec<char>,
    substitutions: &HashMap<(char, char), char>,
) -> HashSet<char> {
    let mut alphabet: HashSet<char> = HashSet::new();
    for ch in template {
        alphabet.insert(*ch);
    }
    for (_k, v) in substitutions {
        alphabet.insert(*v);
    }
    alphabet
}

fn part1(lines: &Vec<String>) {
    let mut template: Vec<char> = (&lines[0]).chars().collect();
    let substitutions = read_substitutions(&lines[1]);
    // println!("subst = '{:?}'", substitutions);
    // println!("{}: tmpl = '{:?}'", template.len(), template);
    let alphabet = get_alphabet(&template, &substitutions);
    for _i in 0..10 {
        template = process_substitution(&template, &substitutions);
        // println!("{}: tmpl = '{:?}'", template.len(), template);
        {
            let mut counts: Vec<usize> = Vec::new();
            for ch in alphabet.iter() {
                counts.push(template.iter().filter(|&n| *n == *ch).count());
            }
            counts.sort();
            let result = counts[counts.len() - 1] - counts[0];
            println!("{}: {} {}", _i, template.len(), result);
        }
    }
    let mut counts: Vec<usize> = Vec::new();
    for ch in alphabet.iter() {
        counts.push(template.iter().filter(|&n| *n == *ch).count());
    }
    counts.sort();
    let result = counts[counts.len() - 1] - counts[0];

    println!("Result1 = {}", result); // 2447
}

fn read_pairs(template: &Vec<char>) -> (HashMap<(char, char), usize>, HashMap<char, usize>) {
    // Return map of pairs / counts  +   double counted common letters between pairs / counts
    let mut pairs: HashMap<(char, char), usize> = HashMap::new();
    let mut common: HashMap<char, usize> = HashMap::new();

    // Get pairs
    for i in 0..template.len() - 1 {
        let key = (template[i], template[i + 1]);
        if let Some(p) = pairs.get(&key).cloned() {
            pairs.insert(key, p + 1);
        } else {
            pairs.insert(key, 1);
        }
    }
    // Get double counted common letters
    for n in (1..template.len() - 1).step_by(1) {
        let key = &template[n];
        if let Some(count) = common.get(key).cloned() {
            common.insert(*key, count + 1);
        } else {
            common.insert(*key, 1);
        }
    }

    (pairs, common)
}

fn process_pair_substitution(
    pairs: &mut HashMap<(char, char), usize>,
    common: &mut HashMap<char, usize>,
    substitutions: &HashMap<(char, char), char>,
) {
    // Return map of pairs / counts  +   double counted common letters between pairs / counts
    let oldpairs: HashMap<(char, char), usize> = pairs.clone();
    pairs.clear();

    for (k, v) in oldpairs {
        if let Some(subst) = substitutions.get(&k) {
            // Create 2 new pairs
            let pair1 = (k.0, *subst);
            let pair2 = (*subst, k.1);
            let dupes = v;
            if let Some(p) = pairs.get(&pair1).cloned() {
                pairs.insert(pair1, p + v);
            } else {
                pairs.insert(pair1, v);
            }
            if let Some(p) = pairs.get(&pair2).cloned() {
                pairs.insert(pair2, p + v);
            } else {
                pairs.insert(pair2, v);
            }
            if let Some(p) = common.get(subst).cloned() {
                common.insert(*subst, p + dupes);
            } else {
                common.insert(*subst, dupes);
            }
        }
    }
}
fn count_pair_chars(
    pairs: &mut HashMap<(char, char), usize>,
    common: &mut HashMap<char, usize>,
    _alphabet: &HashSet<char>,
) -> (Vec<usize>, usize) {
    let mut counts: HashMap<char, usize> = HashMap::new();
    for (k, v) in pairs {
        if let Some(c) = counts.get(&k.0).cloned() {
            counts.insert(k.0, c + *v);
        } else {
            counts.insert(k.0, *v);
        }
        if let Some(c) = counts.get(&k.1).cloned() {
            counts.insert(k.1, c + *v);
        } else {
            counts.insert(k.1, *v);
        }
    }
    // Subract common
    for (k, v) in common {
        if let Some(c) = counts.get(k).cloned() {
            counts.insert(*k, c - *v);
        }
    }
    // Sum up
    let mut result = Vec::<usize>::new();
    for (_k, v) in counts {
        result.push(v);
    }
    result.sort();
    let total_count = result.iter().sum();
    (result, total_count)
}

fn part2(lines: &Vec<String>) {
    let template: Vec<char> = (&lines[0]).chars().collect();
    let substitutions = read_substitutions(&lines[1]);
    // println!("subst = '{:?}'", substitutions);
    // println!("{}: tmpl = '{:?}'", template.len(), template);
    let alphabet = get_alphabet(&template, &substitutions);
    let (mut pairs, mut common) = read_pairs(&template);
    for _i in 0..40 {
        process_pair_substitution(&mut pairs, &mut common, &substitutions);

        let (counts, total) = count_pair_chars(&mut pairs, &mut common, &alphabet);
        let result = counts[counts.len() - 1] - counts[0];

        println!("{} : len={} result={}", _i, total, result);
    }

    let (counts, _total) = count_pair_chars(&mut pairs, &mut common, &alphabet);
    let result = counts[counts.len() - 1] - counts[0];
    println!("Result2 = {}", result); // 3018019237563
}

pub fn main() -> Result<(), Box<dyn std::error::Error>> {
    // let lines = read_lines("input.txt")?;
    let chunks = read_chunks("input14.txt")?;

    part1(&chunks);
    part2(&chunks);
    Ok(())
}

#[cfg(test)]
mod tests {
    use super::*;
    #[test]
    fn it_works() {}
}
