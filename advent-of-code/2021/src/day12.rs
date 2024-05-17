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
fn read_numbers(line: &str) -> Vec<isize> {
    let numbers: Vec<isize> = line
        .split([',', ' ', '\r', '\n'].as_ref())
        .filter(|&x| !x.is_empty())
        .map(|x| x.to_string().parse::<isize>().unwrap())
        .collect();

    numbers
}

#[allow(dead_code)]
fn print_type_of<T>(_: &T) {
    println!("{}", std::any::type_name::<T>())
}

type GetNeighbors<'a> = fn(&'a str, &HashMap<&'a str, Vec<&'a str>>, &Vec<&'a str>) -> Vec<&'a str>;

fn get_neighbors<'a>(
    vertex: &str,
    successors: &HashMap<&str, Vec<&'a str>>,
    path: &Vec<&str>,
) -> Vec<&'a str> {
    let mut result: Vec<&str> = Vec::new();
    if let Some(slist) = successors.get(vertex) {
        for s in slist {
            if &s.to_lowercase() == s {
                // Cannot revisit lowercase nodes
                if !path.iter().any(|&x| &x == s) {
                    result.push(s);
                }
            } else if &s.to_uppercase() == s {
                // Avoid simple loops  A->B->A
                let len = result.len();
                if len > 1
                    && result[len - 1].to_uppercase() == result[len - 1]
                    && &result[len - 2] == s
                {
                    // Ignore
                    continue;
                } else {
                    result.push(s);
                }
            }
        }
    }
    // println!("get_neighbors v={} p={:?} => {:?}", vertex, path, result);
    return result;
}

fn contains_duplicates(path: &Vec<&str>) -> bool {
    let mut counter: HashMap<&str, usize> = HashMap::new();
    for &p in path {
        if &p.to_lowercase() == p {
            // lowercase nodes can be revisited at most in total.
            // uppercase nodes any number of times
            // https://stackoverflow.com/questions/47618823/cannot-borrow-as-mutable-because-it-is-also-borrowed-as-immutable
            let getval = counter.get(p).cloned();
            match getval {
                Some(x) => counter.insert(p, x + 1),
                None => counter.insert(p, 1),
            };
        }
    }

    for (_key, value) in &counter {
        if value > &1 {
            // println!("DUPE {:?}", path);
            return true;
        }
    }
    false
}

fn get_neighbors2<'a>(
    vertex: &str,
    successors: &HashMap<&str, Vec<&'a str>>,
    path: &Vec<&str>,
) -> Vec<&'a str> {
    let mut result: Vec<&str> = Vec::new();
    if let Some(slist) = successors.get(vertex) {
        for s in slist {
            if &"start" == s {
                // Cannot revisit start
                continue;
            } else if &s.to_lowercase() == s {
                if contains_duplicates(&path) && path.contains(&s) {
                    // Cannot revisit lowercase nodes more than once
                    continue;
                } else {
                    result.push(s);
                }
            } else if &s.to_uppercase() == s {
                // Avoid simple loops  A->B->A
                let len = result.len();
                if len > 1
                    && result[len - 1].to_uppercase() == result[len - 1]
                    && &result[len - 2] == s
                {
                    // Ignore
                    continue;
                } else {
                    result.push(s);
                }
            }
        }
    }
    // println!("get_neighbors v={} p={:?} => {:?}", vertex, path, result);
    return result;
}

fn dfs_paths<'a>(
    successors: HashMap<&'a str, Vec<&'a str>>,
    start: &'a str,
    goal: &'a str,
    get_neighbors_fn: GetNeighbors<'a>,
) -> Vec<Vec<&'a str>> {
    let mut result: Vec<Vec<&'a str>> = Vec::new();
    let mut stack = vec![(start, vec![start])];
    while stack.len() > 0 {
        let (vertex, path): (&'a str, Vec<&'a str>) = stack.remove(0);
        for next in get_neighbors_fn(vertex, &successors, &path) {
            if next == goal {
                // yield path + [next];
                let mut newpath: Vec<&'a str> = path.to_vec();
                newpath.push(next);
                // println!("!!! {:?}", newpath);
                result.push(newpath);
            } else {
                let mut newpath: Vec<&str> = path.to_vec();
                newpath.push(next);
                stack.push((&next, newpath));
            }
        }
    }
    result
}

fn read_graph<'a>(
    lines: &'a Vec<String>,
) -> (
    HashSet<&'a str>,
    Vec<(&'a str, &'a str)>,
    HashMap<&'a str, Vec<&'a str>>,
) {
    let mut nodes: HashSet<&str> = HashSet::new();
    let mut edges: Vec<(&str, &str)> = Vec::new();
    let mut successors: HashMap<&str, Vec<&str>> = HashMap::new();
    for line in lines.iter() {
        let parts: Vec<&str> = line
            .split(['-'].as_ref())
            .filter(|&x| !x.is_empty())
            .map(|x| x)
            .collect();
        nodes.insert(parts[0].clone());
        nodes.insert(parts[1].clone());
        edges.push((parts[0].clone(), parts[1].clone()));
        let map = successors
            .entry(parts[0].clone())
            .or_insert(Vec::<&str>::new());
        map.push(parts[1].clone());
        let map = successors
            .entry(parts[1].clone())
            .or_insert(Vec::<&str>::new());
        map.push(parts[0].clone());
    }
    // Create an undirected graph with `i32` nodes and edges with `()` associated data.
    (nodes, edges, successors)
}

fn part1(lines: &Vec<String>) {
    let (nodes, edges, successors) = read_graph(lines);
    println!("Nodes = {:?}", nodes);
    println!("Edges = {:?}", edges);
    println!("Successors = {:?}", successors);
    let paths = dfs_paths(successors, "start", "end", get_neighbors);
    println!("Result1 = {:?}", paths.len()); // 3450
}

fn part2(lines: &Vec<String>) {
    let (_nodes, _edges, successors) = read_graph(lines);
    let paths = dfs_paths(successors, "start", "end", get_neighbors2);
    println!("Result2 = {:?}", paths.len()); // 96528 (takes a long time)
}

pub fn main() -> Result<(), Box<dyn std::error::Error>> {
    let lines = read_lines("input12.txt")?;
    // let chunks = read_chunks("input.txt")?;

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
