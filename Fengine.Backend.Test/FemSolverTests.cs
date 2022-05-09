using System;
using NUnit.Framework;

namespace Fengine.Backend.Test;

[TestFixture]
public class FemSolverTests
{
    [SetUp]
    public void SetUp()
    {
        var slaeSolver = new LinearAlgebra.SlaeSolver.GaussSeidel();
        var integrator = new Integration.GaussFourPoints();
        var matrixType = new LinearAlgebra.Matrix.ThreeDiagonal();
        var slaeType = new Fem.Slae.Elliptic1DLinearBasisFNonLinear();
        var differentiatorType = new Differentiation.TwoPoints();

        _simpleIteration = new Fem.Solver.SimpleIteration(
            slaeSolver,
            integrator,
            matrixType,
            slaeType
        );

        _newton = new Fem.Solver.SimpleIteration(
            slaeSolver,
            integrator,
            matrixType,
            slaeType,
            differentiatorType
        );
    }

    private Fem.Solver.SimpleIteration _simpleIteration;
    private Fem.Solver.SimpleIteration _newton;

    #region SI Tests

    [Test]
    public void FemSolverSimpleIterationTest_WhenPassSimpleFuncAndUniformGrid_ShouldReturnCorrectResult()
    {
        // Arrange
        var area = new DataModels.Areas.OneDim
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

        var grid = new Fem.Mesh.Cartesian.OneDim(area);

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
        var area = new DataModels.Areas.OneDim
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

        var grid = new Fem.Mesh.Cartesian.OneDim(area);

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
        var area = new DataModels.Areas.OneDim
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

        var grid = new Fem.Mesh.Cartesian.OneDim(area);

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
        var area = new DataModels.Areas.OneDim
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

        var grid = new Fem.Mesh.Cartesian.OneDim(area);

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
        var area = new DataModels.Areas.OneDim
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

        var grid = new Fem.Mesh.Cartesian.OneDim(area);

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
        var area = new DataModels.Areas.OneDim
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

        var grid = new Fem.Mesh.Cartesian.OneDim(area);

        var expected = new double[11];

        for (var i = 0; i < expected.Length; i++)
        {
            expected[i] = Math.Sin(grid.Nodes[i].Coordinates[Fem.Mesh.Axis.X]);
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

    #endregion

    #region Newton tests

    [Test]
    public void FemSolverNewtonTest_WhenPassSimpleFuncAndUniformGrid_ShouldReturnCorrectResult()
    {
        // Arrange
        var area = new DataModels.Areas.OneDim
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
            Eps = 1.0e-10,
            Delta = 1.0e-7,
            MaxIter = 1000,
            RelaxRatio = 0.52
        };

        var grid = new Fem.Mesh.Cartesian.OneDim(area);

        var expected = new[] {2.0, 2.1, 2.2, 2.3, 2.4, 2.5, 2.6, 2.7, 2.8, 2.9, 3.0};

        // Act
        var result = _newton.Solve(
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
    public void FemSolverNewtonWithLowerEpsTest_WhenPassSimpleFuncAndUniformGrid_ShouldReturnCorrectResult()
    {
        // Arrange
        var area = new DataModels.Areas.OneDim
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

        var grid = new Fem.Mesh.Cartesian.OneDim(area);

        var expected = new[] {2.0, 2.1, 2.2, 2.3, 2.4, 2.5, 2.6, 2.7, 2.8, 2.9, 3.0};

        // Act
        var result = _newton.Solve(
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
    public void FemSolverNewtonAutoRelaxTest_WhenPassSimpleFuncAndUniformGrid_ShouldReturnCorrectResult()
    {
        // Arrange
        var area = new DataModels.Areas.OneDim
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
            Eps = 1.0e-10,
            Delta = 1.0e-5,
            MaxIter = 1000,
            AutoRelax = true,
            RelaxRatio = 0.52
        };

        var grid = new Fem.Mesh.Cartesian.OneDim(area);

        var expected = new[] {2.0, 2.1, 2.2, 2.3, 2.4, 2.5, 2.6, 2.7, 2.8, 2.9, 3.0};

        // Act
        var result = _newton.Solve(
            grid,
            inputFuncs,
            area,
            boundaryConditions,
            accuracy
        );

        // Assert
        for (var i = 0; i < expected.Length; i++)
        {
            Assert.AreEqual(result.Values[i], expected[i], 1.0e-3);
        }
    }

    [Test]
    public void
        FemSolverNewtonTest_WhenPassSimpleFuncAndThirdBoundaryConditions_ShouldReturnCorrectResult()
    {
        // Arrange
        var area = new DataModels.Areas.OneDim
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

        var grid = new Fem.Mesh.Cartesian.OneDim(area);

        var expected = new[] {2.0, 2.1, 2.2, 2.3, 2.4, 2.5, 2.6, 2.7, 2.8, 2.9, 3.0};

        // Act
        var result = _newton.Solve(
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
        FemSolverNewtonTest_WhenPassSimpleFuncAndThirdBoundaryRightConditions_ShouldReturnCorrectResult()
    {
        // Arrange
        var area = new DataModels.Areas.OneDim
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

        var grid = new Fem.Mesh.Cartesian.OneDim(area);

        var expected = new[] {2.0, 2.1, 2.2, 2.3, 2.4, 2.5, 2.6, 2.7, 2.8, 2.9, 3.0};

        // Act
        var result = _newton.Solve(
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
        FemSolverNewtonTest_WhenPassSimpleFuncAndSecondBoundaryConditions_ShouldReturnCorrectResult()
    {
        // Arrange
        var area = new DataModels.Areas.OneDim
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

        var grid = new Fem.Mesh.Cartesian.OneDim(area);

        var expected = new[] {2.0, 2.1, 2.2, 2.3, 2.4, 2.5, 2.6, 2.7, 2.8, 2.9, 3.0};

        // Act
        var result = _newton.Solve(
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
        FemSolverNewtonTest_WhenPassSimpleFuncAndSecondBoundaryRightConditions_ShouldReturnCorrectResult()
    {
        // Arrange
        var area = new DataModels.Areas.OneDim
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

        var grid = new Fem.Mesh.Cartesian.OneDim(area);

        var expected = new[] {2.0, 2.1, 2.2, 2.3, 2.4, 2.5, 2.6, 2.7, 2.8, 2.9, 3.0};

        // Act
        var result = _newton.Solve(
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
    public void FemSolverNewtonTest_WhenPassSinFuncAndFirstBoundaryConditions_ShouldReturnCorrectResult()
    {
        // Arrange
        var area = new DataModels.Areas.OneDim
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

        var grid = new Fem.Mesh.Cartesian.OneDim(area);

        var expected = new double[11];

        for (var i = 0; i < expected.Length; i++)
        {
            expected[i] = Math.Sin(grid.Nodes[i].Coordinates[Fem.Mesh.Axis.X]);
        }

        // Act
        var result = _newton.Solve(
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

    #endregion
}