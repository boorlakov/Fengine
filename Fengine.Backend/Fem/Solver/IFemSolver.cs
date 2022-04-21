using Fengine.Backend.Fem.Mesh;
using Fengine.Backend.Models;

namespace Fengine.Backend.Fem.Solver;

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