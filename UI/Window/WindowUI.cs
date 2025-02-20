using GraphManagementApp.Services.Data;
using GraphManagementApp.Models;

namespace GraphManagementApp.UI
{
    public class WindowUI : Form
    {
        private GraphManager graphManager;
        private GraphRenderer graphView;
        private ListView graphDisplay;
        private ComboBox nodeSelector;
        private Label statusLabel;
        private MenuStrip menuStrip;
        private DataHandler dataHandler;

        public WindowUI()
        {
            InitializeWindow();
            InitializeGraph();
            InitializeMenu();
            InitializeUI();

            dataHandler = new DataHandler(
                new FileHandler(),
                new DatabaseHandler(),
                new CSVHandler(),
                new ExcelHandler()
            );

            //graphManager.GenerateRandomGraph(10, 15, 10);
        }

        private void InitializeWindow()
        {
            this.Text = "Graph Management";
            this.Width = 1366;
            this.Height = 800;
            this.StartPosition = FormStartPosition.CenterScreen;
        }

        private void InitializeGraph()
        {
            graphManager = new GraphManager(UpdateUI);
            graphView = new GraphRenderer(graphManager)
            {
                Left = 500,
                Top = 40,
                Width = 800,
                Height = 700,
                BorderStyle = BorderStyle.FixedSingle
            };
        }

        private void InitializeMenu()
        {
            menuStrip = new MenuStrip();

            ToolStripMenuItem fileMenu = new ToolStripMenuItem("File");
            ToolStripMenuItem databaseMenu = new ToolStripMenuItem("Database");
            ToolStripMenuItem calculateMenu = new ToolStripMenuItem("Calculate");
            ToolStripMenuItem helpMenu = new ToolStripMenuItem("Help");

            // Create Load and Save submenus
            ToolStripMenuItem loadMenu = new ToolStripMenuItem("Load");
            loadMenu.DropDownItems.Add("From File", null, LoadGraphFromFile_Click);
            loadMenu.DropDownItems.Add("From CSV", null, LoadGraphFromCSV_Click);
            loadMenu.DropDownItems.Add("From Excel", null, LoadGraphFromExcel_Click);
            loadMenu.DropDownItems.Add("From Database", null, LoadGraphFromDatabase_Click);

            ToolStripMenuItem saveMenu = new ToolStripMenuItem("Save");
            saveMenu.DropDownItems.Add("To File", null, SaveGraphToFile_Click);
            saveMenu.DropDownItems.Add("To CSV", null, SaveGraphToCSV_Click);
            saveMenu.DropDownItems.Add("To Excel", null, SaveGraphToExcel_Click);
            saveMenu.DropDownItems.Add("To Database", null, SaveGraphToDatabase_Click);

            fileMenu.DropDownItems.Add("New", null, NewGraph_Click);
            fileMenu.DropDownItems.Add("Generate random graph", null, RandomGraph_Click);
            fileMenu.DropDownItems.Add(loadMenu);  // Add Load submenu
            fileMenu.DropDownItems.Add(saveMenu);  // Add Save submenu
            fileMenu.DropDownItems.Add("Exit", null, Exit_Click);

            // Database Menu Items
            databaseMenu.DropDownItems.Add("Connect", null, ConnectToDatabase_Click);
            databaseMenu.DropDownItems.Add("Disconnect", null, DisconnectDatabase_Click);

            // Calculate Menu Items
            calculateMenu.DropDownItems.Add("BFS", null, BFS_Click);
            calculateMenu.DropDownItems.Add("DFS", null, DFS_Click);
            calculateMenu.DropDownItems.Add("Dijkstra", null, Dijkstra_Click);

            // Help Menu
            helpMenu.DropDownItems.Add("About", null, About_Click);

            // Add menus to the main menu strip
            menuStrip.Items.Add(fileMenu);
            menuStrip.Items.Add(databaseMenu);
            menuStrip.Items.Add(calculateMenu);
            menuStrip.Items.Add(helpMenu);

            this.MainMenuStrip = menuStrip;
            this.Controls.Add(menuStrip);
        }



        private void InitializeUI()
        {
            Panel controlPanel = UIControls.CreateControlPanel(this, graphManager, UpdateUI, out graphDisplay, out nodeSelector, out statusLabel);
            this.Controls.AddRange(new Control[] { controlPanel, graphView });
            UpdateUI();
        }

        private void UpdateUI()
        {
            List<int> vertices = graphManager.GetVertices();
            string[] vertexArray = vertices.Select(v => v.ToString()).ToArray();

            nodeSelector.Items.Clear();
            nodeSelector.Items.Add("");
            nodeSelector.Items.AddRange(vertexArray);

            if (nodeSelector.Items.Count > 0)
            {
                nodeSelector.SelectedIndex = 0;
            }

            UIControls.UpdateGraphDisplay(graphDisplay, graphManager);
            graphView.RecalculateGraph();
            graphView.Invalidate();
        }

        private void NewGraph_Click(object sender, EventArgs e)
        {
            graphManager.clearGraph();
            ShowStatus("New graph created.");
            UpdateUI();
        }

        private void RandomGraph_Click(object sender, EventArgs e)
        {
            graphManager.GenerateRandomGraph(10, 15, 10);
            ShowStatus("New random graph generated.");
            UpdateUI();
        }

        private void LoadGraphFromFile_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Graph Files (*.txt)|*.txt|All Files (*.*)|*.*",
                Title = "Load Graph from File"
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                graphManager.SetGraph(dataHandler.LoadFromFile(openFileDialog.FileName));
                ShowStatus("Graph loaded from file.");
                UpdateUI();
            }
        }


        private void SaveGraphToFile_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "Graph Files (*.txt)|*.txt|All Files (*.*)|*.*",
                Title = "Save Graph to File"
            };

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    dataHandler.SaveToFile(saveFileDialog.FileName, graphManager.GetGraph());
                    ShowStatus("Graph saved to file.");
                }
                catch (Exception ex)
                {
                    ShowStatus($"Error saving file: {ex.Message}");
                }
            }
        }

        private void LoadGraphFromDatabase_Click(object sender, EventArgs e)
        {
            Graph loadedGraph = dataHandler.LoadFromDatabase();
            if (loadedGraph == null)
            {
                MessageBox.Show("You are not connected to any database.");
                return;
            }

            graphManager.SetGraph(loadedGraph);
            ShowStatus("Graph loaded from database.");
            UpdateUI();
        }



        private void SaveGraphToDatabase_Click(object sender, EventArgs e)
        {
            dataHandler.SaveToDatabase(graphManager.GetGraph());
            ShowStatus("Graph saved to database.");
        }

        private void LoadGraphFromCSV_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "CSV Files (*.csv)|*.csv|All Files (*.*)|*.*",
                Title = "Load Graph from CSV"
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                graphManager.SetGraph(dataHandler.ImportFromCSV(openFileDialog.FileName));
                ShowStatus("Graph loaded from CSV.");
                UpdateUI();
            }
        }

        private void SaveGraphToCSV_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "CSV Files (*.csv)|*.csv|All Files (*.*)|*.*",
                Title = "Save Graph to CSV"
            };

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                dataHandler.ExportToCSV(saveFileDialog.FileName, graphManager.GetGraph());
                ShowStatus("Graph saved to CSV.");
            }
        }

        private void LoadGraphFromExcel_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Excel Files (*.xlsx)|*.xlsx|All Files (*.*)|*.*",
                Title = "Load Graph from Excel"
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                graphManager.SetGraph(dataHandler.ImportFromExcel(openFileDialog.FileName));
                ShowStatus("Graph loaded from Excel.");
                UpdateUI();
            }
        }

        private void SaveGraphToExcel_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "Excel Files (*.xlsx)|*.xlsx|All Files (*.*)|*.*",
                Title = "Save Graph to Excel"
            };

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                dataHandler.ExportToExcel(saveFileDialog.FileName, graphManager.GetGraph());
                ShowStatus("Graph saved to Excel.");
            }
        }

        private void Exit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void About_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Graph Management Application\nVersion 1.0\nCreated by Kamil Socha", "About", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BFS_Click(object sender, EventArgs e) => ExecuteGraphTraversal("BFS");
        private void DFS_Click(object sender, EventArgs e) => ExecuteGraphTraversal("DFS");
        private void Dijkstra_Click(object sender, EventArgs e) => ExecuteGraphTraversal("Dijkstra");

        private int? ShowInputDialog(string title, string prompt)
        {
            Form inputForm = new Form()
            {
                Width = 300,
                Height = 180,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                Text = title,
                StartPosition = FormStartPosition.CenterScreen
            };

            Label textLabel = new Label() { Left = 20, Top = 20, Width = 250, Text = prompt };
            TextBox inputBox = new TextBox() { Left = 20, Top = 50, Width = 250 };
            Button okButton = new Button() { Text = "OK", Left = 50, Width = 80, Top = 90, DialogResult = DialogResult.OK };
            Button cancelButton = new Button() { Text = "Cancel", Left = 150, Width = 80, Top = 90, DialogResult = DialogResult.Cancel };

            inputForm.Controls.Add(textLabel);
            inputForm.Controls.Add(inputBox);
            inputForm.Controls.Add(okButton);
            inputForm.Controls.Add(cancelButton);
            inputForm.AcceptButton = okButton;
            inputForm.CancelButton = cancelButton;

            if (inputForm.ShowDialog() == DialogResult.OK)
            {
                if (int.TryParse(inputBox.Text, out int startNode))
                    return startNode;
                else
                    MessageBox.Show("Invalid input. Please enter a valid node number.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            return null;
        }

        private void ExecuteGraphTraversal(string algorithm)
        {
            int? startNode = ShowInputDialog($"Start {algorithm} Traversal", "Enter starting node:");

            if (!startNode.HasValue)
            {
                ShowStatus("Traversal canceled.");
                return;
            }

            if (!graphManager.GetVertices().Contains(startNode.Value))
            {
                MessageBox.Show("Node does not exist in the graph!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            List<int> traversalResult = new List<int>();
            Dictionary<int, int> shortestPaths = new Dictionary<int, int>();

            switch (algorithm)
            {
                case "BFS":
                    traversalResult = graphManager.BFS(startNode.Value);
                    break;
                case "DFS":
                    traversalResult = graphManager.DFS(startNode.Value);
                    break;
                case "Dijkstra":
                    shortestPaths = graphManager.Dijkstra(startNode.Value);
                    traversalResult = shortestPaths.Keys.ToList();
                    break;
                default:
                    ShowStatus("Unknown algorithm.");
                    return;
            }

            ShowStatus($"{algorithm} Traversal from {startNode} completed.");

            // Get adjacency list & node positions
            var adjacencyList = graphManager.GetAdjacencyList();
            var nodePositions = graphView.GetNodePositions();

            // Open GraphTraversalWindow
            GraphTraversalWindow traversalWindow = new GraphTraversalWindow(
                adjacencyList, traversalResult, algorithm, nodePositions, shortestPaths);

            traversalWindow.Show();
        }

        public void ShowStatus(string message)
        {
            statusLabel.Text = message;
        }

        private void ConnectToDatabase_Click(object sender, EventArgs e)
        {
            DatabaseConnectionForm dbForm = new DatabaseConnectionForm();

            if (dbForm.ShowDialog() == DialogResult.OK)
            {
                bool success = dataHandler.ConnectToDatabase(dbForm.IPAddress, dbForm.DatabaseName, dbForm.Username, dbForm.Password);
                ShowStatus(success ? "Connected to database successfully." : "Failed to connect.");
            }
        }

        private void DisconnectDatabase_Click(object sender, EventArgs e)
        {
            dataHandler.DisconnectDatabase();
            ShowStatus("Disconnected from database.");
        }
    }
}
