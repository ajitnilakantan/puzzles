## quantopian

Puzzles fro quantopian.com

### puzzle_0

- https://community.quantopian.com/c/community-forums/quantopian-coding-challenge-7a558893-fd3f-402b-a1fe-ab46c77d6001
- https://colab.research.google.com/gist/joshpayne/dd0c4d5c1bb307a669ebe86028e9027f/challenge_0.ipynb

Sharpe ratio = $\bar{R^e}/\sigma^e$

$\bar{R^e}$ = Average excess return
= ${1\over{n}} \Sigma_{i=1}^{n} (R_i-RF_i)$

$\sigma^e$ = Average standard deviation of excess returns =
$\sqrt{{1\over{n}}\Sigma\_{i=1}^{n} (R_i - RF_i - \bar{R^e})^2} $

$R_i$ = Return<br/>
$RF_i$ = Risk free return

Annuallized Sharpe ratio = Monthly Sharpe ratio $\times \sqrt{12}$

Useful links:


- https://kurtverstegen.wordpress.com/2013/12/07/simulation/
- https://medium.com/@MachineLearningYearning/how-to-simulate-stock-prices-452042862989
- https://qoppac.blogspot.com/2015/11/using-random-data.html
- https://www.interviewqs.com/blog/efficient-frontier
- https://www.youtube.com/playlist?list=PL5C4op9wSllh9d-ZZFHpvWmX7Hoqtc8Rw
- https://stackoverflow.com/questions/36999776/how-to-simulate-random-returns-with-numpy


## UV ()

```
# uv python install --reinstall # To reinstall uv-managed python
uv init # --bare no src directory

uv add scipy
uv add numpy
uv add pandas
uv add matplotlib
uv add statsmodels
uv add --dev mypy
uv add --dev pandas-stubs types-cffi
uv tool install jupyter-core
uv tool install ruff
uv sync

uv run ruff check challenge_0.py
uv run ruff format .\challenge_0.py
uv run python -mmypy challenge_0.py
uv run python challenge_0.py
```

