namespace ConsoleApp4
{
    internal class Program
    {
        static ThreadLocal<int> _threadLocalValue = new ThreadLocal<int>();
        static void Main(string[] args)
        {
            Thread t1 = new Thread(() =>
            {
                for (int i = 0; i < 3; i++)
                {
                    _threadLocalValue.Value += 1;
                    Console.WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId}: Value: {_threadLocalValue.Value}");
                }                
                
            });
            Thread t2 = new Thread(() =>
            {
                for (int i = 0; i < 3; i++)
                {
                    _threadLocalValue.Value += 1;
                    Console.WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId}: Value: {_threadLocalValue.Value}");
                }
            });
            t1.Start();
            t2.Start();

            t1.Join();
            t2.Join();
            Console.WriteLine(_threadLocalValue.Value);
            Console.WriteLine("Hello, World!");
        }
    }
}
