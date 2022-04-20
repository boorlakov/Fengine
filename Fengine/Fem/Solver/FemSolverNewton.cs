using Fengine.Fem.Mesh;
using Fengine.Models;

namespace Fengine.Fem.Solver;

public class FemSolverNewton : IFemSolver
{
    public Statistics Solve(
        IMesh mesh,
        InputFuncs inputFuncs,
        Area area,
        BoundaryConditions boundaryConditions,
        Accuracy accuracy
    )
    {
        throw new NotImplementedException();
    }
}