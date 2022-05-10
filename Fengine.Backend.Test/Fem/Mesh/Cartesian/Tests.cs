using NUnit.Framework;

namespace Fengine.Backend.Test.Fem.Mesh.Cartesian;

[TestFixture]
public class Tests
{
    [Test]
    public void MeshCtor_WhenPassUniformRatio_ShouldReturnUniformGrid()
    {
        // Arrange
        var area = new DataModels.Area.OneDim
        {
            AmountPoints = 5,
            DischargeRatio = 1.0,
            LeftBorder = 0.0,
            RightBorder = 4.0
        };

        var expected = new[] {0.0, 1.0, 2.0, 3.0, 4.0};

        // Act
        var result = new Backend.Fem.Mesh.Cartesian.OneDim(area);

        // Assert
        for (var i = 0; i < expected.Length; i++)
        {
            Assert.AreEqual(result.Nodes[i].Coordinates[Backend.Fem.Mesh.Axis.X], expected[i], 1.0e-7);
        }
    }

    [Test]
    public void MeshCtor_WhenPassNonUniformRatio_ShouldReturnNonUniformGrid()
    {
        // Arrange
        var area = new DataModels.Area.OneDim
        {
            AmountPoints = 3,
            DischargeRatio = 0.5,
            LeftBorder = 0.0,
            RightBorder = 3.0
        };

        var expected = new[] {0.0, 2.0, 3.0};

        // Act
        var result = new Backend.Fem.Mesh.Cartesian.OneDim(area);

        // Assert
        for (var i = 0; i < expected.Length; i++)
        {
            Assert.AreEqual(result.Nodes[i].Coordinates[Backend.Fem.Mesh.Axis.X], expected[i], 1.0e-7);
        }
    }
}