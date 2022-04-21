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
            .AddTransient<IFemSolver>(x =>
                new FemSolverWithSimpleIteration(
                    x.GetRequiredService<ISlaeSolver>(),
                    x.GetRequiredService<IIntegrator>(),
                    x.GetRequiredService<IMatrix>()
                ));
    }
}