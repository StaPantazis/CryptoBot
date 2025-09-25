import gzip
import json
import pandas as pd
import matplotlib.pyplot as plt
import matplotlib.dates as mdates
import constants as CONST


def plot_candlestick(json_gz_file):
    print("Preparing plot...")

    # Load gzipped JSON file
    with gzip.open(json_gz_file, "rt", encoding="utf-8") as f:
        data = json.load(f)

    # Convert JSON to DataFrame
    df = pd.DataFrame(data)

    # Ensure proper types
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

    # Plot candlestick chart
    _, ax = plt.subplots(figsize=(16, 9))

    for t, row in df.iterrows():
        color = "green" if row[CONST.CLOSE_PRICE] >= row[CONST.OPEN_PRICE] else "red"
        # Wick
        ax.vlines(
            t, row[CONST.LOW_PRICE], row[CONST.HIGH_PRICE], color=color, linewidth=1
        )
        # Body
        ax.vlines(
            t, row[CONST.OPEN_PRICE], row[CONST.CLOSE_PRICE], color=color, linewidth=6
        )

    ax.set_title(f"BTC/USDT 1m Candlesticks (from {json_gz_file})")
    ax.set_xlabel("Time of day")
    ax.set_ylabel("Price (USDT)")
    ax.xaxis.set_major_locator(mdates.HourLocator(interval=2))
    ax.xaxis.set_major_formatter(mdates.DateFormatter("%H:%M"))
    plt.xticks(rotation=45)
    plt.grid(True)

    print("Rendering plot...")
    plt.show(block=True)
