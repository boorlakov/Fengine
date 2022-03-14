namespace umf_2.Integration;

public static class Integrator
{
    /// <summary>
    /// Integrates a 1 dimensional functionFromString of given grid
    /// </summary>
    /// <param name="grid"> Array grid </param>
    /// <param name="function"> Function to integrate. Note: must have 1-dimension </param>
    /// <returns> Value of the definite integral </returns>
    public static double Integrate1D(double[] grid, Func<double, double> function)
    {
        var ti = new[]
        {
            -0.8611363116,
            -0.3399810436,
            0.3399810436,
            0.8611363116
        };

        var ci = new[]
        {
            0.3478548451,
            0.6521451549,
            0.6521451549,
            0.3478548451
        };

        var t = (grid[1] - grid[0]) / 2.0;

        var preRes = 0.0;
        var res = 0.0;

        for (var i = 0; i < grid.Length - 1; i++)
        {
            var c = (grid[i + 1] + grid[i]) / 2.0;

            for (var j = 0; j < 4; j++)
            {
                var arg = t * ti[j] + c;
                preRes += ci[j] * function(arg);
            }

            res = preRes * t;
        }

        return res;
    }

    public static double Integrate1DWithDict(double[] grid, Func<Dictionary<string, double>, double> function)
    {
        var ti = new[]
        {
            -0.8611363116,
            -0.3399810436,
            0.3399810436,
            0.8611363116
        };

        var ci = new[]
        {
            0.3478548451,
            0.6521451549,
            0.6521451549,
            0.3478548451
        };

        var t = (grid[1] - grid[0]) / 2.0;

        var preRes = 0.0;
        var res = 0.0;

        for (var i = 0; i < grid.Length - 1; i++)
        {
            var c = (grid[i + 1] + grid[i]) / 2.0;

            for (var j = 0; j < 4; j++)
            {
                var arg = t * ti[j] + c;
                preRes += ci[j] * function(Utils.MakeDict1D(arg));
            }

            res = preRes * t;
        }

        return res;
    }

    public static double Integrate1DWithStringFunc(double[] grid, string funcFromString)
    {
        var ti = new[]
        {
            -0.8611363116,
            -0.3399810436,
            0.3399810436,
            0.8611363116
        };

        var ci = new[]
        {
            0.3478548451,
            0.6521451549,
            0.6521451549,
            0.3478548451
        };

        var t = (grid[1] - grid[0]) / 2.0;

        var preRes = 0.0;
        var res = 0.0;

        for (var i = 0; i < grid.Length - 1; i++)
        {
            var c = (grid[i + 1] + grid[i]) / 2.0;

            for (var j = 0; j < 4; j++)
            {
                var arg = t * ti[j] + c;
                preRes += ci[j] * Utils.EvalFunc(funcFromString, arg);
            }

            res = preRes * t;
        }

        return res;
    }

    public static double[] MakeGrid(double leftBorder, double rightBorder)
    {
        var integrationGrid = new double[11];
        var step = (rightBorder - leftBorder) / 10.0;

        for (var i = 0; i < integrationGrid.Length; i++)
        {
            integrationGrid[i] = leftBorder + i * step;
        }

        return integrationGrid;
    }

    /// <summary>
    /// Makes Grid 0..1 with 0.1 step size.
    /// For my work no need to make a generative algorithm. That's fine!
    /// </summary>
    /// <returns>integrationGrid from 0 to 1 with 0.1 step size</returns>
    public static double[] Make0To1Grid()
    {
        var integrationGrid = new double[11];
        var step = 0.0;

        for (var i = 0; i < integrationGrid.Length; i++, step += 0.1)
        {
            integrationGrid[i] = step;
        }

        return integrationGrid;
    }
}