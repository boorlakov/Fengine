using Fengine.Backend.Integration;
using Fengine.Backend.LinearAlgebra.Matrix;

namespace Fengine.Backend.Fem.Slae.LinearTask.Elliptic.TwoDim;

public class Biquadratic : ISlae
{
    private readonly FiniteElement[] _finiteElements;

    private IIntegrator _integrator;
    private LinearAlgebra.SlaeSolver.ISlaeSolver _slaeSolver;

    public Biquadratic(IMatrix matrix, double[] rhsVec, double[] resVec)
    {
        Matrix = matrix;
        RhsVec = rhsVec;
        ResVec = resVec;
    }

    public Biquadratic(IMatrix matrix, double[] rhsVec)
    {
        Matrix = matrix;
        RhsVec = rhsVec;
        ResVec = new double[rhsVec.Length];
    }

    public Biquadratic
    (
        Mesh.Cylindrical.TwoDim mesh,
        DataModels.InputFuncs inputFuncs,
        DataModels.Conditions.Boundary.TwoDim boundaryConditions,
        LinearAlgebra.SlaeSolver.ISlaeSolver slaeSolver,
        IIntegrator integrator,
        IMatrix matrix
    )
    {
        _slaeSolver = slaeSolver;
        _integrator = integrator;

        _finiteElements = GetFiniteElementsFrom(mesh);

        (Matrix, RhsVec) = GetPortraitFrom(mesh);
        ResVec = new double[RhsVec.Length];

        AssemblyGlobally();

        ApplyBoundaryConditions(boundaryConditions);
    }

    public double[] Solve(DataModels.Accuracy accuracy)
    {
        return _slaeSolver.Solve(this, accuracy);
    }

    public IMatrix Matrix { get; set; }
    public double[] RhsVec { get; set; }
    public double[] ResVec { get; set; }

    private void ApplyBoundaryConditions(DataModels.Conditions.Boundary.TwoDim boundaryConditions)
    {
        throw new NotImplementedException();
    }

    private (IMatrix Matrix, double[] RhsVec) GetPortraitFrom(Mesh.Cylindrical.TwoDim mesh)
    {
        throw new NotImplementedException();
    }

    private FiniteElement[] GetFiniteElementsFrom(Mesh.Cylindrical.TwoDim mesh)
    {
        var finiteElements = new FiniteElement[(mesh.R.Length - 1) * (mesh.Z.Length - 1)];

        var k = 0;
        var startPos = 0;
        var lineLenght = mesh.R.Length;

        for (var i = 0; i < mesh.Z.Length - 1; i++, startPos = i * lineLenght)
        {
            for (var j = 0; j < lineLenght - 1; j++, startPos += 1, k++)
            {
                AssemblyFiniteElementFrom(mesh, finiteElements, startPos, lineLenght, k);
            }
        }

        return finiteElements;
    }

    private static void AssemblyFiniteElementFrom
    (
        Mesh.Cylindrical.TwoDim mesh,
        FiniteElement[] finiteElements,
        int startPos,
        int lineLen,
        int k
    )
    {
        var localNodes = new[]
        {
            (
                startPos,
                mesh.Nodes[startPos].Coordinates[Mesh.Axis.R],
                mesh.Nodes[startPos].Coordinates[Mesh.Axis.Z]
            ),
            (
                startPos + 1,
                mesh.Nodes[startPos + 1].Coordinates[Mesh.Axis.R],
                mesh.Nodes[startPos + 1].Coordinates[Mesh.Axis.Z]
            ),

            (
                startPos + lineLen,
                mesh.Nodes[startPos + lineLen].Coordinates[Mesh.Axis.R],
                mesh.Nodes[startPos + lineLen].Coordinates[Mesh.Axis.Z]
            ),
            (
                startPos + lineLen + 1,
                mesh.Nodes[startPos + lineLen + 1].Coordinates[Mesh.Axis.R],
                mesh.Nodes[startPos + lineLen + 1].Coordinates[Mesh.Axis.Z]
            )
        };
        finiteElements[k] = new FiniteElement(localNodes);
    }

    private void AssemblyGlobally()
    {
        foreach (var finiteElement in _finiteElements)
        {
            AssemblyLocally(finiteElement);
        }
    }

    private void AssemblyLocally(FiniteElement finiteElement)
    {
        throw new NotImplementedException();
    }

    private class FiniteElement
    {
        public readonly (int globalNumber, double r, double z)[] Nodes;

        public FiniteElement((int, double, double)[] nodes)
        {
            Nodes = nodes;
        }
    }
}