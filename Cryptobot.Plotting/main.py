import sys
import plotter
from pathlib import Path

if __name__ == "__main__":
    BASE_DIR = Path(__file__).resolve().parent
    RESOURCES_DIR = BASE_DIR.parent / "Cryptobot.ConsoleApp" / "Resources"

    first_file = sorted(RESOURCES_DIR.glob("*.json"))[0]
    plotter.plot_candlestick(first_file)
