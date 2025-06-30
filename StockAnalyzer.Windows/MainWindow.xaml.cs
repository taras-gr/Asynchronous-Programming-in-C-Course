using Newtonsoft.Json;
using StockAnalyzer.Core;
using StockAnalyzer.Core.Domain;
using StockAnalyzer.Core.Services;
using StockAnalyzer.Windows.Services;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

    public MainWindow()
    {
        InitializeComponent();
    }


    CancellationTokenSource? cancellationTokenSource;

    private async void Search_ClickDumb(object sender, RoutedEventArgs e)
    {
        Notes.Text = "";
    }

    private async Task Run()
    {
        var result = await Task.Run(() => "Plualsight");

        if (result == "Pluralsight")
        {
            Debug.WriteLine(result);
        }
    }

    private async void Search_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            BeforeLoadingStockData();
            //var progress = new Progress<IEnumerable<StockPrice>>();
            //progress.ProgressChanged += (_, stocks) =>
            //{
            //    StockProgress.Value += 1;
            //    Notes.Text += $"Loaded {stocks.Count()} stocks for {stocks.First().Identifier}{Environment.NewLine}";
            //};

            var data = await GetStockPrices();
            //var data = await GetStocksFor(StockIdentifier.Text);

            Notes.Text = $"Stocks loaded";


            Stocks.ItemsSource = data;
        }
        catch (Exception ex)
        {
            Notes.Text = ex.Message;
        }
        finally
        {
            AfterLoadingStockData();
        }
        //if (cancellationTokenSource is not null)
        //{
        //    // Already have an instance of the cancellation token source?
        //    // This means the button has already been pressed!

        //    cancellationTokenSource.Cancel();
        //    cancellationTokenSource.Dispose();
        //    cancellationTokenSource = null;

        //    Search.Content = "Search";
        //    return;
        //}

        //try
        //{
        //    cancellationTokenSource = new();

        //    cancellationTokenSource.Token.Register(() => {
        //        Notes.Text = "Cancellation requested";
        //    });

        //    Search.Content = "Cancel"; // Button text

        //    BeforeLoadingStockData();

        //    var identifiers = StockIdentifier.Text.Split(',', ' ');

        //    var service = new StockService();

        //    var loadingTasks = new List<Task<IEnumerable<StockPrice>>>();
        //    var stocks = new ConcurrentBag<StockPrice>();

        //    foreach (var identifier in identifiers)
        //    {
        //        var loadTask = service.GetStockPricesFor(identifier, cancellationTokenSource.Token);

        //        loadTask = loadTask.ContinueWith(t =>
        //        {
        //            var aFewStocks = t.Result.Take(5);

        //            foreach (var stock in aFewStocks)
        //            {
        //                stocks.Add(stock);
        //            }

        //            Dispatcher.Invoke(() =>
        //            {
        //                Stocks.ItemsSource = stocks.ToArray();
        //            });

        //            return aFewStocks;
        //        });

        //        loadingTasks.Add(loadTask);
        //    }

        //    var timeout = Task.Delay(7000);
        //    var allStocksLoadingTask = Task.WhenAll(loadingTasks);

        //    var completedTask = await Task.WhenAny(allStocksLoadingTask, timeout);

        //    if (completedTask == timeout)
        //    {
        //        cancellationTokenSource.Cancel();
        //        throw new OperationCanceledException("Timeout!");
        //    }
        //}
        //catch (Exception ex)
        //{
        //    Notes.Text = ex.Message;
        //}
        //finally
        //{
        //    AfterLoadingStockData();
        //    cancellationTokenSource?.Dispose();
        //    cancellationTokenSource = null;

        //    Search.Content = "Search";
        //}
    }

    private Task<IEnumerable<StockPrice>> GetStockPrices()
    {
        var tcs = new TaskCompletionSource<IEnumerable<StockPrice>>();

        ThreadPool.QueueUserWorkItem(_ =>
        {
            var lines = File.ReadAllLines("StockPrices_Small.csv");
            var prices = new List<StockPrice>();

            foreach (var line in lines.Skip(1)) // Skip header
            {
                var price = StockPrice.FromCSV(line);
                prices.Add(price);
            }

            tcs.SetResult(prices);
        });

        return tcs.Task;
    }

    private async Task SearchForStocks()
    {
        var service = new StockService();
        var loadingTasks = new List<Task<IEnumerable<StockPrice>>>();

        foreach (var identifier in StockIdentifier.Text.Split(',', ' '))
        {
            var loadTask = service.GetStockPricesFor(identifier,
                CancellationToken.None);

            loadTask = loadTask.ContinueWith(t =>
            {
                //progress?.Report(t.Result);
                Dispatcher.Invoke(() =>
                {
                    StockProgress.Value += 1;
                    Notes.Text += $"Loaded {t.Result.Count()} stocks for {t.Result.First().Identifier}{Environment.NewLine}";
                });
                
                return t.Result;
            });

            loadingTasks.Add(loadTask);
        }

        var data = await Task.WhenAll(loadingTasks);
    }

    private async Task<IEnumerable<StockPrice>> GetStocksFor(string stockIdentifier)
    {
        var service = new StockService();
        var data = await service.GetStockPricesFor(stockIdentifier, CancellationToken.None).ConfigureAwait(false);


        return data.Take(5);
    }









    private static Task<List<string>> SearchForStocks(
        CancellationToken cancellationToken
    )
    {
        return Task.Run(async () =>
        {
            using var stream = new StreamReader(File.OpenRead("StockPrices_Small.csv"));

            var lines = new List<string>();

            while (await stream.ReadLineAsync() is string line)
            {
                if (cancellationToken.IsCancellationRequested)
                {
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
        catch (Exception ex)
        {
            throw;
        }
    }






    private void BeforeLoadingStockData()
    {
        stopwatch.Restart();
        StockProgress.Visibility = Visibility.Visible;
        StockProgress.IsIndeterminate = false;
        StockProgress.Value = 0;
        StockProgress.Maximum = StockIdentifier.Text.Split(',', ' ').Length;
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