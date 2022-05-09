namespace Fengine.Backend.Fem.Slae;

public interface ISlae
{
    double[] Solve(DataModels.Accuracy accuracy);

    public LinearAlgebra.Matrix.IMatrix Matrix { get; set; }
    public double[] RhsVec { get; set; }
    public double[] ResVec { get; set; }
}