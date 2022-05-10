using Sprache.Calc;

namespace Fengine.Backend.Integration;

/// <summary>
///     Integrator by Gauss (4-points)
/// </summary>
public class GaussFourPoints : IIntegrator
{
    private readonly double[] _ti =
    {
        -0.8611363116,
        -0.3399810436,
        0.3399810436,
        0.8611363116
    };

    private readonly double[] _ci =
    {
        0.3478548451,
        0.6521451549,
        0.6521451549,
        0.3478548451
    };

    /// <summary>
    ///     Integrates a 1 dimensional functionFromString of given grid
    /// </summary>
    /// <param name="grid"> Array grid </param>
    /// <param name="func"> Function to integrate. Note: must have 1-dimension </param>
    /// <returns> Value of the definite integral </returns>
    public double Integrate1D(double[] grid, Func<double, double> func)
    {
        var t = (grid[1] - grid[0]) / 2.0;

        var preRes = 0.0;
        var res = 0.0;

        for (var i = 0; i < grid.Length - 1; i++)
        {
            var c = (grid[i + 1] + grid[i]) / 2.0;

            for (var j = 0; j < 4; j++)
            {
                var arg = t * _ti[j] + c;
                preRes += _ci[j] * func(arg);
            }

            res = preRes * t;
        }

        return res;
    }

    /// <summary>
    ///     Integrates a 1 dimensional func (in string form) of given grid
    /// </summary>
    /// <param name="grid"> Array grid </param>
    /// <param name="func"> Function to integrate. Note: must have 1-dimension </param>
    /// <returns> Value of the definite integral </returns>
    public double Integrate1D(double[] grid, string func)
    {
        var t = (grid[1] - grid[0]) / 2.0;

        var preRes = 0.0;
        var res = 0.0;
        var calc = new XtensibleCalculator();
        var funcToIntegrate = calc.ParseFunction(func).Compile();

        for (var i = 0; i < grid.Length - 1; i++)
        {
            var c = (grid[i + 1] + grid[i]) / 2.0;

            for (var j = 0; j < 4; j++)
            {
                var arg = t * _ti[j] + c;
                preRes += _ci[j] * funcToIntegrate(Utils.MakeDict1D(arg));
            }

            res = preRes * t;
        }

        return res;
    }

    public double Integrate1D(double[] grid, Func<Dictionary<string, double>, double> func)
    {
        var t = (grid[1] - grid[0]) / 2.0;

        var preRes = 0.0;
        var res = 0.0;

        for (var i = 0; i < grid.Length - 1; i++)
        {
            var c = (grid[i + 1] + grid[i]) / 2.0;

            for (var j = 0; j < 4; j++)
            {
                var arg = t * _ti[j] + c;
                preRes += _ci[j] * func(Utils.MakeDict1D(arg));
            }

            res = preRes * t;
        }

        return res;
    }

    /// <summary>
    /// Integrates a 2 dimensional function of given grid
    /// </summary>
    /// <param name="grid"> Array grid </param>
    /// <param name="function"> Function to integrate. Note: must have 2-dimensions </param>
    /// <returns> Value of the definite integral </returns>
    public double Integrate2D(double[] grid, Func<double, double, double> function)
    {
        var t = (grid[1] - grid[0]) / 2.0;

        var res = 0.0;

        for (var i = 0; i < grid.Length - 1; i++)
        {
            var centerZ = (grid[i + 1] + grid[i]) / 2.0;

            for (var ii = 0; ii < grid.Length - 1; ii++)
            {
                var centerR = (grid[ii + 1] + grid[ii]) / 2.0;

                for (var j = 0; j < 4; j++)
                {
                    var argR = t * _ti[j] + centerZ;

                    for (var k = 0; k < 4; k++)
                    {
                        var argZ = t * _ti[k] + centerR;
                        res += _ci[j] * _ci[k] * function(argR, argZ);
                    }
                }
            }
        }

        res *= t * t;
        return res;
    }

    public double Integrate2D(double[] grid, string func)
    {
        var calc = new XtensibleCalculator();
        var funcToIntegrate = calc.ParseFunction(func).Compile();

        var t = (grid[1] - grid[0]) / 2.0;

        var res = 0.0;

        for (var i = 0; i < grid.Length - 1; i++)
        {
            var centerZ = (grid[i + 1] + grid[i]) / 2.0;

            for (var ii = 0; ii < grid.Length - 1; ii++)
            {
                var centerR = (grid[ii + 1] + grid[ii]) / 2.0;

                for (var j = 0; j < 4; j++)
                {
                    var argR = t * _ti[j] + centerZ;

                    for (var k = 0; k < 4; k++)
                    {
                        var argZ = t * _ti[k] + centerR;
                        res += _ci[j] * _ci[k] * funcToIntegrate(Utils.MakeDict2D(argR, argZ));
                    }
                }
            }
        }

        res *= t * t;
        return res;
    }

    public double Integrate2D(double[] grid, Func<Dictionary<string, double>, double> func)
    {
        var t = (grid[1] - grid[0]) / 2.0;

        var res = 0.0;

        for (var i = 0; i < grid.Length - 1; i++)
        {
            var centerZ = (grid[i + 1] + grid[i]) / 2.0;

            for (var ii = 0; ii < grid.Length - 1; ii++)
            {
                var centerR = (grid[ii + 1] + grid[ii]) / 2.0;

                for (var j = 0; j < 4; j++)
                {
                    var argR = t * _ti[j] + centerZ;

                    for (var k = 0; k < 4; k++)
                    {
                        var argZ = t * _ti[k] + centerR;
                        res += _ci[j] * _ci[k] * func(Utils.MakeDict2D(argR, argZ));
                    }
                }
            }
        }

        res *= t * t;
        return res;
    }
}