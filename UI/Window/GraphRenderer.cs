namespace GraphManagementApp.UI
{
    public class GraphRenderer : Panel
    {
        private readonly GraphManager graphManager;
        private readonly Dictionary<int, PointF> nodePositions;
        private readonly Random random;

        private const float RepulsionForce = 10000f;
        private const float AttractionForce = 0.0001f;
        private const float MaxDisplacement = 100f;
        private const float Damping = 0.85f;
        private const int Iterations = 1000;

        public GraphRenderer(GraphManager graphManager)
        {
            this.graphManager = graphManager ?? throw new ArgumentNullException(nameof(graphManager));
            nodePositions = new Dictionary<int, PointF>();
            random = new Random();
            Paint += DrawGraph;

            AssignRandomPositions();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            RecalculateGraph();
        }

        public void RecalculateGraph()
        {
            // Remove any nodes that no longer exist in the GraphManager
            var currentNodes = new HashSet<int>(graphManager.GetVertices());
            var nodesToRemove = nodePositions.Keys.Where(n => !currentNodes.Contains(n)).ToList();

            foreach (var node in nodesToRemove)
            {
                nodePositions.Remove(node);
            }

            AssignRandomPositions();
            ApplyForceDirectedLayout();
            CenterGraph();
            Invalidate();
        }

        public Dictionary<int, PointF> GetNodePositions()
        {
            return new Dictionary<int, PointF>(nodePositions);
        }


        private void AssignRandomPositions()
        {
            foreach (int node in graphManager.GetVertices())
            {
                if (!nodePositions.ContainsKey(node)) 
                {
                    nodePositions[node] = new PointF(
                        random.Next(Width / 4, 3 * Width / 4),
                        random.Next(Height / 4, 3 * Height / 4)
                    );
                }
            }
        }


        private void ApplyForceDirectedLayout()
        {
            for (int i = 0; i < Iterations; i++)
            {
                var forces = CalculateForces();

                foreach (var node in nodePositions.Keys)
                {
                    ApplyDisplacement(node, forces[node]);
                }
            }

            CenterGraph();
        }

        private Dictionary<int, PointF> CalculateForces()
        {
            var forces = new Dictionary<int, PointF>();

            foreach (var v in nodePositions.Keys)
            {
                PointF force = new PointF(0, 0);

                foreach (var u in nodePositions.Keys)
                {
                    if (u == v) continue;

                    force = ApplyRepulsion(force, nodePositions[v], nodePositions[u]);
                }

                foreach (var edge in graphManager.GetEdges().Where(e => e.Item1 == v || e.Item2 == v))
                {
                    int neighbor = (edge.Item1 == v) ? edge.Item2 : edge.Item1;
                    force = ApplyAttraction(force, nodePositions[v], nodePositions[neighbor]);
                }

                forces[v] = force;
            }

            return forces;
        }

        private PointF ApplyRepulsion(PointF force, PointF vPos, PointF uPos)
        {
            PointF delta = new PointF(vPos.X - uPos.X, vPos.Y - uPos.Y);
            float distance = Math.Max(10, Distance(delta));
            float repulsion = RepulsionForce / (distance * distance);

            force.X += repulsion * (delta.X / distance);
            force.Y += repulsion * (delta.Y / distance);

            return force;
        }

        private PointF ApplyAttraction(PointF force, PointF vPos, PointF neighborPos)
        {
            PointF delta = new PointF(neighborPos.X - vPos.X, neighborPos.Y - vPos.Y);
            float distance = Math.Max(10, Distance(delta));
            float attraction = AttractionForce * (distance * distance);

            force.X += attraction * (delta.X / distance);
            force.Y += attraction * (delta.Y / distance);

            return force;
        }

        private void ApplyDisplacement(int node, PointF force)
        {
            PointF displacement = new PointF(
                Math.Min(MaxDisplacement, Math.Max(-MaxDisplacement, force.X)) * Damping,
                Math.Min(MaxDisplacement, Math.Max(-MaxDisplacement, force.Y)) * Damping
            );

            nodePositions[node] = new PointF(
                Math.Max(50, Math.Min(Width - 50, nodePositions[node].X + displacement.X)),
                Math.Max(50, Math.Min(Height - 50, nodePositions[node].Y + displacement.Y))
            );
        }

        private void CenterGraph()
        {
            if (nodePositions.Count == 0) return;

            float minX = nodePositions.Values.Min(p => p.X);
            float minY = nodePositions.Values.Min(p => p.Y);
            float maxX = nodePositions.Values.Max(p => p.X);
            float maxY = nodePositions.Values.Max(p => p.Y);

            float offsetX = (Width - (maxX - minX)) / 2 - minX;
            float offsetY = (Height - (maxY - minY)) / 2 - minY;

            foreach (var key in nodePositions.Keys.ToList())
            {
                nodePositions[key] = new PointF(nodePositions[key].X + offsetX, nodePositions[key].Y + offsetY);
            }
        }

        private void DrawGraph(object sender, PaintEventArgs e)
        {
            AssignRandomPositions();
            ApplyForceDirectedLayout();

            Graphics g = e.Graphics;
            g.Clear(Color.White);

            using (Pen edgePen = new Pen(Color.Gray, 2))
            using (Font font = new Font("Arial", 12, FontStyle.Bold))
            {
                Brush nodeBrush = Brushes.LightBlue;
                Brush textBrush = Brushes.Black;

                DrawEdges(g, edgePen);
                DrawNodes(g, nodeBrush, textBrush, font);
            }
        }

        private void DrawEdges(Graphics g, Pen edgePen)
        {
            foreach (var edge in graphManager.GetEdges())
            {
                if (nodePositions.TryGetValue(edge.Item1, out var p1) &&
                    nodePositions.TryGetValue(edge.Item2, out var p2))
                {
                    g.DrawLine(edgePen, p1, p2);

                    //  calc center
                    PointF midPoint = new PointF((p1.X + p2.X) / 2, (p1.Y + p2.Y) / 2);

                    // take wage
                    int weight = edge.Item3;

                    // protection
                    if (!float.IsNaN(midPoint.X) && !float.IsNaN(midPoint.Y))
                    {
                        g.DrawString(weight.ToString(), new Font("Arial", 10, FontStyle.Bold), Brushes.Blue, midPoint);
                    }
                }
            }
        }



        private void DrawNodes(Graphics g, Brush nodeBrush, Brush textBrush, Font font)
        {
            foreach (var node in nodePositions)
            {
                PointF p = node.Value;
                g.FillEllipse(nodeBrush, p.X - 20, p.Y - 20, 40, 40);
                g.DrawString(node.Key.ToString(), font, textBrush, p.X - 10, p.Y - 10);
            }
        }

        private float Distance(PointF delta) => (float)Math.Sqrt(delta.X * delta.X + delta.Y * delta.Y);
    }
}
