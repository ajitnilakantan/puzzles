use itertools::iproduct;
use regex::Regex;
use std::collections::BTreeSet;
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

#[derive(Copy, Clone, PartialEq, Eq, Hash, Debug)]
enum ChangeState {
    On,
    Off,
}

#[derive(Copy, Clone, PartialEq, Eq, Hash, Debug)]
struct BBox {
    // Coordinates are inclusive
    xmin: isize,
    xmax: isize,
    ymin: isize,
    ymax: isize,
    zmin: isize,
    zmax: isize,
}

impl BBox {
    fn limit_box(self: &BBox) -> Option<BBox> {
        if self.xmin < -50 && self.xmax < -50
            || self.xmin > 50 && self.xmax > 50
            || self.ymin < -50 && self.ymax < -50
            || self.ymin > 50 && self.ymax > 50
            || self.zmin < -50 && self.zmax < -50
            || self.zmin > 50 && self.zmax > 50
        {
            return None;
        }
        let mut bbox = *self;
        bbox.xmin = self.xmin.clamp(-50, 50);
        bbox.xmax = self.xmax.clamp(-50, 50);
        bbox.ymin = self.ymin.clamp(-50, 50);
        bbox.ymax = self.ymax.clamp(-50, 50);
        bbox.zmin = self.zmin.clamp(-50, 50);
        bbox.zmax = self.zmax.clamp(-50, 50);

        Some(bbox)
    }

    fn intersects(self: &BBox, bbox: &BBox) -> bool {
        if self.xmax < bbox.xmin
            || bbox.xmax < self.xmin
            || self.ymax < bbox.ymin
            || bbox.ymax < self.ymin
            || self.zmax < bbox.zmin
            || bbox.zmax < self.zmin
        {
            return false;
        }
        true
    }

    #[allow(dead_code)]
    fn contains(self: &BBox, bbox: &BBox) -> bool {
        if bbox.xmin >= self.xmin
            && bbox.xmin <= self.xmax
            && bbox.xmax >= self.xmin
            && bbox.xmax <= self.xmax
            && bbox.ymin >= self.ymin
            && bbox.ymin <= self.ymax
            && bbox.ymax >= self.ymin
            && bbox.ymax <= self.ymax
            && bbox.zmin >= self.zmin
            && bbox.zmin <= self.zmax
            && bbox.zmax >= self.zmin
            && bbox.zmax <= self.zmax
        {
            return true;
        }
        false
    }
}

fn read_data(lines: &Vec<String>) -> Vec<(ChangeState, BBox)> {
    let mut result: Vec<(ChangeState, BBox)> = Vec::new();
    for line in lines {
        let numbers: &str;
        let op: ChangeState;
        if &line[0..2] == "on" {
            op = ChangeState::On;
            numbers = &line[3..];
        } else {
            op = ChangeState::Off;
            numbers = &line[4..];
        }
        let numbers: Vec<isize> = numbers
            .split(['x', 'y', 'z', '=', '.', ','].as_ref())
            .filter(|&x| !x.is_empty())
            .map(|x| x.to_string().parse::<isize>().unwrap())
            .collect();
        let op = (
            op,
            BBox {
                xmin: numbers[0],
                xmax: numbers[1],
                ymin: numbers[2],
                ymax: numbers[3],
                zmin: numbers[4],
                zmax: numbers[5],
            },
        );
        result.push(op);
    }
    result
}

/// Simple approach, uses a hashset to keep track of on/off cubes.
fn process_commands(commands: &Vec<(ChangeState, BBox)>) -> usize {
    fn create_grid(bbox: &BBox) -> HashSet<(isize, isize, isize)> {
        let mut grid: HashSet<(isize, isize, isize)> = HashSet::new();
        for x in bbox.xmin..=bbox.xmax {
            for y in bbox.ymin..=bbox.ymax {
                for z in bbox.zmin..=bbox.zmax {
                    grid.insert((x, y, z));
                }
            }
        }
        grid
    }
    let mut grid: HashSet<(isize, isize, isize)> = HashSet::new();
    for command in commands.iter() {
        if let Some(bbox) = command.1.limit_box() {
            let newgrid = create_grid(&bbox);
            if command.0 == ChangeState::On {
                grid = grid.union(&newgrid).cloned().collect();
            } else {
                grid = grid.difference(&newgrid).cloned().collect();
            }
        }
    }

    grid.len()
}

// Get the number of interior vertices in the prism (i.e. ignore faces, edges, vertices)
fn get_volume_count(index: &BBox) -> usize {
    let volume = (index.xmax - index.xmin - 1)
        * (index.ymax - index.ymin - 1)
        * (index.zmax - index.zmin - 1);
    if index.xmax == index.xmin || index.ymax == index.ymin || index.zmax == index.zmin {
        // Degenerate case
        return 0;
    }
    assert!(volume >= 0);
    let volume = volume as usize;
    volume
}

/// Get the number if interior face vertices (i.e. ignoring edges)
fn get_face_count(index: &(PrismCoord, PrismCoord)) -> usize {
    let mut count: isize = 0;
    if index.0.x == index.1.x {
        if index.1.y == index.0.y || index.1.z == index.0.z {
            // Degenerate case
            return 0;
        }
        count = (index.1.y - index.0.y - 1) * (index.1.z - index.0.z - 1);
    } else if index.0.y == index.1.y {
        if index.1.x == index.0.x || index.1.z == index.0.z {
            // Degenerate case
            return 0;
        }
        count = (index.1.x - index.0.x - 1) * (index.1.z - index.0.z - 1);
    } else if index.0.z == index.1.z {
        if index.1.y == index.0.y || index.1.x == index.0.x {
            // Degenerate case
            return 0;
        }
        count = (index.1.y - index.0.y - 1) * (index.1.x - index.0.x - 1);
    } else {
        assert!(false, "Bad faces");
    }

    assert!(count >= 0);
    let count = count as usize;
    count
}

/// Get the number if interior edge vertices (i.e. ignoring two terminal vertices)
fn get_edge_count(index: &(PrismCoord, PrismCoord)) -> usize {
    let mut count: isize = 0;
    if index.0.x != index.1.x {
        count = index.1.x - index.0.x - 1;
    } else if index.0.y != index.1.y {
        count = index.1.y - index.0.y - 1;
    } else if index.0.z != index.1.z {
        count = index.1.z - index.0.z - 1;
    }

    assert!(count >= 0);
    let count = count as usize;
    count
}

#[derive(Copy, Clone, PartialEq, Eq, Hash, Debug)]
struct PrismCoord {
    x: isize,
    y: isize,
    z: isize,
}

/// Get the 6 faces, 12 edges, and 8 verticies of the rectangular prism
fn get_fev(
    p: &BBox,
    faces: &mut HashSet<(PrismCoord, PrismCoord)>,
    edges: &mut HashSet<(PrismCoord, PrismCoord)>,
    vertices: &mut HashSet<PrismCoord>,
) {
    let p: (PrismCoord, PrismCoord) = (
        PrismCoord {
            x: p.xmin,
            y: p.ymin,
            z: p.zmin,
        },
        PrismCoord {
            x: p.xmax,
            y: p.ymax,
            z: p.zmax,
        },
    );
    // E.g.: https://www.mathworks.com/help/matlab/visualize/multifaceted-patches.html
    let p000 = PrismCoord {
        x: p.0.x,
        y: p.0.y,
        z: p.0.z,
    };
    let p100 = PrismCoord {
        x: p.1.x,
        y: p.0.y,
        z: p.0.z,
    };
    let p010 = PrismCoord {
        x: p.0.x,
        y: p.1.y,
        z: p.0.z,
    };
    let p110 = PrismCoord {
        x: p.1.x,
        y: p.1.y,
        z: p.0.z,
    };
    let p001 = PrismCoord {
        x: p.0.x,
        y: p.0.y,
        z: p.1.z,
    };
    let p101 = PrismCoord {
        x: p.1.x,
        y: p.0.y,
        z: p.1.z,
    };
    let p011 = PrismCoord {
        x: p.0.x,
        y: p.1.y,
        z: p.1.z,
    };
    let p111 = PrismCoord {
        x: p.1.x,
        y: p.1.y,
        z: p.1.z,
    };
    faces.extend(vec![
        (p000, p101),
        (p000, p011),
        (p000, p110),
        (p010, p111),
        (p100, p111),
        (p001, p111),
    ]);
    edges.extend(vec![
        (p000, p100),
        (p001, p101),
        (p010, p110),
        (p011, p111),
        (p000, p010),
        (p100, p110),
        (p001, p011),
        (p101, p111),
        (p000, p001),
        (p100, p101),
        (p010, p011),
        (p110, p111),
    ]);
    vertices.extend(vec![p000, p100, p010, p110, p001, p101, p011, p111]);
}

fn create_prisms(
    commands: &Vec<(ChangeState, BBox)>,
    coords: &Vec<(BTreeSet<isize>, BTreeSet<isize>, BTreeSet<isize>)>,
) -> HashMap<usize, Vec<BBox>> {
    let mut prisms: HashMap<usize, Vec<BBox>> = HashMap::new();

    for (index, _command) in commands.iter().enumerate() {
        let mut x_coords: Vec<isize> = coords[index].0.iter().copied().collect();
        let mut y_coords: Vec<isize> = coords[index].1.iter().copied().collect();
        let mut z_coords: Vec<isize> = coords[index].2.iter().copied().collect();
        // Handle degenerate vertex counts
        if x_coords.len() == 1 {
            x_coords.push(x_coords[0]);
        }
        if y_coords.len() == 1 {
            y_coords.push(y_coords[0]);
        }
        if z_coords.len() == 1 {
            z_coords.push(z_coords[0]);
        }

        for p in iproduct!(
            0..x_coords.len() - 1,
            0..y_coords.len() - 1,
            0..z_coords.len() - 1
        ) {
            let map = prisms.entry(index).or_insert(Vec::<BBox>::new());
            map.push(BBox {
                xmin: x_coords[p.0],
                ymin: y_coords[p.1],
                zmin: z_coords[p.2],
                xmax: x_coords[p.0 + 1],
                ymax: y_coords[p.1 + 1],
                zmax: z_coords[p.2 + 1],
            });
        }
    }
    prisms
}

/// More complex implementation for part2.  Slice and dice the rectangular prisms into
/// a union of non overlapping cuboids.  The boundaries are along the planes defined
/// by the x/y/z coordinates of each prism.  Keep track of the "cut" prisms which comprise
/// each original prism. Some may belong to more than one in areas where there is overlap.
fn split_prisms(commands: &Vec<(ChangeState, BBox)>) -> usize {
    // Initialize the comprising prism coordinates for each box with the box itself.
    let mut coords: Vec<(BTreeSet<isize>, BTreeSet<isize>, BTreeSet<isize>)> = Vec::new();
    for i in 0..commands.len() {
        let bbox = commands[i].1;
        coords.push((
            BTreeSet::from([bbox.xmin, bbox.xmax]),
            BTreeSet::from([bbox.ymin, bbox.ymax]),
            BTreeSet::from([bbox.zmin, bbox.zmax]),
        ));
    }

    fn insert_pair<'a>(
        set: &mut BTreeSet<isize>,
        vals: impl Iterator<Item = &'a isize>,
        limits: &[isize],
    ) -> bool {
        let mut dirty: bool = false;
        for x in vals {
            if x >= &limits[0] && x <= &limits[1] {
                dirty |= set.insert(*x);
            }
        }
        dirty
    }

    fn get_mut_pair<'a, T>(
        vals: &'a mut [T],
        index1: usize,
        index2: usize,
    ) -> (&'a mut T, &'a mut T) {
        if index1 < index2 {
            let (left, right) = vals.split_at_mut(index2);
            return (&mut left[index1], &mut right[0]);
        } else {
            let (left, right) = vals.split_at_mut(index1);
            return (&mut right[0], &mut left[index2]);
        }
    }

    // For any intersecting boxes, add each other to the list of prism coordinates
    let mut dirty: bool = true;
    while dirty {
        dirty = false;
        // let prisms = create_prisms(&commands, &coords);
        for i in 0..commands.len() {
            let bbox = commands[i].1;
            for j in 0..commands.len() {
                if i == j || !bbox.intersects(&commands[j].1) {
                    continue;
                }
                let (coords_i, coords_j) = get_mut_pair(&mut coords, i, j);
                dirty |= insert_pair(&mut coords_i.0, coords_j.0.iter(), &[bbox.xmin, bbox.xmax]);
                dirty |= insert_pair(&mut coords_i.1, coords_j.1.iter(), &[bbox.ymin, bbox.ymax]);
                dirty |= insert_pair(&mut coords_i.2, coords_j.2.iter(), &[bbox.zmin, bbox.zmax]);
            }
        }
    }

    // The coordinates of sub-prisms for each box are already sorted and deduped by the BTreeSet

    // Order the subdivided prisms as (x, y, z)
    // where x:0..x_coords.len()-1  y:0..y_coords.len()-1  z:0..z_coords.len()-1
    // and each prism is defined by:
    // - The open box:
    //   (x_coords[x],y_coords[y],z_coords[z]) -> (x_coords[x+1],y_coords[y+1],z_coords[z+1])
    // - The 6 open faces with x,x+1; y,y+1; z,z+1 fixed (not including the edges)
    // - The 12 open edges with two of x,x+1;y,y+1;z,z+1 fixed and the other varying (not including end vertices)
    // - The 8 vertices

    // HashMap command# -> Vec of subdivided prisms
    dbg!("Getting prisms map");
    let prisms = create_prisms(&commands, &coords);

    // Process commands
    dbg!("Process commands");
    let mut on_prisms: HashSet<BBox> = HashSet::new();
    let mut on_faces: HashSet<(PrismCoord, PrismCoord)> = HashSet::new();
    let mut on_edges: HashSet<(PrismCoord, PrismCoord)> = HashSet::new();
    let mut on_vertices: HashSet<PrismCoord> = HashSet::new();

    // Preallocate
    let mut faces: HashSet<(PrismCoord, PrismCoord)> = HashSet::with_capacity(6);
    let mut edges: HashSet<(PrismCoord, PrismCoord)> = HashSet::with_capacity(12);
    let mut vertices: HashSet<PrismCoord> = HashSet::with_capacity(8);

    for (index, command) in commands.iter().enumerate() {
        if None == prisms.get(&index) {
            println!("Prism {} is degenerate", index);
            continue;
        }

        for p in prisms.get(&index).unwrap() {
            faces.clear();
            edges.clear();
            vertices.clear();
            get_fev(&p, &mut faces, &mut edges, &mut vertices);
            if command.0 == ChangeState::On {
                on_prisms.insert(*p);
                on_faces.extend(&faces);
                on_edges.extend(&edges);
                on_vertices.extend(&vertices);
            } else {
                on_prisms.remove(p);
                faces.iter().for_each(|x| {
                    on_faces.remove(x);
                });
                edges.iter().for_each(|x| {
                    on_edges.remove(x);
                });
                vertices.iter().for_each(|x| {
                    on_vertices.remove(x);
                });
            }
        }
    }

    // Process on prisms, faces, edges, vertices
    dbg!("Get counts");
    let mut count = 0;
    for p in on_prisms {
        count += get_volume_count(&p);
    }
    for f in on_faces {
        count += get_face_count(&f);
    }
    for e in on_edges {
        count += get_edge_count(&e);
    }
    for _v in on_vertices {
        count += 1;
    }

    count
}

/// Simple implementation. Use a HashMap to keep track of on/off grid nodes.
/// Prune elements outside of -50..50
fn part1(lines: &Vec<String>) {
    let commands = read_data(lines);
    let result1 = process_commands(&commands);
    println!("Result1 = {}", result1); // 551693
}

fn part2(lines: &Vec<String>) {
    let commands = read_data(lines);
    let result2 = split_prisms(&commands);
    println!("Result2 = {}", result2); // 1165737675582132 (takes a long time... > 1hr)
}

pub fn main() -> Result<(), Box<dyn std::error::Error>> {
    let args: Vec<String> = std::env::args().collect();
    let file_name = if args.len() > 1 {
        &args[1]
    } else {
        "input22.txt"
    };
    let lines = read_lines(file_name)?;
    //let chunks = read_chunks(file_name)?;

    part1(&lines);
    part2(&lines);

    Ok(())
}

#[cfg(test)]
mod tests {
    use super::*;
    use std::path::PathBuf;

    #[test]
    fn part1_works() {}
    #[test]
    fn part2_works() {
        let input = [env!("CARGO_MANIFEST_DIR"), "src", "input22ex.txt"]
            .iter()
            .collect::<PathBuf>();
        let lines = read_lines(input).unwrap();
        let commands = read_data(&lines);
        let result1 = process_commands(&commands);
        println!("result = {} from {} commands", result1, commands.len());
        // https://stackoverflow.com/questions/44662312/how-to-filter-a-vector-of-custom-structs-in-rust
        let commands = commands
            .into_iter()
            .filter(|x| {
                if let Some(_) = &x.1.limit_box() {
                    true
                } else {
                    false
                }
            })
            .collect();
        let result2 = process_commands(&commands);
        println!("result = {} from {} commands", result2, commands.len());
        assert_eq!(result1, result2);
        let result3 = split_prisms(&commands);
        println!("result = {} from split_prisms", result3);
        assert_eq!(result2, result3);

        //for t in iproduct!(0..2, 0..3, 0..4) {
        //    println!("{:?}", t)
        //}
    }
}
