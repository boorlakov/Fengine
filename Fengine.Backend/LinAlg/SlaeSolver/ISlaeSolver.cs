using Fengine.Backend.DataModels;
using Fengine.Backend.Fem.Slae;

namespace Fengine.Backend.LinAlg.SlaeSolver;

public interface ISlaeSolver
{
    double[] Solve(ISlae slae, Accuracy accuracy);
}