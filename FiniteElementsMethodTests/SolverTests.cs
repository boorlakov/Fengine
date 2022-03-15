using System;
using FiniteElementsMethod.Fem;
using FiniteElementsMethod.Models;
using NUnit.Framework;

namespace FiniteElementsMethodTests;

[TestFixture]
public class SolverTests
{
    private static bool IsNearby(double lhs, double rhs, double eps)
    {
        return Math.Abs(lhs - rhs) < eps;
    }

    [Test]
    public void FemSolverTest()
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

        var grid = new Grid(area);

        var expected = new[] {2.0, 2.1, 2.2, 2.3, 2.4, 2.5, 2.6, 2.7, 2.8, 2.9, 3.0};

        // Act
        var result = Solver.SolveWithSimpleIteration(
            grid,
            inputFuncs,
            area,
            boundaryConditions,
            accuracy
        );

        // Assert
        for (var i = 0; i < expected.Length; i++)
        {
            Assert.IsTrue(IsNearby(result[i], expected[i], 1.0e-7));
        }
    }
}