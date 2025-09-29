using Cryptobot.ConsoleApp.Backtesting.OutputModels;
using Cryptobot.ConsoleApp.Bybit.Models;
using Parquet;
using Parquet.Data;
using Parquet.Schema;

namespace Cryptobot.ConsoleApp.Utils;

public static class ParquetManager
{
    // CREATE

    public static async Task SaveBybitCandles(IEnumerable<BybitCandle> candles, string filepath)
    {
        PathHelper.CheckFixFilepathExtensions(ref filepath, Constants.PARQUET);

        var schema = new ParquetSchema(
            new DateTimeDataField(nameof(BybitCandle.OpenTime), DateTimeFormat.DateAndTime),
            new DateTimeDataField(nameof(BybitCandle.CloseTime), DateTimeFormat.DateAndTime),
            new DataField<double>(nameof(BybitCandle.OpenPrice)),
            new DataField<double>(nameof(BybitCandle.ClosePrice)),
            new DataField<double>(nameof(BybitCandle.HighPrice)),
            new DataField<double>(nameof(BybitCandle.LowPrice)),
            new DataField<double>(nameof(BybitCandle.Volume)),
            new DataField<double>(nameof(BybitCandle.QuoteVolume)));

        await using var stream = File.Create(filepath);
        using var writer = await ParquetWriter.CreateAsync(schema, stream);

        using var groupWriter = writer.CreateRowGroup();
        await groupWriter.WriteColumnAsync(new DataColumn((DateTimeDataField)schema[0], candles.Select(c => c.OpenTime).ToArray()));
        await groupWriter.WriteColumnAsync(new DataColumn((DateTimeDataField)schema[1], candles.Select(c => c.CloseTime).ToArray()));
        await groupWriter.WriteColumnAsync(new DataColumn((DataField<double>)schema[2], candles.Select(c => c.OpenPrice).ToArray()));
        await groupWriter.WriteColumnAsync(new DataColumn((DataField<double>)schema[3], candles.Select(c => c.ClosePrice).ToArray()));
        await groupWriter.WriteColumnAsync(new DataColumn((DataField<double>)schema[4], candles.Select(c => c.HighPrice).ToArray()));
        await groupWriter.WriteColumnAsync(new DataColumn((DataField<double>)schema[5], candles.Select(c => c.LowPrice).ToArray()));
        await groupWriter.WriteColumnAsync(new DataColumn((DataField<double>)schema[6], candles.Select(c => c.Volume).ToArray()));
        await groupWriter.WriteColumnAsync(new DataColumn((DataField<double>)schema[7], candles.Select(c => c.QuoteVolume).ToArray()));
    }

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
            new DataField<DateTime>(nameof(BybitOutputCandle.OpenTime)),
            new DataField<DateTime>(nameof(BybitOutputCandle.CloseTime)),
            new DataField<double>(nameof(BybitOutputCandle.OpenPrice)),
            new DataField<double>(nameof(BybitOutputCandle.ClosePrice)),
            new DataField<double>(nameof(BybitOutputCandle.HighPrice)),
            new DataField<double>(nameof(BybitOutputCandle.LowPrice)),
            new DataField<double>(nameof(BybitOutputCandle.Volume)));

        using var fileStream = File.Create(filepath);
        using var writer = await ParquetWriter.CreateAsync(schema, fileStream);

        using var groupWriter = writer.CreateRowGroup();
        await groupWriter.WriteColumnAsync(new DataColumn((DataField<double?>)schema[0], candles.Select(x => x.EntryPrice).ToArray()));
        await groupWriter.WriteColumnAsync(new DataColumn((DataField<double?>)schema[1], candles.Select(x => x.StopLoss).ToArray()));
        await groupWriter.WriteColumnAsync(new DataColumn((DataField<double?>)schema[2], candles.Select(x => x.TakeProfit).ToArray()));
        await groupWriter.WriteColumnAsync(new DataColumn((DataField<double?>)schema[3], candles.Select(x => x.EntryTradeSize).ToArray()));
        await groupWriter.WriteColumnAsync(new DataColumn((DataField<double?>)schema[4], candles.Select(x => x.ExitPrice).ToArray()));
        await groupWriter.WriteColumnAsync(new DataColumn((DataField<double?>)schema[5], candles.Select(x => x.PnL).ToArray()));
        await groupWriter.WriteColumnAsync(new DataColumn((DataField<double?>)schema[6], candles.Select(x => x.TotalPnL).ToArray()));
        await groupWriter.WriteColumnAsync(new DataColumn((DataField<bool?>)schema[7], candles.Select(x => x.IsProfit).ToArray()));
        await groupWriter.WriteColumnAsync(new DataColumn((DataField<DateTime>)schema[8], candles.Select(x => x.OpenTime).ToArray()));
        await groupWriter.WriteColumnAsync(new DataColumn((DataField<DateTime>)schema[9], candles.Select(x => x.CloseTime).ToArray()));
        await groupWriter.WriteColumnAsync(new DataColumn((DataField<double>)schema[10], candles.Select(x => x.OpenPrice).ToArray()));
        await groupWriter.WriteColumnAsync(new DataColumn((DataField<double>)schema[11], candles.Select(x => x.ClosePrice).ToArray()));
        await groupWriter.WriteColumnAsync(new DataColumn((DataField<double>)schema[12], candles.Select(x => x.HighPrice).ToArray()));
        await groupWriter.WriteColumnAsync(new DataColumn((DataField<double>)schema[13], candles.Select(x => x.LowPrice).ToArray()));
        await groupWriter.WriteColumnAsync(new DataColumn((DataField<double>)schema[14], candles.Select(x => x.Volume).ToArray()));
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

    // READ

    public static async Task<List<BybitCandle>> LoadCandlesAsync(string filepath)
    {
        PathHelper.CheckFixFilepathExtensions(ref filepath, Constants.PARQUET);

        var candles = new List<BybitCandle>();

        await using var stream = File.OpenRead(filepath);
        using var reader = await ParquetReader.CreateAsync(stream);

        for (var g = 0; g < reader.RowGroupCount; g++)
        {
            using var groupReader = reader.OpenRowGroupReader(g);

            var openTimeCol = await groupReader.ReadColumnAsync((DateTimeDataField)reader.Schema[0]);
            var closeTimeCol = await groupReader.ReadColumnAsync((DateTimeDataField)reader.Schema[1]);
            var openPriceCol = await groupReader.ReadColumnAsync((DataField)reader.Schema[2]);
            var closePriceCol = await groupReader.ReadColumnAsync((DataField)reader.Schema[3]);
            var highPriceCol = await groupReader.ReadColumnAsync((DataField)reader.Schema[4]);
            var lowPriceCol = await groupReader.ReadColumnAsync((DataField)reader.Schema[5]);
            var volumeCol = await groupReader.ReadColumnAsync((DataField)reader.Schema[6]);
            var quoteVolumeCol = await groupReader.ReadColumnAsync((DataField)reader.Schema[7]);

            var openTimes = openTimeCol.Data as DateTime[];
            var closeTimes = closeTimeCol.Data as DateTime[];
            var openPrices = openPriceCol.Data as double[];
            var closePrices = closePriceCol.Data as double[];
            var highPrices = highPriceCol.Data as double[];
            var lowPrices = lowPriceCol.Data as double[];
            var volumes = volumeCol.Data as double[];
            var quoteVolumes = quoteVolumeCol.Data as double[];

            for (var i = 0; i < openTimes!.Length; i++)
            {
                candles.Add(new BybitCandle
                {
                    OpenTime = openTimes![i],
                    CloseTime = closeTimes![i],
                    OpenPrice = openPrices![i],
                    ClosePrice = closePrices![i],
                    HighPrice = highPrices![i],
                    LowPrice = lowPrices![i],
                    Volume = volumes![i],
                    QuoteVolume = quoteVolumes![i],
                });
            }
        }

        return candles;
    }
}
