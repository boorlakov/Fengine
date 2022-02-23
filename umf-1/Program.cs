using System.Text.Json;

namespace umf_1;

public static class Program
{
    public static void Main()
    {
        var input = JsonSerializer.Deserialize<JsonModel>(File.ReadAllText("input.json"));
        var grid = new Grid(input!);
        var bim = 0;
    }
}