namespace umf_2;

public class FemUtils
{
    public static double[,] LocalStiffness =
    {
        {1.0 / 2.0, -1.0 / 2.0},
        {-1.0 / 2.0, 1.0 / 2.0}
    };

    public static double[,] LocalMass =
    {
        {2.0 / 6.0, 1.0 / 6.0},
        {1.0 / 6.0, 2.0 / 6.0}
    };
}