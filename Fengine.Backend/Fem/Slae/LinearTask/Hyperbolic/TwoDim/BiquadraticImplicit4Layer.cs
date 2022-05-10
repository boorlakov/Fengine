using Fengine.Backend.LinearAlgebra.Matrix;

namespace Fengine.Backend.Fem.Slae.LinearTask.Hyperbolic.TwoDim;

public class BiquadraticImplicit4Layer : ISlae
{
    public double[] Solve(DataModels.Accuracy accuracy)
    {
        throw new NotImplementedException();
    }

    public IMatrix Matrix { get; set; }
    public double[] RhsVec { get; set; }
    public double[] ResVec { get; set; }
}