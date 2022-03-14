using System;
using NUnit.Framework;
using umf_2.Fem;

namespace umf_2_Tests;

[TestFixture]
public class GridTests
{
    private static bool IsNearby(double lhs, double rhs, double eps)
    {
        return Math.Abs(lhs - rhs) < eps;
    }

    [Test]
    public void GridCtor_WhenPassUniformRatio_ShouldReturnUniformGrid()
    {
        // Arrange
        var area = new umf_2.Models.Area
        {
            AmountPoints = 5,
            DischargeRatio = 1.0,
            LeftBorder = 0.0,
            RightBorder = 4.0
        };

        var expected = new[] {0.0, 1.0, 2.0, 3.0, 4.0};

        // Act
        var result = new Grid(area);

        // Assert
        for (var i = 0; i < expected.Length; i++)
        {
            Assert.IsTrue(IsNearby(result.X[i], expected[i], 1.0e-7));
        }
    }

    [Test]
    public void GridCtor_WhenPassNonUniformRatio_ShouldReturnNonUniformGrid()
    {
        // Arrange
        var area = new umf_2.Models.Area
        {
            AmountPoints = 3,
            DischargeRatio = 0.5,
            LeftBorder = 0.0,
            RightBorder = 3.0
        };

        var expected = new[] {0.0, 2.0, 3.0};

        // Act
        var result = new Grid(area);

        // Assert
        for (var i = 0; i < expected.Length; i++)
        {
            Assert.IsTrue(IsNearby(result.X[i], expected[i], 1.0e-7));
        }
    }
}