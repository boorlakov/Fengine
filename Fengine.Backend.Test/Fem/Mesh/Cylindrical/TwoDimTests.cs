using NUnit.Framework;

namespace Fengine.Backend.Test.Fem.Mesh.Cylindrical;

[TestFixture]
public class TwoDimTests
{
    [Test]
    public void MeshCtor_WhenPassUniformRatio_ShouldReturnUniformGrid()
    {
        // Arrange
        var area = new DataModels.Area.TwoDim
        {
            AmountPointsR = 5,
            LeftBorder = 1.0,
            RightBorder = 2.0,
            DischargeRatioR = 1.0,
            AmountPointsZ = 5,
            LowerBorder = 1.0,
            UpperBorder = 2.0,
            DischargeRatioZ = 1.0
        };

        var expectedR = new[]
        {
            1.0, 1.25, 1.5, 1.75, 2.0,
            1.0, 1.25, 1.5, 1.75, 2.0,
            1.0, 1.25, 1.5, 1.75, 2.0,
            1.0, 1.25, 1.5, 1.75, 2.0,
            1.0, 1.25, 1.5, 1.75, 2.0
        };
        var expectedZ = new[]
        {
            1.0, 1.0, 1.0, 1.0, 1.0,
            1.25, 1.25, 1.25, 1.25, 1.25,
            1.5, 1.5, 1.5, 1.5, 1.5,
            1.75, 1.75, 1.75, 1.75, 1.75,
            2.0, 2.0, 2.0, 2.0, 2.0
        };

        // Act
        var result = new Backend.Fem.Mesh.Cylindrical.TwoDim(area);

        // Assert
        for (var i = 0; i < expectedR.Length; i++)
        {
            Assert.AreEqual(expectedR[i], result.Nodes[i].Coordinates[Backend.Fem.Mesh.Axis.R], 1.0e-7);
            Assert.AreEqual(expectedZ[i], result.Nodes[i].Coordinates[Backend.Fem.Mesh.Axis.Z], 1.0e-7);
        }
    }

    [Test]
    public void MeshCtor_WhenPassNonUniformRatio_ShouldReturnNonUniformGrid()
    {
        // Arrange
        var area = new DataModels.Area.TwoDim
        {
            AmountPointsR = 3,
            LeftBorder = 0.0,
            RightBorder = 3.0,
            DischargeRatioR = 0.5,
            AmountPointsZ = 3,
            LowerBorder = 0.0,
            UpperBorder = 3.0,
            DischargeRatioZ = 0.5,
        };

        var expectedR = new[]
        {
            0.0, 2.0, 3.0,
            0.0, 2.0, 3.0,
            0.0, 2.0, 3.0
        };

        var expectedZ = new[]
        {
            0.0, 0.0, 0.0,
            2.0, 2.0, 2.0,
            3.0, 3.0, 3.0
        };

        // Act
        var result = new Backend.Fem.Mesh.Cylindrical.TwoDim(area);

        // Assert
        // Assert
        for (var i = 0; i < expectedR.Length; i++)
        {
            Assert.AreEqual(expectedR[i], result.Nodes[i].Coordinates[Backend.Fem.Mesh.Axis.R], 1.0e-7);
            Assert.AreEqual(expectedZ[i], result.Nodes[i].Coordinates[Backend.Fem.Mesh.Axis.Z], 1.0e-7);
        }
    }
}