namespace Fengine.Backend.Fem.Mesh.Cylindrical;

public class TwoDim : IMesh
{
    public TwoDim(DataModels.Area.TwoDim area)
    {
        var r = new List<double> {area.LeftBorder};
        var z = new List<double> {area.LowerBorder};

        if (Math.Abs(area.DischargeRatioR - 1) > 1e-10)
        {
            var sumKr = (1 - Math.Pow(area.DischargeRatioR, area.AmountPointsR - 1)) / (1 - area.DischargeRatioR);
            var hR = (area.RightBorder - area.LeftBorder) / sumKr;

            for (var i = 1; i < area.AmountPointsR; i++)
            {
                r.Add(area.LeftBorder + hR * (1 - Math.Pow(area.DischargeRatioR, i)) / (1 - area.DischargeRatioR));
            }
        }
        else
        {
            var hR = (area.RightBorder - area.LeftBorder) / (area.AmountPointsR - 1);

            for (var i = 1; i < area.AmountPointsR; i++)
            {
                r.Add(area.LeftBorder + i * hR);
            }
        }

        if (Math.Abs(area.DischargeRatioZ - 1) > 1e-10)
        {
            var sumKz = (1 - Math.Pow(area.DischargeRatioZ, area.AmountPointsZ - 1)) / (1 - area.DischargeRatioZ);
            var hZ = (area.UpperBorder - area.LowerBorder) / sumKz;

            for (var i = 1; i < area.AmountPointsZ; i++)
            {
                z.Add(area.LowerBorder + hZ * (1 - Math.Pow(area.DischargeRatioZ, i)) / (1 - area.DischargeRatioZ));
            }
        }
        else
        {
            var hZ = (area.UpperBorder - area.LowerBorder) / (area.AmountPointsZ - 1);

            for (var i = 1; i < area.AmountPointsZ; i++)
            {
                z.Add(area.LowerBorder + i * hZ);
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