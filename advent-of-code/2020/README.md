## 2020 Advent of Code in Python

## Running
- Install uv

- Initialization
```
# uv python install --reinstall # To reinstall uv-managed python
uv init # --bare no src directory

uv add scipy
uv add numpy
uv add --dev mypy
uv tool install ruff
uv sync
```

- Running
```
uv run ruff check NN/inputN.py
uv run ruff format NN/inputN.py
uv run python -mmypy NN/inputN.py
uv run python NN/inputN.py
```
e.g.
```
uv run ruff check 01/input1.py
uv run ruff format 01/input1.py
uv run python -mmypy 01/input1.py
uv run python 01/input1.py
```


