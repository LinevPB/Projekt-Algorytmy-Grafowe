namespace GraphManagementApp.Models
{
    // Class representing an edge in the graph
    public class Edge
    {
        // Starting vertex of the edge
        public int Start { get; set; }

        // Ending vertex of the edge
        public int End { get; set; }

        // Weight of the edge
        public int Weight { get; set; }

        // Constructor creating an edge between two vertices with a specified weight
        public Edge(int start, int end, int weight)
        {
            Start = start;
            End = end;
            Weight = weight;
        }
    }
}
