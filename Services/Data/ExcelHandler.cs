using GraphManagementApp.Models;
using OfficeOpenXml;

namespace GraphManagementApp.Services.Data
{
    public class ExcelHandler : IExcelHandler
    {
        // Eksportuje graf do pliku Excel
        public void ExportToExcel(string filename, Graph graph)
        {
            try
            {
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial; // Wymagane dla EPPlus

                using (var package = new ExcelPackage())
                {
                    var worksheet = package.Workbook.Worksheets.Add("Graph");

                    // Nagłówki kolumn
                    worksheet.Cells[1, 1].Value = "Vertex";
                    worksheet.Cells[1, 2].Value = "Edges (Neighbor:Weight)";

                    int row = 2;
                    foreach (var vertex in graph.AdjacencyList)
                    {
                        worksheet.Cells[row, 1].Value = vertex.Key;
                        worksheet.Cells[row, 2].Value = string.Join(",", vertex.Value.Select(e => $"{e.neighbor}:{e.weight}"));
                        row++;
                    }

                    package.SaveAs(new FileInfo(filename));
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error saving to Excel: {ex.Message}");
            }
        }

        // Importuje graf z pliku Excel
        public Graph ImportFromExcel(string filename)
        {
            Graph graph = new Graph();

            try
            {
                if (!File.Exists(filename))
                    throw new FileNotFoundException("Excel file not found.", filename);

                ExcelPackage.LicenseContext = LicenseContext.NonCommercial; // Wymagane dla EPPlus

                using (var package = new ExcelPackage(new FileInfo(filename)))
                {
                    var worksheet = package.Workbook.Worksheets[0];
                    if (worksheet == null)
                        throw new Exception("Excel sheet is empty or corrupt.");

                    int row = 2; // Pomijamy nagłówki, zaczynamy od drugiego wiersza

                    while (worksheet.Cells[row, 1].Value != null)
                    {
                        // Parsowanie wierzchołka
                        if (!int.TryParse(worksheet.Cells[row, 1].Value.ToString(), out int vertex))
                            throw new FormatException($"Invalid vertex format at row {row}");

                        graph.AddVertex(vertex);

                        string edgesCell = worksheet.Cells[row, 2].Value?.ToString();
                        if (!string.IsNullOrWhiteSpace(edgesCell))
                        {
                            var edges = edgesCell.Split(',');

                            foreach (var edge in edges)
                            {
                                var edgeParts = edge.Split(':');
                                if (edgeParts.Length != 2 ||
                                    !int.TryParse(edgeParts[0], out int neighbor) ||
                                    !int.TryParse(edgeParts[1], out int weight))
                                {
                                    throw new FormatException($"Invalid edge format at row {row}: {edge}");
                                }

                                graph.AddEdge(vertex, neighbor, weight);
                            }
                        }
                        row++;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error loading from Excel: {ex.Message}");
            }

            return graph;
        }
    }
}
