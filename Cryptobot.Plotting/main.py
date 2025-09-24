import sys
import plotter
from pathlib import Path

if __name__ == "__main__":
    BASE_DIR = Path(__file__).resolve().parent
    OUTPUT_DIR = BASE_DIR.parent / "Cryptobot.ConsoleApp" / "Backtesting" / "Output"

    first_file = sorted(OUTPUT_DIR.glob("*.gz"))[0]
    plotter.plot_candlestick(first_file)
