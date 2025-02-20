namespace GraphManagementApp.Interfaces.Graph
{
    public interface IGraphOperations
    {
        void AddVertex(int vertex);
        void AddEdge(int vertex1, int vertex2, int weight, bool isDirected);
        void RemoveVertex(int vertex);
        void RemoveEdge(int vertex1, int vertex2, bool isDirected);
        void DisplayGraph();
    }
}
