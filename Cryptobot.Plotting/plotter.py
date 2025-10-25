import pandas as pd
import matplotlib
import matplotlib.pyplot as plt
import matplotlib.dates as mdates
import matplotlib.ticker as mticker
import zipfile
import tempfile
import constants as CONST
import mplcursors
import warnings
from matplotlib.collections import LineCollection
from pathlib import Path
from plotSettings import PlotSettings
from datetime import datetime

matplotlib.use("TkAgg")
warnings.filterwarnings("ignore", message="Glyph .* missing from font")
plt.rcParams["font.family"] = "Segoe UI Emoji"


# -----------------------------
#  CANDLE PLOT
# -----------------------------
def plot_candle_chart(df, title):
    """Draw a standalone candlestick chart."""
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

    def make_lines(idx, lows, highs):
        x = mdates.date2num(idx.to_pydatetime())
        return [((xi, low), (xi, high)) for xi, low, high in zip(x, lows, highs)]

    def make_bodies(idx, opens, closes):
        x = mdates.date2num(idx.to_pydatetime())
        return [((xi, o), (xi, c)) for xi, o, c in zip(x, opens, closes)]

    fig, ax = plt.subplots(figsize=(16, 9))

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

    ax.set_title(title)
    ax.set_xlabel("Time")
    ax.set_ylabel("Price (USDT)")
    ax.yaxis.set_major_formatter(mticker.FuncFormatter(lambda x, _: f"{x:.0f}€"))
    ax.xaxis_date()
    ax.xaxis.set_major_formatter(mdates.DateFormatter("%y-%m-%d %H:%M"))
    plt.xticks(rotation=45)
    plt.grid(True)
    ax.autoscale_view()

    plt.show(block=False)


# -----------------------------
#  LINEAR GRAPH PLOT
# -----------------------------
def plot_linear_chart(df: pd.DataFrame, title: str):
    if df is None or df.empty:
        print("⚠️ No data to plot.")
        return

    # Sort by time; keep entry before exit on same timestamp
    df = df.copy()
    df["Timestamp"] = pd.to_datetime(df["Timestamp"])
    df = df.sort_values(["Timestamp", "IsOpen"], ascending=[True, False])

    # Line color by overall result
    start_budget = float(df["Budget"].iloc[0])
    end_budget = float(df["Budget"].iloc[-1])
    line_color = "green" if end_budget >= start_budget else "red"

    fig, ax = plt.subplots(figsize=(16, 6))

    # STEP LINE on the actual event timestamps → markers will sit exactly on it
    ax.plot(
        df["Timestamp"],
        df["Budget"],
        drawstyle="steps-post",
        linewidth=1.2,
        color=line_color,
        label="Budget over Time",
    )

    # Markers
    entry_nodes = df[df["IsOpen"] == True]
    exit_nodes = df[df["IsOpen"] == False]

    entry_scatter = ax.scatter(
        entry_nodes["Timestamp"],
        entry_nodes["Budget"],
        color="gold",
        s=5,
        label="Entry",
        zorder=3,
    )
    exit_scatter = ax.scatter(
        exit_nodes["Timestamp"],
        exit_nodes["Budget"],
        color="royalblue",
        s=5,
        label="Exit",
        zorder=3,
    )

    # Axes/formatting
    ax.set_title(title)
    ax.set_xlabel("Time")
    ax.set_ylabel("Budget (€)")
    ax.yaxis.set_major_formatter(mticker.FuncFormatter(lambda x, _: f"{x:,.0f}€"))
    ax.grid(True, alpha=0.4)
    ax.legend(loc="best")

    # Start y-axis at 0
    ax.set_ylim(bottom=0)

    # Hover tooltips (keep emojis only if your font supports them)
    cursor = mplcursors.cursor([entry_scatter, exit_scatter], hover=True)

    @cursor.connect("add")
    def on_hover(sel):
        ind = sel.index
        if sel.artist == entry_scatter:
            node = entry_nodes.iloc[ind]
            sel.annotation.set_text(
                f"Entry #{int(node.TradeIndex)}\n"
                f"Budget: {node.Budget:,.2f}€\n"
                f"Time: {node.Timestamp:%Y-%m-%d %H:%M}"
            )
        else:
            node = exit_nodes.iloc[ind]
            sel.annotation.set_text(
                f"Exit  #{int(node.TradeIndex)}\n"
                f"Budget: {node.Budget:,.2f}€\n"
                f"Time: {node.Timestamp:%Y-%m-%d %H:%M}"
            )
        sel.annotation.get_bbox_patch().set(fc="white", alpha=0.9, edgecolor="black")

    plt.tight_layout()
    plt.show(block=False)


# -----------------------------
#  MAIN ENTRIES
# -----------------------------
def plot_backtest_candles(zip_path: str | Path, settings: "PlotSettings"):
    zip_path = Path(zip_path)
    if not zip_path.exists():
        print(f"❌ File not found: {zip_path}")
        return

    print(f"Extracting: {zip_path.name}")

    with tempfile.TemporaryDirectory() as tmpdir:
        tmpdir = Path(tmpdir)
        with zipfile.ZipFile(zip_path, "r") as z:
            z.extractall(tmpdir)

        parquet_files = list(tmpdir.glob("*.parquet"))
        if not parquet_files:
            print("❌ No parquet files found in archive.")
            return

        # --- Linear Graph ---
        if getattr(settings, "plot_linear_time_graph", True):  # only if enabled
            linear_time_files = [
                f for f in parquet_files if "linear-" in f.name.lower()
            ]
            if not linear_time_files:
                print("❌ No 'linear' parquet file found inside ZIP.")
            else:
                linear_time_file = linear_time_files[0]
                print(f"✅ Linear file: {linear_time_file.name}")
                df_linear_time = pd.read_parquet(linear_time_file)
                plot_linear_chart(df_linear_time, f"Timeline – {zip_path.name}")

        # --- Candle Graph ---
        if settings.plot_candles_graph:
            candle_files = [f for f in parquet_files if "candles" in f.name.lower()]
            if not candle_files:
                print("❌ No 'candles' parquet file found inside ZIP.")
            else:
                candle_file = candle_files[0]
                print(f"✅ Candle file: {candle_file.name}")
                df_candles = pd.read_parquet(candle_file)
                plot_candle_chart(df_candles, f"Candles – {zip_path.name}")

        print("✅ Rendering complete. Close the plots to continue.")
        plt.show(block=True)


def plot_trend_candles(parquet_path: str | Path, title: str | None = None):
    parquet_path = Path(parquet_path)

    df = pd.read_parquet(parquet_path)

    # Ensure types
    df[CONST.OPEN_TIME] = pd.to_datetime(df[CONST.OPEN_TIME])
    for col in [CONST.OPEN_PRICE, CONST.HIGH_PRICE, CONST.LOW_PRICE, CONST.CLOSE_PRICE]:
        df[col] = pd.to_numeric(df[col])

    # Sort and index by time
    df.sort_values(CONST.OPEN_TIME, inplace=True)
    df.set_index(CONST.OPEN_TIME, inplace=True)

    # Trend must be numeric (1..5)
    if "Trend" not in df.columns:
        raise ValueError(
            "Column 'Trend' is required in the Parquet for trend coloring."
        )
    df["Trend"] = pd.to_numeric(df["Trend"])

    # Helpers to create wick/body segments
    def make_lines(idx, lows, highs):
        x = mdates.date2num(idx.to_pydatetime())
        return [((xi, lo), (xi, hi)) for xi, lo, hi in zip(x, lows, highs)]

    def make_bodies(idx, opens, closes):
        x = mdates.date2num(idx.to_pydatetime())
        return [((xi, o), (xi, c)) for xi, o, c in zip(x, opens, closes)]

    _, ax = plt.subplots(figsize=(16, 9))

    # For each trend bucket, add wick + body collections in that trend's color
    for trend_value, color in CONST.TREND_COLORS.items():
        sub = df[df["Trend"] == trend_value]
        if sub.empty:
            continue

        # Wicks
        ax.add_collection(
            LineCollection(
                make_lines(sub.index, sub[CONST.LOW_PRICE], sub[CONST.HIGH_PRICE]),
                colors=color,
                linewidths=1,
            )
        )
        # Bodies
        ax.add_collection(
            LineCollection(
                make_bodies(sub.index, sub[CONST.OPEN_PRICE], sub[CONST.CLOSE_PRICE]),
                colors=color,
                linewidths=6,
            )
        )

    # Axes formatting
    ax.set_title(title or f"Trend Candlesticks – {parquet_path.name}")
    ax.set_xlabel("Time")
    ax.set_ylabel("Price (USDT)")
    ax.yaxis.set_major_formatter(mticker.FuncFormatter(lambda x, _: f"{x:.0f}€"))
    ax.xaxis_date()
    ax.xaxis.set_major_formatter(mdates.DateFormatter("%y-%m-%d %H:%M"))
    plt.xticks(rotation=45)
    plt.grid(True, alpha=0.3)
    ax.autoscale_view()

    plt.show(block=True)
