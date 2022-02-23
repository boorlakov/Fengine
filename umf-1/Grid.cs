using System.Diagnostics;

namespace umf_1;

public class Grid
{
    public class Node
    {
        public double X;
        public double Y;
        public bool IsFictive;

        public Node(double x, double y, bool isFictive)
        {
            X = x;
            Y = y;
            IsFictive = isFictive;
        }
    }

    public double Hx { get; set; }
    public double Hy { get; set; }

    public double[] X { get; set; }
    public double[] Y { get; set; }


    public readonly Node[] Nodes;

    private static bool _isFictive(JsonModel input, double x, double y)
    {
        // Here is for L-form
        // +-+
        // | |
        // | |
        // +-+--+
        // +----+
        if (input.AnchorX[0] > x || input.AnchorY[0] > y)
        {
            return true;
        }

        if (input.AnchorX[1] < x && input.AnchorY[1] < y)
        {
            return true;
        }

        if (input.AnchorX[2] < x || input.AnchorY[2] < y)
        {
            return true;
        }

        return false;
    }

    public Grid(JsonModel input)
    {
        if (Math.Abs(input.DischargeCoefX - 1) > 1e-10)
        {
            var sumKx = (1 - Math.Pow(input.DischargeCoefX, input.PointsNumX - 1)) / (1 - input.DischargeCoefX);
            Hx = (input.AnchorX[2] - input.AnchorX[0]) / sumKx;
            var x = new double[input.PointsNumX];
            for (var i = 0; i < input.PointsNumX; i++)
            {
                x[i] = input.AnchorX[0] + Hx * (1 - Math.Pow(input.DischargeCoefX, i)) / (1 - input.DischargeCoefX);
            }
        }
        else
        {
            var x = new double[input.PointsNumX + 1];
            Hx = (input.AnchorX[2] - input.AnchorX[0]) / input.PointsNumX;
            for (var i = 0; i <= input.PointsNumX; i++)
            {
                x[i] = input.AnchorX[0] + i * Hx;
            }
            X = x;
        }

        if (Math.Abs(input.DischargeCoefY - 1) > 1e-10)
        {
            var sumKy = (1 - Math.Pow(input.DischargeCoefY, input.PointsNumY - 1)) / (1 - input.DischargeCoefY);
            Hy = (input.AnchorY[2] - input.AnchorY[0]) / sumKy;
            var y = new double[input.PointsNumY];
            for (var i = 0; i < input.PointsNumY; i++)
            {
                y[i] = input.AnchorY[0] + Hy * (1 - Math.Pow(input.DischargeCoefY, i)) / (1 - input.DischargeCoefY);
            }
            Y = y;
        }
        else
        {
            var y = new double[input.PointsNumY + 1];
            Hy = (input.AnchorY[2] - input.AnchorY[0]) / input.PointsNumY;
            for (var i = 0; i <= input.PointsNumY; i++)
            {
                y[i] = input.AnchorY[0] + i * Hy;
            }
            Y = y;
        }

        Debug.Assert(X != null, nameof(X) + " != null");
        Debug.Assert(Y != null, nameof(Y) + " != null");

        var nodes = new Node[X.Length * Y.Length];
        var num = 0;

        foreach (var i in Y)
        {
            foreach (var j in X)
            {
                nodes[num] = new Node(j, i, _isFictive(input, j, i));
                num++;
            }
        }

        Nodes = nodes;
    }
}