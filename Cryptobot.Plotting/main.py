import plotter
from plotSettings import PlotSettings
from pathlib import Path


def list_files(directory: Path, pattern: str):
    """Return all files matching a pattern (e.g., '*.parquet' or '*.zip')."""
    if not directory.exists():
        print(f"Directory not found: {directory}")
        return []
    return sorted(directory.glob(pattern))


def choose_file(files):
    """Ask user to pick a file from a list."""
    if not files:
        print("No files found.")
        return None

    print("\nAvailable files:")
    for i, f in enumerate(files, 1):
        print(f"{i}) {f.name}")

    while True:
        choice = input("\nSelect a file by number (or press Enter to cancel): ").strip()
        if not choice:
            return None
        if choice.isdigit() and 1 <= int(choice) <= len(files):
            return files[int(choice) - 1]
        print("Invalid choice. Try again.")


def main():
    base_dir = Path(__file__).resolve().parent
    output_root = base_dir.parent / "Cryptobot.ConsoleApp" / "Backtesting" / "Output"
    plotSettings = PlotSettings(plot_candles_graph=False, plot_linear_graph=True)

    while True:
        print("\n=== Cryptobot Viewer ===")
        print("1) View Backtest")
        print("2) View Trend Profiling")
        print("3) Exit")

        option = input("Choose an option: ").strip()

        if option == "1":
            folder = output_root / "Backtest"
            files = list_files(folder, "*.zip")
            selected = choose_file(files)
            if selected:
                print(f"\nSelected: {selected.name}")
                plotter.plot_backtest_candles(selected, plotSettings)

        elif option == "2":
            folder = output_root / "TrendProfiling"
            files = list_files(folder, "*.parquet")
            selected = choose_file(files)
            if selected:
                print(f"\nSelected: {selected.name}")
                plotter.plot_trend_profiling_candles(selected)

        elif option == "3":
            print("Goodbye!")
            break

        else:
            print("Invalid choice, try again.")


if __name__ == "__main__":
    main()
