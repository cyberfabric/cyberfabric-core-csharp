// See https://aka.ms/new-console-template for more information

internal class Program
{
    public static void Main(string[] args)
    {
        var task = Task.Delay(1000).ContinueWith(t => "test");
        Console.WriteLine(task.Result);
        Console.WriteLine(task.GetAwaiter().GetResult());
    }
}