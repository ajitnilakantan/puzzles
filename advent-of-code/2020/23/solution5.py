class Node:
    def __init__(self, data):
        self.item = data
        self.next = None

cups = [3, 8, 9, 1, 2, 5, 4, 6, 7]  # Sample input
cups = [5, 8, 9, 1, 7, 4, 2, 6, 3]  # Real input

num_rounds = 10000000
total_cups = 1000000
#num_rounds = 10
#total_cups = 9
cups = cups + [x for x in range(len(cups)+1, total_cups+1)]

# Create nodes
nodes = [None for x in range(total_cups+1)]

_nodes = []
for c in cups:
    node = Node(c)
    _nodes.append(node)
    nodes[c] = node

# Create linked list
for n in range(0, total_cups - 1):
    _nodes[n].next = _nodes[n+1]
_nodes[-1].next = _nodes[0]

cups = _nodes[0]

def print_cups(cups, current_cup = None):
    n = cups
    while True:
        if current_cup and n.item == current_cup.item:
            print(f" ({n.item}) ", end='')
        else:
            print(f" {n.item} ", end='')
        n = n.next
        if n == cups:
            break
    print('')

current_cup = cups
loop_counter = 1
#print(f"{loop_counter:03d}: ", end='');
#print_cups(cups, current_cup)


for loop_counter in range(2, num_rounds+2):
    taken_values = [current_cup.next.item, current_cup.next.next.item, current_cup.next.next.next.item]
    destination_value = current_cup.item - 1
    # while destination not in cups:
    while destination_value in taken_values:
        destination_value -= 1
        if destination_value <= 0:
            break
    if destination_value <= 0:
        destination_value = total_cups
        while destination_value in taken_values:
            destination_value -= 1
    destination = nodes[destination_value]

    triplet = current_cup.next
    current_cup.next = current_cup.next.next.next.next
    current_cup = current_cup.next
    destination_next = destination.next
    destination.next = triplet
    triplet.next.next.next = destination_next

    #print(f"{loop_counter:03d}: ", end='');
    #print_cups(cups, current_cup)

cup1 = nodes[1]
val1 = cup1.next.item
val2 = cup1.next.next.item

print(f"Solution2 = {val1} * {val2} = {val1*val2}")
