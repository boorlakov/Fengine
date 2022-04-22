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
            .AddTransient<IMatrix, Matrix3Diagonal>()
            .AddTransient<IIntegrator, IntegratorGauss4Points>()
            .AddTransient<IMesh, Cartesian1DMesh>()
            .AddTransient<ISlaeSolver, SlaeSolverGaussSeidel>()
            .AddTransient<ISlae, Slae1DEllipticLinearFNonLinear>()
            .AddTransient<IFemSolver, FemSolverWithSimpleIteration>
            (
                x => new FemSolverWithSimpleIteration
                (
                    x.GetRequiredService<ISlaeSolver>(),
                    x.GetRequiredService<IIntegrator>(),
                    x.GetRequiredService<IMatrix>(),
                    x.GetRequiredService<ISlae>()
                )
            );
    }
}