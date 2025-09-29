import pandas as pd
import matplotlib.pyplot as plt
import matplotlib.dates as mdates
from matplotlib.collections import LineCollection
import constants as CONST


def plot_candlestick(parquet_file):
    print("Preparing plot...")

    df = pd.read_parquet(parquet_file)

    df[CONST.OPEN_TIME] = pd.to_datetime(df[CONST.OPEN_TIME])
    df.set_index(CONST.OPEN_TIME, inplace=True)

    for col in [
        CONST.OPEN_PRICE,
        CONST.HIGH_PRICE,
        CONST.LOW_PRICE,
        CONST.CLOSE_PRICE,
        CONST.VOLUME,
    ]:
        df[col] = pd.to_numeric(df[col])

    up = df[df[CONST.CLOSE_PRICE] >= df[CONST.OPEN_PRICE]]
    down = df[df[CONST.CLOSE_PRICE] < df[CONST.OPEN_PRICE]]

    _, ax = plt.subplots(figsize=(16, 9))

    def make_lines(idx, lows, highs):
        x = mdates.date2num(idx.to_pydatetime())  # convert datetime to floats
        return [((xi, low), (xi, high)) for xi, low, high in zip(x, lows, highs)]

    def make_bodies(idx, opens, closes):
        x = mdates.date2num(idx.to_pydatetime())
        return [((xi, o), (xi, c)) for xi, o, c in zip(x, opens, closes)]

    # Add wick collections
    ax.add_collection(
        LineCollection(
            make_lines(up.index, up[CONST.LOW_PRICE], up[CONST.HIGH_PRICE]),
            colors="green",
            linewidths=1,
        )
    )
    ax.add_collection(
        LineCollection(
            make_lines(down.index, down[CONST.LOW_PRICE], down[CONST.HIGH_PRICE]),
            colors="red",
            linewidths=1,
        )
    )

    # Add body collections
    ax.add_collection(
        LineCollection(
            make_bodies(up.index, up[CONST.OPEN_PRICE], up[CONST.CLOSE_PRICE]),
            colors="green",
            linewidths=6,
        )
    )
    ax.add_collection(
        LineCollection(
            make_bodies(down.index, down[CONST.OPEN_PRICE], down[CONST.CLOSE_PRICE]),
            colors="red",
            linewidths=6,
        )
    )

    ax.set_title(f"BTC/USDT 1m Candlesticks (from {parquet_file})")
    ax.set_xlabel("Time of day")
    ax.set_ylabel("Price (USDT)")

    ax.xaxis_date()  # interpret x as dates
    ax.xaxis.set_major_formatter(mdates.DateFormatter("%y-%m-%d %H:%M"))
    plt.xticks(rotation=45)
    plt.grid(True)

    ax.autoscale_view()  # ensure everything fits

    print("Rendering plot...")
    plt.show(block=True)
