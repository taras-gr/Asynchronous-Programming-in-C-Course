using Newtonsoft.Json;
using StockAnalyzer.Core;
using StockAnalyzer.Core.Domain;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Navigation;

namespace StockAnalyzer.Windows;

public partial class MainWindow : Window
{
    private static string API_URL = "https://ps-async.fekberg.com/api/stocks";
    private Stopwatch stopwatch = new Stopwatch();
    private CancellationTokenSource? CancellationTokenSource;
    private CancellationToken cancellationToken;

    public MainWindow()
    {
        //cancellationToken = CancellationTokenSource.Token;
        InitializeComponent();
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        //CancellationTokenSource = new CancellationTokenSource();
        if (CancellationTokenSource is not null)
        {
            CancellationTokenSource.Cancel();
            CancellationTokenSource.Dispose();
        }
    }

    private void Search_Click(object sender, RoutedEventArgs e)
    {
        CancellationTokenSource = new CancellationTokenSource();
        try
        {
            BeforeLoadingStockData();

            CancellationTokenSource.Token.Register(() => Notes.Text = "Z delegate");

            var loadLinesTask = SearchForStocks(CancellationTokenSource.Token);

            loadLinesTask.ContinueWith(t =>
            {
                Dispatcher.Invoke(() =>
                {
                    Notes.Text = t.Exception?.InnerException.Message ?? "Lines loaded successfully.";
                });
            }, TaskContinuationOptions.OnlyOnFaulted);

            var processStocksTask = loadLinesTask.ContinueWith((completedTask) =>
            {
                var lines = completedTask.Result;

                var data = new List<StockPrice>();

                foreach (var line in lines.Skip(1))
                {
                    var price = StockPrice.FromCSV(line);

                    data.Add(price);
                }

                Dispatcher.Invoke(() =>
                {
                    Stocks.ItemsSource = data.Where(sp => sp.Identifier == StockIdentifier.Text);
                });
            },
            TaskContinuationOptions.OnlyOnRanToCompletion
            );

            processStocksTask.ContinueWith(_ =>
            {
                Dispatcher.Invoke(() =>
                {
                    AfterLoadingStockData();
                });
            });
        }
        catch (Exception ex)
        {
            Notes.Text = ex.Message;
        }
        finally
        {
            //AfterLoadingStockData();
        }
    }

    private Task<List<string>> SearchForStocks(CancellationToken cancellationToken)
    {
        return Task.Run(async () =>
        {
            Thread.Sleep(5000); // Simulate some delay for loading data
            using var stream = new StreamReader(File.OpenRead("StockPrices_Small.csv"));
            var lines = new List<string>();

            while (await stream.ReadLineAsync() is string line)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    Dispatcher.Invoke(() =>
                    {
                        Notes.Text = "Cancelled!";
                    });
                    break;
                }
                lines.Add(line);
            }

            return lines;
        }, cancellationToken);
    }

    private async Task GetStocks()
    {
        try
        {
            var store = new DataStore();

            var responseTask = store.GetStockPrices(StockIdentifier.Text);

            Stocks.ItemsSource = await responseTask;
        }
        catch (System.Exception ex)
        {
            throw;
        }
    }






    private void BeforeLoadingStockData()
    {
        stopwatch.Restart();
        StockProgress.Visibility = Visibility.Visible;
        StockProgress.IsIndeterminate = true;
    }

    private void AfterLoadingStockData()
    {
        StocksStatus.Text = $"Loaded stocks for {StockIdentifier.Text} in {stopwatch.ElapsedMilliseconds}ms";
        StockProgress.Visibility = Visibility.Hidden;
    }

    private void Hyperlink_OnRequestNavigate(object sender, RequestNavigateEventArgs e)
    {
        Process.Start(new ProcessStartInfo { FileName = e.Uri.AbsoluteUri, UseShellExecute = true });

        e.Handled = true;
    }

    private void Close_OnClick(object sender, RoutedEventArgs e)
    {
        Application.Current.Shutdown();
    }
}