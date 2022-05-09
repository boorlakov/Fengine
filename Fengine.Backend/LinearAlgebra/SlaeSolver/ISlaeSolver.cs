namespace Fengine.Backend.LinearAlgebra.SlaeSolver;

public interface ISlaeSolver
{
    double[] Solve(Fem.Slae.ISlae slae, DataModels.Accuracy accuracy);
}