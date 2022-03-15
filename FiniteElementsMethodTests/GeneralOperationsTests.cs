using System;
using FiniteElementsMethod.LinAlg;
using NUnit.Framework;

namespace FiniteElementsMethodTests;

[TestFixture]
public class GeneralOperationsTests
{
    private static bool IsNearby(double lhs, double rhs, double eps)
    {
        return Math.Abs(lhs - rhs) < eps;
    }

    [Test]
    public void Norm_WhenPassVec_ShouldReturnSqrtOfSqrVec()
    {
        // Arrange
        var vec = new[] {1.0, 2.0, 3.0};
        var expected = Math.Sqrt(14.0);

        // Act
        var result = GeneralOperations.Norm(vec);

        // Assert
        Assert.IsTrue(IsNearby(result, expected, 1.0e-7));
    }

    [Test]
    public void Dot_WhenPassSparseMatrixAndVec_ShouldReturnScalar()
    {
        // Arrange
        var upper = new[] {1.0, 1.0};
        var center = new[] {2.0, 2.0, 2.0};
        var lower = new[] {3.0, 3.0};

        var matrix = new Matrix(upper, center, lower);

        const int i = 1;
        var vec = new[] {1.0, 1.0, 1.0};

        const double expected = 6.0;

        // Act
        var result = GeneralOperations.Dot(i, matrix, vec);

        // Assert
        Assert.IsTrue(IsNearby(result, expected, 1.0e-7));
    }

    [Test]
    public void MatMul_WhenPassSparseMatrixAndVec_ShouldReturnVec()
    {
        // Arrange
        var upper = new[] {1.0, 1.0};
        var center = new[] {2.0, 2.0, 2.0};
        var lower = new[] {3.0, 3.0};

        var matrix = new Matrix(upper, center, lower);

        var vec = new[] {1.0, 1.0, 1.0};

        var expected = new[] {3.0, 6.0, 5.0};

        // Act
        var result = GeneralOperations.MatMul(matrix, vec);

        // Assert
        for (var i = 0; i < expected.Length; i++)
        {
            Assert.IsTrue(IsNearby(result[i], expected[i], 1.0e-7));
        }
    }
}