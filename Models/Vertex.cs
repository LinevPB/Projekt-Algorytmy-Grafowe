namespace GraphManagementApp.Models
{
    public class Vertex
    {
        public int Id { get; set; }
        public Dictionary<int, int> Neighbors { get; private set; } = new(); // Now stores weights

        public Vertex(int id) => Id = id;

        public void AddNeighbor(int neighbor, int weight)
        {
            if (!Neighbors.ContainsKey(neighbor))
                Neighbors[neighbor] = weight;
        }
    }
}
