import js_2024_06_altered_states_2.as2 as this


def test_board() -> None:
    board = this.Board(5, 5, 1)
    print("")
    print(f"board={board}")
    for i in range(5):
        board.set(i, i, 10 + board.get(i, i))
    print(f"board={board._data}")
    # assert this.test_example_provided() == 24898, "Test example should be 24898"
