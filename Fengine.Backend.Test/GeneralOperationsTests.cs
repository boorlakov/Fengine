using System;
using NUnit.Framework;

namespace Fengine.Backend.Test;

[TestFixture]
public class GeneralOperationsTests
{
    [Test]
    public void Norm_WhenPassVec_ShouldReturnSqrtOfSqrVec()
    {
        // Arrange
        var vec = new[] {1.0, 2.0, 3.0};
        var expected = Math.Sqrt(14.0);

        // Act
        var result = LinearAlgebra.GeneralOperations.Norm(vec);

        // Assert
        Assert.AreEqual(result, expected, 1.0e-7);
    }

    [Test]
    public void Dot_WhenPassSparseMatrixAndVec_ShouldReturnScalar()
    {
        // Arrange
        var upper = new[] {1.0, 1.0};
        var center = new[] {2.0, 2.0, 2.0};
        var lower = new[] {3.0, 3.0};

        var matrix = new LinearAlgebra.Matrix.ThreeDiagonal(upper, center, lower);

        const int i = 1;
        var vec = new[] {1.0, 1.0, 1.0};

        const double expected = 6.0;

        // Act
        var result = LinearAlgebra.GeneralOperations.Dot(i, matrix, vec);

        // Assert
        Assert.AreEqual(result, expected, 1.0e-7);
    }

    [Test]
    public void MatMul_WhenPassSparseMatrixAndVec_ShouldReturnVec()
    {
        // Arrange
        var upper = new[] {1.0, 1.0};
        var center = new[] {2.0, 2.0, 2.0};
        var lower = new[] {3.0, 3.0};

        var matrix = new LinearAlgebra.Matrix.ThreeDiagonal(upper, center, lower);

        var vec = new[] {1.0, 1.0, 1.0};

        var expected = new[] {3.0, 6.0, 5.0};

        // Act
        var result = LinearAlgebra.GeneralOperations.MatrixMultiply(matrix, vec);

        // Assert
        for (var i = 0; i < expected.Length; i++)
        {
            Assert.AreEqual(result[i], expected[i], 1.0e-7);
        }
    }
}