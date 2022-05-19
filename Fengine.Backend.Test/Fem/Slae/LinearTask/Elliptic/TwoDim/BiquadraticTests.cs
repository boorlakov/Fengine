using NUnit.Framework;

namespace Fengine.Backend.Test.Fem.Slae.LinearTask.Elliptic.TwoDim;

[TestFixture]
public class BiquadraticTests
{
    private Backend.LinearAlgebra.SlaeSolver.ISlaeSolver _slaeSolver;
    private Backend.Integration.IIntegrator _integrator;
    private Backend.LinearAlgebra.Matrix.IMatrix _matrix;

    [SetUp]
    public void SetUp()
    {
        _slaeSolver = new Backend.LinearAlgebra.SlaeSolver.LocalOptimalScheme();
        _integrator = new Backend.Integration.GaussFourPoints();
        _matrix = new Backend.LinearAlgebra.Matrix.Sparse();
    }

    [Test]
    public void CtorTestParabolicConst_WhenPass()
    {
        // Arrange
        var inputFuncs = new DataModels.InputFuncs
        {
            Lambda = "1",
            UStar = "5",
            RhsFunc = "0",
            Sigma = "1",
            Chi = "0"
        };

        var area = new DataModels.Area.TwoDim
        {
            AmountPointsR = 2,
            AmountPointsZ = 2,
            LeftBorder = 1,
            RightBorder = 2,
            LowerBorder = 1,
            UpperBorder = 2
        };

        var timeArea = new DataModels.Area.OneDim
        {
            AmountPoints = 4,
            LeftBorder = 0,
            RightBorder = 1
        };

        var boundaryConditions = new DataModels.Conditions.Boundary.TwoDim
        {
            LeftType = "First",
            LeftFunc = "5",
            RightType = "First",
            RightFunc = "5",
            LowerType = "First",
            LowerFunc = "5",
            UpperType = "First",
            UpperFunc = "5"
        };

        var initialConditions = new DataModels.Conditions.Initial
        {
            T0 = "5"
        };

        var meshSpatial = new Backend.Fem.Mesh.Cylindrical.TwoDim(area);
        var meshTime = new Backend.Fem.Mesh.Time.OneDim(timeArea);

        var accuracy = new DataModels.Accuracy
        {
            Eps = 1e-15,
            Delta = 1e-15,
            MaxIter = 1000
        };

        var slae = new Backend.Fem.Slae.LinearTask.Hyperbolic.TwoDim.BiquadraticImplicit4Layer(
            area,
            meshSpatial,
            meshTime,
            inputFuncs,
            boundaryConditions,
            _slaeSolver,
            _integrator,
            initialConditions,
            accuracy
        );

        var expected = new[]
        {
            5.0, 5.0, 5.0,
            5.0, 5.0, 5.0,
            5.0, 5.0, 5.0,
        };

        // Act
        var actual = slae.Solve(accuracy);

        // Assert
        for (var i = 0; i < expected.Length; i++)
        {
            Assert.AreEqual(expected[i], actual[i], 1e-5);
        }
    }

    [Test]
    public void CtorTestHyperbolicConst_WhenPass()
    {
        // Arrange
        var inputFuncs = new DataModels.InputFuncs
        {
            Lambda = "1",
            UStar = "5",
            RhsFunc = "0",
            Sigma = "1",
            Chi = "1"
        };

        var area = new DataModels.Area.TwoDim
        {
            AmountPointsR = 2,
            AmountPointsZ = 2,
            LeftBorder = 1,
            RightBorder = 2,
            LowerBorder = 1,
            UpperBorder = 2
        };

        var timeArea = new DataModels.Area.OneDim
        {
            AmountPoints = 4,
            LeftBorder = 0,
            RightBorder = 1
        };

        var boundaryConditions = new DataModels.Conditions.Boundary.TwoDim
        {
            LeftType = "First",
            LeftFunc = "5",
            RightType = "First",
            RightFunc = "5",
            LowerType = "First",
            LowerFunc = "5",
            UpperType = "First",
            UpperFunc = "5"
        };

        var initialConditions = new DataModels.Conditions.Initial
        {
            T0 = "5"
        };

        var meshSpatial = new Backend.Fem.Mesh.Cylindrical.TwoDim(area);
        var meshTime = new Backend.Fem.Mesh.Time.OneDim(timeArea);

        var accuracy = new DataModels.Accuracy
        {
            Eps = 1e-15,
            Delta = 1e-15,
            MaxIter = 1000
        };

        var slae = new Backend.Fem.Slae.LinearTask.Hyperbolic.TwoDim.BiquadraticImplicit4Layer(
            area,
            meshSpatial,
            meshTime,
            inputFuncs,
            boundaryConditions,
            _slaeSolver,
            _integrator,
            initialConditions,
            accuracy
        );

        var expected = new[]
        {
            5.0, 5.0, 5.0,
            5.0, 5.0, 5.0,
            5.0, 5.0, 5.0,
        };

        // Act
        var actual = slae.Solve(accuracy);

        // Assert
        for (var i = 0; i < expected.Length; i++)
        {
            Assert.AreEqual(expected[i], actual[i], 1e-5);
        }
    }

    [Test]
    public void CtorTestHyperbolicConstTimeR_WhenPass()
    {
        // Arrange
        var inputFuncs = new DataModels.InputFuncs
        {
            Lambda = "1",
            UStar = "r + z",
            RhsFunc = "-1.0 / r",
            Sigma = "1",
            Chi = "1"
        };

        var area = new DataModels.Area.TwoDim
        {
            AmountPointsR = 2,
            AmountPointsZ = 2,
            LeftBorder = 1,
            RightBorder = 2,
            LowerBorder = 1,
            UpperBorder = 2
        };

        var timeArea = new DataModels.Area.OneDim
        {
            AmountPoints = 4,
            LeftBorder = 0,
            RightBorder = 1
        };

        var boundaryConditions = new DataModels.Conditions.Boundary.TwoDim
        {
            LeftType = "First",
            LeftFunc = "r + z",
            RightType = "First",
            RightFunc = "r + z",
            LowerType = "First",
            LowerFunc = "r + z",
            UpperType = "First",
            UpperFunc = "r + z"
        };

        var initialConditions = new DataModels.Conditions.Initial
        {
            T0 = "r + z"
        };

        var meshSpatial = new Backend.Fem.Mesh.Cylindrical.TwoDim(area);
        var meshTime = new Backend.Fem.Mesh.Time.OneDim(timeArea);

        var accuracy = new DataModels.Accuracy
        {
            Eps = 1e-15,
            Delta = 1e-15,
            MaxIter = 1000
        };

        var slae = new Backend.Fem.Slae.LinearTask.Hyperbolic.TwoDim.BiquadraticImplicit4Layer
        (
            area,
            meshSpatial,
            meshTime,
            inputFuncs,
            boundaryConditions,
            _slaeSolver,
            _integrator,
            initialConditions,
            accuracy
        );

        var expected = new[]
        {
            2.0, 2.5, 3.0,
            2.5, 3.0, 3.5,
            3.0, 3.5, 4.0
        };

        // Act
        var actual = slae.Weights[3];

        // Assert
        for (var i = 0; i < expected.Length; i++)
        {
            Assert.AreEqual(expected[i], actual[i], 1e-5);
        }
    }

    [Test]
    public void CtorTestHyperbolicTPow1_WhenPass()
    {
        // Arrange
        var inputFuncs = new DataModels.InputFuncs
        {
            Lambda = "1",
            UStar = "t",
            RhsFunc = "1",
            Sigma = "1",
            Chi = "1"
        };

        var area = new DataModels.Area.TwoDim
        {
            AmountPointsR = 2,
            AmountPointsZ = 2,
            LeftBorder = 1,
            RightBorder = 2,
            LowerBorder = 1,
            UpperBorder = 2
        };

        var timeArea = new DataModels.Area.OneDim
        {
            AmountPoints = 4,
            LeftBorder = 0,
            RightBorder = 1
        };

        var boundaryConditions = new DataModels.Conditions.Boundary.TwoDim
        {
            LeftType = "First",
            LeftFunc = "t",
            RightType = "First",
            RightFunc = "t",
            LowerType = "First",
            LowerFunc = "t",
            UpperType = "First",
            UpperFunc = "t"
        };

        var initialConditions = new DataModels.Conditions.Initial
        {
            T0 = "t"
        };

        var meshSpatial = new Backend.Fem.Mesh.Cylindrical.TwoDim(area);
        var meshTime = new Backend.Fem.Mesh.Time.OneDim(timeArea);

        var accuracy = new DataModels.Accuracy
        {
            Eps = 1e-15,
            Delta = 1e-15,
            MaxIter = 1000
        };

        var slae = new Backend.Fem.Slae.LinearTask.Hyperbolic.TwoDim.BiquadraticImplicit4Layer
        (
            area,
            meshSpatial,
            meshTime,
            inputFuncs,
            boundaryConditions,
            _slaeSolver,
            _integrator,
            initialConditions,
            accuracy
        );

        var expected = new[]
        {
            1.0, 1.0, 1.0,
            1.0, 1.0, 1.0,
            1.0, 1.0, 1.0,
        };

        // Act
        var actual = slae.Weights[3];

        // Assert
        for (var i = 0; i < expected.Length; i++)
        {
            Assert.AreEqual(expected[i], actual[i], 1e-5);
        }
    }

    [Test]
    public void CtorTestHyperbolicTPow2_WhenPass()
    {
        // Arrange
        var inputFuncs = new DataModels.InputFuncs
        {
            Lambda = "1",
            UStar = "t*t",
            RhsFunc = "2*t + 2",
            Sigma = "1",
            Chi = "1"
        };

        var area = new DataModels.Area.TwoDim
        {
            AmountPointsR = 2,
            AmountPointsZ = 2,
            LeftBorder = 1,
            RightBorder = 2,
            LowerBorder = 1,
            UpperBorder = 2
        };

        var timeArea = new DataModels.Area.OneDim
        {
            AmountPoints = 4,
            LeftBorder = 0,
            RightBorder = 1
        };

        var boundaryConditions = new DataModels.Conditions.Boundary.TwoDim
        {
            LeftType = "First",
            LeftFunc = "t * t",
            RightType = "First",
            RightFunc = "t * t",
            LowerType = "First",
            LowerFunc = "t * t",
            UpperType = "First",
            UpperFunc = "t * t"
        };

        var initialConditions = new DataModels.Conditions.Initial
        {
            T0 = "t * t"
        };

        var meshSpatial = new Backend.Fem.Mesh.Cylindrical.TwoDim(area);
        var meshTime = new Backend.Fem.Mesh.Time.OneDim(timeArea);

        var accuracy = new DataModels.Accuracy
        {
            Eps = 1e-15,
            Delta = 1e-15,
            MaxIter = 1000
        };

        var slae = new Backend.Fem.Slae.LinearTask.Hyperbolic.TwoDim.BiquadraticImplicit4Layer(
            area,
            meshSpatial,
            meshTime,
            inputFuncs,
            boundaryConditions,
            _slaeSolver,
            _integrator,
            initialConditions,
            accuracy
        );

        var expected = new[]
        {
            1.0, 1.0, 1.0,
            1.0, 1.0, 1.0,
            1.0, 1.0, 1.0,
        };

        // Act
        var actual = slae.Weights[3];

        // Assert
        for (var i = 0; i < expected.Length; i++)
        {
            Assert.AreEqual(expected[i], actual[i], 1e-5);
        }
    }

    [Test]
    public void CtorTestHyperbolicTPow3_WhenPass()
    {
        // Arrange
        var inputFuncs = new DataModels.InputFuncs
        {
            Lambda = "1",
            UStar = "t*t*t",
            RhsFunc = "6 * t + 3 * t * t",
            Sigma = "1",
            Chi = "1"
        };

        var area = new DataModels.Area.TwoDim
        {
            AmountPointsR = 2,
            AmountPointsZ = 2,
            LeftBorder = 1,
            RightBorder = 2,
            LowerBorder = 1,
            UpperBorder = 2
        };

        var timeArea = new DataModels.Area.OneDim
        {
            AmountPoints = 4,
            LeftBorder = 0,
            RightBorder = 1
        };

        var boundaryConditions = new DataModels.Conditions.Boundary.TwoDim
        {
            LeftType = "First",
            LeftFunc = "t * t * t",
            RightType = "First",
            RightFunc = "t * t * t",
            LowerType = "First",
            LowerFunc = "t * t * t",
            UpperType = "First",
            UpperFunc = "t * t * t"
        };

        var initialConditions = new DataModels.Conditions.Initial
        {
            T0 = "t * t * t"
        };

        var meshSpatial = new Backend.Fem.Mesh.Cylindrical.TwoDim(area);
        var meshTime = new Backend.Fem.Mesh.Time.OneDim(timeArea);

        var accuracy = new DataModels.Accuracy
        {
            Eps = 1e-15,
            Delta = 1e-15,
            MaxIter = 1000
        };

        var slae = new Backend.Fem.Slae.LinearTask.Hyperbolic.TwoDim.BiquadraticImplicit4Layer(
            area,
            meshSpatial,
            meshTime,
            inputFuncs,
            boundaryConditions,
            _slaeSolver,
            _integrator,
            initialConditions,
            accuracy
        );

        var expected = new[]
        {
            1.0, 1.0, 1.0,
            1.0, 1.0, 1.0,
            1.0, 1.0, 1.0,
        };

        // Act
        var actual = slae.Weights[3];

        // Assert
        for (var i = 0; i < expected.Length; i++)
        {
            Assert.AreEqual(expected[i], actual[i], 1e-5);
        }
    }
}