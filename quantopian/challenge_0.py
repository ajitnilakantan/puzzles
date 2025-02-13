from __future__ import annotations  # for pandas typing
import numpy as np
import numpy.typing as npt
import pandas as pd
import math
from datetime import datetime
import matplotlib.pyplot as plt


DAYS_IN_YEAR = 256.0
ROOT_DAYS_IN_YEAR = DAYS_IN_YEAR**0.5


def RandomReturns(
    years: float, num_steps: int, mu: float, sigma: float, init_price: float
) -> list[float]:
    """
    Create randomized returns
    years: time period in years
    num_steps: number of steps in similation
    mu: average yearly return
    sigma: standard deviation
    init_price: starting value
    """
    DeltaY: float = years / num_steps
    SqrtDeltaY: float = math.sqrt(DeltaY)
    DeltaW = SqrtDeltaY * np.random.normal(mu, sigma, size=num_steps)
    DeltaW = SqrtDeltaY * np.random.normal([0] * num_steps , [1] * num_steps , size=num_steps)
    print(
        f"average = {np.average(DeltaW)}, stddev={np.std(DeltaW)} DeltaY={DeltaY} sqrt={SqrtDeltaY} expect {mu} {sigma}"
    )
    Increments = (mu - sigma * sigma / 2) * DeltaY + sigma * DeltaW
    ExpIncr = np.exp(Increments)
    PricePath = np.cumprod(init_price * ExpIncr)
    return PricePath

def skew_returns_annualised(annualSR=1.0, want_skew=0.0, voltarget=0.20, size=10000):
    annual_rets=annualSR*voltarget
    daily_rets=annual_rets/DAYS_IN_YEAR
    daily_vol=voltarget/ROOT_DAYS_IN_YEAR
    
    return skew_returns(want_mean=daily_rets,  want_stdev=daily_vol,want_skew=want_skew, size=size)

def skew_returns(want_mean,  want_stdev, want_skew, size=10000):
    
    EPSILON=0.0000001
    shapeparam=(2/(EPSILON+abs(want_skew)))**2
    scaleparam=want_stdev/(shapeparam)**.5
    
    sample = list(np.random.gamma(shapeparam, scaleparam, size=size))
    
    if want_skew<0.0:
        signadj=-1.0
    else:
        signadj=1.0
    
    natural_mean=shapeparam*scaleparam*signadj
    mean_adjustment=want_mean - natural_mean 

    sample=[(x*signadj)+mean_adjustment for x in sample]
    
    return sample

"""
# time parameters
T = 1.0 # simulate half a year 
delta_t = 1.0/252.0 # each day is 1/252 of a trading year
n = int(T / delta_t) # total number of samples
# https://medium.com/@MachineLearningYearning/how-to-simulate-stock-prices-452042862989
def simulate_prices(n, k, mu, sigma, delta_t, S0):
    r = (mu*delta_t + sigma * np.sqrt(delta_t) * np.random.normal(size=(n, k)))
    p = np.cumprod(r+1, axis=0) * S0 # compute cumulative returns, and multiply by initial price to calculate prices
    return r, p
"""

def simulate_price(duration_in_years: float, num_steps: int, mu: float, sigma: float, initial_price: float):
    days_in_year = 256
    # Each sample is delta_t of a year
    delta_t = duration_in_years / num_steps
    num_steps_in_year = 1/delta_t
    delta_mu = math.exp(math.log(1+mu) / num_steps_in_year)-1
    values = np.random.normal(1+delta_mu, sigma/(num_steps_in_year), size=num_steps)
    print(f"delta_t = {delta_t} num_steps_in_year={num_steps_in_year} delta_mu={delta_mu} mu={mu} annualized={(1+delta_mu)**num_steps_in_year-1}")
    print(f"values={values}")
    values = np.cumprod(values)
    return_series = values[:]
    return_series = [x-1 for x in return_series]

    print(f"values={values}")
    values = initial_price * values
    print(f"values={values}")
    print(f"mean={np.mean(values)} std={np.std(values)}")
    rf = 0
    mean = np.mean(return_series) * num_steps_in_year - rf
    sigma = np.std(return_series) * np.sqrt(num_steps_in_year)
    sharpe = mean/sigma
    print(f"mean={mean} sigma={sigma} sharpe={sharpe}")
    print(f"return_series={return_series}")

def main() -> None:
    simulate_price(duration_in_years=1, num_steps=12,mu=0.07, sigma=1, initial_price=100)

"""
def main4() -> None:
    S0 = 10 # initial price of stock
    mu = 1 # drift
    sigma = 1 # volatility
    k = 10000 # number of realizations
    r, p = simulate_prices(n, k, mu, sigma, delta_t, S0)

    print(f"type={type(p)} {p.shape} {type(r)} {r.shape}")
    plt.figure()
    avg = np.mean(p, (1))
    plt.plot(avg)
    plt.title(f'Simulated Stock Prices\n{100} Realizations')
    plt.show()
"""

def main1() -> None:
    # ans=arbitrary_timeseries(skew_returns_annualised(annualSR=0.5, want_skew=0.0, size=2500))
    ans=skew_returns_annualised(annualSR=0.5, want_skew=0.0, size=1024)
    print(f"ans = {ans}")
    ans = [1+x for x in ans]
    print(f"ans = {ans}")
    ans = np.cumprod(ans)
    print(f"ans = {ans}")
    print( f"average = {np.average(ans)}, stddev={np.std(ans)} ")
    # cum_perc(ans).plot()
    plt.title("Line graph")
    plt.xlabel("X axis")
    plt.ylabel("Y axis")
    plt.plot(ans)
    plt.show()

def main2() -> None:

    plt.title("Line graph")
    plt.xlabel("X axis")
    plt.ylabel("Y axis")
    data = RandomReturns(1, 256, 10, 2, 100)
    plt.plot(data)
    plt.show()


if __name__ == "__main__":
    main()
