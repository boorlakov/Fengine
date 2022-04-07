using FiniteElementsMethod.Fem;
using FiniteElementsMethod.Integration;
using FiniteElementsMethod.LinAlg;
using Microsoft.Extensions.DependencyInjection;

namespace Femer;

public class DependencyInjectionModule
{
    public static IServiceCollection ConfigureServices()
    {
        var services = new ServiceCollection();
        services.AddTransient<Solver>();
        services.AddTransient<SlaeSolver>();
        services.AddTransient<IIntegrator, IntegratorG4>();
        return services;
    }
}