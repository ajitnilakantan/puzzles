from datetime import datetime
import math
import numpy as np
import pandas as pd
import scipy.signal as sg

DAYS_IN_YEAR = 256.0
ROOT_DAYS_IN_YEAR = DAYS_IN_YEAR**0.5

def PricePath(
    Y: float, NbSteps: int, mu: float, sigma: float, InitPrice: float
) -> list[float]:
    """
    # See: https://stackoverflow.com/questions/8597731/are-there-known-techniques-to-generate-realistic-looking-fake-stock-data
    # The following is an adaptation from a program shown at page 140 in
    # "Stochastic Simulations and Applications in Finance",
    # a book written by Huynh, Lai and Soumaré.
    # That program was written in MatLab and this one was written in R by me.
    # That program produced many price paths and this one produces one.
    # The latter is also somewhat simpler and faster.

    # Y is the time period in years, for instance 1 (year)
    # NbSteps is the number of steps in the simulation,
    # for instance 250 (trading days in a year).
    # DeltaY is the resulting time step.

    # The computations shown implement the exact solution
    # to the stochastic differential equation for
    # the geometric Brownian motion modelling stock prices,
    # with mean mu and volatility sigma, thus generating a stochastic price path
    # such as that exhibited by stock prices when price jumps are rare.
    """

    DeltaY = Y / NbSteps
    SqrtDeltaY = math.sqrt(DeltaY)
    DeltaW = SqrtDeltaY * np.random.normal([0] * NbSteps, [1] * NbSteps, size=NbSteps)
    Increments = (mu - sigma * sigma / 2) * DeltaY + sigma * DeltaW
    ExpIncr = np.exp(Increments)
    PricePath = np.cumprod(InitPrice * ExpIncr)
    return PricePath
"""

"""

def skew_returns_annualised(annualSR=1.0, want_skew=0.0, voltarget=0.20, size=10000):
    annual_rets = annualSR * voltarget
    daily_rets = annual_rets / DAYS_IN_YEAR
    daily_vol = voltarget / ROOT_DAYS_IN_YEAR

    return skew_returns(
        want_mean=daily_rets, want_stdev=daily_vol, want_skew=want_skew, size=size
    )


def skew_returns(want_mean, want_stdev, want_skew, size=10000):
    EPSILON = 0.0000001
    shapeparam = (2 / (EPSILON + abs(want_skew))) ** 2
    scaleparam = want_stdev / (shapeparam) ** 0.5

    sample = list(np.random.gamma(shapeparam, scaleparam, size=size))

    if want_skew < 0.0:
        signadj = -1.0
    else:
        signadj = 1.0

    natural_mean = shapeparam * scaleparam * signadj
    mean_adjustment = want_mean - natural_mean

    sample = [(x * signadj) + mean_adjustment for x in sample]

    return sample


def autocorr_skewed_returns(rho, want_mean, want_stdev, want_skew, size=10000):
    ## closed form correction for ar1 process noise
    noise_stdev = (want_stdev**2 * (1 - rho)) ** 0.5
    noise_terms = skew_returns(want_mean, noise_stdev, want_skew, size)

    ## combine the noise with a filter
    return sg.lfilter((1,), (1, -rho), noise_terms)


def adj_moments_for_rho(want_rho, want_mean, want_skew, want_stdev):
    """
    Autocorrelation introduces biases into other moments of a distribution

    Here I correct for these
    """
    assert abs(want_rho) <= 0.8

    mean_correction = 1 / (1 - want_rho)

    if want_rho >= 0.0:
        skew_correction = (1 - want_rho) ** 0.5
    else:
        skew_correction = np.interp(
            want_rho,
            [-0.8, -0.7, -0.6, -0.5, -0.4, -0.3, -0.2, -0.1],
            [0.14, 0.27, 0.42, 0.58, 0.72, 0.84, 0.93, 0.98],
        )

    ## somewhat hacky, but we do a correction inside the random generation function already

    stdev_correction = np.interp(
        want_rho,
        [
            -0.8,
            -0.7,
            -0.6,
            -0.5,
            -0.4,
            -0.3,
            -0.2,
            -0.1,
            0.0,
            0.1,
            0.2,
            0.3,
            0.4,
            0.5,
            0.6,
            0.7,
            0.8,
        ],
        [
            2.24,
            1.83,
            1.58,
            1.41,
            1.29,
            1.19,
            1.12,
            1.05,
            1.0,
            0.95,
            0.91,
            0.88,
            0.85,
            0.82,
            0.79,
            0.77,
            0.75,
        ],
    )

    adj_want_stdev = want_stdev / stdev_correction
    adj_want_mean = want_mean / mean_correction
    adj_want_skew = want_skew / skew_correction

    return (adj_want_mean, adj_want_skew, adj_want_stdev)


def cum_perc(pd_timeseries: "pd.Series[float]"):
    """
    Cumulate percentage returns for a pandas time series
    """
    cum_dataindex = [1 + x for x in pd_timeseries]
    cum_datalist = pd.Series(cum_dataindex, index=pd_timeseries.index)
    return cum_datalist.cumprod()


def arbitrary_timeindex(Nperiods, index_start=datetime(2000, 1, 1)):
    """
    For nice plotting, convert a list of prices or returns into an arbitrary pandas time series
    """

    ans = pd.bdate_range(start=index_start, periods=Nperiods)

    return ans


def arbitrary_timeseries(datalist, index_start=datetime(2000, 1, 1)):
    """
    For nice plotting, convert a list of prices or returns into an arbitrary pandas time series
    """

    ans = pd.Series(datalist, index=arbitrary_timeindex(len(datalist), index_start))

    return ans


def annualised_rets(total_rets):
    mean_rets = total_rets.mean(skipna=True)
    annualised_rets = mean_rets * DAYS_IN_YEAR
    return annualised_rets


def annualised_vol(total_rets):
    actual_total_daily_vol = total_rets.std(skipna=True)
    actual_total_annual_vol = actual_total_daily_vol * ROOT_DAYS_IN_YEAR
    return actual_total_annual_vol


def sharpe(total_rets):
    sharpe = annualised_rets(total_rets) / annualised_vol(total_rets)

    return sharpe


def sharpe_ratio(return_series, N, rf):
    # N = 255 #255 trading days in a year
    # rf =0.01 #1% risk free rate
    mean = return_series.mean() * N - rf
    sigma = return_series.std() * np.sqrt(N)
    return mean / sigma


def main2() -> None:
    mu, sigma = 2.0, 0.01  # mean and standard deviation
    # samples = 1000
    s = np.random.normal(mu, sigma, 1000)
    print(f"mean = {np.mean(s)} diff={math.fabs(mu - np.mean(s))}")
    print(f"stddev = {np.std(s, ddof=1)} diff={math.fabs(sigma - np.std(s, ddof=1))}")

    return_series = np.diff(np.log(s))
    print(
        f"mean = {np.mean(return_series)} diff={math.fabs(mu - np.mean(return_series))}"
    )
    print(
        f"stddev = {np.std(return_series, ddof=1)} diff={math.fabs(sigma - np.std(return_series, ddof=1))}"
    )

    sr = sharpe_ratio(return_series, 256, 0.0)
    print(f"Sharp ratio = {sr}")

    import matplotlib.pyplot as plt

    # count, bins, ignored = plt.hist(return_series, 30, density=True)
    # plt.plot(bins, 1/(sigma * np.sqrt(2 * np.pi)) * np.exp( - (bins - mu)**2 / (2 * sigma**2) ), linewidth=2, color='r')
    # plt.show()
    plt.title("Line graph")
    plt.xlabel("X axis")
    plt.ylabel("Y axis")
    plt.plot(return_series, color="red")
    plt.show()


def main3() -> None:
    import matplotlib.pyplot as plt

    plt.title("Line graph")
    plt.xlabel("X axis")
    plt.ylabel("Y axis")
    data = cum_perc(
        arbitrary_timeseries(
            skew_returns_annualised(annualSR=2.0, want_skew=0.0, size=256)
        )
    )
    data = arbitrary_timeseries(
        skew_returns_annualised(annualSR=2.0, want_skew=0.0, size=256)
    )
    print(
        f"annualised_rets={annualised_rets(data)} annualised_vol={annualised_vol(data)} sharpe={sharpe(data)}"
    )
    data.plot()
    plt.show()

if __name__ == "__main__":
    main2()
    main3()
