namespace umf_1;

public static class Utils
{
    public static bool CheckGridConsistency(double[] grid, IEnumerable<double> pivot)
    {
        foreach (var point in pivot)
        {
            if (Array.FindIndex(grid, x => Math.Abs(x - point) < 1e-10) < 0)
            {
                return false;
            }
        }

        return true;
    }

    public static double[] ExcludeFictiveFrom(double[] rawResVec, Grid grid)
    {
        var result = new List<double>();

        for (var i = 0; i < grid.Nodes.Length; i++)
        {
            if (!grid.Nodes[i].IsFictive)
            {
                result.Add(rawResVec[i]);
            }
        }

        return result.ToArray();
    }
}