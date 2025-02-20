namespace GraphManagementApp.Interfaces.Data
{
    using Models;
    public interface IDatabaseHandler
    {
        void SaveToDatabase(Graph graph);
        Graph LoadFromDatabase();
    }
}