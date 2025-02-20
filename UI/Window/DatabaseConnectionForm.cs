namespace GraphManagementApp.UI
{
    public class DatabaseConnectionForm : Form
    {
        public string IPAddress { get; private set; }
        public string DatabaseName { get; private set; }
        public string TableName { get; private set; }
        public string Username { get; private set; }
        public string Password { get; private set; }

        private TextBox ipBox, dbNameBox, tableBox, userBox, passBox;
        private Button connectButton, cancelButton;

        public DatabaseConnectionForm()
        {
            this.Text = "Database Connection";
            this.Width = 350;
            this.Height = 300;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;

            Label ipLabel = new Label() { Text = "IP Address:", Left = 20, Top = 20, Width = 100 };
            ipBox = new TextBox() { Left = 130, Top = 20, Width = 180 };
            ipBox.Text = "";

            Label dbNameLabel = new Label() { Text = "Database Name:", Left = 20, Top = 50, Width = 100 };
            dbNameBox = new TextBox() { Left = 130, Top = 50, Width = 180 };
            dbNameBox.Text = "";

            Label tableLabel = new Label() { Text = "Table Name:", Left = 20, Top = 80, Width = 100 };
            tableBox = new TextBox() { Left = 130, Top = 80, Width = 180 };
            tableBox.Text = "test";
            tableBox.Visible = false;

            Label userLabel = new Label() { Text = "Username:", Left = 20, Top = 130, Width = 100 };
            userBox = new TextBox() { Left = 130, Top = 130, Width = 180 };
            userBox.Text = "";

            Label passLabel = new Label() { Text = "Password:", Left = 20, Top = 160, Width = 100 };
            passBox = new TextBox() { Left = 130, Top = 160, Width = 180, PasswordChar = '*' };

            connectButton = new Button() { Text = "Connect", Left = 50, Top = 220, Width = 100 };
            cancelButton = new Button() { Text = "Cancel", Left = 180, Top = 220, Width = 100 };

            connectButton.Click += ConnectButton_Click;
            cancelButton.Click += (sender, e) => this.DialogResult = DialogResult.Cancel;

            this.Controls.Add(ipLabel);
            this.Controls.Add(ipBox);
            this.Controls.Add(dbNameLabel);
            this.Controls.Add(dbNameBox);
            //this.Controls.Add(tableLabel);
            this.Controls.Add(tableBox);
            this.Controls.Add(userLabel);
            this.Controls.Add(userBox);
            this.Controls.Add(passLabel);
            this.Controls.Add(passBox);
            this.Controls.Add(connectButton);
            this.Controls.Add(cancelButton);
        }

        private void ConnectButton_Click(object sender, EventArgs e)
        {
            IPAddress = ipBox.Text;
            DatabaseName = dbNameBox.Text;
            TableName = tableBox.Text;
            Username = userBox.Text;
            Password = passBox.Text;

            if (string.IsNullOrWhiteSpace(IPAddress) || string.IsNullOrWhiteSpace(DatabaseName) ||
                string.IsNullOrWhiteSpace(TableName) || string.IsNullOrWhiteSpace(Username))
            {
                MessageBox.Show("Please fill all fields.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
