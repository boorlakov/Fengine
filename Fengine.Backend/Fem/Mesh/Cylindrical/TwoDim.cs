namespace Fengine.Backend.Fem.Mesh.Cylindrical;

public class TwoDim : IMesh
{
    public TwoDim(DataModels.Areas.TwoDim area)
    {
        var nodes = new IMesh.Node[area.AmountPointsR * area.AmountPointsZ];

        for (var i = 0; i < nodes.Length; i++)
        {
            nodes[i] = new IMesh.Node();
        }

        nodes[0].Coordinates[Axis.R] = area.LeftBorder;
        nodes[0].Coordinates[Axis.Z] = area.LowerBorder;

        if (Math.Abs(area.DischargeRatio - 1) > 1e-10)
        {
            // Nonuniform case
            var sumKr = (1 - Math.Pow(area.DischargeRatio, area.AmountPointsR - 1)) / (1 - area.DischargeRatio);
            var sumKz = (1 - Math.Pow(area.DischargeRatio, area.AmountPointsZ - 1)) / (1 - area.DischargeRatio);

            var stepR = (area.RightBorder - area.LeftBorder) / sumKr;
            var stepZ = (area.UpperBorder - area.LowerBorder) / sumKz;

            for (var i = 1; i < area.AmountPointsR; i++)
            {
                nodes[i].Coordinates[Axis.R] = area.LeftBorder +
                                               stepR * (1 - Math.Pow(area.DischargeRatio, i)) /
                                               (1 - area.DischargeRatio);
                nodes[i].Coordinates[Axis.Z] = area.LowerBorder +
                                               stepZ * (1 - Math.Pow(area.DischargeRatio, i)) /
                                               (1 - area.DischargeRatio);
            }
        }
        else
        {
            // Uniform case
            var stepR = (area.RightBorder - area.LeftBorder) / (area.AmountPointsR - 1);
            var stepZ = (area.UpperBorder - area.LowerBorder) / (area.AmountPointsZ - 1);

            for (var i = 1; i < area.AmountPointsR; i++)
            {
                nodes[i].Coordinates[Axis.R] = area.LeftBorder + i * stepR;
                nodes[i].Coordinates[Axis.Z] = area.LowerBorder + i * stepZ;
            }
        }

        Nodes = nodes;
    }

    public IMesh.Node[] Nodes { get; init; }
}