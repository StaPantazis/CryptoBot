import json
import pandas as pd
import matplotlib.pyplot as plt
import matplotlib.dates as mdates


def plot_candlestick(json_file):
    print("Preparing plot...")

    # Load JSON file
    with open(json_file, "r") as f:
        data = json.load(f)

    # Convert JSON to DataFrame
    df = pd.DataFrame(data)

    # Ensure proper types
    df["OpenTime"] = pd.to_datetime(df["OpenTime"])
    df.set_index("OpenTime", inplace=True)

    for col in ["OpenPrice", "HighPrice", "LowPrice", "ClosePrice", "Volume"]:
        df[col] = pd.to_numeric(df[col])

    # Plot candlestick chart
    _, ax = plt.subplots(figsize=(16, 9))

    for t, row in df.iterrows():
        color = "green" if row["ClosePrice"] >= row["OpenPrice"] else "red"
        # Wick
        ax.vlines(t, row["LowPrice"], row["HighPrice"], color=color, linewidth=1)
        # Body (thicker vertical line between open and close)
        ax.vlines(t, row["OpenPrice"], row["ClosePrice"], color=color, linewidth=6)

    ax.set_title(f"BTC/USDT 1m Candlesticks")
    ax.set_xlabel("Time of day")
    ax.set_ylabel("Price (USDT)")
    ax.xaxis.set_major_locator(mdates.HourLocator(interval=2))
    ax.xaxis.set_major_formatter(mdates.DateFormatter("%H:%M"))
    plt.xticks(rotation=45)
    plt.grid(True)
    plt.show(block=True)
