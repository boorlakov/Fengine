namespace umf_1;

public class Slae
{
    public void Solve(SlaeAccuracyModel accuracy)
    {
        ResVec = LinAlg.SlaeSolver.Iterate(ResVec, Matrix, 1.0, RhsVec);
        var residual = LinAlg.SlaeSolver.RelResidual(Matrix, ResVec, RhsVec);
        var iter = 1;
        var prevResVec = new double[ResVec.Length];

        while (iter <= accuracy.MaxIter && accuracy.Eps < residual &&
               !LinAlg.SlaeSolver.CheckIsStagnate(prevResVec, ResVec, accuracy.Delta))
        {
            ResVec.AsSpan().CopyTo(prevResVec);
            ResVec = LinAlg.SlaeSolver.Iterate(ResVec, Matrix, 1.0, RhsVec);
            residual = LinAlg.SlaeSolver.RelResidual(Matrix, ResVec, RhsVec);
            iter++;
        }
    }

    public Slae(AreaModel area, Grid grid, IReadOnlyDictionary<string, string> boundaryConditions)
    {
        var diag = new double[grid.Nodes.Length];

        var l0 = new double[grid.Nodes.Length - 1];
        var l1 = new double[grid.Nodes.Length - grid.X.Length];

        var u0 = new double[grid.Nodes.Length - 1];
        var u1 = new double[grid.Nodes.Length - grid.X.Length];

        var shift = grid.X.Length;

        var resVec = new double[grid.Nodes.Length];
        var rhsVec = new double[grid.Nodes.Length];

        for (var i = 0; i < grid.Nodes.Length; i++)
        {
            if (grid.Nodes[i].IsFictive) continue;

            if (grid.Nodes[i].IsOnBorder)
            {
                if (boundaryConditions[grid.Nodes[i].BorderType] == "First")
                {
                    diag[i] = 1.0;
                    rhsVec[i] = BoundaryFunc.First[grid.Nodes[i].BorderType](grid.Nodes[i].X, grid.Nodes[i].Y);
                }
                else
                {
                    if (grid.Nodes[i].BorderType == "Left")
                    {
                        var hX = grid.Nodes[i + 1].X - grid.Nodes[i].X;
                        diag[i] = area.Lambda / hX;
                        u0[i] = -area.Lambda / hX;
                        rhsVec[i] = BoundaryFunc.Second[grid.Nodes[i].BorderType](grid.Nodes[i].X, grid.Nodes[i].Y);
                    }

                    if (grid.Nodes[i].BorderType == "Upper")
                    {
                        var hY = grid.Nodes[i].Y - grid.Nodes[i - shift].Y;
                        diag[i] = area.Lambda / hY;
                        l1[i - shift] = -area.Lambda / hY;
                        rhsVec[i] = BoundaryFunc.Second[grid.Nodes[i].BorderType](grid.Nodes[i].X, grid.Nodes[i].Y);
                    }

                    if (grid.Nodes[i].BorderType is "RightUpper" or "RightLower")
                    {
                        var hX = grid.Nodes[i].X - grid.Nodes[i - 1].X;
                        diag[i] = area.Lambda / hX;
                        l0[i - 1] = -area.Lambda / hX;
                        rhsVec[i] = BoundaryFunc.Second[grid.Nodes[i].BorderType](grid.Nodes[i].X, grid.Nodes[i].Y);
                    }

                    if (grid.Nodes[i].BorderType is "LowerRight" or "LowerLeft")
                    {
                        var hY = grid.Nodes[i + shift].Y - grid.Nodes[i].Y;
                        diag[i] = area.Lambda / hY;
                        u1[i] = -area.Lambda / hY;
                        rhsVec[i] = BoundaryFunc.Second[grid.Nodes[i].BorderType](grid.Nodes[i].X, grid.Nodes[i].Y);
                    }
                }
            }
            else
            {
                var hXLeft = grid.Nodes[i].X - grid.Nodes[i - 1].X;
                var hXRight = grid.Nodes[i + 1].X - grid.Nodes[i].X;

                var hYLower = grid.Nodes[i].Y - grid.Nodes[i - shift].Y;
                var hYUpper = grid.Nodes[i + shift].Y - grid.Nodes[i].Y;

                diag[i] = -area.Lambda * (2 / (hXLeft * hXRight) + 2 / (hYLower * hYUpper)) + area.Gamma;

                u0[i] = area.Lambda * 2 / (hXRight * (hXRight + hXLeft));
                l0[i - 1] = area.Lambda * 2 / (hXLeft * (hXRight + hXLeft));

                l1[i - shift] = area.Lambda * 2 / (hYUpper * (hYUpper + hYLower));
                u1[i] = area.Lambda * 2 / (hYLower * (hYUpper + hYLower));

                rhsVec[i] = RhsFunc.Eval(grid.Nodes[i].X, grid.Nodes[i].Y);
            }
        }

        Matrix = new Matrix(diag, l0, l1, u0, u1, shift);
        RhsVec = rhsVec;
        ResVec = resVec;
    }

    public double[] RhsVec;
    public Matrix Matrix;
    public double[] ResVec;
}