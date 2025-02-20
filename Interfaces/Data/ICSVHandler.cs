namespace GraphManagementApp.Interfaces.Data
{
    using Models;
    public interface ICSVHandler
    {
        void ExportToCSV(string filename, Graph graph);
        Graph ImportFromCSV(string filename);
    }
}