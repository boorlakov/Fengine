using Fengine.Fem;
using Fengine.Models;

namespace Fengine.LinAlg.SlaeSolver;

public interface ISlaeSolver
{
    double[] Solve(Slae slae, Accuracy accuracy);
}