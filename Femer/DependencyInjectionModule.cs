using Fengine.Fem.Mesh;
using Fengine.Fem.Solver;
using Fengine.Integration;
using Fengine.LinAlg.Matrix;
using Fengine.LinAlg.SlaeSolver;
using Microsoft.Extensions.DependencyInjection;

namespace Femer;

public static class DependencyInjectionModule
{
    public static IServiceCollection ConfigureServices()
    {
        return new ServiceCollection()
            .AddTransient<IFemSolver, FemSolverWithSimpleIteration>()
            .AddTransient<ISlaeSolver, SlaeSolverGs>()
            .AddTransient<IIntegrator, IntegratorG4>()
            .AddTransient<IMesh, Cartesian1DMesh>()
            .AddTransient<IMatrix, Matrix3Diag>()
            .AddTransient<double[]>();
    }
}