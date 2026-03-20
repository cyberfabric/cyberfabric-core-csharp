// See https://aka.ms/new-console-template for more information

internal class Program
{
    public static async Task Main(string[] args)
    {
        HttpClient httpClient = new HttpClient();
        var response = await httpClient.GetAsync("https://example.com");
        
        Console.WriteLine(await response.Content.ReadAsStringAsync());
    }
}