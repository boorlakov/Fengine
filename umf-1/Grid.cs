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

    public double StepX { get; }
    public double StepY { get; }

    public Node[] Nodes;

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
        StepX = (input.AnchorX[2] - input.AnchorX[0]) / input.StepsNumX;
        StepY = (input.AnchorY[2] - input.AnchorY[0]) / input.StepsNumY;
        var x = new double[input.StepsNumX + 1];
        var y = new double[input.StepsNumY + 1];

        for (var i = 0; i < input.StepsNumX + 1; i++)
        {
            x[i] = input.AnchorX[0] + i * StepX;
        }
        for (var i = 0; i < input.StepsNumY + 1; i++)
        {
            y[i] = input.AnchorY[0] + i * StepY;
        }

        var nodes = new Node[(input.StepsNumX + 1) * (input.StepsNumY + 1)];

        var globalNum = 0;
        for (var i = 0; i < input.StepsNumY + 1; i++)
        {
            for (var j = 0; j < input.StepsNumX + 1; j++)
            {
                nodes[globalNum] = new Node(x[j], y[i], _isFictive(input, x[j], y[i]));

                globalNum++;
            }
        }

        Nodes = nodes;
    }
}