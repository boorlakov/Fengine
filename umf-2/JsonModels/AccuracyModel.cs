namespace umf_2.JsonModels;

public class AccuracyModel
{
    public int MaxIter { get; init; }
    public double Eps { get; init; }
    public double Delta { get; init; }

    public double RelaxRatio { get; init; }
}