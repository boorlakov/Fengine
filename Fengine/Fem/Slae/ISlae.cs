using Fengine.LinAlg.Matrix;
using Fengine.Models;

namespace Fengine.Fem.Slae;

public interface ISlae
{
    double[] Solve(Accuracy accuracy);

    public IMatrix Matrix { get; set; }
    public double[] RhsVec { get; set; }
    public double[] ResVec { get; set; }
}