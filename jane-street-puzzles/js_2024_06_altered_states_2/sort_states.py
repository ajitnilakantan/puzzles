from pathlib import Path

data_file = Path(__file__).with_name("states.txt")
# Read states and populations: tab separated values
with open(data_file, "r", encoding="utf-8") as f:
    lines = [line.strip("\n").upper() for line in f.readlines()]
    states = [x.split("\t")[0].replace("\x20", "").strip() for x in lines]
    populations = [int(x.split("\t")[1].strip("\x20").strip()) for x in lines]
assert len(states) == 51, "Should have 51 states"

temp = list(zip(states, populations))
temp = sorted(temp, key=lambda x: x[1], reverse=True)
res1, res2 = zip(*temp)
# res1 and res2 come out as tuples, and so must be converted to lists.
states_sorted, populations_sorted = list(res1), list(res2)


out_file = Path(__file__).with_name("states_sorted.txt")
with open(out_file, "w", encoding="utf-8") as f:
    for s, pop in zip(states_sorted, populations_sorted):
        f.write(f"{s}\t{pop:_}\n")
