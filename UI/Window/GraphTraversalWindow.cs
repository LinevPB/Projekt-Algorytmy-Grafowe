namespace GraphManagementApp.UI
{
    public class GraphTraversalWindow : Form
    {
        private readonly Dictionary<int, List<(int neighbor, int weight)>> adjacencyList;
        private readonly List<int> traversalPath;
        private readonly Dictionary<int, PointF> nodePositions;

        private int currentStep = -1;
        private RichTextBox traversalTextBox;
        private Button nextButton, prevButton;

        private readonly Dictionary<int, int> shortestPaths;
        private readonly bool isDijkstra;

        public GraphTraversalWindow(
            Dictionary<int, List<(int neighbor, int weight)>> adjacencyList,
            List<int> traversalPath,
            string algorithmName,
            Dictionary<int, PointF> nodePositions,
            Dictionary<int, int> shortestPaths = null)
        {
            this.adjacencyList = adjacencyList;
            this.traversalPath = traversalPath;
            this.nodePositions = new Dictionary<int, PointF>(nodePositions);
            this.shortestPaths = shortestPaths ?? new Dictionary<int, int>();
            this.isDijkstra = algorithmName == "Dijkstra";

            this.Text = $"{algorithmName} Traversal";
            this.Width = 800;
            this.Height = 750;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Paint += DrawTraversalGraph;

            InitializeUI();
        }


        private void InitializeUI()
        {
            traversalTextBox = new RichTextBox
            {
                Font = new Font("Arial", 12, FontStyle.Bold),
                ReadOnly = true,
                Multiline = true,
                BackColor = Color.White,
                Dock = DockStyle.Top,
                Height = 100
            };

            // Determine traversal path for Dijkstra
            if (isDijkstra)
            {
                traversalPath.Clear(); // Ensure it's empty before adding nodes
                traversalPath.AddRange(shortestPaths.Keys.OrderBy(k => shortestPaths[k])); // Order by distance

                traversalTextBox.Text = "Shortest Paths:\n" +
                    string.Join("\n", shortestPaths.Select(kv => $"Node {kv.Key}: Distance {kv.Value}"));
            }
            else
            {
                traversalTextBox.Text = $"Traversal Order: {string.Join(" → ", traversalPath)}";
            }

            this.Controls.Add(traversalTextBox);

            // Create navigation buttons for **all** algorithms
            Panel buttonPanel = new Panel { Dock = DockStyle.Bottom, Height = 50 };

            prevButton = new Button
            {
                Text = "← Previous",
                Enabled = false,
                Width = 100,
                Left = (this.Width / 2) - 120,
                Top = 10
            };
            prevButton.Click += (s, e) => ChangeStep(-1);
            buttonPanel.Controls.Add(prevButton);

            nextButton = new Button
            {
                Text = "Next →",
                Width = 100,
                Left = (this.Width / 2) + 20,
                Top = 10
            };
            nextButton.Click += (s, e) => ChangeStep(1);
            buttonPanel.Controls.Add(nextButton);

            this.Controls.Add(buttonPanel);
        }




        private void ChangeStep(int direction)
        {
            currentStep += direction;

            prevButton.Enabled = currentStep > 0;
            nextButton.Enabled = currentStep < traversalPath.Count - 1;

            HighlightTraversalText();
            Invalidate(); // Redraw the window
        }


        private void HighlightTraversalText()
        {
            traversalTextBox.SelectAll();
            traversalTextBox.SelectionColor = Color.Black;

            if (isDijkstra)
            {
                string fullText = traversalTextBox.Text;
                foreach (var node in traversalPath.Take(currentStep + 1))
                {
                    int nodeIndex = fullText.IndexOf($"Node {node}:");

                    if (nodeIndex >= 0)
                    {
                        traversalTextBox.Select(nodeIndex, $"Node {node}".Length);
                        traversalTextBox.SelectionColor = Color.Orange;
                        traversalTextBox.SelectionFont = new Font(traversalTextBox.Font, FontStyle.Bold);
                    }
                }
            }
            else
            {
                string fullText = traversalTextBox.Text;
                int startIndex = fullText.IndexOf(":") + 2;
                string[] nodes = traversalPath.Select(n => n.ToString()).ToArray();
                int currentHighlightIndex = Math.Max(0, currentStep);

                traversalTextBox.Select(startIndex, fullText.Length - startIndex);
                traversalTextBox.SelectionFont = new Font(traversalTextBox.Font, FontStyle.Regular);

                for (int i = 0; i <= currentHighlightIndex; i++)
                {
                    int nodeIndex = fullText.IndexOf(nodes[i], startIndex);

                    if (nodeIndex >= 0)
                    {
                        traversalTextBox.Select(nodeIndex, nodes[i].Length);
                        traversalTextBox.SelectionColor = Color.Orange;
                        traversalTextBox.SelectionFont = new Font(traversalTextBox.Font, FontStyle.Bold);
                    }
                }
            }

            traversalTextBox.DeselectAll();
        }


        private void DrawTraversalGraph(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.Clear(Color.White);

            using (Pen edgePen = new Pen(Color.Gray, 2))
            using (Font font = new Font("Arial", 12, FontStyle.Bold))
            {
                Brush defaultBrush = Brushes.LightBlue;
                Brush visitedBrush = Brushes.Orange;
                Brush currentBrush = Brushes.Red;
                Brush textBrush = Brushes.Black;

                DrawEdges(g, font, textBrush);
                DrawNodes(g, defaultBrush, visitedBrush, currentBrush, textBrush, font);
            }
        }

        private void DrawEdges(Graphics g, Font font, Brush textBrush)
        {
            foreach (var node in adjacencyList)
            {
                foreach (var (neighbor, weight) in node.Value)
                {
                    if (nodePositions.TryGetValue(node.Key, out var p1) && nodePositions.TryGetValue(neighbor, out var p2))
                    {
                        using (Pen edgePen = new Pen(Color.Gray, 2)
                        { EndCap = System.Drawing.Drawing2D.LineCap.ArrowAnchor }) // Add arrow for direction
                        {
                            g.DrawLine(edgePen, p1, p2);
                        }

                        // Draw weight label at the midpoint of the edge
                        float midX = (p1.X + p2.X) / 2;
                        float midY = (p1.Y + p2.Y) / 2;
                        g.DrawString(weight.ToString(), font, Brushes.Blue, midX, midY);
                    }
                }
            }
        }




        private void DrawNodes(Graphics g, Brush defaultBrush, Brush visitedBrush, Brush currentBrush, Brush textBrush, Font font)
        {
            foreach (var node in nodePositions)
            {
                PointF p = node.Value;
                Brush brushToUse = defaultBrush; // Default: Light Blue

                if (currentStep >= 0 && traversalPath.Take(currentStep).Contains(node.Key))
                {
                    brushToUse = visitedBrush; // Orange for visited nodes (Dijkstra, BFS, DFS)
                }
                if (currentStep >= 0 && traversalPath[currentStep] == node.Key)
                {
                    brushToUse = currentBrush; // Current node is red
                }

                // Draw node
                g.FillEllipse(brushToUse, p.X - 20, p.Y - 20, 40, 40);
                g.DrawString(node.Key.ToString(), font, textBrush, p.X - 10, p.Y - 10);
            }
        }



    }
}
