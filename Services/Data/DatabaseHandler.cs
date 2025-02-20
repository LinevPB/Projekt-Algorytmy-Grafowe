using System.Data;
using MySql.Data.MySqlClient;
using GraphManagementApp.Interfaces.Data;
using GraphManagementApp.Models;

public class DatabaseHandler : IDatabaseHandler
{
    private MySqlConnection dbConnection;

    public bool Connect(string ip, string database, string user, string password)
    {
        string baseConnectionString = $"Server={ip};User ID={user};Password={password};";
        string fullConnectionString = $"{baseConnectionString}Database={database};";

        try
        {
            // 1. Connection without specified db
            using (var tempConnection = new MySqlConnection(baseConnectionString))
            {
                tempConnection.Open();

                // 2. Check if db exists
                string checkDbQuery = $"SELECT SCHEMA_NAME FROM INFORMATION_SCHEMA.SCHEMATA WHERE SCHEMA_NAME = '{database}';";
                using (var cmd = new MySqlCommand(checkDbQuery, tempConnection))
                using (var reader = cmd.ExecuteReader())
                {
                    if (!reader.HasRows) // If not exists -> create
                    {
                        reader.Close();
                        using (var createDbCmd = new MySqlCommand($"CREATE DATABASE {database};", tempConnection))
                        {
                            createDbCmd.ExecuteNonQuery();
                            Console.WriteLine($"Database '{database}' created successfully.");
                        }
                    }
                }
            }

            dbConnection = new MySqlConnection(fullConnectionString);
            dbConnection.Open();

            // 3. Creating tables if dont exist
            CreateTables();

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Database connection failed: {ex.Message}");
            return false;
        }
    }

    private void CreateTables()
    {
        string createNodesTable = @"
        CREATE TABLE IF NOT EXISTS GraphNodes (
            NodeID INT PRIMARY KEY
        );";

        string createEdgesTable = @"
        CREATE TABLE IF NOT EXISTS GraphEdges (
            FromNode INT,
            ToNode INT,
            Weight INT,
            PRIMARY KEY (FromNode, ToNode),
            FOREIGN KEY (FromNode) REFERENCES GraphNodes(NodeID) ON DELETE CASCADE,
            FOREIGN KEY (ToNode) REFERENCES GraphNodes(NodeID) ON DELETE CASCADE
        );";

        try
        {
            using (var cmd = new MySqlCommand(createNodesTable, dbConnection))
            {
                cmd.ExecuteNonQuery();
            }
            using (var cmd = new MySqlCommand(createEdgesTable, dbConnection))
            {
                cmd.ExecuteNonQuery();
            }

            Console.WriteLine("Database tables ensured.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating tables: {ex.Message}");
        }
    }



    public void Disconnect()
    {
        if (dbConnection != null && dbConnection.State == ConnectionState.Open)
        {
            dbConnection.Close();
        }
    }

    public void SaveToDatabase(Graph graph)
    {
        if (dbConnection == null || dbConnection.State != ConnectionState.Open)
        {
            MessageBox.Show("You are not connected to any database.");
            return;
        }

        using (var transaction = dbConnection.BeginTransaction())
        {
            try
            {
                // 1. Clear existing data (optional)
                using (var clearCmd = new MySqlCommand("DELETE FROM GraphEdges; DELETE FROM GraphNodes;", dbConnection, transaction))
                {
                    clearCmd.ExecuteNonQuery();
                }

                // 2. Insert Nodes
                string insertNodeQuery = "INSERT INTO GraphNodes (NodeID) VALUES (@NodeID) ON DUPLICATE KEY UPDATE NodeID = NodeID;";
                using (var insertNodeCmd = new MySqlCommand(insertNodeQuery, dbConnection, transaction))
                {
                    insertNodeCmd.Parameters.Add("@NodeID", MySqlDbType.Int32);

                    foreach (var vertex in graph.GetVertices())
                    {
                        insertNodeCmd.Parameters["@NodeID"].Value = vertex;
                        insertNodeCmd.ExecuteNonQuery();
                    }
                }

                // 3. Insert Edges
                string insertEdgeQuery = "INSERT INTO GraphEdges (FromNode, ToNode, Weight) VALUES (@FromNode, @ToNode, @Weight) " +
                                         "ON DUPLICATE KEY UPDATE Weight = VALUES(Weight);";
                using (var insertEdgeCmd = new MySqlCommand(insertEdgeQuery, dbConnection, transaction))
                {
                    insertEdgeCmd.Parameters.Add("@FromNode", MySqlDbType.Int32);
                    insertEdgeCmd.Parameters.Add("@ToNode", MySqlDbType.Int32);
                    insertEdgeCmd.Parameters.Add("@Weight", MySqlDbType.Int32);

                    foreach (var (from, to, weight) in graph.GetEdges())
                    {
                        insertEdgeCmd.Parameters["@FromNode"].Value = from;
                        insertEdgeCmd.Parameters["@ToNode"].Value = to;
                        insertEdgeCmd.Parameters["@Weight"].Value = weight;
                        insertEdgeCmd.ExecuteNonQuery();
                    }
                }

                // Commit transaction
                transaction.Commit();
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                throw new Exception("Error saving graph to database: " + ex.Message);
            }
        }
    }


    public Graph LoadFromDatabase()
    {
        if (dbConnection == null || dbConnection.State != ConnectionState.Open)
        {
            return null;
        }

        Graph graph = new Graph();

        try
        {
            // 1. Load Nodes
            string loadNodesQuery = "SELECT NodeID FROM GraphNodes;";
            using (var cmd = new MySqlCommand(loadNodesQuery, dbConnection))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    int nodeId = reader.GetInt32(0);
                    graph.AddVertex(nodeId);
                }
            }

            // 2. Load Edges
            string loadEdgesQuery = "SELECT FromNode, ToNode, Weight FROM GraphEdges;";
            using (var cmd = new MySqlCommand(loadEdgesQuery, dbConnection))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    int fromNode = reader.GetInt32(0);
                    int toNode = reader.GetInt32(1);
                    int weight = reader.GetInt32(2);
                    graph.AddEdge(fromNode, toNode, weight);
                }
            }
        }
        catch (Exception ex)
        {
            throw new Exception("Error loading graph from database: " + ex.Message);
        }

        return graph;
    }


    public MySqlConnection GetConnection() => dbConnection;
}
