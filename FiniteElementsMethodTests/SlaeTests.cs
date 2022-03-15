using System;
using FiniteElementsMethod.Fem;
using FiniteElementsMethod.LinAlg;
using FiniteElementsMethod.Models;
using NUnit.Framework;

namespace FiniteElementsMethodTests;

[TestFixture]
public class SlaeTests
{
    private static bool IsNearby(double lhs, double rhs, double eps)
    {
        return Math.Abs(lhs - rhs) < eps;
    }

    [Test]
    public void Solve_WhenPass1DiagMatrix_ShouldReturnCorrectResVec()
    {
        // Arrange
        var upper = new[] {0.0, 0.0};
        var center = new[] {2.0, 2.0, 2.0};
        var lower = new[] {0.0, 0.0};

        var matrix = new Matrix(upper, center, lower);

        var vec = new[] {4.0, 6.0, 8.0};
        var slae = new Slae(matrix, vec);
        var expected = new[] {2.0, 3.0, 4.0};
        var accuracy = new Accuracy
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
            Assert.IsTrue(IsNearby(result[i], expected[i], 1.0e-7));
        }
    }

    [Test]
    public void Solve_WhenPass3DiagMatrix_ShouldReturnCorrectResVec()
    {
        // Arrange
        var upper = new[] {1.0, 0.0};
        var center = new[] {2.0, 2.0, 2.0};
        var lower = new[] {0.0, 2.0};

        var matrix = new Matrix(upper, center, lower);

        var vec = new[] {3.0, 2.0, 4.0};
        var accuracy = new Accuracy
        {
            MaxIter = 1000,
            Eps = 1.0e-7,
            Delta = 1.0e-7
        };
        var slae = new Slae(matrix, vec);
        var expected = new[] {1.0, 1.0, 1.0};

        // Act
        slae.Solve(accuracy);
        var result = new double[slae.ResVec.Length];
        slae.ResVec.AsSpan().CopyTo(result);

        // Assert
        for (var i = 0; i < result.Length; i++)
        {
            Assert.IsTrue(IsNearby(result[i], expected[i], 1.0e-7));
        }
    }

    [Test]
    public void Solve_WhenPass3DiagMatrixWithLessDiagDominance_ShouldReturnCorrectResVec()
    {
        // Arrange
        var upper = new[] {1.0, 1.0};
        var center = new[] {2.0, 2.0, 2.0};
        var lower = new[] {1.0, 1.0};

        var matrix = new Matrix(upper, center, lower);

        var vec = new[] {3.0, 4.0, 3.0};

        var accuracy = new Accuracy
        {
            MaxIter = 1000,
            Eps = 1.0e-7,
            Delta = 1.0e-7
        };
        var slae = new Slae(matrix, vec);
        var expected = new[] {1.0, 1.0, 1.0};

        // Act
        slae.Solve(accuracy);
        var result = new double[slae.ResVec.Length];
        slae.ResVec.AsSpan().CopyTo(result);

        // Assert
        for (var i = 0; i < result.Length; i++)
        {
            Assert.IsTrue(IsNearby(result[i], expected[i], 1.0e-5));
        }
    }

    [Test]
    public void RelResigual()
    {
        // Arrange
        var upper = new[] {1.0, 1.0};
        var center = new[] {2.0, 2.0, 2.0};
        var lower = new[] {1.0, 1.0};

        var matrix = new Matrix(upper, center, lower);

        var vec = new[] {3.0, 4.0, 3.0};

        var accuracy = new Accuracy
        {
            MaxIter = 1000,
            Eps = 1.0e-7,
            Delta = 1.0e-7
        };
        var slae = new Slae(matrix, vec);
        var expected = 1.0e-5;

        // Act
        slae.Solve(accuracy);
        var result = SlaeSolver.RelResidual(slae);

        // Assert
        Assert.IsTrue(IsNearby(result, expected, 1.0e-5));
    }
}