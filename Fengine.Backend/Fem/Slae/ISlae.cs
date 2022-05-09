using Fengine.Backend.DataModels;
using Fengine.Backend.LinearAlgebra.Matrix;

namespace Fengine.Backend.Fem.Slae;

public interface ISlae
{
    double[] Solve(Accuracy accuracy);

    public IMatrix Matrix { get; set; }
    public double[] RhsVec { get; set; }
    public double[] ResVec { get; set; }
}