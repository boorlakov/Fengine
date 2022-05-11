using Fengine.Backend.DataModels;
using Fengine.Backend.Fem.Basis;
using Fengine.Backend.Integration;
using Fengine.Backend.LinearAlgebra.Matrix;
using Sprache.Calc;

namespace Fengine.Backend.Fem.Slae.LinearTask.Elliptic.TwoDim;

public class Biquadratic : ISlae
{
    private readonly FiniteElement[] _finiteElements;
    private SortedSet<int>[] _boundsList;

    private Func<Dictionary<string, double>, double> _evalRhsFuncAt;
    private Func<Dictionary<string, double>, double> _lambdaFunc;
    private Func<Dictionary<string, double>, double> _evalGammaFuncAt;

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
        InputFuncs inputFuncs,
        DataModels.Conditions.Boundary.TwoDim boundaryConditions,
        LinearAlgebra.SlaeSolver.ISlaeSolver slaeSolver,
        IIntegrator integrator
    )
    {
        ConfigureServices(slaeSolver, integrator);

        // CompileInputFunctions(inputFuncs);

        _finiteElements = GetFiniteElementsFrom(mesh);

        (Matrix, ResVec, RhsVec) = GetPortraitFrom(mesh);

        AssemblyGlobally(inputFuncs);

        ApplyBoundaryConditions(boundaryConditions);
    }

    private void CompileInputFunctions(InputFuncs inputFuncs)
    {
        var calculator = new XtensibleCalculator();
        _evalRhsFuncAt = calculator.ParseFunction(inputFuncs.RhsFunc).Compile();
        _lambdaFunc = calculator.ParseFunction(inputFuncs.Lambda).Compile();
        _evalGammaFuncAt = calculator.ParseFunction(inputFuncs.Gamma).Compile();
    }

    public double[] Solve(Accuracy accuracy)
    {
        return _slaeSolver.Solve(this, accuracy);
    }

    public IMatrix Matrix { get; set; }
    public double[] RhsVec { get; set; }
    public double[] ResVec { get; set; }

    private void ConfigureServices
    (
        LinearAlgebra.SlaeSolver.ISlaeSolver slaeSolver,
        IIntegrator integrator
    )
    {
        _slaeSolver = slaeSolver;
        _integrator = integrator;
    }

    private void ApplyBoundaryConditions(DataModels.Conditions.Boundary.TwoDim boundaryConditions)
    {
        throw new NotImplementedException();
    }

    private (IMatrix Matrix, double[] ResVec, double[] RhsVec) GetPortraitFrom(Mesh.Cylindrical.TwoDim mesh)
    {
        _boundsList = GetBoundList(mesh);
        var di = new double[(2 * mesh.R.Length - 1) * (2 * mesh.Z.Length - 1)];
        var ig = new int[di.Length + 1];

        ig[0] = 0;
        ig[1] = 0;

        var jg = new List<int>();

        for (var i = 0; i < di.Length; i++)
        {
            var k = 0;

            for (var j = 0; j < _boundsList[i].Count; j++, k++)
            {
                jg.Add(_boundsList[i].ElementAt(j));
            }

            ig[i + 1] = ig[i] + k;
        }

        var ggl = new double[jg.Count];
        var ggu = new double[jg.Count];

        var matrixPortrait = new Sparse(ggl, ggu, di, ig, jg.ToArray());
        var rhsPortrait = new double[di.Length];
        var resPortrait = new double[di.Length];

        return (matrixPortrait, resPortrait, rhsPortrait);
    }

    private SortedSet<int>[] GetBoundList(Mesh.Cylindrical.TwoDim mesh)
    {
        var boundsList = new SortedSet<int>[(2 * mesh.R.Length - 1) * (2 * mesh.R.Length - 1)];

        for (var i = 0; i < boundsList.Length; i++)
        {
            boundsList[i] = new SortedSet<int>();
        }

        foreach (var finiteElement in _finiteElements)
        {
            MapBoundsIn(finiteElement, boundsList);
        }

        return boundsList;
    }

    private static void MapBoundsIn(FiniteElement finiteElement, SortedSet<int>[] boundsList)
    {
        for (var j = 0; j < 9; j++)
        {
            for (var k = 0; k < 9; k++)
            {
                TryAddToBoundaryList(finiteElement, boundsList, j, k);
            }
        }
    }

    private static void TryAddToBoundaryList
    (
        FiniteElement finiteElement,
        SortedSet<int>[] boundsList,
        int j,
        int k
    )
    {
        if (finiteElement.FictiveNumeration[j] > finiteElement.FictiveNumeration[k])
        {
            boundsList[finiteElement.FictiveNumeration[j]].Add(finiteElement.FictiveNumeration[k]);
        }
    }

    private FiniteElement[] GetFiniteElementsFrom(Mesh.Cylindrical.TwoDim mesh)
    {
        var finiteElements = new FiniteElement[(mesh.R.Length - 1) * (mesh.Z.Length - 1)];
        GetNodesFor(finiteElements, mesh);
        GetFictiveNumerationFor(finiteElements, mesh);
        return finiteElements;
    }

    private static void GetNodesFor(FiniteElement[] finiteElements, Mesh.Cylindrical.TwoDim mesh)
    {
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
    }
    private static void GetFictiveNumerationFor(FiniteElement[] finiteElements, Mesh.Cylindrical.TwoDim mesh)
    {
        var k = 0;
        var startPosition = 0;
        var lineLenght = mesh.R.Length * 2 - 1;

        for (var i = 0; i < mesh.Z.Length - 1; i++, startPosition = 2 * i * lineLenght)
        {
            for (var j = 0; j < mesh.R.Length - 1; j++, startPosition += 2, k++)
            {
                var fictiveNumeration = new[]
                {
                    startPosition,
                    startPosition + 1,
                    startPosition + 2,

                    startPosition + lineLenght,
                    startPosition + lineLenght + 1,
                    startPosition + lineLenght + 2,

                    startPosition + 2 * lineLenght,
                    startPosition + 2 * lineLenght + 1,
                    startPosition + 2 * lineLenght + 2
                };
                fictiveNumeration.AsSpan().CopyTo(finiteElements[k].FictiveNumeration);
            }
        }
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

    private void AssemblyGlobally(InputFuncs inputFunctions)
    {
        foreach (var finiteElement in _finiteElements)
        {
            AssemblyLocally(finiteElement, inputFunctions);
        }
    }

    private void AssemblyLocally
    (
        FiniteElement finiteElement,
        InputFuncs inputFuncs
    )
    {
        var localLambdaValue = GetLocalLambdaValueFrom(finiteElement, inputFuncs.Lambda);

        var hR = (finiteElement.Nodes[1].r - finiteElement.Nodes[0].r) / 2.0;
        var hZ = (finiteElement.Nodes[1].z - finiteElement.Nodes[0].z) / 2.0;

        var localStiffness = new double[9, 9];
        var localC = new double[9, 9];
        var localMass = new double[9, 9];

        var localF = new double[9];

        var localNumber = 0;

        foreach (var localNode in finiteElement.Nodes)
        {
            var point = Utils.MakeDict2DCylindrical(localNode.r, localNode.z);

            localF[localNumber] = _evalRhsFuncAt(point);
            localNumber++;

            AssemblyLocalMatrices
            (
                localStiffness, localMass, localC,
                hR, hZ,
                localLambdaValue,
                point
            );
        }

        var localMatrix = AssemblyLocalMatrixFrom(localMass, localStiffness);
        var localB = LinearAlgebra.GeneralOperations.MatrixMultiply(localC, localF);

        AddToGlobal(finiteElement, localMatrix);
        AddToGlobal(finiteElement, localB);
    }

    private static double[,] AssemblyLocalMatrixFrom(double[,] localMass, double[,] localStiffness)
    {
        var localMatrix = new double[9, 9];

        for (var i = 0; i < localMatrix.GetLength(1); i++)
        {
            for (var j = 0; j < localMatrix.GetLength(0); j++)
            {
                localMatrix[i, j] = localMass[i, j] + localStiffness[i, j];
            }
        }

        return localMatrix;
    }

    private void AssemblyLocalMatrices
    (
        double[,] localStiffness, double[,] localMass, double[,] localC,
        double hR, double hZ,
        double localLambdaValue,
        Dictionary<string, double> point
    )
    {
        var integrationGrid = Utils.Create1DIntegrationMesh(0, 1);

        for (var i = 0; i < 9; i++)
        {
            for (var j = 0; j < 9; j++)
            {
                var indXi = i % 3;
                var indYi = i / 3;

                var indXj = j % 3;
                var indYj = j / 3;

                var dX = (double r, double z) =>
                    QuadraticBasis.FirstDerivative[indXi](r) * QuadraticBasis.Func[indYi](z) *
                    QuadraticBasis.FirstDerivative[indXj](r) * QuadraticBasis.Func[indYj](z);

                var dY = (double r, double z) =>
                    QuadraticBasis.Func[indXi](r) * QuadraticBasis.FirstDerivative[indYi](z) *
                    QuadraticBasis.Func[indXj](r) * QuadraticBasis.FirstDerivative[indYj](z);

                var stiffnessIntegrand = (double r, double z) =>
                    dX(r, z) + dY(r, z);

                localStiffness[i, j] +=
                    hR * hZ * localLambdaValue * _integrator.Integrate2D(integrationGrid, stiffnessIntegrand);

                var basisPartMassIntegrand = (double x, double y) =>
                    QuadraticBasis.Func[indXi](x) *
                    QuadraticBasis.Func[indYi](y) *
                    QuadraticBasis.Func[indXj](x) *
                    QuadraticBasis.Func[indYj](y);

                AssemblyLocalMass
                (
                    localMass,
                    point,
                    i, j,
                    integrationGrid,
                    basisPartMassIntegrand
                );

                localMass[i, j] *= hR * hZ;

                localC[i, j] += hR * hZ * _integrator.Integrate2D(integrationGrid, basisPartMassIntegrand);
            }
        }
    }
    private void AssemblyLocalMass
    (
        double[,] localMass,
        Dictionary<string, double> point,
        int i, int j,
        double[] integrationGrid,
        Func<double, double, double> basisPartMassIntegrand
    )
    {
        for (var k = 0; k < 4; k++)
        {
            var k1 = k;
            localMass[i, j] += _evalGammaFuncAt(point) * _integrator.Integrate2D
            (
                integrationGrid,
                (r, z) => basisPartMassIntegrand(r, z) * BilinearBasis.Func[k1](r, z)
            );
        }
    }

    private void AddToGlobal(FiniteElement finiteElement, double[] localB)
    {
        throw new NotImplementedException();
    }

    private void AddToGlobal(FiniteElement finiteElement, double[,] localMatrix)
    {
        throw new NotImplementedException();
    }

    private double GetLocalLambdaValueFrom(FiniteElement finiteElement, string inputFuncsLambda)
    {
        throw new NotImplementedException();
    }

    private class FiniteElement
    {
        public readonly (int globalNumber, double r, double z)[] Nodes;

        public int[] FictiveNumeration { get; set; } = new int[9];

        public FiniteElement((int, double, double)[] nodes)
        {
            Nodes = nodes;
        }
    }
}