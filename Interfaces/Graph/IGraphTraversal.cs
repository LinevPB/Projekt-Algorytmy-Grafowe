namespace GraphManagementApp.Interfaces.Graph
{
    public interface IGraphTraversal
    {
        void BFS(int startVertex);
        void DFS(int startVertex);
    }
}