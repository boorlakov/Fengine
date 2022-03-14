namespace umf_2.JsonModels;

public class BoundaryConditions
{
    public string Left { get; init; }
    public string LeftFunc { get; init; }

    public string Right { get; init; }
    public string RightFunc { get; init; }
    public double Beta { get; init; }
}