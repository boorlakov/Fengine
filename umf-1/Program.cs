using System.Text.Json;

namespace umf_1;

public static class Program
{
    public static void Main()
    {
        var content = File.ReadAllText("input.json");
        var input = JsonSerializer.Deserialize<JsonModel>(content);
        var grid = new Grid(input);
    }
}