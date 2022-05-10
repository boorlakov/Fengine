using System;
using NUnit.Framework;

namespace Fengine.Backend.Test.Fem.Solver;

[TestFixture]
public class SimpleIterationTests
{
    [SetUp]
    public void SetUp()
    {
        var slaeSolver = new Backend.LinearAlgebra.SlaeSolver.GaussSeidel();
        var integrator = new Backend.Integration.GaussFourPoints();
        var matrixType = new Backend.LinearAlgebra.Matrix.ThreeDiagonal();
        var slaeType = new Backend.Fem.Slae.NonlinearTask.Elliptic.OneDim.Linear();
        var differentiatorType = new Differentiation.TwoPoints();

        _simpleIteration = new Backend.Fem.Solver.SimpleIteration(
            slaeSolver,
            integrator,
            matrixType,
            slaeType
        );

        _newton = new Backend.Fem.Solver.SimpleIteration(
            slaeSolver,
            integrator,
            matrixType,
            slaeType,
            differentiatorType
        );
    }

    private Backend.Fem.Solver.SimpleIteration _simpleIteration;
    private Backend.Fem.Solver.SimpleIteration _newton;

    [Test]
    public void FemSolverSimpleIterationTest_WhenPassSimpleFuncAndUniformGrid_ShouldReturnCorrectResult()
    {
        // Arrange
        var area = new DataModels.Area.OneDim
        {
            LeftBorder = 0.0,
            RightBorder = 1.0,
            DischargeRatio = 1.0,
            AmountPoints = 11
        };

        var inputFuncs = new DataModels.InputFuncs
        {
            Lambda = "1",
            Gamma = "1",
            RhsFunc = "u",
            UStar = "x+2"
        };

        var boundaryConditions = new DataModels.Conditions.Boundary.OneDim
        {
            Left = "First",
            LeftFunc = "2",
            Right = "First",
            RightFunc = "3"
        };

        var accuracy = new DataModels.Accuracy
        {
            Eps = 1.0e-8,
            Delta = 1.0e-5,
            MaxIter = 1000,
            RelaxRatio = 0.52
        };

        var grid = new Backend.Fem.Mesh.Cartesian.OneDim(area);

        var expected = new[] {2.0, 2.1, 2.2, 2.3, 2.4, 2.5, 2.6, 2.7, 2.8, 2.9, 3.0};

        // Act
        var result = _simpleIteration.Solve(
            grid,
            inputFuncs,
            area,
            boundaryConditions,
            accuracy
        );

        // Assert
        for (var i = 0; i < expected.Length; i++)
        {
            Assert.AreEqual(result.Values[i], expected[i], 1.0e-4);
        }
    }

    [Test]
    public void
        FemSolverSimpleIterationTest_WhenPassSimpleFuncAndThirdBoundaryConditions_ShouldReturnCorrectResult()
    {
        // Arrange
        var area = new DataModels.Area.OneDim
        {
            LeftBorder = 0.0,
            RightBorder = 1.0,
            DischargeRatio = 1.0,
            AmountPoints = 11
        };

        var inputFuncs = new DataModels.InputFuncs
        {
            Lambda = "1",
            Gamma = "1",
            RhsFunc = "u",
            UStar = "x+2"
        };

        var boundaryConditions = new DataModels.Conditions.Boundary.OneDim
        {
            Left = "Third",
            LeftFunc = "0",
            Right = "First",
            RightFunc = "3",
            Beta = 0.5
        };

        var accuracy = new DataModels.Accuracy
        {
            Eps = 1.0e-8,
            Delta = 1.0e-5,
            MaxIter = 1000,
            RelaxRatio = 0.52
        };

        var grid = new Backend.Fem.Mesh.Cartesian.OneDim(area);

        var expected = new[] {2.0, 2.1, 2.2, 2.3, 2.4, 2.5, 2.6, 2.7, 2.8, 2.9, 3.0};

        // Act
        var result = _simpleIteration.Solve(
            grid,
            inputFuncs,
            area,
            boundaryConditions,
            accuracy
        );

        // Assert
        for (var i = 0; i < expected.Length; i++)
        {
            Assert.AreEqual(result.Values[i], expected[i], 1.0e-4);
        }
    }

    [Test]
    public void
        FemSolverSimpleIterationTest_WhenPassSimpleFuncAndThirdBoundaryRightConditions_ShouldReturnCorrectResult()
    {
        // Arrange
        var area = new DataModels.Area.OneDim
        {
            LeftBorder = 0.0,
            RightBorder = 1.0,
            DischargeRatio = 1.0,
            AmountPoints = 11
        };

        var inputFuncs = new DataModels.InputFuncs
        {
            Lambda = "1",
            Gamma = "1",
            RhsFunc = "u",
            UStar = "x+2"
        };

        var boundaryConditions = new DataModels.Conditions.Boundary.OneDim
        {
            Left = "First",
            LeftFunc = "2",
            Right = "Third",
            RightFunc = "5",
            Beta = 0.5
        };

        var accuracy = new DataModels.Accuracy
        {
            Eps = 1.0e-8,
            Delta = 1.0e-5,
            MaxIter = 1000,
            RelaxRatio = 0.52
        };

        var grid = new Backend.Fem.Mesh.Cartesian.OneDim(area);

        var expected = new[] {2.0, 2.1, 2.2, 2.3, 2.4, 2.5, 2.6, 2.7, 2.8, 2.9, 3.0};

        // Act
        var result = _simpleIteration.Solve(
            grid,
            inputFuncs,
            area,
            boundaryConditions,
            accuracy
        );

        // Assert
        for (var i = 0; i < expected.Length; i++)
        {
            Assert.AreEqual(result.Values[i], expected[i], 1.0e-4);
        }
    }

    [Test]
    public void
        FemSolverSimpleIterationTest_WhenPassSimpleFuncAndSecondBoundaryConditions_ShouldReturnCorrectResult()
    {
        // Arrange
        var area = new DataModels.Area.OneDim
        {
            LeftBorder = 0.0,
            RightBorder = 1.0,
            DischargeRatio = 1.0,
            AmountPoints = 11
        };

        var inputFuncs = new DataModels.InputFuncs
        {
            Lambda = "1",
            Gamma = "1",
            RhsFunc = "u",
            UStar = "x+2"
        };

        var boundaryConditions = new DataModels.Conditions.Boundary.OneDim
        {
            Left = "Second",
            LeftFunc = "-1",
            Right = "First",
            RightFunc = "3"
        };

        var accuracy = new DataModels.Accuracy
        {
            Eps = 1.0e-8,
            Delta = 1.0e-5,
            MaxIter = 1000,
            RelaxRatio = 0.52
        };

        var grid = new Backend.Fem.Mesh.Cartesian.OneDim(area);

        var expected = new[] {2.0, 2.1, 2.2, 2.3, 2.4, 2.5, 2.6, 2.7, 2.8, 2.9, 3.0};

        // Act
        var result = _simpleIteration.Solve(
            grid,
            inputFuncs,
            area,
            boundaryConditions,
            accuracy
        );

        // Assert
        for (var i = 0; i < expected.Length; i++)
        {
            Assert.AreEqual(result.Values[i], expected[i], 1.0e-4);
        }
    }

    [Test]
    public void
        FemSolverSimpleIterationTest_WhenPassSimpleFuncAndSecondBoundaryRightConditions_ShouldReturnCorrectResult()
    {
        // Arrange
        var area = new DataModels.Area.OneDim
        {
            LeftBorder = 0.0,
            RightBorder = 1.0,
            DischargeRatio = 1.0,
            AmountPoints = 11
        };

        var inputFuncs = new DataModels.InputFuncs
        {
            Lambda = "1",
            Gamma = "1",
            RhsFunc = "u",
            UStar = "x+2"
        };

        var boundaryConditions = new DataModels.Conditions.Boundary.OneDim
        {
            Left = "First",
            LeftFunc = "2",
            Right = "Second",
            RightFunc = "1"
        };

        var accuracy = new DataModels.Accuracy
        {
            Eps = 1.0e-8,
            Delta = 1.0e-5,
            MaxIter = 1000,
            RelaxRatio = 0.52
        };

        var grid = new Backend.Fem.Mesh.Cartesian.OneDim(area);

        var expected = new[] {2.0, 2.1, 2.2, 2.3, 2.4, 2.5, 2.6, 2.7, 2.8, 2.9, 3.0};

        // Act
        var result = _simpleIteration.Solve(
            grid,
            inputFuncs,
            area,
            boundaryConditions,
            accuracy
        );

        // Assert
        for (var i = 0; i < expected.Length; i++)
        {
            Assert.AreEqual(result.Values[i], expected[i], 1.0e-4);
        }
    }

    [Test]
    public void FemSolverSimpleIterationTest_WhenPassSinFuncAndFirstBoundaryConditions_ShouldReturnCorrectResult()
    {
        // Arrange
        var area = new DataModels.Area.OneDim
        {
            LeftBorder = 0.0,
            RightBorder = 2.0,
            DischargeRatio = 1.0,
            AmountPoints = 11
        };

        var inputFuncs = new DataModels.InputFuncs
        {
            Lambda = "1",
            Gamma = "1",
            RhsFunc = "2 * u",
            UStar = "Sin(x)"
        };

        var boundaryConditions = new DataModels.Conditions.Boundary.OneDim
        {
            Left = "First",
            LeftFunc = "0",
            Right = "First",
            RightFunc = "0.9092974268"
        };

        var accuracy = new DataModels.Accuracy
        {
            Eps = 1.0e-8,
            Delta = 1.0e-5,
            MaxIter = 1000,
            RelaxRatio = 0.52
        };

        var grid = new Backend.Fem.Mesh.Cartesian.OneDim(area);

        var expected = new double[11];

        for (var i = 0; i < expected.Length; i++)
        {
            expected[i] = Math.Sin(grid.Nodes[i].Coordinates[Backend.Fem.Mesh.Axis.X]);
        }

        // Act
        var result = _simpleIteration.Solve(
            grid,
            inputFuncs,
            area,
            boundaryConditions,
            accuracy
        );

        // Assert
        for (var i = 0; i < expected.Length; i++)
        {
            Assert.AreEqual(result.Values[i], expected[i], 1.0e-2);
        }
    }
}