using Fengine.Backend.Fem.Slae;
using Fengine.Backend.Models;

namespace Fengine.Backend.LinAlg.SlaeSolver;

public interface ISlaeSolver
{
    double[] Solve(ISlae slae, Accuracy accuracy);
}