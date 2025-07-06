using System.Diagnostics;
using System.Threading.Tasks;

namespace ConsoleApp3
{
    internal class Program
    {
        static object syncRoot = new object();
        static object lock1 = new object();
        static object lock2 = new object();

        static AsyncLocal<decimal?> threadLocalValue = new();
        static async Task Main(string[] args)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            //int total = 0;

            //Parallel.For(0, 100, (i) =>
            //{
            //    var tempRes = Compute(i);
            //    lock (syncRoot)
            //    {
            //        total += tempRes;
            //    }
            //});

            //Parallel.For(0, 100, (i) =>
            //{
            //    var tempRes = Compute(i);
            //    Interlocked.Add(ref total, (int)tempRes);
            //});

            //for (int i = 0; i < 100; i++)
            //{
            //    total += Compute(i);
            //}


            //var t1 = Task.Run(() =>
            //{
            //    lock(lock1)
            //    {
            //        Thread.Sleep(1);
            //        lock (lock2)
            //        {
            //            Console.WriteLine("hello");
            //        }
            //    }
            //});
            //var t2 = Task.Run(() =>
            //{ 
            //    lock(lock2)
            //    {
            //        Thread.Sleep(1);
            //        lock (lock1)
            //        {
            //            Console.WriteLine("world");
            //        }
            //    }
            //});

            //await Task.WhenAll(t1, t2);

            //var cancellationTokenSource = new CancellationTokenSource();
            //cancellationTokenSource.CancelAfter(2000); // Cancel after 1 second

            //var parallelOptions = new ParallelOptions
            //{
            //    MaxDegreeOfParallelism = 1,
            //    CancellationToken = cancellationTokenSource.Token
            //};
            //int total = 0;
            //try
            //{
            //    Parallel.For(0, 100, parallelOptions, (i) =>
            //    {
            //        Interlocked.Add(ref total, (int)Compute(i));
            //    });
            //}
            //catch (OperationCanceledException ex)
            //{
            //    Console.WriteLine("Cancellation requested");
            //    Console.WriteLine(total);
            //}
            threadLocalValue.Value = 200;
            var options = new ParallelOptions
            {
                MaxDegreeOfParallelism = 1
            };
            Parallel.For(0, 100, options, async (i) =>
            {
                var currentValue = threadLocalValue.Value;
                threadLocalValue.Value = Compute(i);
            });


            //Console.WriteLine(total);
            //Console.WriteLine($"It took: {stopwatch.ElapsedMilliseconds}ms to run");
            Console.ReadLine();
        }

        static Random random = new Random();
        static decimal Compute(int value)
        {
            var randomMilliseconds = random.Next(1, 50);
            var end = DateTime.Now + TimeSpan.FromMilliseconds(randomMilliseconds);

            while (DateTime.Now < end)
            {
                // Simulate some work
            }

            return value + 0.5m;
        }
    }
}
