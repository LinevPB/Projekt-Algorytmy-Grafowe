using GraphManagementApp.Interfaces.Data;
using GraphManagementApp.Models;

namespace GraphManagementApp.Services.Data
{
    public class FileHandler : IFileHandler
    {
        // Zapisuje graf do pliku w formacie: Wierzchołek:Sąsiad1:Waga1,Sąsiad2:Waga2,...
        public void SaveToFile(string filename, Graph graph)
        {
            if (graph == null || graph.AdjacencyList.Count == 0)
            {
                throw new InvalidOperationException("Graph is empty. Nothing to save.");
            }

            using (StreamWriter writer = new StreamWriter(filename))
            {
                foreach (var vertex in graph.AdjacencyList)
                {
                    writer.WriteLine($"{vertex.Key}:{string.Join(",", vertex.Value.Select(e => $"{e.neighbor}:{e.weight}"))}");
                }
            }
        }

        // Wczytuje graf z pliku zapisanego w formacie: Wierzchołek:Sąsiad1:Waga1,Sąsiad2:Waga2,...
        public Graph LoadFromFile(string filename)
        {
            Graph graph = new Graph();

            foreach (var line in File.ReadAllLines(filename))
            {
                if (string.IsNullOrWhiteSpace(line)) continue; // Pomija puste linie

                var parts = line.Split(new[] { ':' }, 2); // Dzieli linię tylko raz (wierzchołek : reszta)
                if (parts.Length != 2) continue; // Pomija niepoprawne linie

                if (!int.TryParse(parts[0], out int vertex)) continue; // Pomija niepoprawne wierzchołki
                graph.AddVertex(vertex);

                string edgesCell = parts[1];
                if (!string.IsNullOrWhiteSpace(edgesCell))
                {
                    var edges = edgesCell.Split(',');

                    foreach (var edge in edges)
                    {
                        var edgeParts = edge.Split(':');
                        if (edgeParts.Length != 2) continue; // Pomija niepoprawne krawędzie

                        if (int.TryParse(edgeParts[0], out int neighbor) && int.TryParse(edgeParts[1], out int weight))
                        {
                            graph.AddEdge(vertex, neighbor, weight);
                        }
                    }
                }
            }
            return graph;
        }
    }
}
