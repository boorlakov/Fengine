namespace Fdm;

public static class BoundaryFunc
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

    public static readonly Dictionary<string, Func<double, double, double>> First = new()
    {
        {
            "Left", (x, y) => 0.0
        },
        {
            "Upper", (x, y) => x
        },
        {
            "RightUpper", (x, y) => 3.0
        },
        {
            "LowerRight", (x, y) => x
        },
        {
            "RightLower", (x, y) => 1.0
        },
        {
            "LowerLeft", (x, y) => x
        }
    };

    public static readonly Dictionary<string, Func<double, double, double>> Second = new()
    {
        {
            "Left", (x, y) => throw new Exception("Illegal access!")
        },
        {
            "Upper", (x, y) => throw new Exception("Illegal access!")
        },
        {
            "RightUpper", (x, y) => throw new Exception("Illegal access!")
        },
        {
            "LowerRight", (x, y) => throw new Exception("Illegal access!")
        },
        {
            "RightLower", (x, y) => throw new Exception("Illegal access!")
        },
        {
            "LowerLeft", (x, y) => throw new Exception("Illegal access!")
        }
    };
}