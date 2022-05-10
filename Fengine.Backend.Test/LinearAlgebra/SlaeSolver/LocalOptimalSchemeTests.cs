using NUnit.Framework;

namespace Fengine.Backend.Test.LinearAlgebra.SlaeSolver;

[TestFixture]
public class LocalOptimalSchemeTests
{
    [Test]
    public void Solve_WhenPass11NegativeDefiniteMatrix_ShouldReturnCorrectResVec()
    {
        // Arrange
        var ig = new[] {1, 1, 2, 3, 4, 5, 7, 10, 13, 16, 19, 22, 25};
        var jg = new[] {1, 2, 3, 4, 1, 5, 1, 2, 6, 2, 3, 7, 3, 4, 8, 4, 5, 9, 5, 6, 10, 6, 7, 11};

        for (var i = 0; i < ig.Length; i++)
        {
            ig[i]--;
        }

        for (var i = 0; i < jg.Length; i++)
        {
            jg[i]--;
        }

        var ggl = new[]
        {
            -2.0, -1.0, -4.0, -2.0, -3.0, -3.0,
            -3.0, -2.0, -4.0, -2.0, -1.0, -5.0,
            -2.0, -1.0, -1.0, -1.0, -4.0, -2.0,
            -1.0, -1.0, -3.0, -2.0, -3.0, -1.0
        };

        var ggu = new[]
        {
            -2.0, -1.0, -4.0, -2.0, -3.0, -3.0,
            -4.0, -2.0, -4.0, -2.0, -1.0, -5.0,
            -2.0, -1.0, -1.0, -1.0, -4.0, -2.0,
            -1.0, -1.0, -3.0, -1.0, -2.0, -2.0,
        };

        var di = new[] {10.0, 7.0, 8.0, 8.0, 10.0, 12.0, 16.0, 9.0, 6.0, 10.0, 7.0, 6};

        var matrix = new Backend.LinearAlgebra.Matrix.Sparse(ggl, ggu, di, ig, jg);
        var rhs = new[] {-40.0, -21.0, -20.0, -9.0, -27.0, 3.0, 17.0, 21.0, 16.0, 25.0, 12.0, 28.0};

        var accuracy = new DataModels.Accuracy
        {
            Delta = 1e-15,
            Eps = 1e-15,
            MaxIter = 100
        };

        var expected = new[] {1.0, 2.0, 3.0, 4.0, 5.0, 6.0, 7.0, 8.0, 9.0, 10.0, 11.0};

        // Act
        var actual = Backend.LinearAlgebra.SlaeSolver.LocalOptimalScheme.Solve(matrix, rhs, accuracy);

        // Assert
        for (var i = 0; i < expected.Length; i++)
        {
            Assert.AreEqual(expected[i], actual[i], 1e-6);
        }
    }

    [Test]
    public void Solve_WhenPass11PositiveDefiniteMatrix_ShouldReturnCorrectResVec()
    {
        // Arrange         
        var ig = new[] {1, 1, 2, 3, 4, 5, 7, 10, 13, 16, 19, 22, 25};
        var jg = new[] {1, 2, 3, 4, 1, 5, 1, 2, 6, 2, 3, 7, 3, 4, 8, 4, 5, 9, 5, 6, 10, 6, 7, 11};

        for (var i = 0; i < ig.Length; i++)
        {
            ig[i]--;
        }

        for (var i = 0; i < jg.Length; i++)
        {
            jg[i]--;
        }

        var ggl = new[]
        {
            2.0, 1.0, 4.0, 2.0, 3.0, 3.0,
            3.0, 2.0, 4.0, 2.0, 1.0, 5.0,
            2.0, 1.0, 1.0, 1.0, 4.0, 2.0,
            1.0, 1.0, 3.0, 2.0, 3.0, 1.0
        };

        var ggu = new[]
        {
            2.0, 1.0, 4.0, 2.0, 3.0, 3.0,
            4.0, 2.0, 4.0, 2.0, 1.0, 5.0,
            2.0, 1.0, 1.0, 1.0, 4.0, 2.0,
            1.0, 1.0, 3.0, 1.0, 2.0, 2.0
        };

        var di = new[] {10.0, 7.0, 8.0, 8.0, 10.0, 12.0, 16.0, 9.0, 6.0, 10.0, 7.0, 6.0};

        var matrix = new Backend.LinearAlgebra.Matrix.Sparse(ggl, ggu, di, ig, jg);
        var rhs = new[] {60.0, 49.0, 68.0, 73.0, 127.0, 141.0, 207.0, 123.0, 92.0, 175.0, 142.0, 116.0};

        var accuracy = new DataModels.Accuracy
        {
            Delta = 1e-15,
            Eps = 1e-15,
            MaxIter = 100
        };

        var expected = new[] {1.0, 2.0, 3.0, 4.0, 5.0, 6.0, 7.0, 8.0, 9.0, 10.0, 11.0};

        // Act
        var actual = Backend.LinearAlgebra.SlaeSolver.LocalOptimalScheme.Solve(matrix, rhs, accuracy);

        // Assert
        for (var i = 0; i < expected.Length; i++)
        {
            Assert.AreEqual(expected[i], actual[i], 1e-6);
        }
    }
}