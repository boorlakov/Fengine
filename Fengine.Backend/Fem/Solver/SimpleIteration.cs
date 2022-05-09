using Sprache.Calc;

namespace Fengine.Backend.Fem.Solver;

public class SimpleIteration : IFemSolver
{
    private readonly Integration.IIntegrator _integrator;
    private readonly Slae.ISlae _slae;
    private readonly LinearAlgebra.SlaeSolver.ISlaeSolver _slaeSolver;
    private readonly LinearAlgebra.Matrix.IMatrix _matrix;
    private readonly Differentiation.IDerivative? _derivative;

    public SimpleIteration
    (
        LinearAlgebra.SlaeSolver.ISlaeSolver slaeSolver,
        Integration.IIntegrator integrator,
        LinearAlgebra.Matrix.IMatrix matrix,
        Slae.ISlae slae,
        Differentiation.IDerivative? derivative = null
    )
    {
        _slaeSolver = slaeSolver;
        _integrator = integrator;
        _matrix = matrix;
        _slae = slae;
        _derivative = derivative;
    }

    public Statistics Solve
    (
        Mesh.IMesh mesh,
        DataModels.InputFuncs inputFuncs,
        DataModels.Area.OneDim area,
        DataModels.Conditions.Boundary.OneDim boundaryConditions,
        DataModels.Accuracy accuracy
    )
    {
        var withLinearization = _derivative is not null;
        var iter = 0;
        var initApprox = new double[mesh.Nodes.Length];

        var relaxRatio = accuracy.RelaxRatio;
        var slae = new Slae.OneDim.EllipticLinearBasisFNonLinear();

        do
        {
            slae.ResVec.AsSpan().CopyTo(initApprox);

            slae = withLinearization
                ? new Slae.OneDim.EllipticLinearBasisFNonLinear(mesh, inputFuncs, initApprox, _slaeSolver, _integrator,
                    _matrix,
                    _derivative)
                : new Slae.OneDim.EllipticLinearBasisFNonLinear(mesh, inputFuncs, initApprox, _slaeSolver, _integrator,
                    _matrix);

            ApplyBoundaryConditions(slae.Matrix, slae.RhsVec, area, boundaryConditions);
            slae.Solve(accuracy);

            if (accuracy.AutoRelax)
            {
                relaxRatio = EvalRelaxRatio
                (
                    slae.ResVec,
                    mesh,
                    inputFuncs,
                    initApprox,
                    accuracy,
                    area,
                    boundaryConditions
                );
            }

            initApprox = UpdateApprox(slae.ResVec, initApprox, relaxRatio);

            iter++;

            Console.Write($"\r[INFO] RelRes = {LinearAlgebra.Utils.RelResidual(slae):G10} | Iter: {iter}");
        } while (iter < accuracy.MaxIter &&
                 LinearAlgebra.Utils.RelResidual(slae.NonLinearMatrix, slae.ResVec, slae.NonLinearRhsVec) >
                 accuracy.Eps &&
                 !LinearAlgebra.Utils.CheckIsStagnate(slae.ResVec, initApprox, accuracy.Delta));

        var funcCalc = new XtensibleCalculator();
        var uStar = funcCalc.ParseFunction(inputFuncs.UStar).Compile();

        var absError = new double[slae.ResVec.Length];
        var u = new double[slae.ResVec.Length];

        for (var i = 0; i < slae.ResVec.Length; i++)
        {
            u[i] = uStar(Utils.MakeDict1D(mesh.Nodes[i].Coordinates[Mesh.Axis.X]));
            absError[i] = u[i] - slae.ResVec[i];
        }

        var error = LinearAlgebra.GeneralOperations.Norm(absError) / LinearAlgebra.GeneralOperations.Norm(u);

        var stat = new Statistics
        {
            Iterations = iter,
            Residual = LinearAlgebra.Utils.RelResidual(slae.NonLinearMatrix, slae.ResVec, slae.NonLinearRhsVec),
            Error = error,
            Values = slae.ResVec,
            RelaxRatio = relaxRatio
        };

        return stat;
    }

    private double EvalRelaxRatio(
        double[] resVec,
        Mesh.IMesh cartesian1DMesh,
        DataModels.InputFuncs inputFuncs,
        double[] prevResVec,
        DataModels.Accuracy accuracy,
        DataModels.Area.OneDim area,
        DataModels.Conditions.Boundary.OneDim boundaryConditions
    )
    {
        var gold = (Math.Sqrt(5) - 1.0) / 2.0;
        var left = .0;
        var right = 1.0;
        var xLeft = 1 - gold;
        var xRight = gold;

        var fLeft = EvalResidualFunc(
            resVec,
            cartesian1DMesh,
            inputFuncs,
            xLeft,
            prevResVec,
            area,
            boundaryConditions
        );
        var fRight = EvalResidualFunc(
            resVec,
            cartesian1DMesh,
            inputFuncs,
            xRight,
            prevResVec,
            area,
            boundaryConditions
        );

        while (Math.Abs(right - left) > accuracy.Eps)
        {
            if (fLeft > fRight)
            {
                left = xLeft;
                xLeft = xRight;
                fLeft = fRight;
                xRight = left + gold * (right - left);
                fRight = EvalResidualFunc(
                    resVec,
                    cartesian1DMesh,
                    inputFuncs,
                    xRight,
                    prevResVec,
                    area,
                    boundaryConditions
                );
            }
            else
            {
                right = xRight;
                xRight = xLeft;
                fRight = fLeft;
                xLeft = left + (1.0 - gold) * (right - left);
                fLeft = EvalResidualFunc(
                    resVec,
                    cartesian1DMesh,
                    inputFuncs,
                    xLeft,
                    prevResVec,
                    area,
                    boundaryConditions
                );
            }
        }

        return (left + right) / 2.0;
    }

    private double EvalResidualFunc(
        double[] resVec,
        Mesh.IMesh cartesian1DMesh,
        DataModels.InputFuncs inputFuncs,
        double x,
        double[] prevResVec,
        DataModels.Area.OneDim area,
        DataModels.Conditions.Boundary.OneDim boundaryConditions
    )
    {
        var approx = new double[prevResVec.Length];

        for (var i = 0; i < approx.Length; i++)
        {
            approx[i] = x * resVec[i] + (1.0 - x) * prevResVec[i];
        }

        var slae = new Slae.OneDim.EllipticLinearBasisFNonLinear(
            cartesian1DMesh,
            inputFuncs,
            approx,
            _slaeSolver,
            _integrator,
            _matrix
        );
        ApplyBoundaryConditions(slae.Matrix, slae.RhsVec, area, boundaryConditions);
        return LinearAlgebra.Utils.RelResidual(slae.Matrix, approx, slae.RhsVec);
    }

    private static double[] UpdateApprox(
        double[] resVec,
        double[] approx,
        double relaxRatio
    )
    {
        var newApprox = new double[resVec.Length];

        for (var i = 0; i < resVec.Length; i++)
        {
            newApprox[i] = relaxRatio * resVec[i] + (1.0 - relaxRatio) * approx[i];
        }

        return newApprox;
    }

    private static void ApplyBoundaryConditions(
        LinearAlgebra.Matrix.IMatrix m,
        double[] rhs,
        DataModels.Area.OneDim area,
        DataModels.Conditions.Boundary.OneDim boundaryConditions
    )
    {
        switch (boundaryConditions.Left)
        {
            case "First":
                m.Data["center"][0] = 1.0;
                m.Data["upper"][0] = 0.0;
                rhs[0] = Utils.EvalFunc(boundaryConditions.LeftFunc, area.LeftBorder);
                break;
            case "Second":
                rhs[0] += Utils.EvalFunc(boundaryConditions.LeftFunc, area.LeftBorder);
                break;
            case "Third":
                m.Data["center"][0] += boundaryConditions.Beta;
                rhs[0] += boundaryConditions.Beta * Utils.EvalFunc(boundaryConditions.LeftFunc, area.LeftBorder);
                break;
        }

        switch (boundaryConditions.Right)
        {
            case "First":
                m.Data["center"][^1] = 1.0;
                m.Data["lower"][^1] = 0.0;
                rhs[^1] = Utils.EvalFunc(boundaryConditions.RightFunc, area.RightBorder);
                break;
            case "Second":
                rhs[^1] += Utils.EvalFunc(boundaryConditions.RightFunc, area.RightBorder);
                break;
            case "Third":
                m.Data["center"][^1] += boundaryConditions.Beta;
                rhs[^1] += boundaryConditions.Beta * Utils.EvalFunc(boundaryConditions.RightFunc, area.RightBorder);
                break;
        }
    }
}