namespace umf_2.Models;

public class Accuracy
{
    public int MaxIter { get; init; }
    public double Eps { get; init; }
    public double Delta { get; init; }

    public double RelaxRatio { get; init; }
}