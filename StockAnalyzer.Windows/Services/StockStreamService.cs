using StockAnalyzer.Core.Domain;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace StockAnalyzer.Windows.Services
{
    public interface IStockStreamService
    {
        IAsyncEnumerable<StockPrice> GetAllStockPrices(
            CancellationToken cancellationToken = default);
    }

    public class MockStockStreamService : IStockStreamService
    {
        public async IAsyncEnumerable<StockPrice> GetAllStockPrices(
            CancellationToken cancellationToken = default)
        {
            await Task.Delay(500, cancellationToken);

            yield return new StockPrice { Identifier = "MSFT", Change = 0.5m };

            await Task.Delay(500, cancellationToken);

            yield return new StockPrice { Identifier = "MSFT", Change = 0.2m };

            await Task.Delay(500, cancellationToken);

            yield return new StockPrice { Identifier = "GOOG", Change = 0.8m };

            await Task.Delay(500, cancellationToken);

            yield return new StockPrice { Identifier = "GOOG", Change = 1m };
        }
    }

    public class StockDiskStreamService : IStockStreamService
    {
        public async IAsyncEnumerable<StockPrice> GetAllStockPrices(CancellationToken cancellationToken = default)
        {
            using var stream = new StreamReader(File.OpenRead("StockPrices_Small.csv"));

            await stream.ReadLineAsync();

            while (await stream.ReadLineAsync() is string line)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                yield return StockPrice.FromCSV(line);
            }
        }
    }
}
