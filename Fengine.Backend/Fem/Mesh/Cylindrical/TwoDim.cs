namespace Fengine.Backend.Fem.Mesh.Cylindrical;

public class TwoDim : IMesh
{
    public TwoDim(DataModels.Area.TwoDim area)
    {
        var r = new List<double> {area.LeftBorder};
        var z = new List<double> {area.LowerBorder};

        if (Math.Abs(area.DischargeRatioR - 1) > 1e-10)
        {
            var sumKr = (1 - Math.Pow(area.DischargeRatioR, area.AmountPointsR - 1)) /
                        (1 - area.DischargeRatioR);
            var hR = (area.LeftBorder - area.LeftBorder) / sumKr;

            for (var i = 1; i < area.AmountPointsR; i++)
            {
                r.Add(area.LeftBorder +
                      hR * (1 - Math.Pow(area.DischargeRatioR, i)) / (1 - area.DischargeRatioR));
            }
        }
        else
        {
            var hX = (area.LeftBorder - area.LeftBorder) / (area.AmountPointsR - 1);

            for (var i = 1; i < area.AmountPointsR; i++)
            {
                r.Add(area.LeftBorder + i * hX);
            }
        }

        if (Math.Abs(area.DischargeRatioZ - 1) > 1e-10)
        {
            var sumKy = (1 - Math.Pow(area.DischargeRatioZ, area.AmountPointsZ - 1)) /
                        (1 - area.DischargeRatioZ);
            var hY = (area.LowerBorder - area.LowerBorder) / sumKy;

            for (var i = 1; i < area.AmountPointsZ; i++)
            {
                z.Add(area.LowerBorder +
                      hY * (1 - Math.Pow(area.DischargeRatioZ, i)) / (1 - area.DischargeRatioZ));
            }
        }
        else
        {
            var hY = (area.LowerBorder - area.LowerBorder) / (area.AmountPointsZ - 1);

            for (var i = 1; i < area.AmountPointsZ; i++)
            {
                z.Add(area.LowerBorder + i * hY);
            }
        }

        var nodes = new IMesh.Node[r.Count * z.Count];

        for (var i = 0; i < nodes.Length; i++)
        {
            nodes[i] = new IMesh.Node();
        }

        var num = 0;

        foreach (var i in z)
        {
            foreach (var j in r)
            {
                nodes[num].Coordinates[Axis.R] = j;
                nodes[num].Coordinates[Axis.Z] = i;
                num++;
            }
        }

        Nodes = nodes;
    }

    public IMesh.Node[] Nodes { get; init; }
}