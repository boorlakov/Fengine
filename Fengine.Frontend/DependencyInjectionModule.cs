using Microsoft.Extensions.DependencyInjection;

namespace Fengine.Frontend;

public static class DependencyInjectionModule
{
    public static IServiceCollection ConfigureServices()
    {
        return new ServiceCollection()
            .AddTransient<Backend.DataModels.Accuracy>()
            .AddTransient<Backend.DataModels.Areas.OneDim>()
            .AddTransient<Backend.DataModels.Conditions.Boundary.OneDim>()
            .AddTransient<Backend.DataModels.InputFuncs>()
            .AddTransient<Backend.LinearAlgebra.Matrix.IMatrix, Backend.LinearAlgebra.Matrix.ThreeDiagonal>()
            .AddTransient<Backend.Integration.IIntegrator, Backend.Integration.GaussFourPoints>()
            .AddTransient<Backend.Fem.Mesh.IMesh, Backend.Fem.Mesh.Cartesian.OneDim>()
            .AddTransient<Backend.LinearAlgebra.SlaeSolver.ISlaeSolver, Backend.LinearAlgebra.SlaeSolver.GaussSeidel>()
            .AddTransient<Backend.Fem.Slae.ISlae, Backend.Fem.Slae.Elliptic1DLinearBasisFNonLinear>()
            .AddTransient<Backend.Fem.Solver.IFemSolver, Backend.Fem.Solver.SimpleIteration>
            (
                x => new Backend.Fem.Solver.SimpleIteration
                (
                    x.GetRequiredService<Backend.LinearAlgebra.SlaeSolver.ISlaeSolver>(),
                    x.GetRequiredService<Backend.Integration.IIntegrator>(),
                    x.GetRequiredService<Backend.LinearAlgebra.Matrix.IMatrix>(),
                    x.GetRequiredService<Backend.Fem.Slae.ISlae>()
                )
            );
    }
}