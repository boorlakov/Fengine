using FiniteElementsMethod.LinAlg;
using FiniteElementsMethod.Models;

namespace FiniteElementsMethod.Fem;

public class Statistics
{
    public int Iterations;
    public double Residual;
    public double Error;
    public double[] Result;
}

public static class Solver
{
    public static Statistics SolveWithSimpleIteration(
        Grid grid,
        InputFuncs inputFuncs,
        Area area,
        BoundaryConditions boundaryConditions,
        Accuracy accuracy
    )
    {
        var iter = 1;
        var initApprox = new double[grid.X.Length];
        initApprox.AsSpan().Fill(2.0);
        var slae = new Slae(grid, inputFuncs, initApprox);
        ApplyBoundaryConditions(slae.Matrix, slae.RhsVec, area, boundaryConditions);
        slae.Solve(accuracy);

        while (SlaeSolver.RelResidual(slae) > accuracy.Eps &&
               !SlaeSolver.CheckIsStagnate(slae.ResVec, initApprox, accuracy.Delta))
        {
            slae.ResVec.AsSpan().CopyTo(initApprox);
            slae = new Slae(grid, inputFuncs, initApprox);
            ApplyBoundaryConditions(slae.Matrix, slae.RhsVec, area, boundaryConditions);
            slae.Solve(accuracy);
            iter++;
        }
        
        var funcCalc = new Sprache.Calc.XtensibleCalculator();
        var uStar = funcCalc.ParseFunction(inputFuncs.UStar).Compile();

        var absError = new double[slae.ResVec.Length];
        var u = new double[slae.ResVec.Length];

        for (var i = 0; i < slae.ResVec.Length; i++)
        {
            u[i] = uStar(Utils.MakeDict1D(grid.X[i]));
            absError[i] = u[i] - slae.ResVec[i];
        }

        var error = GeneralOperations.Norm(absError) / GeneralOperations.Norm(u);
            
        var stat = new Statistics()
        {
            Iterations = iter,
            Residual = SlaeSolver.RelResidual(slae),
            Error = error,
            Result = slae.ResVec
        };
        
        return stat;
    }

    private static void ApplyBoundaryConditions(
        Matrix m,
        double[] rhs,
        Area area,
        BoundaryConditions boundaryConditions
    )
    {
        switch (boundaryConditions.Left)
        {
            case "First":
                m.Center[0] = 1.0;
                m.Upper[0] = 0.0;
                rhs[0] = Utils.EvalFunc(boundaryConditions.LeftFunc, area.LeftBorder);
                break;
            case "Second":
                rhs[0] += Utils.EvalFunc(boundaryConditions.LeftFunc, area.LeftBorder);
                break;
            case "Third":
                m.Center[0] += boundaryConditions.Beta;
                rhs[0] += boundaryConditions.Beta * Utils.EvalFunc(boundaryConditions.LeftFunc, area.LeftBorder);
                break;
        }

        switch (boundaryConditions.Right)
        {
            case "First":
                m.Center[^1] = 1.0;
                m.Lower[^1] = 0.0;
                rhs[^1] = Utils.EvalFunc(boundaryConditions.RightFunc, area.RightBorder);
                break;
            case "Second":
                rhs[^1] += Utils.EvalFunc(boundaryConditions.RightFunc, area.RightBorder);
                break;
            case "Third":
                m.Center[^1] += boundaryConditions.Beta;
                rhs[^1] += boundaryConditions.Beta * Utils.EvalFunc(boundaryConditions.RightFunc, area.RightBorder);
                break;
        }
    }

    public static double[] SolveWithNewton(Slae slae)
    {
        throw new NotImplementedException();
    }
}