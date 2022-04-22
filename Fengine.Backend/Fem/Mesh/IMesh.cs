namespace Fengine.Backend.Fem.Mesh;

public interface IMesh
{
    public static class Axis
    {
        public const int X = 0;
        public const int Y = 1;
        public const int Z = 2;

        public const int R = 3;
        public const int Phi = 4;
        public const int Psi = 5;
    }

    public class Node
    {
        public double[] Coords { get; set; } = new double[6];
    }

    Node[] Nodes { get; init; }
}