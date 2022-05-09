namespace Fengine.Backend.Fem.Solver;

public interface IFemSolver
{
    Statistics Solve() => throw new NotSupportedException();
}