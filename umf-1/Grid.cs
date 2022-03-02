namespace umf_1;

public class Grid
{
    public class Node
    {
        public double X { get; }
        public double Y { get; }

        public bool IsFictive { get; }

        public bool IsOnBorder { get; }
        public string BorderType { get; }

        public Node(double x, double y, bool isFictive, bool isOnBorder, string borderType)
        {
            X = x;
            Y = y;
            IsFictive = isFictive;
            IsOnBorder = isOnBorder;
            BorderType = borderType;
        }
    }

    public double[] X { get; }
    public double[] Y { get; }

    public readonly Node[] Nodes;

    private static bool IsFictiveCheck(AreaModel area, double x, double y)
    {
        //    GRID TYPE
        //   +--Upper----
        //   |         LeftUpper
        //   |          +
        //   |     |LowerRight
        //   |     |
        //   |     |
        // Left   LeftLower
        //   |     |
        //   |     |
        //   +-----+
        //    LowerLeft

        if (x > area.PivotX[1] && y < area.PivotY[1])
        {
            return true;
        }

        return false;
    }

    private static bool IsOnBorderCheck(AreaModel area, double x, double y)
    {
        // ReSharper disable CompareOfFloatsByEqualityOperator
        return x == area.PivotX[0] || x == area.PivotX[2] || y == area.PivotY[0] || y == area.PivotY[2] ||
               y == area.PivotY[1] && x >= area.PivotX[1] || x == area.PivotX[1] && y <= area.PivotY[1];
    }

    private static string GetBorderType(AreaModel area, double x, double y)
    {
        if (x == area.PivotX[0]) return "Left";

        if (y == area.PivotY[2]) return "Upper";

        if (x == area.PivotX[2] && y >= area.PivotY[1]) return "RightUpper";

        if (y == area.PivotY[1] && x >= area.PivotX[1]) return "LowerRight";

        if (x == area.PivotX[1] && y <= area.PivotY[1]) return "RightLower";

        if (y == area.PivotY[0] && x <= area.PivotX[1]) return "LowerLeft";

        return "None";
    }

    public Grid(AreaModel area)
    {
        if (Math.Abs(area.DischargeRatioX - 1) > 1e-10)
        {
            var sumKx = (1 - Math.Pow(area.DischargeRatioX, area.AmountX - 1)) / (1 - area.DischargeRatioX);
            var hX = (area.PivotX[2] - area.PivotX[0]) / sumKx;
            var x = new double[area.AmountX];

            for (var i = 0; i < area.AmountX; i++)
            {
                x[i] = area.PivotX[0] + hX * (1 - Math.Pow(area.DischargeRatioX, i)) / (1 - area.DischargeRatioX);
            }

            X = x;
        }
        else
        {
            var x = new double[area.AmountX];
            var hX = (area.PivotX[2] - area.PivotX[0]) / (area.AmountX - 1);

            for (var i = 0; i < area.AmountX; i++)
            {
                x[i] = area.PivotX[0] + i * hX;
            }

            X = x;
        }

        if (Math.Abs(area.DischargeRatioY - 1) > 1e-10)
        {
            var sumKy = (1 - Math.Pow(area.DischargeRatioY, area.AmountY - 1)) / (1 - area.DischargeRatioY);
            var hY = (area.PivotY[2] - area.PivotY[0]) / sumKy;
            var y = new double[area.AmountY];

            for (var i = 0; i < area.AmountY; i++)
            {
                y[i] = area.PivotY[0] + hY * (1 - Math.Pow(area.DischargeRatioY, i)) / (1 - area.DischargeRatioY);
            }

            Y = y;
        }
        else
        {
            var y = new double[area.AmountY];
            var hY = (area.PivotY[2] - area.PivotY[0]) / (area.AmountY - 1);

            for (var i = 0; i < area.AmountY; i++)
            {
                y[i] = area.PivotY[0] + i * hY;
            }

            Y = y;
        }

        if (!Utils.CheckGridConsistency(X, area.PivotX) || !Utils.CheckGridConsistency(Y, area.PivotY))
        {
            throw new Exception("Non consistent data");
        }

        var nodes = new Node[X.Length * Y.Length];
        var num = 0;

        foreach (var i in Y)
        {
            foreach (var j in X)
            {
                nodes[num] = new Node(
                    j,
                    i,
                    IsFictiveCheck(area, j, i),
                    IsOnBorderCheck(area, j, i),
                    GetBorderType(area, j, i)
                );
                num++;
            }
        }

        Nodes = nodes;
    }
}