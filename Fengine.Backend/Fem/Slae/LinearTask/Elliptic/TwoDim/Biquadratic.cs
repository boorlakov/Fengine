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
        var startPosition = 0;
        var lineLenght = mesh.R.Length;

        for (var i = 0; i < mesh.Z.Length - 1; i++, startPosition = i * lineLenght)
        {
            for (var j = 0; j < lineLenght - 1; j++, startPosition += 1, k++)
            {
                var localNodes = GetLocalNodesFrom(mesh, startPosition, lineLenght);
                finiteElements[k] = new FiniteElement(localNodes);
            }
        }

        return finiteElements;
    }

    private static (int, double, double)[] GetLocalNodesFrom
    (
        Mesh.Cylindrical.TwoDim mesh,
        int startPosition,
        int lineLenght
    )
    {
        return new[]
        {
            (
                startPosition,
                mesh.Nodes[startPosition].Coordinates[Mesh.Axis.R],
                mesh.Nodes[startPosition].Coordinates[Mesh.Axis.Z]
            ),
            (
                startPosition + 1,
                mesh.Nodes[startPosition + 1].Coordinates[Mesh.Axis.R],
                mesh.Nodes[startPosition + 1].Coordinates[Mesh.Axis.Z]
            ),

            (
                startPosition + lineLenght,
                mesh.Nodes[startPosition + lineLenght].Coordinates[Mesh.Axis.R],
                mesh.Nodes[startPosition + lineLenght].Coordinates[Mesh.Axis.Z]
            ),
            (
                startPosition + lineLenght + 1,
                mesh.Nodes[startPosition + lineLenght + 1].Coordinates[Mesh.Axis.R],
                mesh.Nodes[startPosition + lineLenght + 1].Coordinates[Mesh.Axis.Z]
            )
        };
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