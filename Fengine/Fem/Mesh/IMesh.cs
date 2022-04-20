namespace Fengine.Fem.Mesh;

public interface IMesh
{
    public class Node
    {
        public Node()
        {
            Coordinates = new Dictionary<string, double>();
        }

        public Node(Dictionary<string, double> coordinates)
        {
            Coordinates = coordinates;
        }

        public Dictionary<string, double> Coordinates { get; set; }
    }

    Node[] Nodes { get; init; }
}