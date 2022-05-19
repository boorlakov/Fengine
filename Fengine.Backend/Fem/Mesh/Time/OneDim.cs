namespace Fengine.Backend.Fem.Mesh.Time;

/// <summary>
///     1D grid class. Can be uniform or non-uniform due to given discharge ratio
/// </summary>
public class OneDim : IMesh
{
    /// <summary>
    ///     Cartesian1D constructor. Can be uniform or non-uniform due to given discharge ratio
    /// </summary>
    /// <param name="area">Given area settings</param>
    public OneDim(DataModels.Area.OneDim area)
    {
        var nodes = new IMesh.Node[area.AmountPoints];

        for (var i = 0; i < nodes.Length; i++)
        {
            nodes[i] = new IMesh.Node();
        }

        nodes[0].Coordinates[Axis.T] = area.LeftBorder;

        if (Math.Abs(area.DischargeRatio - 1) > 1e-10)
        {
            // Nonuniform case
            var sumKt = (1 - Math.Pow(area.DischargeRatio, area.AmountPoints - 1)) / (1 - area.DischargeRatio);
            var stepT = (area.RightBorder - area.LeftBorder) / sumKt;

            for (var i = 1; i < area.AmountPoints; i++)
            {
                nodes[i].Coordinates[Axis.T] = area.LeftBorder +
                                               stepT * (1 - Math.Pow(area.DischargeRatio, i)) /
                                               (1 - area.DischargeRatio);
            }
        }
        else
        {
            // Uniform case
            var stepT = (area.RightBorder - area.LeftBorder) / (area.AmountPoints - 1);

            for (var i = 1; i < area.AmountPoints; i++)
            {
                nodes[i].Coordinates[Axis.T] = area.LeftBorder + i * stepT;
            }
        }

        Nodes = nodes;
    }

    public IMesh.Node[] Nodes { get; init; }
}