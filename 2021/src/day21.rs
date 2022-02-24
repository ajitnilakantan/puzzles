use regex::Regex;
use std::collections::HashMap;
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

fn read_data(lines: &Vec<String>) -> (usize, usize) {
    let player1_pos: usize = lines[0][28..].to_string().parse().unwrap();
    let player2_pos: usize = lines[1][28..].to_string().parse().unwrap();
    (player1_pos, player2_pos)
}

fn simulate_dice(player1_pos: usize, player2_pos: usize) -> usize {
    let (mut player1_pos, mut player2_pos) = (player1_pos, player2_pos);
    let (mut player1_score, mut player2_score) = (0, 0);
    let mut dice_val: usize = 1;
    let mut roll_count = 0;
    loop {
        player1_pos += ((dice_val - 1) % 100)
            + 1
            + ((dice_val + 1 - 1) % 100)
            + 1
            + ((dice_val + 2 - 1) % 100)
            + 1;
        player1_pos = (player1_pos - 1) % 10 + 1;
        player1_score += player1_pos;
        dice_val = (dice_val + 3 - 1) % 100 + 1;
        roll_count += 3;
        if player1_score >= 1000 {
            break;
        }
        player2_pos += ((dice_val - 1) % 100)
            + 1
            + ((dice_val + 1 - 1) % 100)
            + 1
            + ((dice_val + 2 - 1) % 100)
            + 1;
        player2_pos = (player2_pos - 1) % 10 + 1;
        player2_score += player2_pos;
        dice_val = (dice_val + 3 - 1) % 100 + 1;
        roll_count += 3;
        if player2_score >= 1000 {
            break;
        }
    }
    println!(
        "player1 pos = {} score={}  player2 pos = {} score = {}",
        player1_pos, player1_score, player2_pos, player2_score,
    );
    let result1 = roll_count
        * if player1_score >= 1000 {
            player2_score
        } else {
            player1_score
        };
    result1
}

#[derive(Copy, Clone, PartialEq, Eq, Hash, Debug)]
enum Turn {
    Player1,
    Player2,
}

#[derive(Copy, Clone, Debug)]
struct Wins(usize, usize);

impl std::ops::Add<Wins> for Wins {
    type Output = Wins;

    #[inline(always)]
    fn add(self, rhs: Wins) -> Wins {
        Wins(self.0 + rhs.0, self.1 + rhs.1)
    }
}

impl std::ops::Mul<usize> for Wins {
    type Output = Wins;

    #[inline(always)]
    fn mul(self, rhs: usize) -> Wins {
        Wins(self.0 * rhs, self.1 * rhs)
    }
}

impl std::ops::Mul<Wins> for usize {
    type Output = Wins;

    #[inline(always)]
    fn mul(self, rhs: Wins) -> Wins {
        Wins(self * rhs.0, self * rhs.1)
    }
}

// maps (player1_pos, player1_score, player2_pos, player2_score, Turn) -> (player1_wins, player2_wins)
#[derive(Copy, Clone, Debug, PartialEq, Eq, Hash)]
struct HistoryKey(usize, usize, usize, usize, Turn);

type History = HashMap<HistoryKey, Wins>;

/// Count the number of wins for player1 and player2, returned as tuple
/// Memoize the previous results. Each 3-throw of the die results in rolls of
/// 3..9 with different distributions. Recursively count up the wins for each player.
fn count_wins(
    player1_pos: usize,
    player1_score: usize,
    player2_pos: usize,
    player2_score: usize,
    turn: Turn,
    history: &mut History,
) -> Wins {
    // On each 3 rolls of the die can end up with:
    // 111 211 311  121 221 321  131 231 331  112 212 312  122 222 322  132 232 332  113 213 313  123 223 323  133 233 333
    // = 3, 4, 5,   4, 5, 6,     5, 6, 7,     4, 5, 6,     5, 6, 7,     6, 7, 8,     5, 6, 7,     6, 7, 8,     7, 8, 9
    // = 1*(3)  3*(4)  6*(5)  7*(6)  6*(7)  3*(8)  1*(9)

    let key = HistoryKey(player1_pos, player1_score, player2_pos, player2_score, turn);

    // Check already won
    if player1_score >= 21 {
        let wins = Wins(1, 0);
        history.insert(key, wins);
        return wins;
    }
    if player2_score >= 21 {
        let wins = Wins(0, 1);
        history.insert(key, wins);
        return wins;
    }

    // Check history
    if let Some(wins) = history.get(&key) {
        return *wins;
    }

    fn move_pos(old_pos: usize, amount: usize) -> usize {
        (old_pos + amount - 1) % 10 + 1
    }

    let moves = vec![(1, 3), (3, 4), (6, 5), (7, 6), (6, 7), (3, 8), (1, 9)];
    let mut wins = Wins(0, 0);

    if turn == Turn::Player1 {
        for m in moves {
            let new_pos = move_pos(player1_pos, m.1);
            let new_score = player1_score + new_pos;
            wins = wins
                + m.0
                    * count_wins(
                        new_pos,
                        new_score,
                        player2_pos,
                        player2_score,
                        Turn::Player2,
                        history,
                    );
        }
        history.insert(key, wins);
        return wins;
    } else {
        for m in moves {
            let new_pos = move_pos(player2_pos, m.1);
            let new_score = player2_score + new_pos;
            wins = wins
                + m.0
                    * count_wins(
                        player1_pos,
                        player1_score,
                        new_pos,
                        new_score,
                        Turn::Player1,
                        history,
                    );
        }
        history.insert(key, wins);
        return wins;
    }
}

fn simulate_dirac(player1_pos: usize, player2_pos: usize) -> Wins {
    let mut history = HashMap::new();
    let wins = count_wins(player1_pos, 0, player2_pos, 0, Turn::Player1, &mut history);
    println!(
        "Player1:{} Player2: {}, Wins = {:?}",
        player1_pos, player2_pos, wins
    );
    wins
}

fn part1(lines: &Vec<String>) {
    let (player1_pos, player2_pos) = read_data(lines);
    let result1 = simulate_dice(player1_pos, player2_pos);
    println!("Result1 = {}", result1); // 908595
}

fn part2(lines: &Vec<String>) {
    let (player1_pos, player2_pos) = read_data(lines);
    let Wins(wins1, wins2) = simulate_dirac(player1_pos, player2_pos);
    let result2 = if wins1 > wins2 { wins1 } else { wins2 };

    println!("Result2 = {}", result2); // 91559198282731
}

pub fn main() -> Result<(), Box<dyn std::error::Error>> {
    let lines = read_lines("input21.txt")?;
    //let chunks = read_chunks("input.txt")?;

    part1(&lines);
    part2(&lines);

    Ok(())
}

#[cfg(test)]
mod tests {
    use super::*;
    #[test]
    fn part1_works() {}
    #[test]
    fn part2_works() {
        let a = Wins(5, 6);
        println!("{:#?}", a * 2); // works
        println!("{:#?}", 3 * a); // err
        println!("{:#?}", a + a); // err
        println!("{:#?}", a); // err
    }
}
