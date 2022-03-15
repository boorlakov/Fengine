namespace FiniteDifferenceMethod;

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
        //   |         RightUpper
        //   |          +
        //   |     |LowerRight
        //   |     |
        //   |     |
        // Left   RightLower
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
        var x = new List<double> {area.PivotX[0]};
        var y = new List<double> {area.PivotY[0]};

        for (var block = 0; block < 2; block++)
        {
            if (Math.Abs(area.DischargeRatioX[block] - 1) > 1e-10)
            {
                var sumKx = (1 - Math.Pow(area.DischargeRatioX[block], area.AmountX[block] - 1)) /
                            (1 - area.DischargeRatioX[block]);
                var hX = (area.PivotX[block + 1] - area.PivotX[block]) / sumKx;

                for (var i = 1; i < area.AmountX[block]; i++)
                {
                    x.Add(area.PivotX[block] +
                          hX * (1 - Math.Pow(area.DischargeRatioX[block], i)) / (1 - area.DischargeRatioX[block]));
                }
            }
            else
            {
                var hX = (area.PivotX[block + 1] - area.PivotX[block]) / (area.AmountX[block] - 1);

                for (var i = 1; i < area.AmountX[block]; i++)
                {
                    x.Add(area.PivotX[block] + i * hX);
                }
            }

            if (Math.Abs(area.DischargeRatioY[block] - 1) > 1e-10)
            {
                var sumKy = (1 - Math.Pow(area.DischargeRatioY[block], area.AmountY[block] - 1)) /
                            (1 - area.DischargeRatioY[block]);
                var hY = (area.PivotY[block + 1] - area.PivotY[block]) / sumKy;

                for (var i = 1; i < area.AmountY[block]; i++)
                {
                    y.Add(area.PivotY[block] +
                          hY * (1 - Math.Pow(area.DischargeRatioY[block], i)) / (1 - area.DischargeRatioY[block]));
                }
            }
            else
            {
                var hY = (area.PivotY[block + 1] - area.PivotY[block]) / (area.AmountY[block] - 1);

                for (var i = 1; i < area.AmountY[block]; i++)
                {
                    y.Add(area.PivotY[block] + i * hY);
                }
            }

            X = x.ToArray();
            Y = y.ToArray();
        }

        var nodes = new Node[X!.Length * Y!.Length];
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