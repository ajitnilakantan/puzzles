## Jane Street Puzzles

Located at: (https://www.janestreet.com/puzzles/archive/)


## UV
This project is managed with [uv](https://github.com/astral-sh/uv).

Initialized with:
```
uv init
uv tool install mypy
uv add typing-extensions keyboard
uv add scipy
uv add --dev scipy-stubs
uv add --dev pytest types-keyboard
uv tool install ruff
uv sync # run after adding/removing packages
```

To run:
```
uv run nc4
```

Also available:
```
uv ruff check . # Linting
uv ruff format .  # Ruff formatting
uv run python -mmypy ./as2.py # E.g. Check file 'as2.py' with mypy
```
