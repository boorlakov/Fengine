namespace Fengine.Fem.Mesh;

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