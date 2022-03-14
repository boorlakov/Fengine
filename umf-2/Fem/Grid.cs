namespace umf_2.Fem;

public class Grid
{
    public double[] X { get; }

    public Grid(JsonModels.Area area)
    {
        var x = new double[area.AmountPoints];
        x[0] = area.LeftBorder;

        if (Math.Abs(area.DischargeRatio - 1) > 1e-10)
        {
            var sumKx = (1 - Math.Pow(area.DischargeRatio, area.AmountPoints - 1)) /
                        (1 - area.DischargeRatio);
            var hX = (area.RightBorder - area.LeftBorder) / sumKx;

            for (var i = 1; i < area.AmountPoints; i++)
            {
                x[i] = area.LeftBorder +
                       hX * (1 - Math.Pow(area.DischargeRatio, i)) / (1 - area.DischargeRatio);
            }
        }
        else
        {
            var hX = (area.RightBorder - area.LeftBorder) / (area.AmountPoints - 1);

            for (var i = 1; i < area.AmountPoints; i++)
            {
                x[i] = area.LeftBorder + i * hX;
            }
        }

        X = x.ToArray();
    }
}