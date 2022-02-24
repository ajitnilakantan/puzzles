use regex::Regex;
use std::collections::HashSet;
use std::fs;
use std::fs::File;
use std::io::{self, BufRead, Error};
use std::iter::FromIterator;
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

fn read_beacons(lines: &Vec<String>) -> (Vec<Vec<(isize, isize, isize)>>, Vec<Match>) {
    // Part 1: Read the beacons
    let mut beacons: Vec<Vec<(isize, isize, isize)>> = Vec::new();
    for line in lines.iter() {
        let lines: Vec<String> = line
            .split(['\r', '\n'].as_ref())
            .filter(|&x| !x.is_empty())
            .map(|x| x.to_string())
            .collect();
        let mut vec: Vec<(isize, isize, isize)> = Vec::new();
        for i in 1..lines.len() {
            let numbers = read_numbers(&lines[i]);
            vec.push((numbers[0], numbers[1], numbers[2]));
        }
        beacons.push(vec);
    }

    // Part2:  Match up pairs of beacons
    let mut to_process: Vec<usize> = vec![0];
    let mut processed: Vec<usize> = Vec::new();
    let mut all_matches: Vec<Match> = Vec::new();

    while to_process.len() > 0 {
        let scanner = to_process.pop().unwrap();
        processed.push(scanner);
        for i in 0..beacons.len() {
            if processed.contains(&i) {
                continue;
            }
            if let Some(matchh) = match_up(&beacons[scanner], &beacons[i]) {
                // println!("      ====> MATCH at {} {}", scanner, i);
                all_matches.push(Match {
                    index1: scanner,
                    index2: i,
                    pos2: matchh.0,
                    mapping: matchh.1,
                });
                to_process.push(i);
            }
        }
    }

    (beacons, all_matches)
}

fn vec_wrt(vec: &Vec<(isize, isize, isize)>, wrt: usize) -> Vec<isize> {
    let wrt = vec[wrt];
    let result: Vec<isize> = vec
        .iter()
        .map(|x| {
            (x.0 - wrt.0) * (x.0 - wrt.0)
                + (x.1 - wrt.1) * (x.1 - wrt.1)
                + (x.2 - wrt.2) * (x.2 - wrt.2)
        })
        .collect();
    result
}

// Calculate the mapping e.g. (a, b, c) -> (-c, a, -b) == (1, 2, 3) -> (-3, 1, -2)
fn get_map(diff: &isize, diff2: &Vec<isize>) -> isize {
    // Helper to map e.g. from a -> -c, a, b --> would return +2
    if diff.abs() == diff2[0].abs() {
        return if diff == &diff2[0] { 1 } else { -1 };
    }
    if diff.abs() == diff2[1].abs() {
        return if diff == &diff2[1] { 2 } else { -2 };
    }
    if diff.abs() == diff2[2].abs() {
        return if diff == &diff2[2] { 3 } else { -3 };
    }
    panic!();
}
fn get_sign(val: isize) -> isize {
    if val < 0 {
        -1
    } else {
        1
    }
}

fn find_match(vec_rel1: &Vec<isize>, vec_rel2: &Vec<isize>) -> Option<HashSet<isize>> {
    let h1: HashSet<isize> = HashSet::from_iter(vec_rel1.iter().cloned());
    let h2: HashSet<isize> = HashSet::from_iter(vec_rel2.iter().cloned());
    let intersection: HashSet<isize> = h1.into_iter().filter(|e| h2.contains(e)).collect();
    // let intersection: HashSet<isize> = h1.intersection(&h2).into_iter().collect::<isize>();
    if intersection.len() >= 12 {
        return Some(intersection);
    } else {
        return None;
    }
}
fn match_up(
    vec1: &Vec<(isize, isize, isize)>,
    vec2: &Vec<(isize, isize, isize)>,
) -> Option<(Vec<isize>, Vec<isize>)> {
    for i in 0..vec1.len() {
        let vec_rel1 = vec_wrt(&vec1, i);
        for j in 0..vec2.len() {
            let vec_rel2 = vec_wrt(&vec2, j);
            if let Some(intersection) = find_match(&vec_rel1, &vec_rel2) {
                // println!("FOUND MATCH");
                let mut indices1: Vec<usize> = Vec::new();
                let mut indices2: Vec<usize> = Vec::new();
                for h in intersection {
                    if let Some(index1) = vec_rel1.iter().position(|x| x == &h) {
                        if let Some(index2) = vec_rel2.iter().position(|x| x == &h) {
                            // println!("  {:?} {:?}", vec1[index1], vec2[index2]);
                            indices1.push(index1);
                            indices2.push(index2);
                        }
                    }
                }
                let diff1 = vec![
                    vec1[indices1[1]].0 - vec1[indices1[0]].0,
                    vec1[indices1[1]].1 - vec1[indices1[0]].1,
                    vec1[indices1[1]].2 - vec1[indices1[0]].2,
                ];
                let diff2 = vec![
                    vec2[indices2[1]].0 - vec2[indices2[0]].0,
                    vec2[indices2[1]].1 - vec2[indices2[0]].1,
                    vec2[indices2[1]].2 - vec2[indices2[0]].2,
                ];
                // println!("diff1={:?}  diff2={:?}", diff1, diff2);
                let mapping: Vec<isize> = vec![
                    get_map(&diff1[0], &diff2),
                    get_map(&diff1[1], &diff2),
                    get_map(&diff1[2], &diff2),
                ];
                // And Calculate the coordinates of scanner1 wrt to scanner0 (as the origin (0, 0, 0))
                let p1 = vec1[indices1[0]];
                let p1 = vec![p1.0, p1.1, p1.2];
                let p2 = vec2[indices2[0]];
                let p2 = vec![p2.0, p2.1, p2.2];
                let mut pos2: Vec<isize> = Vec::new();
                pos2.push(p1[0] - get_sign(mapping[0]) * p2[mapping[0].abs() as usize - 1]);
                pos2.push(p1[1] - get_sign(mapping[1]) * p2[mapping[1].abs() as usize - 1]);
                pos2.push(p1[2] - get_sign(mapping[2]) * p2[mapping[2].abs() as usize - 1]);

                return Some((pos2, mapping));
            }
        }
    }
    None
}

struct Match {
    index1: usize,
    index2: usize,
    pos2: Vec<isize>,    // pos2 wrt pos1 at (0,0,0)
    mapping: Vec<isize>, // mapping e.g. (a, b, c) -> (-c, a, -b) == (1, 2, 3) -> (-3, 1, -2)
}

fn count_beacons(beacons: &Vec<Vec<(isize, isize, isize)>>, matches: &Vec<Match>) -> usize {
    // All beacons in the coords of scanner0
    let mut all_beacons: HashSet<(isize, isize, isize)> = HashSet::new();
    for scanner in 0..beacons.len() {
        let mut cur_scanner = scanner;
        let mut beacons0 = beacons[scanner].clone(); // beacons of scanner cur_scanner with scanner_0
        while cur_scanner != 0 {
            // Transfrom down until we reach scanner 0
            let index = matches
                .iter()
                .position(|x| x.index2 == cur_scanner)
                .unwrap();
            for b in &mut beacons0 {
                // Go from index2 -> index1
                let p2 = vec![b.0, b.1, b.2];
                let pos2 = &matches[index].pos2;
                let mapping = &matches[index].mapping;
                b.0 = pos2[0] + get_sign(mapping[0]) * p2[mapping[0].abs() as usize - 1];
                b.1 = pos2[1] + get_sign(mapping[1]) * p2[mapping[1].abs() as usize - 1];
                b.2 = pos2[2] + get_sign(mapping[2]) * p2[mapping[2].abs() as usize - 1];
            }
            cur_scanner = matches[index].index1;
        }
        // Add to all
        for b in &beacons0 {
            all_beacons.insert((b.0, b.1, b.2));
        }
    }
    all_beacons.len()
}

fn max_distance_apart(beacons: &Vec<Vec<(isize, isize, isize)>>, matches: &Vec<Match>) -> usize {
    // All scanners in the coords of scanner0
    let mut all_scanners: Vec<Vec<isize>> = Vec::new();
    for scanner in 0..beacons.len() {
        let mut cur_scanner = scanner;
        let mut scanner0: Vec<isize> = vec![0, 0, 0]; // beacons of scanner cur_scanner with scanner_0
        while cur_scanner != 0 {
            // Transfrom down until we reach scanner 0
            let index = matches
                .iter()
                .position(|x| x.index2 == cur_scanner)
                .unwrap();

            // Go from index2 -> index1
            let p2 = scanner0.clone();
            let pos2 = &matches[index].pos2;
            let mapping = &matches[index].mapping;
            scanner0[0] = pos2[0] + get_sign(mapping[0]) * p2[mapping[0].abs() as usize - 1];
            scanner0[1] = pos2[1] + get_sign(mapping[1]) * p2[mapping[1].abs() as usize - 1];
            scanner0[2] = pos2[2] + get_sign(mapping[2]) * p2[mapping[2].abs() as usize - 1];

            cur_scanner = matches[index].index1;
        }
        // Add to all
        all_scanners.push(scanner0);
    }
    // println!("SCANNERS = {:#?}", all_scanners);

    // Part2 : Find the maximum pairwise distance
    let mut max_dist: isize = 0;
    for i in 0..all_scanners.len() {
        for j in 0..i {
            let dist: isize = all_scanners[i]
                .iter()
                .zip(all_scanners[j].iter())
                .map(|(x, y)| (x - y).abs())
                .sum();
            if dist > max_dist {
                max_dist = dist;
            }
        }
    }
    max_dist as usize
}

fn part1(lines: &Vec<String>) {
    let (beacons, all_matches) = read_beacons(lines);
    let result1 = count_beacons(&beacons, &all_matches);

    // println!("Beacons = {:?}", beacons);
    println!("Result1 = {}", result1); // 335
}

fn part2(lines: &Vec<String>) {
    let (beacons, all_matches) = read_beacons(lines);
    let result2 = max_distance_apart(&beacons, &all_matches);

    println!("Result2 = {}", result2); // 10864
}

pub fn main() -> Result<(), Box<dyn std::error::Error>> {
    //let lines = read_lines("input.txt")?;
    let chunks = read_chunks("input19.txt")?;

    part1(&chunks);
    part2(&chunks);
    Ok(())
}

#[cfg(test)]
mod tests {
    use super::*;
    #[test]
    fn part1_works() {
        let vec0: Vec<(isize, isize, isize)> = vec![
            (404, -588, -901),
            (528, -643, 409),
            (-838, 591, 734),
            (390, -675, -793),
            (-537, -823, -458),
            (-485, -357, 347),
            (-345, -311, 381),
            (-661, -816, -575),
            (-876, 649, 763),
            (-618, -824, -621),
            (553, 345, -567),
            (474, 580, 667),
            (-447, -329, 318),
            (-584, 868, -557),
            (544, -627, -890),
            (564, 392, -477),
            (455, 729, 728),
            (-892, 524, 684),
            (-689, 845, -530),
            (423, -701, 434),
            (7, -33, -71),
            (630, 319, -379),
            (443, 580, 662),
            (-789, 900, -551),
            (459, -707, 401),
        ];
        let vec1: Vec<(isize, isize, isize)> = vec![
            (686, 422, 578),
            (605, 423, 415),
            (515, 917, -361),
            (-336, 658, 858),
            (95, 138, 22),
            (-476, 619, 847),
            (-340, -569, -846),
            (567, -361, 727),
            (-460, 603, -452),
            (669, -402, 600),
            (729, 430, 532),
            (-500, -761, 534),
            (-322, 571, 750),
            (-466, -666, -811),
            (-429, -592, 574),
            (-355, 545, -477),
            (703, -491, -529),
            (-328, -685, 520),
            (413, 935, -424),
            (-391, 539, -444),
            (586, -435, 557),
            (-364, -763, -893),
            (807, -499, -711),
            (755, -354, -619),
            (553, 889, -390),
        ];
        match_up(&vec0, &vec1);
    }
    #[test]
    fn part2_works() {}
}
