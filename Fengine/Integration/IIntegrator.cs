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

    /// <summary>
    ///     Makes 1D grid with 11 points in it
    /// </summary>
    /// <param name="leftBorder">Left border point</param>
    /// <param name="rightBorder">Right border point</param>
    /// <returns>integrationGrid from leftBorder to rightBorder with 11 points in it</returns>
    double[] Create1DIntegrationMesh(double leftBorder, double rightBorder)
    {
        var integrationGrid = new double[11];
        var step = (rightBorder - leftBorder) / 10.0;

        for (var i = 0; i < integrationGrid.Length; i++)
        {
            integrationGrid[i] = leftBorder + i * step;
        }

        return integrationGrid;
    }
}