using Fengine.Fem.Solver;
using Fengine.Integration;
using Fengine.LinAlg.SlaeSolver;
using Microsoft.Extensions.DependencyInjection;

namespace Femer;

public class DependencyInjectionModule
{
    public static IServiceCollection ConfigureServices()
    {
        var services = new ServiceCollection();
        services.AddTransient<FemSolverWithSimpleIteration>();
        services.AddTransient<SlaeSolverGs>();
        services.AddTransient<IIntegrator, IntegratorG4>();
        return services;
    }
}