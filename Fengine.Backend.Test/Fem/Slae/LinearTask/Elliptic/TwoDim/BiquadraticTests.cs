// using NUnit.Framework;
//
// namespace Fengine.Backend.Test.Fem.Slae.LinearTask.Elliptic.TwoDim;
//
// [TestFixture]
// public class BiquadraticTests
// {
//     private Backend.LinearAlgebra.SlaeSolver.ISlaeSolver _slaeSolver;
//     private Backend.Integration.IIntegrator _integrator;
//     private Backend.LinearAlgebra.Matrix.IMatrix _matrix;
//
//     [SetUp]
//     public void SetUp()
//     {
//         _slaeSolver = new Backend.LinearAlgebra.SlaeSolver.LocalOptimalScheme();
//         _integrator = new Backend.Integration.GaussFourPoints();
//         _matrix = new Backend.LinearAlgebra.Matrix.Sparse();
//     }
//
//     [Test]
//     public void CtorTest_WhenPass()
//     {
//         // Arrange
//         var inputFuncs = new DataModels.InputFuncs
//         {
//             Gamma = "1",
//             Lambda = "1",
//             UStar = "5",
//             RhsFunc = "5",
//             Sigma = "1",
//             Chi = "1"
//         };
//
//         var area = new DataModels.Area.TwoDim
//         {
//             AmountPointsR = 2,
//             AmountPointsZ = 2,
//             LeftBorder = 1,
//             RightBorder = 2,
//             LowerBorder = 1,
//             UpperBorder = 2
//         };
//
//         var timeArea = new DataModels.Area.OneDim
//         {
//             AmountPoints = 4,
//             LeftBorder = 1,
//             RightBorder = 2
//         };
//
//         var boundaryConditions = new DataModels.Conditions.Boundary.TwoDim
//         {
//             LeftType = "First",
//             LeftFunc = "5",
//             RightType = "First",
//             RightFunc = "5",
//             LowerType = "First",
//             LowerFunc = "5",
//             UpperType = "First",
//             UpperFunc = "5"
//         };
//
//         var initialConditions = new DataModels.Conditions.Initial
//         {
//             T0 = "t"
//         };
//
//         var meshSpatial = new Backend.Fem.Mesh.Cylindrical.TwoDim(area);
//         var meshTime = new Backend.Fem.Mesh.Time.OneDim(timeArea);
//
//         var accuracy = new DataModels.Accuracy
//         {
//             Eps = 1e-15,
//             Delta = 1e-15,
//             MaxIter = 1000
//         };
//
//         var slae = new Backend.Fem.Slae.LinearTask.Hyperbolic.TwoDim.BiquadraticImplicit4Layer(
//             area,
//             meshSpatial,
//             meshTime,
//             inputFuncs,
//             boundaryConditions,
//             _slaeSolver,
//             _integrator,
//             initialConditions
//         );
//
//         var expected = new[]
//         {
//             5.0, 5.0, 5.0,
//             5.0, 5.0, 5.0,
//             5.0, 5.0, 5.0,
//         };
//         var test = Backend.LinearAlgebra.GeneralOperations.MatrixMultiply(slae.Matrix, expected);
//
//         // Act
//         var actual = slae.Solve(accuracy);
//
//         // Assert
//         for (var i = 0; i < expected.Length; i++)
//         {
//             Assert.AreEqual(expected[i], actual[i], 1e-3);
//         }
//     }
// }

