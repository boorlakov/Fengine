using Fengine.Backend.LinAlg.Matrix;
using Fengine.Backend.Models;

namespace Fengine.Backend.Fem.Slae;

public interface ISlae
{
    double[] Solve(Accuracy accuracy);

    public IMatrix Matrix { get; set; }
    public double[] RhsVec { get; set; }
    public double[] ResVec { get; set; }
}