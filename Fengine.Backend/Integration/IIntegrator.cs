namespace Fengine.Backend.Integration;

public interface IIntegrator
{
    /// <summary>
    ///     Integrates a 1 dimensional functionFromString of given grid
    /// </summary>
    /// <param name="grid"> Array grid </param>
    /// <param name="func"> Function to integrate. Note: must have 1-dimension </param>
    /// <returns> Value of the definite integral </returns>
    double Integrate1D(double[] grid, Func<double, double> func);

    /// <summary>
    ///     Integrates a 1 dimensional func (in string form) of given grid
    /// </summary>
    /// <param name="grid"> Array grid </param>
    /// <param name="func"> Function to integrate. Note: must have 1-dimension </param>
    /// <returns> Value of the definite integral </returns>
    double Integrate1D(double[] grid, string func);

    /// <summary>
    ///     Integrates a 1 dimensional functionFromString of given grid
    /// </summary>
    /// <param name="grid"> Array grid </param>
    /// <param name="func"> Function to integrate. Note: must have 1-dimension </param>
    /// <returns> Value of the definite integral </returns>
    double Integrate1D(double[] grid, Func<Dictionary<string, double>, double> func);

    /// <summary>
    ///     Integrates a 2 dimensional functionFromString of given grid
    /// </summary>
    /// <param name="grid"> Array grid </param>
    /// <param name="func"> Function to integrate. Note: must have 2-dimension </param>
    /// <returns> Value of the definite integral </returns>
    double Integrate2D(double[] grid, Func<double, double, double> func);

    /// <summary>
    ///     Integrates a 2 dimensional func (in string form) of given grid
    /// </summary>
    /// <param name="grid"> Array grid </param>
    /// <param name="func"> Function to integrate. Note: must have 2-dimension </param>
    /// <returns> Value of the definite integral </returns>
    double Integrate2D(double[] grid, string func);

    /// <summary>
    ///     Integrates a 2 dimensional functionFromString of given grid
    /// </summary>
    /// <param name="grid"> Array grid </param>
    /// <param name="func"> Function to integrate. Note: must have 2-dimension </param>
    /// <returns> Value of the definite integral </returns>
    double Integrate2D(double[] grid, Func<Dictionary<string, double>, double> func);

    /// <summary>
    ///     Integrates a 2 dimensional functionFromString of given grid
    /// </summary>
    /// <param name="grid"> Array grid </param>
    /// <param name="func"> Function to integrate. Note: must have 2-dimension </param>
    /// <param name="makeDict2D">TODO</param>
    /// <returns> Value of the definite integral </returns>
    double Integrate2D
    (
        double[] grid,
        Func<Dictionary<string, double>, double> func,
        Func<double, double, Dictionary<string, double>> makeDict2D
    );
}