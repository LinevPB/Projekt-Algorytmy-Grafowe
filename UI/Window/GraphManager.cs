using GraphManagementApp.Models;

namespace GraphManagementApp
{
    public class GraphManager
    {
        private Dictionary<int, List<(int neighbor, int weight)>> adjacencyList;
        private Graph graph;
        private readonly Action updateUI;
        private readonly Random random;

        public GraphManager(Action updateUI)
        {
            graph = new Graph();
            this.updateUI = updateUI ?? throw new ArgumentNullException(nameof(updateUI));
            adjacencyList = new Dictionary<int, List<(int neighbor, int weight)>>();
            random = new Random();
        }

        public void SetGraph(Graph newGraph)
        {
            if (newGraph == null)
            {
                throw new ArgumentNullException(nameof(newGraph), "Graph cannot be null.");
            }

            graph = newGraph;  // Set the new graph
            adjacencyList.Clear();
            distributeEdgesAndNodes();
            updateUI?.Invoke(); // Refresh UI if callback exists
        }

        void distributeEdgesAndNodes()
        {
            adjacencyList = graph.AdjacencyList;
        }

        public Graph GetGraph()
        {
            Graph newGraph = new Graph();

            foreach (var vertex in adjacencyList)
            {
                newGraph.AddVertex(vertex.Key);
                foreach (var edge in vertex.Value)
                {
                    newGraph.AddEdge(vertex.Key, edge.neighbor, edge.weight);
                }
            }

            return newGraph;
        }


        public void AddVertex(int vertex)
        {
            if (!adjacencyList.ContainsKey(vertex))
            {
                adjacencyList[vertex] = new List<(int, int)>();
                updateUI();
            }
        }

        public void AddEdge(int vertex1, int vertex2, int weight = 1)
        {
            if (vertex1 == vertex2 || !adjacencyList.ContainsKey(vertex1) || !adjacencyList.ContainsKey(vertex2))
                return;

            if (!adjacencyList[vertex1].Any(e => e.neighbor == vertex2))
            {
                adjacencyList[vertex1].Add((vertex2, weight));
                adjacencyList[vertex2].Add((vertex1, weight)); 
                updateUI();
            }
        }

        public void RemoveVertex(int vertex)
        {
            if (adjacencyList.ContainsKey(vertex))
            {
                foreach (var neighbor in adjacencyList[vertex])
                {
                    adjacencyList[neighbor.neighbor].RemoveAll(e => e.neighbor == vertex);
                }
                adjacencyList.Remove(vertex);
                updateUI();
            }
        }

        public void RemoveEdge(int vertex1, int vertex2)
        {
            if (adjacencyList.ContainsKey(vertex1) && adjacencyList.ContainsKey(vertex2))
            {
                adjacencyList[vertex1].RemoveAll(e => e.neighbor == vertex2);
                adjacencyList[vertex2].RemoveAll(e => e.neighbor == vertex1);
                updateUI();
            }
        }

        public List<int> GetVertices() => adjacencyList.Keys.ToList();

        public List<Tuple<int, int, int>> GetEdges()
        {
            var edges = new HashSet<Tuple<int, int, int>>();

            foreach (var vertex in adjacencyList)
            {
                foreach (var (neighbor, weight) in vertex.Value)
                {
                    var edge = Tuple.Create(Math.Min(vertex.Key, neighbor), Math.Max(vertex.Key, neighbor), weight);
                    edges.Add(edge);
                }
            }

            return edges.ToList();
        }

        public List<string> GetGraphDisplay()
        {
            return adjacencyList
                .Select(vertex => $"{vertex.Key}: {string.Join(", ", vertex.Value.Select(e => $"{e.neighbor}({e.weight})"))}")
                .ToList();
        }

        public void clearGraph()
        {
            adjacencyList.Clear();
            updateUI();
        }

        public void GenerateRandomGraph(int nodes, int edges, int maxWeight = 10)
        {
            adjacencyList.Clear();

            for (int i = 0; i < nodes; i++)
            {
                AddVertex(i);
            }

            int maxEdges = (nodes * (nodes - 1)) / 2;
            edges = Math.Min(edges, maxEdges);

            var possibleEdges = new HashSet<Tuple<int, int>>();

            while (possibleEdges.Count < edges)
            {
                int v1 = random.Next(nodes);
                int v2 = random.Next(nodes);
                int weight = random.Next(1, maxWeight + 1);

                if (v1 != v2)
                {
                    var edge = Tuple.Create(Math.Min(v1, v2), Math.Max(v1, v2));
                    if (possibleEdges.Add(edge))
                    {
                        AddEdge(edge.Item1, edge.Item2, weight);
                    }
                }
            }

            updateUI();
        }

        public List<int> BFS(int startVertex)
        {
            List<int> bfsResult = new();
            if (!adjacencyList.ContainsKey(startVertex)) return bfsResult;

            Queue<int> queue = new();
            HashSet<int> visited = new();

            queue.Enqueue(startVertex);
            visited.Add(startVertex);

            while (queue.Count > 0)
            {
                int current = queue.Dequeue();
                bfsResult.Add(current);

                foreach (var (neighbor, _) in adjacencyList[current])
                {
                    if (!visited.Contains(neighbor))
                    {
                        queue.Enqueue(neighbor);
                        visited.Add(neighbor);
                    }
                }
            }
            return bfsResult;
        }

        public List<int> DFS(int startVertex)
        {
            List<int> dfsResult = new();
            if (!adjacencyList.ContainsKey(startVertex)) return dfsResult;

            HashSet<int> visited = new();
            DFSUtil(startVertex, visited, dfsResult);
            return dfsResult;
        }

        private void DFSUtil(int vertex, HashSet<int> visited, List<int> dfsResult)
        {
            visited.Add(vertex);
            dfsResult.Add(vertex);

            foreach (var (neighbor, _) in adjacencyList[vertex])
            {
                if (!visited.Contains(neighbor))
                    DFSUtil(neighbor, visited, dfsResult);
            }
        }

        public Dictionary<int, int> Dijkstra(int startNode)
        {
            if (!adjacencyList.ContainsKey(startNode))
                return new Dictionary<int, int>();

            Dictionary<int, int> distances = adjacencyList.Keys.ToDictionary(node => node, node => int.MaxValue);
            distances[startNode] = 0;

            PriorityQueue<int, int> pq = new PriorityQueue<int, int>();
            pq.Enqueue(startNode, 0);

            while (pq.Count > 0)
            {
                int currentNode = pq.Dequeue();

                foreach (var (neighbor, weight) in adjacencyList[currentNode])
                {
                    int newDist = distances[currentNode] + weight;
                    if (newDist < distances[neighbor])
                    {
                        distances[neighbor] = newDist;
                        pq.Enqueue(neighbor, newDist);
                    }
                }
            }

            return distances;
        }



        public void UpdateEdgeWeight(int node1, int node2, int newWeight)
        {
            if (adjacencyList.ContainsKey(node1))
            {
                var edges = adjacencyList[node1];
                for (int i = 0; i < edges.Count; i++)
                {
                    if (edges[i].neighbor == node2)
                    {
                        edges[i] = (node2, newWeight); // Update the weight
                        break;
                    }
                }
            }

            if (adjacencyList.ContainsKey(node2)) // If the graph is undirected, update the other direction
            {
                var edges = adjacencyList[node2];
                for (int i = 0; i < edges.Count; i++)
                {
                    if (edges[i].neighbor == node1)
                    {
                        edges[i] = (node1, newWeight);
                        break;
                    }
                }
            }
        }

        public Dictionary<int, List<(int neighbor, int weight)>> GetAdjacencyList() => new(adjacencyList);
    }
}
