namespace Fengine.Backend.Fem.Mesh.Cartesian;

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

        nodes[0].Coordinates[Axis.X] = area.LeftBorder;

        if (Math.Abs(area.DischargeRatio - 1) > 1e-10)
        {
            // Nonuniform case
            var sumKx = (1 - Math.Pow(area.DischargeRatio, area.AmountPoints - 1)) / (1 - area.DischargeRatio);
            var stepX = (area.RightBorder - area.LeftBorder) / sumKx;

            for (var i = 1; i < area.AmountPoints; i++)
            {
                nodes[i].Coordinates[Axis.X] = area.LeftBorder +
                                               stepX * (1 - Math.Pow(area.DischargeRatio, i)) /
                                               (1 - area.DischargeRatio);
            }
        }
        else
        {
            // Uniform case
            var stepX = (area.RightBorder - area.LeftBorder) / (area.AmountPoints - 1);

            for (var i = 1; i < area.AmountPoints; i++)
            {
                nodes[i].Coordinates[Axis.X] = area.LeftBorder + i * stepX;
            }
        }

        Nodes = nodes;
    }

    public IMesh.Node[] Nodes { get; init; }
}