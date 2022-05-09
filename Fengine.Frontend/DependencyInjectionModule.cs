using Fengine.Backend.DataModels;
using Fengine.Backend.DataModels.Conditions.Boundary;
using Fengine.Backend.Fem.Mesh;
using Fengine.Backend.Fem.Slae;
using Fengine.Backend.Fem.Solver;
using Fengine.Backend.Integration;
using Fengine.Backend.LinearAlgebra.Matrix;
using Fengine.Backend.LinearAlgebra.SlaeSolver;
using Microsoft.Extensions.DependencyInjection;

namespace Fengine.Frontend;

public static class DependencyInjectionModule
{
    public static IServiceCollection ConfigureServices()
    {
        return new ServiceCollection()
            .AddTransient<Accuracy>()
            .AddTransient<Backend.DataModels.Areas.OneDim>()
            .AddTransient<OneDim>()
            .AddTransient<InputFuncs>()
            .AddTransient<IMatrix, ThreeDiagonal>()
            .AddTransient<IIntegrator, GaussFourPoints>()
            .AddTransient<IMesh, Cartesian1D>()
            .AddTransient<ISlaeSolver, GaussSeidel>()
            .AddTransient<ISlae, Elliptic1DLinearBasisFNonLinear>()
            .AddTransient<IFemSolver, SimpleIteration>
            (
                x => new SimpleIteration
                (
                    x.GetRequiredService<ISlaeSolver>(),
                    x.GetRequiredService<IIntegrator>(),
                    x.GetRequiredService<IMatrix>(),
                    x.GetRequiredService<ISlae>()
                )
            );
    }
}