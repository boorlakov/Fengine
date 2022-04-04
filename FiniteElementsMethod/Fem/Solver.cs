using System.Reflection.PortableExecutable;
using FiniteElementsMethod.LinAlg;
using FiniteElementsMethod.Models;

namespace FiniteElementsMethod.Fem;

public class Statistics
{
    public int Iterations;
    public double Residual;
    public double Error;
    public double[] Values;
    public double RelaxRatio;
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
        var iter = 0;
        var coef = 0.0;
        var initApprox = new double[grid.X.Length];
        var relaxRatio = accuracy.RelaxRatio;
        initApprox.AsSpan().Fill(2.0);
        var slae = new Slae(grid, inputFuncs, initApprox);
        ApplyBoundaryConditions(slae.Matrix, slae.RhsVec, area, boundaryConditions);
        slae.Solve(accuracy);

        do
        {
            slae.ResVec.AsSpan().CopyTo(initApprox);
            slae = new Slae(grid, inputFuncs, initApprox);
            ApplyBoundaryConditions(slae.Matrix, slae.RhsVec, area, boundaryConditions);
            slae.Solve(accuracy);

            if (accuracy.AutoRelax)
            {
                relaxRatio = RelaxRatio(slae.ResVec, grid, inputFuncs, initApprox, accuracy, area, boundaryConditions);
            }

            coef = relaxRatio;
            initApprox = UpdateApprox(slae.ResVec, initApprox, relaxRatio);
            iter++;
        } while (iter < accuracy.MaxIter && SlaeSolver.RelResidual(slae) > accuracy.Eps &&
                  !SlaeSolver.CheckIsStagnate(slae.ResVec, initApprox, accuracy.Delta));

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
            Values = slae.ResVec,
            RelaxRatio = coef
        };

        return stat;
    }

    private static double RelaxRatio(
        double[] resVec, 
        Grid grid, 
        InputFuncs inputFuncs, 
        double[] prevResVec,
        Accuracy accuracy,
        Area area, 
        BoundaryConditions boundaryConditions
        )
    {
        var gold = (Math.Pow(5, 0.5) - 1.0) / 2.0;
        var left = .0;
        var right = 1.0;
        var xLeft = (1 - gold);
        var xRight = gold;
        var fLeft = ResidualFunc(resVec, grid, inputFuncs, xLeft, prevResVec, area, boundaryConditions);
        var fRight = ResidualFunc(resVec, grid, inputFuncs, xRight, prevResVec, area, boundaryConditions);

        while (Math.Abs(right - left) > accuracy.Eps)
        {
            if (fLeft > fRight)
            {
                left = xLeft;
                xLeft = xRight;
                fLeft = fRight;
                xRight = left + gold * (right - left);
                fRight = ResidualFunc(resVec, grid, inputFuncs, xRight, prevResVec, area, boundaryConditions);
            }
            else
            {
                right = xRight;
                xRight = xLeft;
                fRight = fLeft;
                xLeft = left + (1.0 - gold) * (right - left);
                fLeft = ResidualFunc(resVec, grid, inputFuncs, xLeft, prevResVec, area, boundaryConditions);
            }
        }

        return (left + right) / 2.0;
    }

    private static double ResidualFunc(
        double[] resVec,
        Grid grid,
        InputFuncs inputFuncs,
        double x,
        double[] prevResVec,
        Area area,
        BoundaryConditions boundaryConditions
        )
    {
        var approx = new double[prevResVec.Length];
        for (int i = 0; i < approx.Length; i++)
        {
            approx[i] = x * resVec[i] + (1.0 - x) * prevResVec[i];
        }

        var slae = new Slae(grid, inputFuncs, approx);
        ApplyBoundaryConditions(slae.Matrix, slae.RhsVec, area, boundaryConditions);
        return SlaeSolver.RelResidual(slae.Matrix, approx, slae.RhsVec);
    }

    private static double[] UpdateApprox(
        double[] resVec,
        double[] approx,
        double relaxRatio
    )
    {
        var newApprox = new double[resVec.Length];
        for (int i = 0; i < resVec.Length; i++)
        {
            newApprox[i] = relaxRatio * resVec[i] + (1.0 - relaxRatio) * approx[i];
        }

        return newApprox;
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