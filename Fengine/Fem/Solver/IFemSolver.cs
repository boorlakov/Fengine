using Fengine.Fem.Mesh;
using Fengine.Models;

namespace Fengine.Fem.Solver;

public interface IFemSolver
{
    Statistics Solve(
        IMesh mesh,
        InputFuncs inputFuncs,
        Area area,
        BoundaryConditions boundaryConditions,
        Accuracy accuracy
    );
}