using Fengine.Backend.DataModels.Areas;

namespace Fengine.Backend.Fem.Mesh;

/// <summary>
///     1D grid class. Can be uniform or non-uniform due to given discharge ratio
/// </summary>
public class Cartesian1D : IMesh
{
    /// <summary>
    ///     Cartesian1D constructor. Can be uniform or non-uniform due to given discharge ratio
    /// </summary>
    /// <param name="oneDim">Given oneDim settings</param>
    public Cartesian1D(OneDim oneDim)
    {
        var nodes = new IMesh.Node[oneDim.AmountPoints];

        for (var i = 0; i < nodes.Length; i++)
        {
            nodes[i] = new IMesh.Node();
        }

        nodes[0].Coordinates[Axis.X] = oneDim.LeftBorder;

        if (Math.Abs(oneDim.DischargeRatio - 1) > 1e-10)
        {
            // Nonuniform case
            var sumKx = (1 - Math.Pow(oneDim.DischargeRatio, oneDim.AmountPoints - 1)) / (1 - oneDim.DischargeRatio);
            var stepX = (oneDim.RightBorder - oneDim.LeftBorder) / sumKx;

            for (var i = 1; i < oneDim.AmountPoints; i++)
            {
                nodes[i].Coordinates[Axis.X] = oneDim.LeftBorder +
                                               stepX * (1 - Math.Pow(oneDim.DischargeRatio, i)) /
                                               (1 - oneDim.DischargeRatio);
            }
        }
        else
        {
            // Uniform case
            var stepX = (oneDim.RightBorder - oneDim.LeftBorder) / (oneDim.AmountPoints - 1);

            for (var i = 1; i < oneDim.AmountPoints; i++)
            {
                nodes[i].Coordinates[Axis.X] = oneDim.LeftBorder + i * stepX;
            }
        }

        Nodes = nodes;
    }

    public IMesh.Node[] Nodes { get; init; }
}