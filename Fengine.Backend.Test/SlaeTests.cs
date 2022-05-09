using System;
using NUnit.Framework;

namespace Fengine.Backend.Test;

[TestFixture]
public class SlaeTests
{
    [SetUp]
    public void SetUp()
    {
        _gaussSeidel = new LinearAlgebra.SlaeSolver.GaussSeidel();
        _integrator = new Integration.GaussFourPoints();
    }
    private LinearAlgebra.SlaeSolver.GaussSeidel _gaussSeidel;
    private Integration.IIntegrator _integrator;

    [Test]
    public void Solve_WhenPass1DiagMatrix_ShouldReturnCorrectResVec()
    {
        // Arrange
        var upper = new[] {0.0, 0.0};
        var center = new[] {2.0, 2.0, 2.0};
        var lower = new[] {0.0, 0.0};

        var matrix = new LinearAlgebra.Matrix.ThreeDiagonal(upper, center, lower);

        var vec = new[] {4.0, 6.0, 8.0};
        var slae = new Fem.Slae.NonlinearTask.Elliptic.OneDim.Linear(matrix, vec, _gaussSeidel, _integrator);
        var expected = new[] {2.0, 3.0, 4.0};
        var accuracy = new DataModels.Accuracy
        {
            MaxIter = 1000,
            Eps = 1.0e-7,
            Delta = 1.0e-7
        };

        // Act
        slae.Solve(accuracy);
        var result = new double[slae.ResVec.Length];
        slae.ResVec.AsSpan().CopyTo(result);

        // Assert
        for (var i = 0; i < result.Length; i++)
        {
            Assert.AreEqual(result[i], expected[i], 1.0e-7);
        }
    }

    [Test]
    public void Solve_WhenPass3DiagMatrix_ShouldReturnCorrectResVec()
    {
        // Arrange
        var upper = new[] {1.0, 0.0};
        var center = new[] {2.0, 2.0, 2.0};
        var lower = new[] {0.0, 2.0};

        var matrix = new LinearAlgebra.Matrix.ThreeDiagonal(upper, center, lower);

        var vec = new[] {3.0, 2.0, 4.0};
        var accuracy = new DataModels.Accuracy
        {
            MaxIter = 1000,
            Eps = 1.0e-7,
            Delta = 1.0e-7
        };
        var slae = new Fem.Slae.NonlinearTask.Elliptic.OneDim.Linear(matrix, vec, _gaussSeidel, _integrator);
        var expected = new[] {1.0, 1.0, 1.0};

        // Act
        slae.Solve(accuracy);
        var result = new double[slae.ResVec.Length];
        slae.ResVec.AsSpan().CopyTo(result);

        // Assert
        for (var i = 0; i < result.Length; i++)
        {
            Assert.AreEqual(result[i], expected[i], 1.0e-7);
        }
    }

    [Test]
    public void Solve_WhenPass3DiagMatrixWithLessDiagDominance_ShouldReturnCorrectResVec()
    {
        // Arrange
        var upper = new[] {1.0, 1.0};
        var center = new[] {2.0, 2.0, 2.0};
        var lower = new[] {1.0, 1.0};

        var matrix = new LinearAlgebra.Matrix.ThreeDiagonal(upper, center, lower);

        var vec = new[] {3.0, 4.0, 3.0};

        var accuracy = new DataModels.Accuracy
        {
            MaxIter = 1000,
            Eps = 1.0e-7,
            Delta = 1.0e-7
        };
        var slae = new Fem.Slae.NonlinearTask.Elliptic.OneDim.Linear(matrix, vec, _gaussSeidel, _integrator);
        var expected = new[] {1.0, 1.0, 1.0};

        // Act
        slae.Solve(accuracy);
        var result = new double[slae.ResVec.Length];
        slae.ResVec.AsSpan().CopyTo(result);

        // Assert
        for (var i = 0; i < result.Length; i++)
        {
            Assert.AreEqual(result[i], expected[i], 1.0e-5);
        }
    }

    [Test]
    public void RelativeResidual()
    {
        // Arrange
        var upper = new[] {1.0, 1.0};
        var center = new[] {2.0, 2.0, 2.0};
        var lower = new[] {1.0, 1.0};

        var matrix = new LinearAlgebra.Matrix.ThreeDiagonal(upper, center, lower);

        var vec = new[] {3.0, 4.0, 3.0};

        var accuracy = new DataModels.Accuracy
        {
            MaxIter = 1000,
            Eps = 1.0e-7,
            Delta = 1.0e-7
        };
        var slae = new Fem.Slae.NonlinearTask.Elliptic.OneDim.Linear(matrix, vec, _gaussSeidel, _integrator);
        var expected = 1.0e-5;

        // Act
        slae.Solve(accuracy);
        var result = LinearAlgebra.Utils.RelativeResidual(slae);

        // Assert
        Assert.AreEqual(result, expected, 1.0e-5);
    }
}