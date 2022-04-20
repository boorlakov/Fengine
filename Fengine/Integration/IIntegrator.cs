namespace Fengine.Integration;

public interface IIntegrator
{
    /// <summary>
    ///     Integrates a 1 dimensional functionFromString of given grid
    /// </summary>
    /// <param name="grid"> Array grid </param>
    /// <param name="function"> Function to integrate. Note: must have 1-dimension </param>
    /// <returns> Value of the definite integral </returns>
    double Integrate1D(double[] grid, Func<double, double> function);

    /// <summary>
    ///     Integrates a 1 dimensional function (in string form) of given grid
    /// </summary>
    /// <param name="grid"> Array grid </param>
    /// <param name="funcFromString"> Function to integrate. Note: must have 1-dimension </param>
    /// <returns> Value of the definite integral </returns>
    double Integrate1D(double[] grid, string funcFromString);
}