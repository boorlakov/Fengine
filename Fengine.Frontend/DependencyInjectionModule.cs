using Fengine.Backend.DataModels;
using Fengine.Backend.Fem.Mesh;
using Fengine.Backend.Fem.Slae;
using Fengine.Backend.Fem.Solver;
using Fengine.Backend.Integration;
using Fengine.Backend.LinAlg.Matrix;
using Fengine.Backend.LinAlg.SlaeSolver;
using Microsoft.Extensions.DependencyInjection;

namespace Fengine.Frontend;

public static class DependencyInjectionModule
{
    public static IServiceCollection ConfigureServices()
    {
        return new ServiceCollection()
            .AddTransient<Accuracy>()
            .AddTransient<Area>()
            .AddTransient<BoundaryConditions>()
            .AddTransient<InputFuncs>()
            .AddTransient<IMatrix, ThreeDiagonal>()
            .AddTransient<IIntegrator, Gauss4Points>()
            .AddTransient<IMesh, Cartesian1D>()
            .AddTransient<ISlaeSolver, GaussSeidel>()
            .AddTransient<ISlae, Elliptic1DLinearFNonLinear>()
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