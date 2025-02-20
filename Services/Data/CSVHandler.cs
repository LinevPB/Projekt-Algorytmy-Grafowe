using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using GraphManagementApp.Interfaces.Data;
using GraphManagementApp.Models;

namespace GraphManagementApp.Services.Data
{
    public class CSVHandler : ICSVHandler
    {
        public void ExportToCSV(string filename, Graph graph)
        {
            try
            {
                using (var writer = new StreamWriter(filename))
                using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                {
                    csv.WriteField("Vertex");
                    csv.WriteField("Edges (Neighbor:Weight)");
                    csv.NextRecord();

                    foreach (var vertex in graph.AdjacencyList)
                    {
                        csv.WriteField(vertex.Key);
                        csv.WriteField(string.Join(",", vertex.Value.Select(e => $"{e.neighbor}:{e.weight}")));
                        csv.NextRecord();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error saving to CSV: {ex.Message}");
            }
        }

        public Graph ImportFromCSV(string filename)
        {
            Graph graph = new Graph();

            try
            {
                if (!File.Exists(filename))
                    throw new FileNotFoundException("CSV file not found.", filename);

                using (var reader = new StreamReader(filename))
                using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture) { HasHeaderRecord = true }))
                {
                    if (!csv.Read())
                        throw new Exception("CSV file is empty or has an invalid format.");

                    csv.ReadHeader();

                    while (csv.Read())
                    {
                        if (!csv.TryGetField(0, out int vertex))
                            throw new FormatException($"Invalid vertex format at row {csv.Parser.Row}");

                        graph.AddVertex(vertex);

                        if (csv.TryGetField(1, out string edgesCell) && !string.IsNullOrWhiteSpace(edgesCell))
                        {
                            var edges = edgesCell.Split(',');

                            foreach (var edge in edges)
                            {
                                var edgeParts = edge.Split(':');
                                if (edgeParts.Length != 2 ||
                                    !int.TryParse(edgeParts[0], out int neighbor) ||
                                    !int.TryParse(edgeParts[1], out int weight))
                                {
                                    throw new FormatException($"Invalid edge format at row {csv.Parser.Row}: {edge}");
                                }

                                graph.AddEdge(vertex, neighbor, weight);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error loading from CSV: {ex.Message}");
            }

            return graph;
        }
    }
}
