using GraphManagementApp.Interfaces.Data;
using GraphManagementApp.Models;

namespace GraphManagementApp.Services.Data
{
    public class DataHandler : IDataHandler
    {
        private readonly IFileHandler _fileHandler;
        private readonly IDatabaseHandler _databaseHandler;
        private readonly ICSVHandler _csvHandler;
        private readonly IExcelHandler _excelHandler;

        public DataHandler(
            IFileHandler fileHandler,
            IDatabaseHandler databaseHandler,
            ICSVHandler csvHandler,
            IExcelHandler excelHandler)
        {
            _fileHandler = fileHandler ?? throw new ArgumentNullException(nameof(fileHandler));
            _databaseHandler = databaseHandler ?? throw new ArgumentNullException(nameof(databaseHandler));
            _csvHandler = csvHandler ?? throw new ArgumentNullException(nameof(csvHandler));
            _excelHandler = excelHandler ?? throw new ArgumentNullException(nameof(excelHandler));
        }

        public bool ConnectToDatabase(string ip, string db, string user, string pass) =>
            (_databaseHandler as DatabaseHandler)?.Connect(ip, db, user, pass) ?? false;

        public void DisconnectDatabase() =>
            (_databaseHandler as DatabaseHandler)?.Disconnect();

        public void SaveToDatabase(Graph graph) => _databaseHandler.SaveToDatabase(graph);
        public Graph LoadFromDatabase() => _databaseHandler.LoadFromDatabase();

        // File Handling
        public void SaveToFile(string filename, Graph graph) => _fileHandler.SaveToFile(filename, graph);
        public Graph LoadFromFile(string filename) => _fileHandler.LoadFromFile(filename);

        // CSV Handling
        public void ExportToCSV(string filename, Graph graph) => _csvHandler.ExportToCSV(filename, graph);
        public Graph ImportFromCSV(string filename) => _csvHandler.ImportFromCSV(filename);

        // Excel Handling
        public void ExportToExcel(string filename, Graph graph) => _excelHandler.ExportToExcel(filename, graph);
        public Graph ImportFromExcel(string filename) => _excelHandler.ImportFromExcel(filename);
    }
}
