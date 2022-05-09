using Fengine.Backend.DataModels;
using Fengine.Backend.Fem.Slae;

namespace Fengine.Backend.LinearAlgebra.SlaeSolver;

public interface ISlaeSolver
{
    double[] Solve(ISlae slae, Accuracy accuracy);
}