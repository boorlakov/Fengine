using umf_2.JsonModels;

namespace umf_2;

public static class Fem
{
    public static double[] SolveWithSimpleIteration(Grid grid, InputFuncs inputFuncs, Area area,
        BoundaryConditions boundaryConds, Accuracy accuracy)
    {
        var initApprox = new double[grid.X.Length];
        var slae = new Slae(grid, inputFuncs, initApprox);
        ApplyBoundaryConditions(slae.Matrix, slae.RhsVec, area, boundaryConds);
        slae.Solve(accuracy);

        while (LinAlg.SlaeSolver.RelResidual(slae) > accuracy.Eps &&
               LinAlg.SlaeSolver.RelTol(slae.ResVec, initApprox) > accuracy.Delta)
        {
            slae.ResVec.AsSpan().CopyTo(initApprox);
            slae = new Slae(grid, inputFuncs, initApprox);
            ApplyBoundaryConditions(slae.Matrix, slae.RhsVec, area, boundaryConds);
            slae.Solve(accuracy);
        }

        return slae.ResVec;
    }

    public static void ApplyBoundaryConditions(
        Matrix m,
        double[] rhs,
        JsonModels.Area area,
        JsonModels.BoundaryConditions boundaryConds
    )
    {
        switch (boundaryConds.Left)
        {
            case "First":
                m.Center[0] = 1.0;
                m.Upper[0] = 0.0;
                m.Lower[0] = 0.0;
                rhs[0] = Utils.EvalFunc(boundaryConds.LeftFunc, area.LeftBorder);
                break;
            case "Second":
                rhs[0] += Utils.EvalFunc(boundaryConds.LeftFunc, area.LeftBorder);
                break;
            case "Third":
                m.Center[0] += boundaryConds.Beta;
                rhs[0] += boundaryConds.Beta * Utils.EvalFunc(boundaryConds.LeftFunc, area.LeftBorder);
                break;
        }

        switch (boundaryConds.Right)
        {
            case "First":
                m.Center[^1] = 1.0;
                m.Upper[^1] = 0.0;
                m.Lower[^1] = 0.0;
                rhs[^1] = Utils.EvalFunc(boundaryConds.RightFunc, area.RightBorder);
                break;
            case "Second":
                rhs[^1] += Utils.EvalFunc(boundaryConds.RightFunc, area.RightBorder);
                break;
            case "Third":
                m.Center[^1] += boundaryConds.Beta;
                rhs[^1] += boundaryConds.Beta * Utils.EvalFunc(boundaryConds.RightFunc, area.RightBorder);
                break;
        }
    }

    public static double[] SolveWithNewton(Slae slae)
    {
        throw new NotImplementedException();
    }
}