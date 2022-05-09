using Fengine.Backend.LinearAlgebra.Matrix;

namespace Fengine.Backend.Fem.Slae.LinearTask.Elliptic.TwoDim;

public class Biquadratic : ISlae
{
    public double[] Solve(DataModels.Accuracy accuracy)
    {
        throw new NotImplementedException();
    }

    public Biquadratic(IMatrix matrix, double[] rhsVec, double[] resVec)
    {
        Matrix = matrix;
        RhsVec = rhsVec;
        ResVec = resVec;
    }

    public Biquadratic
    (
        Mesh.Cylindrical.TwoDim mesh,
        DataModels.InputFuncs inputFuncs,
        double[] initApprox,
        LinearAlgebra.SlaeSolver.ISlaeSolver slaeSolver,
        Integration.IIntegrator integrator,
        IMatrix matrix
    )
    {
    }

    private Integration.IIntegrator _integrator;
    private LinearAlgebra.SlaeSolver.ISlaeSolver _slaeSolver;

    public IMatrix Matrix { get; set; }
    public double[] RhsVec { get; set; }
    public double[] ResVec { get; set; }
}