namespace Fengine.Backend.Fem.Mesh;

public interface IMesh
{
    public class Node
    {
        public double[] Coordinates { get; set; } = new double[6];
    }

    Node[] Nodes { get; init; }
}