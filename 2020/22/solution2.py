from collections import defaultdict, deque
from collections import namedtuple
from functools import cmp_to_key
import itertools
import copy
import re
import sys

file_name = "Input.txt"

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
index += 1 # Blank
index += 1 # Player 2

player2_cards = deque()
while index < len(lines):
    player2_cards.append(int(lines[index]))
    index += 1

print(f"Player1 = {player1_cards}")
print(f"Player2 = {player2_cards}")


def play_game(player1_cards, player2_cards, level = 0):

    previous_rounds = set()

    while len(player1_cards) > 0 and len(player2_cards) > 0:
        round = (tuple(player1_cards), tuple(player2_cards))
        if round in previous_rounds:
            # Player1 wins
            # print(f"LOOP level={level} {round}")
            return 1
        else:
            previous_rounds.add(round)

        card1 = player1_cards.popleft()
        card2 = player2_cards.popleft()

        if len(player1_cards) >= card1 and len(player2_cards) >= card2:
            winner = play_game(deque(list(player1_cards)[0:card1]), deque(list(player2_cards)[0:card2]), level+1)
            if winner == 1:
                player1_cards.append(card1)
                player1_cards.append(card2)
            else:
                player2_cards.append(card2)
                player2_cards.append(card1)
        elif card1 > card2:
            player1_cards.append(card1)
            player1_cards.append(card2)
        else:
            player2_cards.append(card2)
            player2_cards.append(card1)

    return 1 if len(player1_cards) > 0 else 2


play_game(player1_cards, player2_cards)

print(f"Player1 = {player1_cards}")
print(f"Player2 = {player2_cards}")

cards = player1_cards if len(player1_cards) > 0 else player2_cards
result = 0
count = 1
while len(cards) > 0:
    result += cards.pop() * count
    count += 1
print(f"Solution2 = {result}")
