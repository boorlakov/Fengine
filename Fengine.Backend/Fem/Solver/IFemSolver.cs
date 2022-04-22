using Fengine.Backend.DataModels;
using Fengine.Backend.Fem.Mesh;

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