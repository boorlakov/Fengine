// using System;
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
//     [Obsolete("TODO: ADD MORE INFO TO TEST")]
//     [Test]
//     public void CtorTest_WhenPass()
//     {
//         // Arrange
//         var inputFuncs = new DataModels.InputFuncs();
//         var area = new DataModels.Area.TwoDim
//         {
//             AmountPointsR = 5,
//             AmountPointsZ = 5,
//             LeftBorder = 0,
//             RightBorder = 1,
//             LowerBorder = 0,
//             UpperBorder = 1
//         };
//         var boundaryConditions = new DataModels.Conditions.Boundary.TwoDim();
//         var mesh = new Backend.Fem.Mesh.Cylindrical.TwoDim(area);
//
//         // Act
//
//         var slae = new Backend.Fem.Slae.LinearTask.Elliptic.TwoDim.Biquadratic
//         (
//             mesh,
//             inputFuncs,
//             boundaryConditions,
//             _slaeSolver,
//             _integrator
//         );
//
//         // Assert
//     }
// }

