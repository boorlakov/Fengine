using Microsoft.Extensions.DependencyInjection;

namespace Fengine.Frontend;

public static class DependencyInjectionModule
{
    public static IServiceCollection ConfigureServices()
    {
        return new ServiceCollection()
            .AddTransient<Backend.DataModels.Accuracy>()
            .AddTransient<Backend.DataModels.Area.OneDim>()
            .AddTransient<Backend.DataModels.Conditions.Boundary.OneDim>()
            .AddTransient<Backend.DataModels.InputFuncs>()
            .AddTransient<Backend.LinearAlgebra.Matrix.IMatrix, Backend.LinearAlgebra.Matrix.ThreeDiagonal>()
            .AddTransient<Backend.Integration.IIntegrator, Backend.Integration.GaussFourPoints>()
            .AddTransient<Backend.Fem.Mesh.IMesh, Backend.Fem.Mesh.Cartesian.OneDim>()
            .AddTransient<Backend.LinearAlgebra.SlaeSolver.ISlaeSolver, Backend.LinearAlgebra.SlaeSolver.GaussSeidel>()
            .AddTransient<Backend.Fem.Slae.ISlae, Backend.Fem.Slae.OneDim.EllipticLinearBasisFNonLinear>();
    }
}