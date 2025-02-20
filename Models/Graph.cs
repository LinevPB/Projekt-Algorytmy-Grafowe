using GraphManagementApp.Interfaces.Graph;

namespace GraphManagementApp.Models
{
    public class Graph : IGraphOperations, IGraphTraversal
    {
        // Adjacency list storing vertices and their neighbors with edge weights
        public Dictionary<int, List<(int neighbor, int weight)>> AdjacencyList { get; private set; } = new();

        // Adds a vertex to the graph if it doesn't already exist
        public void AddVertex(int vertex)
        {
            if (!AdjacencyList.ContainsKey(vertex))
                AdjacencyList[vertex] = new List<(int, int)>();
        }

        // Adds an edge between two vertices, optionally directed
        public void AddEdge(int vertex1, int vertex2, int weight, bool isDirected = false)
        {
            AddVertex(vertex1);
            AddVertex(vertex2);

            if (!AdjacencyList[vertex1].Any(e => e.neighbor == vertex2))
                AdjacencyList[vertex1].Add((vertex2, weight));

            if (!isDirected && !AdjacencyList[vertex2].Any(e => e.neighbor == vertex1))
                AdjacencyList[vertex2].Add((vertex1, weight));
        }

        // Removes a vertex and all its connections
        public void RemoveVertex(int vertex)
        {
            if (!AdjacencyList.ContainsKey(vertex))
                return;

            foreach (var neighbor in AdjacencyList[vertex])
                AdjacencyList[neighbor.neighbor].RemoveAll(e => e.neighbor == vertex);

            AdjacencyList.Remove(vertex);
        }

        // Removes an edge between two vertices, optionally directed
        public void RemoveEdge(int vertex1, int vertex2, bool isDirected = false)
        {
            if (!AdjacencyList.ContainsKey(vertex1) || !AdjacencyList.ContainsKey(vertex2))
                throw new InvalidOperationException("One or both vertices not found in the graph.");

            AdjacencyList[vertex1].RemoveAll(e => e.neighbor == vertex2);

            if (!isDirected)
                AdjacencyList[vertex2].RemoveAll(e => e.neighbor == vertex1);
        }

        // Returns a string representation of the graph
        public override string ToString()
        {
            return string.Join("\n", AdjacencyList.Select(v =>
                $"{v.Key} -> {string.Join(", ", v.Value.Select(e => $"{e.neighbor}({e.weight})"))}"));
        }

        // Retrieves a list of all vertices in the graph
        public List<int> GetVertices()
        {
            return AdjacencyList.Keys.ToList();
        }

        // Retrieves a list of all edges in the graph (without duplication in undirected graphs)
        public List<(int from, int to, int weight)> GetEdges()
        {
            var edges = new HashSet<(int, int, int)>();

            foreach (var vertex in AdjacencyList)
            {
                foreach (var (neighbor, weight) in vertex.Value)
                {
                    var edge = (Math.Min(vertex.Key, neighbor), Math.Max(vertex.Key, neighbor), weight);
                    edges.Add(edge);
                }
            }

            return edges.ToList();
        }

        // Displays the graph in the console
        public void DisplayGraph()
        {
            foreach (var vertex in AdjacencyList)
            {
                Console.Write($"{vertex.Key} -> ");
                Console.WriteLine(string.Join(", ", vertex.Value.Select(e => $"{e.neighbor}({e.weight})")));
            }
        }

        // Breadth-First Search (BFS) traversal
        public void BFS(int startVertex)
        {
            if (!AdjacencyList.ContainsKey(startVertex)) return;

            Queue<int> queue = new();
            HashSet<int> visited = new();
            queue.Enqueue(startVertex);
            visited.Add(startVertex);

            while (queue.Count > 0)
            {
                int current = queue.Dequeue();
                Console.Write(current + " ");
                foreach (var (neighbor, _) in AdjacencyList[current])
                {
                    if (!visited.Contains(neighbor))
                    {
                        queue.Enqueue(neighbor);
                        visited.Add(neighbor);
                    }
                }
            }
            Console.WriteLine();
        }

        // Depth-First Search (DFS) traversal
        public void DFS(int startVertex)
        {
            if (!AdjacencyList.ContainsKey(startVertex)) return;

            HashSet<int> visited = new();
            DFSUtil(startVertex, visited);
            Console.WriteLine();
        }

        // Recursive helper method for DFS
        private void DFSUtil(int vertex, HashSet<int> visited)
        {
            visited.Add(vertex);
            Console.Write(vertex + " ");
            foreach (var (neighbor, _) in AdjacencyList[vertex])
                if (!visited.Contains(neighbor))
                    DFSUtil(neighbor, visited);
        }
    }
}
