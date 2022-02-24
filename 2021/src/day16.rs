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

#[derive(Debug)]
struct Node {
    version: usize,
    typ: usize,
    data: usize,
    children: Vec<Node>,
}

struct Bits {
    bytes: Vec<u8>,
    byte_offset: usize,
    bit_offset: usize,
}
impl Bits {
    fn from_string(line: &str) -> Bits {
        let mut bytes: Vec<u8> = Vec::new();
        for i in 0..line.len() {
            let val = u8::from_str_radix(&line[i..i + 1], 16).unwrap();
            bytes.push(val);
        }
        Bits {
            bytes: bytes,
            byte_offset: 0,
            bit_offset: 3,
        }
    }
    fn read1(&mut self) -> Option<usize> {
        if self.byte_offset < self.bytes.len() {
            let val = if (self.bytes[self.byte_offset] & (1 << self.bit_offset)) != 0 {
                1
            } else {
                0
            };
            if self.bit_offset == 0 {
                self.bit_offset = 3;
                self.byte_offset += 1;
            } else {
                self.bit_offset -= 1;
            }
            return Some(val);
        } else {
            return None;
        }
    }
    fn read3(&mut self) -> Option<usize> {
        if let (Some(b0), Some(b1), Some(b2)) = (self.read1(), self.read1(), self.read1()) {
            return Some(b0 << 2 | b1 << 1 | b2);
        } else {
            return None;
        }
    }
    fn read5(&mut self) -> Option<usize> {
        if let (Some(b0), Some(b1), Some(b2), Some(b3), Some(b4)) = (
            self.read1(),
            self.read1(),
            self.read1(),
            self.read1(),
            self.read1(),
        ) {
            return Some(b0 << 4 | b1 << 3 | b2 << 2 | b3 << 1 | b4);
        } else {
            return None;
        }
    }
    fn read_n(&mut self, nbits: usize) -> Option<usize> {
        let mut result = 0;
        for _ in 0..nbits {
            if let Some(n) = self.read1() {
                result = result << 1 | n;
            } else {
                return None;
            }
        }
        Some(result)
    }

    fn read_int(&mut self) -> Option<usize> {
        let mut done = false;
        let mut result: usize = 0;
        while !done {
            if let Some(word) = self.read5() {
                if word & (1 << 4) == 0 {
                    done = true;
                }
                result = (result << 4) | (word & 0xF);
            } else {
                return None;
            }
        }
        Some(result)
    }

    fn count_bits_from(&self, byte_offset: usize, bit_offset: usize) -> usize {
        let mut result = (self.byte_offset - byte_offset) * 4;
        result += 3 - self.bit_offset;
        result -= 3 - bit_offset;
        result
    }
}

fn sum_versions(node: &Node) -> usize {
    let mut result = node.version;
    for c in node.children.iter() {
        result += sum_versions(c);
    }
    return result;
}

fn parse(bits: &mut Bits) -> Option<Node> {
    if let (Some(version), Some(typ)) = (bits.read3(), bits.read3()) {
        let mut node = Node {
            version: version,
            typ: typ,
            data: 0,
            children: vec![],
        };
        if typ == 4 {
            node.data = bits.read_int().unwrap();
            return Some(node);
        } else {
            let type_len_i = bits.read1().unwrap();
            if type_len_i == 0 {
                // len is 15 bits: bit count
                let total_bits = bits.read_n(15).unwrap();
                let (byte_offset, bit_offset) = (bits.byte_offset, bits.bit_offset);
                while bits.count_bits_from(byte_offset, bit_offset) < total_bits {
                    if let Some(child) = parse(bits) {
                        node.children.push(child);
                    }
                }
            } else {
                // len is 11 bits: packet count
                let total_packets = bits.read_n(11).unwrap();
                for _ in 0..total_packets {
                    if let Some(child) = parse(bits) {
                        node.children.push(child);
                    }
                }
            }
            return Some(node);
        }
    } else {
        return None;
    }
}
fn evaluate(node: &Node) -> usize {
    let result = match node.typ {
        4 => node.data,
        0 => node.children.iter().map(|x| evaluate(x)).sum(),
        1 => node.children.iter().map(|x| evaluate(x)).product(),
        2 => node.children.iter().map(|x| evaluate(x)).min().unwrap(),
        3 => node.children.iter().map(|x| evaluate(x)).max().unwrap(),
        5 => {
            if evaluate(&node.children[0]) > evaluate(&node.children[1]) {
                1
            } else {
                0
            }
        }
        6 => {
            if evaluate(&node.children[0]) < evaluate(&node.children[1]) {
                1
            } else {
                0
            }
        }
        7 => {
            if evaluate(&node.children[0]) == evaluate(&node.children[1]) {
                1
            } else {
                0
            }
        }
        _ => 0,
    };
    result
}

fn part1(lines: &Vec<String>) {
    let mut bits = Bits::from_string(&lines[0]);
    let node = parse(&mut bits).unwrap();
    let result = sum_versions(&node);
    println!("Result1 = {}", result); // 886
}

fn part2(lines: &Vec<String>) {
    let mut bits = Bits::from_string(&lines[0]);
    let node = parse(&mut bits).unwrap();
    let result = evaluate(&node);

    println!("Result2 = {}", result); // 184487454837
}

pub fn main() -> Result<(), Box<dyn std::error::Error>> {
    let lines = read_lines("input16.txt")?;
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
        let mut bits = Bits::from_string("D2FE28");
        let v = bits.read3().unwrap();
        let t = bits.read3().unwrap();
        println!("v={}, t={}", v, t);
        //let mut bits = Bits::from_string("8A004A801A8002F478");
        //let node = parse(&mut bits).unwrap();
        //println!("node = {:#?} sum={}", node, sum_versions(&node));
        //let mut bits = Bits::from_string("A0016C880162017C3686B18A3D4780");
        //let node = parse(&mut bits).unwrap();
        //println!("sum={}", sum_versions(&node));
    }
    #[test]
    fn part2_works() {
        let mut bits = Bits::from_string("C200B40A82");
        bits.read5();
        let (byte_offset, bit_offset) = (bits.byte_offset, bits.bit_offset);
        for i in 0..8 {
            println!(
                " {} off from 1:  {}",
                i,
                bits.count_bits_from(byte_offset, bit_offset),
            );
            bits.read1();
        }
        let mut bits = Bits::from_string("C200B40A82");
        let node = parse(&mut bits).unwrap();
        println!("node = {:?}", node);
        let result = evaluate(&node);
        println!(" eval : {}", result);
    }
}
