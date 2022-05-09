using Fengine.Backend.Fem.Mesh;

namespace Fengine.Backend.Fem.Solver;

public interface IFemSolver
{
    Statistics Solve(
        IMesh mesh,
        DataModels.InputFuncs inputFuncs,
        DataModels.Areas.OneDim area,
        DataModels.Conditions.Boundary.OneDim boundaryConditions,
        DataModels.Accuracy accuracy
    );
}