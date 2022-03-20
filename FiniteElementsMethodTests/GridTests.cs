using FiniteElementsMethod.Fem;
using NUnit.Framework;

namespace FiniteElementsMethodTests;

[TestFixture]
public class GridTests
{
    [Test]
    public void GridCtor_WhenPassUniformRatio_ShouldReturnUniformGrid()
    {
        // Arrange
        var area = new FiniteElementsMethod.Models.Area
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
            Assert.AreEqual(result.X[i], expected[i], 1.0e-7);
        }
    }

    [Test]
    public void GridCtor_WhenPassNonUniformRatio_ShouldReturnNonUniformGrid()
    {
        // Arrange
        var area = new FiniteElementsMethod.Models.Area
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
            Assert.AreEqual(result.X[i], expected[i], 1.0e-7);
        }
    }
}