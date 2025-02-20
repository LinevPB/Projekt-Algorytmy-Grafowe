using GraphManagementApp.Models;

public interface IExcelHandler
{
    void ExportToExcel(string filename, Graph graph);
    Graph ImportFromExcel(string filename);
}