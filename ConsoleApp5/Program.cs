using System.Diagnostics;

namespace ConsoleApp5
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            var result = Enumerable.Range(0, 100)
                .AsParallel()
                .AsOrdered()
                .Select(Compute)
                .Take(10);

            result.ForAll(x => Console.WriteLine(x));

            Console.WriteLine(result);
            Console.WriteLine($"It took: {stopwatch.ElapsedMilliseconds}ms to run");
            Console.ReadLine();

        }

        static Random Random = new Random();
        static decimal Compute(int value)
        {
            var randomMilliseconds = Random.Next(1, 50);
            var end = DateTime.Now + TimeSpan.FromMilliseconds(randomMilliseconds);

            while (DateTime.Now < end)
            {
                // Simulate some work
            }

            return value + 0.5m;
        }
    }
}
