use ndarray::Array2;
use regex::Regex;
use std::cmp::Reverse;
use std::collections::BinaryHeap;
use std::collections::HashMap;
use std::collections::HashSet;
use std::fs;
use std::fs::File;
use std::hash::Hash;
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

trait Graph<T> {
    fn heuristic(&self, current: &T, neighbor: &T) -> i64;
    fn dist_between(&self, current: &T, neighbor: &T) -> i64;
    fn get_neighbors(&self, current: &T) -> Vec<T>;
}

fn astar_solve<T: Ord + Hash + Copy>(graph: &dyn Graph<T>, start: &T, goal: &T) -> Vec<T> {
    let mut close_set: HashSet<T> = HashSet::new();

    // For node n, cameFrom[n] is the node immediately preceding it
    // on the cheapest path from start to n currently known.
    let mut came_from: HashMap<T, T> = HashMap::new();

    // Actual movement cost to each position from the start position
    let mut gscore: HashMap<T, i64> = HashMap::from([(*start, 0)]);

    // Estimated movement cost of start to end going via this position
    let mut fscore: HashMap<T, i64> = HashMap::from([(*start, graph.heuristic(start, goal))]);

    // The set of discovered nodes that may need to be (re-)expanded.
    // Initially, only the start node is known.
    let mut open_set: BinaryHeap<Reverse<(i64, T)>> = BinaryHeap::new();
    open_set.push(Reverse((*fscore.get(start).unwrap(), *start)));

    while open_set.len() > 0 {
        let Reverse((_, mut current)) = open_set.pop().unwrap();

        if &current == goal {
            // Found path
            let mut data: Vec<T> = vec![];
            while came_from.contains_key(&current) {
                data.push(current.clone());
                current = *came_from.get(&current).unwrap();
            }
            data.push(start.clone());
            data.reverse();
            return data;
        }

        close_set.insert(current);
        let neighbors = graph.get_neighbors(&current);
        for neighbor in &neighbors {
            if close_set.contains(neighbor) {
                // Ignore the neighbor which is already evaluated.
                continue;
            }

            // The distance from start to a neighbor
            let tentative_g_score: i64 =
                gscore.get(&current).unwrap() + graph.dist_between(&current, &neighbor);

            let mut add_to_open_set = false;
            if !open_set.iter().any(|Reverse(x)| x.1 == *neighbor) {
                // Discover a new node
                add_to_open_set = true;
            } else if tentative_g_score >= gscore[neighbor] {
                // This is not a better path.
                continue;
            }

            // This path is the best until now. Record it!
            came_from.insert(*neighbor, current);
            gscore.insert(*neighbor, tentative_g_score);
            fscore.insert(
                *neighbor,
                tentative_g_score + graph.heuristic(neighbor, goal),
            );
            if add_to_open_set {
                open_set.push(Reverse((fscore[neighbor], *neighbor)));
            }
        }
    }

    return vec![];
}

#[derive(Debug)]
struct Grid {
    grid: Array2<usize>,
}
#[derive(Debug, Copy, Clone, PartialEq, Eq, PartialOrd, Ord, Hash)]
struct Node {
    x: usize,
    y: usize,
}

impl Graph<Node> for Grid {
    fn heuristic(&self, current: &Node, neighbor: &Node) -> i64 {
        // Manhattan distance
        let diff = (current.x as isize - neighbor.x as isize).abs()
            + (current.y as isize - neighbor.y as isize).abs();
        //println!("h({:?}->{:?}) = {}", current, neighbor, diff);
        diff as i64
    }
    fn dist_between(&self, _current: &Node, neighbor: &Node) -> i64 {
        let dist = self.grid[[neighbor.y, neighbor.x]] as i64;
        //println!("d({:?}->{:?}) = {}", current, neighbor, dist);
        return dist;
    }
    fn get_neighbors(&self, current: &Node) -> Vec<Node> {
        let (height, width) = self.grid.dim();
        let mut neighbors: Vec<Node> = Vec::new();
        let directions = vec![(0, -1), (0, 1), (-1, 0), (1, 0)];
        for d in directions.iter() {
            let (x, y) = (current.x as isize + d.0, current.y as isize + d.1);
            if x >= 0 && x < width as isize && y >= 0 && y < height as isize {
                neighbors.push(Node {
                    x: x as usize,
                    y: y as usize,
                });
            }
        }
        //println!("n({:?})={:?}", current, neighbors);
        return neighbors;
    }
}

fn create_grid(lines: &Vec<String>) -> Grid {
    // Create grid
    let width = lines[0].len();
    let height = lines.len();
    // println!("w = {} h = {}", width, height);
    // Grid addressed [y, x]
    let mut grid = Array2::<usize>::zeros((height, width));

    // Fill grid
    for j in 0..height {
        let chars: Vec<char> = lines[j].chars().collect();
        for i in 0..width {
            let val = chars[i] as usize - '0' as usize;
            grid[[j, i]] = val;
        }
    }
    // println!("grid = {:#?}", grid);
    Grid { grid: grid }
}

fn expand_grid(grid: &Grid) -> Grid {
    // Expand the grid to 5 times dimensions, increasing weights
    let (height, width) = grid.grid.dim();
    let mut newgrid = Array2::<usize>::zeros((5 * height, 5 * width));
    for j in 0..height {
        for i in 0..width {
            newgrid[[j, i]] = grid.grid[[j, i]];
        }
    }
    for ny in 0..5 {
        for nx in 0..5 {
            if nx == 0 && ny == 0 {
                continue;
            }
            for j in 0..height {
                for i in 0..width {
                    newgrid[[height * ny + j, width * nx + i]] =
                        (grid.grid[[j, i]] + nx + ny - 1) % 9 + 1;
                }
            }
        }
    }
    Grid { grid: newgrid }
}

fn score_path(grid: &Grid, path: &Vec<Node>) -> usize {
    let mut result = path.iter().map(|x| grid.grid[[x.y, x.x]]).sum::<usize>();
    result -= grid.grid[[path[0].y, path[0].x]]; // Ignore start node
    result
}

fn part1(lines: &Vec<String>) {
    let grid = create_grid(lines);
    let (height, width) = grid.grid.dim();
    println!("grid = {:?}\n dim='{:?}'", grid, grid.grid.dim());
    let start = Node { x: 0, y: 0 };
    let goal = Node {
        x: width - 1,
        y: height - 1,
    };
    let path = astar_solve::<Node>(&grid, &start, &goal);
    let result = score_path(&grid, &path);
    // println!("path = {:?} score={}", path, result);
    println!("Result1 = {}", result); // 652
}

fn part2(lines: &Vec<String>) {
    let grid = create_grid(lines);
    let grid = expand_grid(&grid);
    println!("grid = {:?}\n dim='{:?}'", grid, grid.grid.dim());
    let (height, width) = grid.grid.dim();
    let start = Node { x: 0, y: 0 };
    let goal = Node {
        x: width - 1,
        y: height - 1,
    };
    let path = astar_solve::<Node>(&grid, &start, &goal);
    let result = score_path(&grid, &path);
    println!("Result2 = {}", result); // 2938 (takes a long time)
}

pub fn main() -> Result<(), Box<dyn std::error::Error>> {
    let lines = read_lines("input15.txt")?;
    // let chunks = read_chunks("input.txt")?;

    part1(&lines);
    part2(&lines);
    Ok(())
}

#[cfg(test)]
mod tests {
    use super::*;
    #[test]
    fn it_works() {
        let mut open_set: BinaryHeap<Reverse<(i64, Node)>> = BinaryHeap::new(); // (i64, T)
        open_set.push(Reverse((10, Node { x: 1, y: 1 })));
        open_set.push(Reverse((5, Node { x: 1, y: 1 })));
        open_set.push(Reverse((20, Node { x: 1, y: 1 })));
        let Reverse(x) = open_set.pop().unwrap();
        println!("{:#?}", x);
        println!("{:?}", open_set.pop());
        println!("{:?}", open_set.pop());
        let lines: Vec<String> = vec!["8".to_string()];
        let grid = create_grid(&lines);
        println!("Grid =\n{:?}", grid);
        let grid = expand_grid(&grid);
        println!("Grid =\n{:?}", grid);
    }
}
