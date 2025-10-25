class PlotSettings:
    """
    Configurable flags for controlling which graphs to render.
    """

    def __init__(self, plot_candles_graph=True, plot_linear_graph=True):
        self.plot_candles_graph = plot_candles_graph
        self.plot_linear_graph = plot_linear_graph
