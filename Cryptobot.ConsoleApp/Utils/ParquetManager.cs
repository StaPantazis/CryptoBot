using Cryptobot.ConsoleApp.Backtesting.OutputModels;
using Parquet;
using Parquet.Data;
using Parquet.Schema;

namespace Cryptobot.ConsoleApp.Utils;

public static class ParquetManager
{
    public static async Task SaveCandlesAsync(IEnumerable<BybitOutputCandle> candles, string filepath)
    {
        PathHelper.CheckFixFilepathExtensions(ref filepath, Constants.PARQUET);

        var schema = new ParquetSchema(
            new DataField<double?>(nameof(BybitOutputCandle.EntryPrice)),
            new DataField<double?>(nameof(BybitOutputCandle.StopLoss)),
            new DataField<double?>(nameof(BybitOutputCandle.TakeProfit)),
            new DataField<double?>(nameof(BybitOutputCandle.EntryTradeSize)),
            new DataField<double?>(nameof(BybitOutputCandle.ExitPrice)),
            new DataField<double?>(nameof(BybitOutputCandle.PnL)),
            new DataField<double?>(nameof(BybitOutputCandle.TotalPnL)),
            new DataField<bool?>(nameof(BybitOutputCandle.IsProfit)),
            new DataField<DateTime>(nameof(BybitOutputCandle.CloseTime)),
            new DataField<double>(nameof(BybitOutputCandle.OpenPrice)),
            new DataField<double>(nameof(BybitOutputCandle.ClosePrice)),
            new DataField<double>(nameof(BybitOutputCandle.HighPrice)),
            new DataField<double>(nameof(BybitOutputCandle.LowPrice)),
            new DataField<double>(nameof(BybitOutputCandle.Volume)));

        using var fileStream = File.Create(filepath);
        using var writer = await ParquetWriter.CreateAsync(schema, fileStream);

        // Open a row group
        using var groupWriter = writer.CreateRowGroup();

        // Write each column
        await groupWriter.WriteColumnAsync(new DataColumn((DataField<double?>)schema[0], candles.Select(x => x.EntryPrice).ToArray()));
        await groupWriter.WriteColumnAsync(new DataColumn((DataField<double?>)schema[1], candles.Select(x => x.StopLoss).ToArray()));
        await groupWriter.WriteColumnAsync(new DataColumn((DataField<double?>)schema[2], candles.Select(x => x.TakeProfit).ToArray()));
        await groupWriter.WriteColumnAsync(new DataColumn((DataField<double?>)schema[3], candles.Select(x => x.EntryTradeSize).ToArray()));
        await groupWriter.WriteColumnAsync(new DataColumn((DataField<double?>)schema[4], candles.Select(x => x.ExitPrice).ToArray()));
        await groupWriter.WriteColumnAsync(new DataColumn((DataField<double?>)schema[5], candles.Select(x => x.PnL).ToArray()));
        await groupWriter.WriteColumnAsync(new DataColumn((DataField<double?>)schema[6], candles.Select(x => x.TotalPnL).ToArray()));
        await groupWriter.WriteColumnAsync(new DataColumn((DataField<bool?>)schema[7], candles.Select(x => x.IsProfit).ToArray()));
        await groupWriter.WriteColumnAsync(new DataColumn((DataField<DateTime>)schema[8], candles.Select(x => x.CloseTime).ToArray()));
        await groupWriter.WriteColumnAsync(new DataColumn((DataField<double>)schema[9], candles.Select(x => x.OpenPrice).ToArray()));
        await groupWriter.WriteColumnAsync(new DataColumn((DataField<double>)schema[10], candles.Select(x => x.ClosePrice).ToArray()));
        await groupWriter.WriteColumnAsync(new DataColumn((DataField<double>)schema[11], candles.Select(x => x.HighPrice).ToArray()));
        await groupWriter.WriteColumnAsync(new DataColumn((DataField<double>)schema[12], candles.Select(x => x.LowPrice).ToArray()));
        await groupWriter.WriteColumnAsync(new DataColumn((DataField<double>)schema[13], candles.Select(x => x.Volume).ToArray()));
    }

    public static async Task SaveLinearGraph(List<LinearGraphNode> nodes, string filepath)
    {
        PathHelper.CheckFixFilepathExtensions(ref filepath, Constants.PARQUET);

        var schema = new ParquetSchema(
            new DataField<int>(nameof(LinearGraphNode.TradeIndex)),
            new DataField<double?>(nameof(LinearGraphNode.PnL)),
            new DataField<double>(nameof(LinearGraphNode.Budget)),
            new DataField<bool?>(nameof(LinearGraphNode.IsProfit)));

        using var fileStream = File.Create(filepath);
        using var writer = await ParquetWriter.CreateAsync(schema, fileStream);

        // Open a row group
        using var groupWriter = writer.CreateRowGroup();

        // Write each column
        await groupWriter.WriteColumnAsync(new DataColumn((DataField<int>)schema[0], nodes.Select(x => x.TradeIndex).ToArray()));
        await groupWriter.WriteColumnAsync(new DataColumn((DataField<double?>)schema[1], nodes.Select(x => x.PnL).ToArray()));
        await groupWriter.WriteColumnAsync(new DataColumn((DataField<double>)schema[2], nodes.Select(x => x.Budget).ToArray()));
        await groupWriter.WriteColumnAsync(new DataColumn((DataField<bool?>)schema[3], nodes.Select(x => x.IsProfit).ToArray()));
    }
}
