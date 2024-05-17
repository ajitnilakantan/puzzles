from collections import defaultdict, deque
from collections import namedtuple
from functools import cmp_to_key
import itertools
import copy
import re

file_name = "Input1.txt"

'''
with open(file_name, "r") as fp:
    lines = fp.readlines()
lines = [x.strip() for x in lines]

line = lines[0].strip()
toks = line.split(",")
nums = [int(n) for n in toks]
'''

'''
with open(file_name, "r") as fp:
    lines = fp.readlines()
lines = [x.strip() for x in lines]
'''

with open(file_name, "r") as fp:
    lines = fp.readlines()
lines = [x.strip() for x in lines]

index = 0
index += 1 # Player 1
player1_cards = deque()
while lines[index]:
    player1_cards.append(int(lines[index]))
    index += 1
index +=1 # Blank
index +=1 # Player 2

player2_cards = deque()
while index < len(lines):
    player2_cards.append(int(lines[index]))
    index += 1

print(f"Player1 = {player1_cards}")
print(f"Player2 = {player2_cards}")

while len(player1_cards) > 0 and len(player2_cards) > 0:
    card1 = player1_cards.popleft()
    card2 = player2_cards.popleft()
    if card1 > card2:
        player1_cards.append(card1)
        player1_cards.append(card2)
    else:
        player2_cards.append(card2)
        player2_cards.append(card1)

cards = player1_cards if len(player1_cards) > 0 else player2_cards
result = 0
count = 1
while len(cards) > 0:
    result += cards.pop() * count
    count += 1
print(f"Solution1 = {result}")
