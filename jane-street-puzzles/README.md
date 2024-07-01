## Jane Street Puzzles

Located at: (https://www.janestreet.com/puzzles/archive/)


## RYE
This project is managed with [rye](https://github.com/astral-sh/rye).

Initialized with:
```
rye init
rye tools install mypy
rye add typing-extensions keyboard
rye add --dev pytest types-keyboard
rye sync # run after adding/removing packages
```

To run:
```
rye run nc4
```

Also available:
```
rye lint # Linting
rye fmt  # Black formatting
rye run python -mmypy ./as2.py # E.g. Check file 'as2.py' with mypy
```
