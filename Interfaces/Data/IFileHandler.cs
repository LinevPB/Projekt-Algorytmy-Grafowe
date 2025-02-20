namespace GraphManagementApp.Interfaces.Data
{
    using Models;
    public interface IFileHandler
    {
        void SaveToFile(string filename, Graph graph);
        Graph LoadFromFile(string filename);
    }
}