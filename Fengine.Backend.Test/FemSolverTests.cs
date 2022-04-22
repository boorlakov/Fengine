using System;
using Fengine.Backend.DataModels;
using Fengine.Backend.Fem.Mesh;
using Fengine.Backend.Fem.Slae;
using Fengine.Backend.Fem.Solver;
using Fengine.Backend.Integration;
using Fengine.Backend.LinAlg.Matrix;
using Fengine.Backend.LinAlg.SlaeSolver;
using NUnit.Framework;

namespace Fengine.Backend.Test;

[TestFixture]
public class FemSolverTests
{
    [SetUp]
    public void SetUp()
    {
        var slaeSolver = new SlaeSolverGaussSeidel();
        var integrator = new IntegratorGauss4Points();
        var matrixType = new Matrix3Diagonal();
        var slaeType = new Slae1DEllipticLinearFNonLinear();
        _femSolverWithSimpleIteration = new FemSolverWithSimpleIteration(
            slaeSolver,
            integrator,
            matrixType,
            slaeType
        );
    }

    private FemSolverWithSimpleIteration _femSolverWithSimpleIteration;

    [Test]
    public void FemSolverWithSimpleIterationTest_WhenPassSimpleFuncAndUniformGrid_ShouldReturnCorrectResult()
    {
        // Arrange
        var area = new Area
        {
            LeftBorder = 0.0,
            RightBorder = 1.0,
            DischargeRatio = 1.0,
            AmountPoints = 11
        };

        var inputFuncs = new InputFuncs
        {
            Lambda = "1",
            Gamma = "1",
            RhsFunc = "u",
            UStar = "x+2"
        };

        var boundaryConditions = new BoundaryConditions
        {
            Left = "First",
            LeftFunc = "2",
            Right = "First",
            RightFunc = "3"
        };

        var accuracy = new Accuracy
        {
            Eps = 1.0e-7,
            Delta = 1.0e-7,
            MaxIter = 1000
        };

        var grid = new Cartesian1DMesh(area);

        var expected = new[] {2.0, 2.1, 2.2, 2.3, 2.4, 2.5, 2.6, 2.7, 2.8, 2.9, 3.0};

        // Act
        var result = _femSolverWithSimpleIteration.Solve(
            grid,
            inputFuncs,
            area,
            boundaryConditions,
            accuracy
        );

        // Assert
        for (var i = 0; i < expected.Length; i++)
        {
            Assert.AreEqual(result.Values[i], expected[i], 1.0e-7);
        }
    }

    [Test]
    public void
        FemSolverWithSimpleIterationTest_WhenPassSimpleFuncAndThirdBoundaryConditions_ShouldReturnCorrectResult()
    {
        // Arrange
        var area = new Area
        {
            LeftBorder = 0.0,
            RightBorder = 1.0,
            DischargeRatio = 1.0,
            AmountPoints = 11
        };

        var inputFuncs = new InputFuncs
        {
            Lambda = "1",
            Gamma = "1",
            RhsFunc = "u",
            UStar = "x+2"
        };

        var boundaryConditions = new BoundaryConditions
        {
            Left = "Third",
            LeftFunc = "0",
            Right = "First",
            RightFunc = "3",
            Beta = 0.5
        };

        var accuracy = new Accuracy
        {
            Eps = 1.0e-10,
            Delta = 1.0e-10,
            MaxIter = 1000
        };

        var grid = new Cartesian1DMesh(area);

        var expected = new[] {2.0, 2.1, 2.2, 2.3, 2.4, 2.5, 2.6, 2.7, 2.8, 2.9, 3.0};

        // Act
        var result = _femSolverWithSimpleIteration.Solve(
            grid,
            inputFuncs,
            area,
            boundaryConditions,
            accuracy
        );

        // Assert
        for (var i = 0; i < expected.Length; i++)
        {
            Assert.AreEqual(result.Values[i], expected[i], 1.0e-7);
        }
    }

    [Test]
    public void
        FemSolverWithSimpleIterationTest_WhenPassSimpleFuncAndThirdBoundaryRightConditions_ShouldReturnCorrectResult()
    {
        // Arrange
        var area = new Area
        {
            LeftBorder = 0.0,
            RightBorder = 1.0,
            DischargeRatio = 1.0,
            AmountPoints = 11
        };

        var inputFuncs = new InputFuncs
        {
            Lambda = "1",
            Gamma = "1",
            RhsFunc = "u",
            UStar = "x+2"
        };

        var boundaryConditions = new BoundaryConditions
        {
            Left = "First",
            LeftFunc = "2",
            Right = "Third",
            RightFunc = "5",
            Beta = 0.5
        };

        var accuracy = new Accuracy
        {
            Eps = 1.0e-10,
            Delta = 1.0e-10,
            MaxIter = 1000
        };

        var grid = new Cartesian1DMesh(area);

        var expected = new[] {2.0, 2.1, 2.2, 2.3, 2.4, 2.5, 2.6, 2.7, 2.8, 2.9, 3.0};

        // Act
        var result = _femSolverWithSimpleIteration.Solve(
            grid,
            inputFuncs,
            area,
            boundaryConditions,
            accuracy
        );

        // Assert
        for (var i = 0; i < expected.Length; i++)
        {
            Assert.AreEqual(result.Values[i], expected[i], 1.0e-7);
        }
    }

    [Test]
    public void
        FemSolverWithSimpleIterationTest_WhenPassSimpleFuncAndSecondBoundaryConditions_ShouldReturnCorrectResult()
    {
        // Arrange
        var area = new Area
        {
            LeftBorder = 0.0,
            RightBorder = 1.0,
            DischargeRatio = 1.0,
            AmountPoints = 11
        };

        var inputFuncs = new InputFuncs
        {
            Lambda = "1",
            Gamma = "1",
            RhsFunc = "u",
            UStar = "x+2"
        };

        var boundaryConditions = new BoundaryConditions
        {
            Left = "Second",
            LeftFunc = "-1",
            Right = "First",
            RightFunc = "3"
        };

        var accuracy = new Accuracy
        {
            Eps = 1.0e-10,
            Delta = 1.0e-10,
            MaxIter = 1000
        };

        var grid = new Cartesian1DMesh(area);

        var expected = new[] {2.0, 2.1, 2.2, 2.3, 2.4, 2.5, 2.6, 2.7, 2.8, 2.9, 3.0};

        // Act
        var result = _femSolverWithSimpleIteration.Solve(
            grid,
            inputFuncs,
            area,
            boundaryConditions,
            accuracy
        );

        // Assert
        for (var i = 0; i < expected.Length; i++)
        {
            Assert.AreEqual(result.Values[i], expected[i], 1.0e-7);
        }
    }

    [Test]
    public void
        FemSolverWithSimpleIterationTest_WhenPassSimpleFuncAndSecondBoundaryRightConditions_ShouldReturnCorrectResult()
    {
        // Arrange
        var area = new Area
        {
            LeftBorder = 0.0,
            RightBorder = 1.0,
            DischargeRatio = 1.0,
            AmountPoints = 11
        };

        var inputFuncs = new InputFuncs
        {
            Lambda = "1",
            Gamma = "1",
            RhsFunc = "u",
            UStar = "x+2"
        };

        var boundaryConditions = new BoundaryConditions
        {
            Left = "First",
            LeftFunc = "2",
            Right = "Second",
            RightFunc = "1"
        };

        var accuracy = new Accuracy
        {
            Eps = 1.0e-10,
            Delta = 1.0e-10,
            MaxIter = 1000
        };

        var grid = new Cartesian1DMesh(area);

        var expected = new[] {2.0, 2.1, 2.2, 2.3, 2.4, 2.5, 2.6, 2.7, 2.8, 2.9, 3.0};

        // Act
        var result = _femSolverWithSimpleIteration.Solve(
            grid,
            inputFuncs,
            area,
            boundaryConditions,
            accuracy
        );

        // Assert
        for (var i = 0; i < expected.Length; i++)
        {
            Assert.AreEqual(result.Values[i], expected[i], 1.0e-7);
        }
    }

    [Test]
    public void FemSolverWithSimpleIterationTest_WhenPassSinFuncAndFirstBoundaryConditions_ShouldReturnCorrectResult()
    {
        // Arrange
        var area = new Area
        {
            LeftBorder = 0.0,
            RightBorder = 2.0,
            DischargeRatio = 1.0,
            AmountPoints = 11
        };

        var inputFuncs = new InputFuncs
        {
            Lambda = "1",
            Gamma = "1",
            RhsFunc = "2 * u",
            UStar = "Sin(x)"
        };

        var boundaryConditions = new BoundaryConditions
        {
            Left = "First",
            LeftFunc = "0",
            Right = "First",
            RightFunc = "0.9092974268"
        };

        var accuracy = new Accuracy
        {
            Eps = 1.0e-10,
            Delta = 1.0e-10,
            MaxIter = 10000
        };

        var grid = new Cartesian1DMesh(area);

        var expected = new double[11];

        for (var i = 0; i < expected.Length; i++)
        {
            expected[i] = Math.Sin(grid.Nodes[i].Coordinates[IMesh.Axis.X]);
        }

        // Act
        var result = _femSolverWithSimpleIteration.Solve(
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