use keyed_priority_queue::{Entry, KeyedPriorityQueue};
use regex::Regex;
use std::cmp::Reverse;
use std::collections::HashMap;
use std::collections::HashSet;
use std::fs;
use std::fs::File;
use std::hash::Hash;
use std::io::{self, BufRead, Error};
use std::path::Path;

/*
#############
#01x3x5x7x90# Row 0: Columns 0..10
###1#1#1#1### Row 1: Columns 2, 4, 6, 8
  #2#2#2#2#   Row 2: Columns 2, 4, 6, 8
  #3#3#3#3#   Row 3: Columns 2, 4, 6, 8
  #4#4#4#4#   Row 4: Columns 2, 4, 6, 8
  #########
   A B C D    In part 1, have Rows [0..2].  In Part 2 Rows [0..4]
*/

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
    for line in lines.flatten() {
        result.push(line)
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
    // Heuristic to estimate distance between the current and goal
    fn heuristic(&self, _current: &T, _goal: &T) -> i64 {
        0
    }
    // Return a pairs of (cost, NodeId) to each neighbor
    fn get_neighbors(&self, _current: &T) -> Vec<(i64, T)> {
        vec![]
    }

    // Have we reached the goal?
    fn is_goal(&self, _current: &T, _goal: &T) -> bool {
        false
    }
}

/// //en.wikipedia.org/wiki/Dijkstra%27s_algorithm#Practical_optimizations_and_infinite_graphs
/// //www.baeldung.com/cs/find-path-uniform-cost-search
/// Returns the path and total cost
fn uniform_cost_search<T: Ord + Hash + Copy + std::fmt::Debug>(
    graph: &dyn Graph<T>,
    start: &T,
    goal: &T,
) -> Option<(Vec<T>, i64)> {
    // The set of discovered nodes that may need to be (re-)expanded.
    // Initially, only the start node is known.
    let mut frontier: KeyedPriorityQueue<T, Reverse<i64>> = KeyedPriorityQueue::new();
    // Visited nodes
    let mut explored: HashSet<T> = HashSet::new();
    // For node n, cameFrom[n] is the node immediately preceding it
    // on the cheapest path from start to n currently known.
    let mut parent: HashMap<T, T> = HashMap::new();

    frontier.push(*start, Reverse(0));

    while !frontier.is_empty() {
        let (current, Reverse(current_cost)) = frontier.pop().unwrap();

        if graph.is_goal(&current, goal) {
            // Solution. Return chain of nodes start->goal with total cost.
            let mut node = &current;
            let mut result = vec![*node];
            let total_cost = current_cost;
            while node != start {
                node = parent.get(node).unwrap();
                result.push(*node);
            }
            result.reverse();
            return Some((result, total_cost));
        }

        explored.insert(current);

        let neighbors = graph.get_neighbors(&current);
        for neighbor in neighbors.iter().filter(|x| !explored.contains(&x.1)) {
            let cost = current_cost + neighbor.0;

            match frontier.entry(neighbor.1) {
                Entry::Vacant(entry) => {
                    // Add new node to queue. I.e. frontier.push(neighbor.1, Reverse(cost));
                    entry.set_priority(Reverse(cost));
                    parent.insert(neighbor.1, current);
                }
                Entry::Occupied(entry) if *entry.get_priority() < Reverse(cost) => {
                    // Have found better path to node in queue.  Update with new cost.
                    entry.set_priority(Reverse(cost));
                }
                _ => { /* Have found worse path. */ }
            };
        }
    }
    // Goal not found
    None
}

// BEGIN BOARD
#[derive(Debug, Clone, Copy, PartialEq, Eq, PartialOrd, Ord, Hash)]
struct Position {
    //pos: (piece, (column, row))
    num_rows: usize,                    // Rows in home (not counting "hallway")
    pos: [(usize, (usize, usize)); 16], // Allocate for 4 rows of pieces
}

impl Position {
    const IGNORE: usize = 100;
    const GOAL2: Position = Position {
        num_rows: 2,
        pos: [
            (0, (2, 1)),
            (0, (2, 2)),
            (1, (4, 1)),
            (1, (4, 2)),
            (2, (6, 1)),
            (2, (6, 2)),
            (3, (8, 1)),
            (3, (8, 2)),
            (Position::IGNORE, (Position::IGNORE, Position::IGNORE)),
            (Position::IGNORE, (Position::IGNORE, Position::IGNORE)),
            (Position::IGNORE, (Position::IGNORE, Position::IGNORE)),
            (Position::IGNORE, (Position::IGNORE, Position::IGNORE)),
            (Position::IGNORE, (Position::IGNORE, Position::IGNORE)),
            (Position::IGNORE, (Position::IGNORE, Position::IGNORE)),
            (Position::IGNORE, (Position::IGNORE, Position::IGNORE)),
            (Position::IGNORE, (Position::IGNORE, Position::IGNORE)),
        ],
    };
    const GOAL4: Position = Position {
        num_rows: 4,
        pos: [
            (0, (2, 1)),
            (0, (2, 2)),
            (0, (2, 3)),
            (0, (2, 4)),
            (1, (4, 1)),
            (1, (4, 2)),
            (1, (4, 3)),
            (1, (4, 4)),
            (2, (6, 1)),
            (2, (6, 2)),
            (2, (6, 3)),
            (2, (6, 4)),
            (3, (8, 1)),
            (3, (8, 2)),
            (3, (8, 3)),
            (3, (8, 4)),
        ],
    };
    fn new(num_rows: usize) -> Position {
        Position {
            num_rows,
            pos: [(Position::IGNORE, (Position::IGNORE, Position::IGNORE)); 16],
        }
    }
    /// Return manhattan distance * piece value (A=1, B=10, C=100, D=1000)
    fn cost(piece: usize, from: &(usize, usize), to: &(usize, usize)) -> i64 {
        let value: [i64; 4] = [1, 10, 100, 1000];
        value[piece] * ((to.0 as i64 - from.0 as i64).abs() + (to.1 as i64 - from.1 as i64).abs())
    }
    fn at(&self, position: &(usize, usize)) -> Option<usize> {
        self.pos.iter().find(|&x| x.1 == *position).map(|p| p.0)
    }
    fn as_char(&self, position: &(usize, usize)) -> char {
        if let Some(p) = self.at(position) {
            // if let Some(p) = self.pos.iter().find(|&x| x.1 == *position) {
            (p as u8 + b'A') as char
        } else {
            '.'
        }
    }
}

/*
     Columns: 0..10
    #############
    #01234567890# Row 0
    ###2#2#2#2### Row 1
      #2#2#2#2#   Row 2
      #########
       A B C D: Home columns: 2, 4, 6, 8
*/

struct Board {}

impl Board {
    // Return home column for a piece.
    fn home(piece: usize) -> usize {
        match piece {
            0 => 2, // A
            1 => 4, // B
            2 => 6, // C
            3 => 8, // D
            _ => panic!(),
        }
    }

    fn new() -> Board {
        Board {}
    }

    fn read_data(lines: &[&str], num_rows: usize) -> (Board, Position) {
        let board: Board = Board::new();
        let mut position: Position = Position::new(num_rows);

        // Read lines 2 and 3 (the home positions)
        let mut index = 0;
        for (input_row, item) in lines.iter().enumerate().skip(2).take(2) {
            item.split([' ', '#'].as_ref())
                .filter(|&x| !x.is_empty())
                .map(|x| x.chars().next().unwrap())
                .map(|x| x as usize - 'A' as usize)
                .enumerate()
                .for_each(|(i, x)| {
                    let board_row = input_row - 1; // Home rows: 1,2..
                    position.pos[index] = (x, (Board::home(i), board_row));
                    index += 1;
                });
        }
        if position.num_rows == 4 {
            // Add the 2 missing rows
            //    "  #D#C#B#A#",
            //    "  #D#B#A#C#",
            position.pos.copy_within(4..8, 12);
            position.pos[4] = (3, (2, 2));
            position.pos[5] = (2, (4, 2));
            position.pos[6] = (1, (6, 2));
            position.pos[7] = (0, (8, 2));
            position.pos[8] = (3, (2, 3));
            position.pos[9] = (1, (4, 3));
            position.pos[10] = (0, (6, 3));
            position.pos[11] = (2, (8, 3));
            position.pos[12].1 .1 = 4;
            position.pos[13].1 .1 = 4;
            position.pos[14].1 .1 = 4;
            position.pos[15].1 .1 = 4;
        }

        position.pos.sort_unstable();
        (board, position)
    }
    fn print_board(position: &Position) {
        println!("\n#############"); // Top row
        print!("#"); // Hallway
        for i in 0..=10 {
            print!("{}", position.as_char(&(i, 0)));
        }
        println!("#");

        print!("###"); // First Home
        for i in 0..=3 {
            // println!("'{:?}'", (3 + 2 * i, 1));
            print!("{}#", position.as_char(&(2 + 2 * i, 1)));
        }
        println!("##");

        for row in 2..=position.num_rows {
            print!("  #");
            for i in 0..=3 {
                print!("{}#", position.as_char(&(2 + 2 * i, row)));
            }
            println!();
        }
        println!("  #########");
    }
}
impl Graph<Position> for Board {
    // Return a pairs of (cost, NodeId) to each neighbor
    fn get_neighbors(&self, current: &Position) -> Vec<(i64, Position)> {
        let mut result: Vec<(i64, Position)> = vec![];

        for (index, p) in current.pos.iter().enumerate() {
            let &(piece, (piece_col, piece_row)) = p;
            if piece == Position::IGNORE {
                continue;
            }
            if piece_row == 0 {
                // Row 0: In hallway.  Frozen.  Can only move into home.
                // Home must be empty or occupied by same piece in row 2.
                // Path to home must be clear.
                let home_col = Board::home(piece);

                // If any spot is occupied by another color, cannot move there.
                let home_occupied = (1..=current.num_rows).any(|x| {
                    let v = current.at(&(home_col, x));
                    v != None && v != Some(piece)
                });
                if home_occupied {
                    continue;
                }

                // First empty spot, counting from the bottom
                let home_empty_slot = (1..=current.num_rows)
                    .rev()
                    .find(|x| current.at(&(home_col, *x)) == None);

                if home_empty_slot == None {
                    panic!("Should not get here");
                }

                let goal: (usize, usize) = (Board::home(piece), home_empty_slot.unwrap());

                let path_clear = if piece_col < home_col {
                    (piece_col + 1..home_col).all(|x| current.at(&(x, 0)) == None)
                } else {
                    (home_col..piece_col).all(|x| current.at(&(x, 0)) == None)
                };
                if path_clear {
                    // Add...
                    let mut neighbor = *current; // Make a copy
                    neighbor.pos[index].1 = goal;
                    let cost = Position::cost(piece, &current.pos[index].1, &neighbor.pos[index].1);
                    neighbor.pos.sort_unstable();
                    result.push((cost, neighbor));
                }
            } else {
                // piece_row != 0 so piece_col == 2/4/6/8 and row is  1/2{/3/4} - the home rows
                let blocked =
                    piece_row != 1 && (1..piece_row).any(|x| current.at(&(piece_col, x)) != None);
                if blocked {
                    // There is a piece above
                    continue;
                }
                if piece_row == current.num_rows && piece_col == Board::home(piece) {
                    // At home in bottom-most row.
                    continue;
                }
                if piece_col == Board::home(piece)
                    && (piece_row..=current.num_rows)
                        .all(|x| current.at(&(piece_col, x)) == Some(piece))
                {
                    // At home and pieces below are also of same color
                    continue;
                }

                // Can move into the hallway left or right
                // Left
                let mut goal = piece_col as isize;
                while goal >= 0 {
                    if goal == 2 || goal == 4 || goal == 6 || goal == 8 {
                        goal -= 1;
                    }
                    let ugoal = goal as usize;
                    if current.at(&(ugoal, 0)) != None {
                        break;
                    }
                    // Add...
                    let mut neighbor = *current; // Make a copy
                    neighbor.pos[index].1 = (ugoal, 0);
                    let cost = Position::cost(piece, &current.pos[index].1, &neighbor.pos[index].1);
                    neighbor.pos.sort_unstable();
                    result.push((cost, neighbor));
                    goal -= 1;
                }
                // Right
                let mut goal = piece_col as isize;
                while goal <= 10 {
                    if goal == 2 || goal == 4 || goal == 6 || goal == 8 {
                        goal += 1;
                    }
                    let ugoal = goal as usize;
                    if current.at(&(ugoal, 0)) != None {
                        break;
                    }
                    // Add...
                    let mut neighbor = *current; // Make a copy
                    neighbor.pos[index].1 = (ugoal, 0);
                    let cost = Position::cost(piece, &current.pos[index].1, &neighbor.pos[index].1);
                    neighbor.pos.sort_unstable();
                    result.push((cost, neighbor));
                    goal += 1;
                }
            }
        }

        result
    }

    // Have we reached the goal?
    fn is_goal(&self, current: &Position, goal: &Position) -> bool {
        // We sort the position, so we can do a simple comparison
        current == goal
    }
}
// END BOARD
fn part1(lines: &[String]) {
    // https://stackoverflow.com/questions/33216514/how-do-i-convert-a-vecstring-to-vecstr
    let lines: Vec<&str> = lines.iter().map(AsRef::as_ref).collect();
    let (board, position) = Board::read_data(&lines, 2);
    Board::print_board(&position);
    let solution = uniform_cost_search(&board, &position, &Position::GOAL2).unwrap();
    let result1 = solution.1;
    println!("Result1 = {}", result1); // 14148
}

fn part2<S>(lines: &[S])
where
    S: AsRef<str>,
{
    let lines: Vec<&str> = lines.iter().map(AsRef::as_ref).collect();
    let (board, position) = Board::read_data(&lines, 4);
    Board::print_board(&position);
    let solution = uniform_cost_search(&board, &position, &Position::GOAL4).unwrap();
    let result2 = solution.1;
    println!("Result2 = {}", result2); // 43814
}

pub fn main() -> Result<(), Box<dyn std::error::Error>> {
    let args: Vec<String> = std::env::args().collect();
    let file_name = if args.len() > 1 {
        &args[1]
    } else {
        "input23.txt"
    };

    let lines = match read_lines(file_name) {
        Ok(v) => v,
        Err(e) => {
            println!("Error reading file: '{}' error: '{:?}'", file_name, e);
            std::panic::set_hook(Box::new(|_info| {
                // do nothing
            }));
            panic!();
        }
    };
    //let chunks = read_chunks(file_name)?;

    part1(&lines);
    part2(&lines);

    Ok(())
}

#[cfg(test)]
mod tests {
    use super::*;
    //use std::path::PathBuf;

    #[test]
    fn test_heap() {
        use keyed_priority_queue::KeyedPriorityQueue;
        let mut frontier: KeyedPriorityQueue<&str, Reverse<i64>> = KeyedPriorityQueue::new();
        frontier.push("hello", Reverse(9));
        frontier.push("world", Reverse(3));
        frontier.push("hello", Reverse(2));
        while !frontier.is_empty() {
            let (val, Reverse(priority)) = frontier.pop().unwrap();
            println!("Popped '{:?}' priority={:?}", val, priority);
        }
    }
    #[test]
    fn test_search() {
        // Sample from https://doc.rust-lang.org/std/collections/binary_heap/index.html#examples
        // This is the directed graph we're going to use.
        // The node numbers correspond to the different states,
        // and the edge weights symbolize the cost of moving
        // from one node to another.
        // Note that the edges are one-way.
        //
        //                  7
        //          +-----------------+
        //          |                 |
        //          v   1        2    |  2
        //          0 -----> 1 -----> 3 ---> 4
        //          |        ^        ^      ^
        //          |        | 1      |      |
        //          |        |        | 3    | 1
        //          +------> 2 -------+      |
        //           10      |               |
        //                   +---------------+
        //
        // The graph is represented as an adjacency list where each index,
        // corresponding to a node value, has a list of outgoing edges.
        // Chosen for its efficiency.
        struct Edge {
            node: usize,
            cost: usize,
        }
        struct TestGraph {
            graph: Vec<Vec<Edge>>,
        }
        impl Graph<usize> for TestGraph {
            fn heuristic(&self, _current: &usize, _goal: &usize) -> i64 {
                0
            }
            // Return a pairs of (cost, NodeId) to each neighbor
            fn get_neighbors(&self, current: &usize) -> Vec<(i64, usize)> {
                self.graph[*current]
                    .iter()
                    .map(|x| (x.cost as i64, x.node))
                    .collect()
            }

            // Have we reached the goal?
            fn is_goal(&self, current: &usize, goal: &usize) -> bool {
                current == goal
            }
        }

        let graph = TestGraph {
            graph: vec![
                // Node 0
                vec![Edge { node: 2, cost: 10 }, Edge { node: 1, cost: 1 }],
                // Node 1
                vec![Edge { node: 3, cost: 2 }],
                // Node 2
                vec![
                    Edge { node: 1, cost: 1 },
                    Edge { node: 3, cost: 3 },
                    Edge { node: 4, cost: 1 },
                ],
                // Node 3
                vec![Edge { node: 0, cost: 7 }, Edge { node: 4, cost: 2 }],
                // Node 4
                vec![],
            ],
        };
        let result = uniform_cost_search::<usize>(&graph, &0, &4);
        println!("result={:?}", result);
        let result = uniform_cost_search(&graph, &0, &1).unwrap();
        assert_eq!(result.1, 1);
        let result = uniform_cost_search(&graph, &0, &3).unwrap();
        assert_eq!(result.1, 3);
        let result = uniform_cost_search(&graph, &3, &0).unwrap();
        assert_eq!(result.1, 7);
        let result = uniform_cost_search(&graph, &0, &4).unwrap();
        assert_eq!(result.1, 5);
        assert_eq!(uniform_cost_search(&graph, &4, &0), None);
    }
    #[test]
    fn part1_works() {
        let input = vec![
            "#############",
            "#...........#",
            "###B#C#B#D###",
            "  #A#D#C#A#",
            "  #########",
        ];

        let (board, position) = Board::read_data(&input, 2);
        Board::print_board(&position);
        let solution = uniform_cost_search(&board, &position, &Position::GOAL2);
        // println!("Solution = {:?}", solution);
        assert_eq!(solution.unwrap().1, 12521);
    }

    #[test]
    fn part2_works() {
        let input = vec![
            "#############",
            "#...........#",
            "###B#C#B#D###",
            "  #A#D#C#A#",
            "  #########",
        ];
        //    "  #D#C#B#A#",
        //    "  #D#B#A#C#",
        // Get inserted between the two rows

        let (board, position) = Board::read_data(&input, 4);
        Board::print_board(&position);
        let solution = uniform_cost_search(&board, &position, &Position::GOAL4);
        // println!("Solution = {:?}", solution);
        assert_eq!(solution.unwrap().1, 44169);
    }
}
