using Fengine.Fem.Slae;
using Fengine.Models;

namespace Fengine.LinAlg.SlaeSolver;

public interface ISlaeSolver
{
    double[] Solve(ISlae slae, Accuracy accuracy);
}