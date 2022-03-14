using System;
using NUnit.Framework;
using umf_2.Integration;

namespace umf_2_Tests;

[TestFixture]
public class IntegratorTests
{
    private static bool IsNearby(double lhs, double rhs, double eps)
    {
        return Math.Abs(lhs - rhs) < eps;
    }

    [Test]
    public void MakeGrid_WhenPass0To1_ShouldReturn11ElemsArrFrom0To1()
    {
        // Arrange
        const double leftBorder = 0.0;
        const double rightBorder = 1.0;

        var expected = new[] {0.0, 0.1, 0.2, 0.3, 0.4, 0.5, 0.6, 0.7, 0.8, 0.9, 1.0};

        // Act
        var result = Integrator.MakeGrid(leftBorder, rightBorder);

        // Assert 
        for (var i = 0; i < expected.Length; i++)
        {
            Assert.IsTrue(IsNearby(result[i], expected[i], 1.0e-7));
        }
    }

    [Test]
    public void Integrate1D_WhenPassConst_ShouldReturnRectangleArea()
    {
        // Arrange
        var grid = Integrator.MakeGrid(0.0, 2.0);
        var func = (double x) => 1.0;

        const double expected = 2.0;

        // Act
        var result = Integrator.Integrate1D(grid, func);

        // Assert
        Assert.IsTrue(IsNearby(result, expected, 1.0e-7));
    }

    [Test]
    public void Integrate1D_WhenPassLinear_ShouldReturnTriangleArea()
    {
        // Arrange
        var grid = Integrator.MakeGrid(0.0, 2.0);
        var func = (double x) => x;

        const double expected = 2.0;

        // Act
        var result = Integrator.Integrate1D(grid, func);

        // Assert
        Assert.IsTrue(IsNearby(result, expected, 1.0e-7));
    }

    [Test]
    public void Integrate1D_WhenPassCube_ShouldReturnArea()
    {
        // Arrange
        var grid = Integrator.MakeGrid(0.0, 2.0);
        var func = (double x) => x * x * x;

        const double expected = 4.0;

        // Act
        var result = Integrator.Integrate1D(grid, func);

        // Assert
        Assert.IsTrue(IsNearby(result, expected, 1.0e-7));
    }

    [Test]
    public void Integrate1DWithStringFunc_WhenPassConst_ShouldReturnRectangleArea()
    {
        // Arrange
        var grid = Integrator.MakeGrid(0.0, 2.0);
        const string func = "1.0";

        const double expected = 2.0;

        // Act
        var result = Integrator.Integrate1DWithStringFunc(grid, func);

        // Assert
        Assert.IsTrue(IsNearby(result, expected, 1.0e-7));
    }

    [Test]
    public void Integrate1DWithStringFunc_WhenPassLinear_ShouldReturnTriangleArea()
    {
        // Arrange
        var grid = Integrator.MakeGrid(0.0, 2.0);
        const string func = "x";

        const double expected = 2.0;

        // Act
        var result = Integrator.Integrate1DWithStringFunc(grid, func);

        // Assert
        Assert.IsTrue(IsNearby(result, expected, 1.0e-7));
    }

    [Test]
    public void Integrate1DWithStringFunc_WhenPassCube_ShouldReturnArea()
    {
        // Arrange
        var grid = Integrator.MakeGrid(0.0, 2.0);
        const string func = "x * x * x";

        const double expected = 4.0;

        // Act
        var result = Integrator.Integrate1DWithStringFunc(grid, func);

        // Assert
        Assert.IsTrue(IsNearby(result, expected, 1.0e-7));
    }

    [Test]
    public void Integrate1DWithStringFunc_WhenPassPowCube_ShouldReturnArea()
    {
        // Arrange
        var grid = Integrator.MakeGrid(0.0, 2.0);
        const string func = "Pow(x, 3)";

        const double expected = 4.0;

        // Act
        var result = Integrator.Integrate1DWithStringFunc(grid, func);

        // Assert
        Assert.IsTrue(IsNearby(result, expected, 1.0e-7));
    }
}