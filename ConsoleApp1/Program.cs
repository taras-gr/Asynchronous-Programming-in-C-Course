Console.WriteLine("Starting");

//await Task.Factory.StartNew(() =>
//{
//    Task.Factory.StartNew(() =>
//    {
//        Thread.Sleep(1000);

//        Console.WriteLine("Completed 1");
//    }, TaskCreationOptions.AttachedToParent);
//    Task.Factory.StartNew(() =>
//    {
//        Thread.Sleep(1000);

//        Console.WriteLine("Completed 2");
//    }, TaskCreationOptions.AttachedToParent);
//    Task.Factory.StartNew(() =>
//    {
//        Thread.Sleep(1000);

//        Console.WriteLine("Completed 3");
//    }, TaskCreationOptions.AttachedToParent);
//}, TaskCreationOptions.DenyChildAttach);

//var task = Task.Factory.StartNew(async () =>
//{
//    await Task.Delay(2000);

//    return "Pluralsight";
//}).Unwrap();

//var result = await task;

//Console.WriteLine("Completed");

//Console.ReadLine();

for (int i = 0; i < 50; i++)
{
    Task.Run(() => Console.WriteLine(i)); // ЗАМІКАННЯ i
}