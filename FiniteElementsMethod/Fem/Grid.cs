namespace FiniteElementsMethod.Fem;

/// <summary>
/// 1D grid class. Can be uniform or non-uniform due to given discharge ratio
/// </summary>
public class Grid
{
    /// <summary>
    /// Value of points in X axis
    /// </summary>
    public double[] X { get; }

    /// <summary>
    /// Grid constructor. Can be uniform or non-uniform due to given discharge ratio
    /// </summary>
    /// <param name="area">Given area settings</param>
    public Grid(Models.Area area)
    {
        var x = new double[area.AmountPoints];
        x[0] = area.LeftBorder;

        if (Math.Abs(area.DischargeRatio - 1) > 1e-10)
        {
            // Nonuniform case
            var sumKx = (1 - Math.Pow(area.DischargeRatio, area.AmountPoints - 1)) / (1 - area.DischargeRatio);
            var hX = (area.RightBorder - area.LeftBorder) / sumKx;

            for (var i = 1; i < area.AmountPoints; i++)
            {
                x[i] = area.LeftBorder + hX * (1 - Math.Pow(area.DischargeRatio, i)) / (1 - area.DischargeRatio);
            }
        }
        else
        {
            // Uniform case
            var hX = (area.RightBorder - area.LeftBorder) / (area.AmountPoints - 1);

            for (var i = 1; i < area.AmountPoints; i++)
            {
                x[i] = area.LeftBorder + i * hX;
            }
        }

        X = x.ToArray();
    }
}