namespace GraphManagementApp.UI
{
    public static class UIControls
    {
        public static Panel CreateControlPanel(WindowUI parent, GraphManager graphManager, Action updateUI,
    out ListView graphDisplay, out ComboBox nodeSelector, out Label statusLabel)
        {
            Panel panel = new Panel
            {
                Left = 20,
                Top = 40,
                Width = 450,
                Height = 700,
                BorderStyle = BorderStyle.FixedSingle
            };

            // Status label
            statusLabel = new Label
            {
                Left = 10,
                Top = 680,
                Width = 420,
                ForeColor = Color.DarkGreen,
                Font = new Font("Arial", 10, FontStyle.Bold),
                Text = "Ready."
            };

            // Create UI Controls
            graphDisplay = new ListView
            {
                View = View.Details,  // Enables column headers
                FullRowSelect = true,
                GridLines = true,
                Left = 10,
                Top = 470,
                Width = 430,
                Height = 140
            };

            // Add columns
            graphDisplay.Columns.Add("Main Node", 120);
            graphDisplay.Columns.Add("Neighbours", 280);

            nodeSelector = new ComboBox { Left = 20, Top = 30, Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };

            // Add UI Elements to Panel
            panel.Controls.AddRange(new Control[]
            {
        CreateNewNodeSection(graphManager, updateUI, nodeSelector),
        CreateNodeManagementSection(graphManager, updateUI, nodeSelector),
        graphDisplay,
        CreateRefreshButton(updateUI, graphManager, nodeSelector),
        statusLabel
            });

            return panel;
        }



        private static GroupBox CreateNewNodeSection(GraphManager graphManager, Action updateUI, ComboBox nodeSelector)
        {
            GroupBox group = new GroupBox { Text = "New Node", Left = 10, Top = 10, Width = 430, Height = 80 };

            TextBox newNodeInput = new TextBox { Left = 20, Top = 30, Width = 200, PlaceholderText = "Enter node ID..." };
            Button addNodeButton = new Button { Text = "➕ Add Node", Left = 230, Top = 28, Width = 150 };

            addNodeButton.Click += (sender, e) =>
            {
                if (!int.TryParse(newNodeInput.Text, out int nodeValue))
                {
                    MessageBox.Show("⚠️ Please enter a valid number.", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (graphManager.GetVertices().Contains(nodeValue))
                {
                    MessageBox.Show("⚠️ Node already exists!", "Duplicate Node", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                graphManager.AddVertex(nodeValue);
                newNodeInput.Clear();
                updateUI();
                UpdateNodeDropdowns(graphManager, nodeSelector);
            };

            group.Controls.AddRange(new Control[] { newNodeInput, addNodeButton });
            return group;
        }

        private static GroupBox CreateNodeManagementSection(GraphManager graphManager, Action updateUI, ComboBox nodeSelector)
        {
            GroupBox group = new GroupBox { Text = "Node Management", Left = 10, Top = 100, Width = 430, Height = 350 };

            Label nodeTitle = new Label { Text = "Selected Node:", Left = 20, Top = 30, Width = 100 };

            nodeSelector.Width = 260;
            nodeSelector.Left = 120;
            nodeSelector.Top = 25;
            nodeSelector.Items.Add("-- Select Node --");
            nodeSelector.SelectedIndex = 0;

            Label connectTitle = new Label { Text = "Connect to:", Left = 20, Top = 70, Width = 120 };
            ComboBox connectionSelector = new ComboBox
            {
                Left = 20,
                Top = 95,
                Width = 280,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Enabled = false
            };

            Button connectButton = new Button { Text = "🔗 Connect", Left = 310, Top = 96, Width = 110, Enabled = false };

            Label edgesTitle = new Label { Text = "Edges:", Left = 20, Top = 140, Width = 100 };

            ListView edgeListView = new ListView
            {
                Left = 20,
                Top = 165,
                Width = 280,
                Height = 100,
                View = View.Details,  // Widok tabeli
                FullRowSelect = true,
                GridLines = true
            };

            edgeListView.Columns.Add("Node", 140);
            edgeListView.Columns.Add("Value", 140);

            Button removeEdgeButton = new Button { Text = "❌ Remove Edge", Left = 310, Top = 220, Width = 110, Enabled = false };
            Button editValueButton = new Button { Text = "✏️ Edit Value", Left = 310, Top = 180, Width = 110, Enabled = false };
            Button removeNodeButton = new Button { Text = "🗑️ Remove Node", Left = 120, Top = 290, Width = 200, Height = 40, Enabled = false };

            nodeSelector.SelectedIndexChanged += (sender, e) =>
            {
                edgeListView.Items.Clear();
                connectionSelector.Items.Clear();

                bool hasSelection = nodeSelector.SelectedIndex > 0;

                connectionSelector.Enabled = hasSelection;
                connectButton.Enabled = hasSelection;
                edgeListView.Enabled = hasSelection;
                removeEdgeButton.Enabled = hasSelection;
                removeNodeButton.Enabled = hasSelection;

                if (nodeSelector.SelectedIndex == 0) return;

                int selectedNode = Convert.ToInt32(nodeSelector.SelectedItem);
                HashSet<int> connectedNodes = new HashSet<int>();

                foreach (var edge in graphManager.GetEdges())
                {
                    if (edge.Item1 == selectedNode)
                    {
                        connectedNodes.Add(edge.Item2);
                        edgeListView.Items.Add(new ListViewItem(new[] { edge.Item2.ToString(), edge.Item3.ToString() }));
                    }
                    else if (edge.Item2 == selectedNode)
                    {
                        connectedNodes.Add(edge.Item1);
                        edgeListView.Items.Add(new ListViewItem(new[] { edge.Item1.ToString(), edge.Item3.ToString() }));
                    }
                }

                foreach (var node in graphManager.GetVertices())
                {
                    if (node != selectedNode && !connectedNodes.Contains(node))
                    {
                        connectionSelector.Items.Add(node);
                    }
                }
            };

            removeNodeButton.Click += (sender, e) =>
            {
                if (nodeSelector.SelectedIndex <= 0) return;

                int selectedNode = Convert.ToInt32(nodeSelector.SelectedItem);
                var confirm = MessageBox.Show($"Are you sure you want to remove node {selectedNode}?",
                    "Confirm Removal", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (confirm == DialogResult.Yes)
                {
                    graphManager.RemoveVertex(selectedNode);
                    updateUI();
                    UpdateNodeDropdowns(graphManager, nodeSelector);
                }
            };

            edgeListView.SelectedIndexChanged += (sender, e) =>
            {
                editValueButton.Enabled = edgeListView.SelectedItems.Count > 0;
            };

            editValueButton.Click += (sender, e) =>
            {
                if (edgeListView.SelectedItems.Count == 0 || nodeSelector.SelectedIndex <= 0) return;

                int selectedNode = Convert.ToInt32(nodeSelector.SelectedItem);
                int connectedNode = Convert.ToInt32(edgeListView.SelectedItems[0].SubItems[0].Text);
                int currentWeight = Convert.ToInt32(edgeListView.SelectedItems[0].SubItems[1].Text);

                string newWeightStr = Microsoft.VisualBasic.Interaction.InputBox(
                    $"Enter new weight for edge {selectedNode} ↔ {connectedNode}:",
                    "Edit Edge Weight",
                    currentWeight.ToString());

                if (!int.TryParse(newWeightStr, out int newWeight) || newWeight < 0)
                {
                    MessageBox.Show("⚠️ Please enter a valid positive number.", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                graphManager.UpdateEdgeWeight(selectedNode, connectedNode, newWeight);
                updateUI();
            };



            removeEdgeButton.Click += (sender, e) =>
            {
                if (edgeListView.SelectedItems.Count == 0 || nodeSelector.SelectedIndex <= 0) return;

                int connectedNode = Convert.ToInt32(edgeListView.SelectedItems[0].SubItems[0].Text);
                int selectedNode = Convert.ToInt32(nodeSelector.SelectedItem);

                graphManager.RemoveEdge(selectedNode, connectedNode);
                updateUI();
            };

            connectButton.Click += (sender, e) =>
            {
                if (nodeSelector.SelectedIndex <= 0 || connectionSelector.SelectedItem == null)
                {
                    MessageBox.Show("⚠️ Please select two nodes.", "Selection Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                int node1 = Convert.ToInt32(nodeSelector.SelectedItem);
                int node2 = Convert.ToInt32(connectionSelector.SelectedItem);

                graphManager.AddEdge(node1, node2);
                updateUI();
            };

            group.Controls.AddRange(new Control[]
            {
        nodeTitle, nodeSelector, connectTitle, connectionSelector, connectButton,
        edgesTitle, edgeListView, removeEdgeButton, removeNodeButton, editValueButton
            });

            return group;
        }


        private static void UpdateNodeDropdowns(GraphManager graphManager, ComboBox nodeSelector)
        {
            object previousSelection = nodeSelector.SelectedItem;

            nodeSelector.Items.Clear();
            nodeSelector.Items.Add(string.Empty); // Always add an empty option

            foreach (var node in graphManager.GetVertices())
            {
                nodeSelector.Items.Add(node);
            }

            if (previousSelection != null && nodeSelector.Items.Contains(previousSelection))
            {
                nodeSelector.SelectedItem = previousSelection;
            }
            else
            {
                nodeSelector.SelectedIndex = 0; // Always start with an empty selection
            }
        }

        public static void UpdateGraphDisplay(ListView graphDisplay, GraphManager graphManager)
        {
            graphDisplay.Items.Clear(); // Clear previous data

            List<int> allNodes = graphManager.GetVertices();
            if (allNodes.Count == 0)
            {
                graphDisplay.Items.Add(new ListViewItem(new[] { "Graph is empty.", "" }));
                return;
            }

            foreach (int node in allNodes)
            {
                List<int> neighbors = graphManager.GetEdges()
                    .Where(edge => edge.Item1 == node || edge.Item2 == node)
                    .Select(edge => edge.Item1 == node ? edge.Item2 : edge.Item1)
                    .Distinct()
                    .ToList();

                string neighborsList = neighbors.Count > 0 ? string.Join(", ", neighbors) : "None";

                // Add row to ListView
                graphDisplay.Items.Add(new ListViewItem(new[] { node.ToString(), neighborsList }));
            }
        }


        private static Button CreateRefreshButton(Action updateUI, GraphManager graphManager, ComboBox nodeSelector)
        {
            Button refreshButton = new Button { Text = "🔄 Refresh Graph", Left = 10, Top = 645, Width = 420 };
            refreshButton.Click += (sender, e) =>
            {
                updateUI();
                UpdateNodeDropdowns(graphManager, nodeSelector);
            };
            return refreshButton;
        }
    }
}
