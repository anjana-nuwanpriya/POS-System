using HardwareShopPOS.Helpers;
using HardwareShopPOS.Services;

namespace HardwareShopPOS.Forms
{
    public class SettingsForm : Form
    {
        private TextBox txtHost = null!;
        private TextBox txtPort = null!;
        private TextBox txtDatabase = null!;
        private TextBox txtUsername = null!;
        private TextBox txtPassword = null!;
        private ComboBox cmbPrinter = null!;
        private CheckBox chkPrinterEnabled = null!;
        private Label lblStatus = null!;
        private bool _isInitialSetup;

        public SettingsForm(bool isInitialSetup = false)
        {
            _isInitialSetup = isInitialSetup;
            InitializeComponent();
            LoadSettings();
        }

        private void InitializeComponent()
        {
            Text = _isInitialSetup ? "Initial Setup" : "Settings";
            Size = new Size(500, 480);
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            BackColor = UIHelper.BgPrimary;

            var panel = new Panel { Location = new Point(20, 20), Size = new Size(445, 350), BackColor = Color.White };

            int y = 20;
            panel.Controls.Add(new Label { Text = "Database Connection", Font = UIHelper.FontBold, Location = new Point(15, y), AutoSize = true });
            y += 30;

            panel.Controls.Add(new Label { Text = "Server IP:", Location = new Point(15, y + 3), AutoSize = true });
            txtHost = new TextBox { Location = new Point(120, y), Size = new Size(300, 30) };
            panel.Controls.Add(txtHost);
            y += 35;

            panel.Controls.Add(new Label { Text = "Port:", Location = new Point(15, y + 3), AutoSize = true });
            txtPort = new TextBox { Location = new Point(120, y), Size = new Size(80, 30) };
            panel.Controls.Add(txtPort);
            y += 35;

            panel.Controls.Add(new Label { Text = "Database:", Location = new Point(15, y + 3), AutoSize = true });
            txtDatabase = new TextBox { Location = new Point(120, y), Size = new Size(300, 30) };
            panel.Controls.Add(txtDatabase);
            y += 35;

            panel.Controls.Add(new Label { Text = "Username:", Location = new Point(15, y + 3), AutoSize = true });
            txtUsername = new TextBox { Location = new Point(120, y), Size = new Size(300, 30) };
            panel.Controls.Add(txtUsername);
            y += 35;

            panel.Controls.Add(new Label { Text = "Password:", Location = new Point(15, y + 3), AutoSize = true });
            txtPassword = new TextBox { Location = new Point(120, y), Size = new Size(300, 30), UseSystemPasswordChar = true };
            panel.Controls.Add(txtPassword);
            y += 45;

            panel.Controls.Add(new Label { Text = "Printer Settings", Font = UIHelper.FontBold, Location = new Point(15, y), AutoSize = true });
            y += 30;

            chkPrinterEnabled = new CheckBox { Text = "Enable Thermal Printer", Location = new Point(120, y), AutoSize = true };
            panel.Controls.Add(chkPrinterEnabled);
            y += 30;

            panel.Controls.Add(new Label { Text = "Printer:", Location = new Point(15, y + 3), AutoSize = true });
            cmbPrinter = new ComboBox { Location = new Point(120, y), Size = new Size(300, 30), DropDownStyle = ComboBoxStyle.DropDownList };
            LoadPrinters();
            panel.Controls.Add(cmbPrinter);

            Controls.Add(panel);

            lblStatus = new Label { Location = new Point(20, 380), Size = new Size(200, 25), ForeColor = UIHelper.TextSecondary };
            Controls.Add(lblStatus);

            var btnTest = UIHelper.CreateButton("Test Connection", UIHelper.Info, 130, 40);
            btnTest.Location = new Point(160, 390);
            btnTest.Click += BtnTest_Click;
            Controls.Add(btnTest);

            var btnSave = UIHelper.CreateButton("Save & Continue", UIHelper.Success, 130, 40);
            btnSave.Location = new Point(300, 390);
            btnSave.Click += BtnSave_Click;
            Controls.Add(btnSave);

            if (!_isInitialSetup)
            {
                var btnCancel = UIHelper.CreateButton("Cancel", UIHelper.Danger, 100, 40);
                btnCancel.Location = new Point(20, 390);
                btnCancel.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };
                Controls.Add(btnCancel);
            }

            KeyPreview = true;
            KeyDown += (s, e) => { if (e.KeyCode == Keys.Escape && !_isInitialSetup) { DialogResult = DialogResult.Cancel; Close(); } };
        }

        private void LoadPrinters()
        {
            cmbPrinter.Items.Clear();
            cmbPrinter.Items.Add("(None)");
            foreach (var printer in PrinterService.GetPrinters())
                cmbPrinter.Items.Add(printer);
            cmbPrinter.SelectedIndex = 0;
        }

        private void LoadSettings()
        {
            txtHost.Text = AppSettings.DbHost;
            txtPort.Text = AppSettings.DbPort.ToString();
            txtDatabase.Text = AppSettings.DbName;
            txtUsername.Text = AppSettings.DbUser;
            txtPassword.Text = AppSettings.DbPassword;
            chkPrinterEnabled.Checked = AppSettings.PrinterEnabled;

            if (!string.IsNullOrEmpty(AppSettings.PrinterName))
            {
                int idx = cmbPrinter.Items.IndexOf(AppSettings.PrinterName);
                if (idx >= 0) cmbPrinter.SelectedIndex = idx;
            }
        }

        private void SaveSettings()
        {
            AppSettings.DbHost = txtHost.Text.Trim();
            AppSettings.DbPort = int.TryParse(txtPort.Text, out int port) ? port : 5432;
            AppSettings.DbName = txtDatabase.Text.Trim();
            AppSettings.DbUser = txtUsername.Text.Trim();
            AppSettings.DbPassword = txtPassword.Text;
            AppSettings.PrinterEnabled = chkPrinterEnabled.Checked;
            AppSettings.PrinterName = cmbPrinter.SelectedIndex > 0 ? cmbPrinter.SelectedItem?.ToString() ?? "" : "";
            AppSettings.Save();
        }

        private async void BtnTest_Click(object? sender, EventArgs e)
        {
            SaveSettings();
            lblStatus.Text = "Testing...";
            lblStatus.ForeColor = UIHelper.TextSecondary;

            bool success = await Task.Run(() => DatabaseService.TestConnection());

            lblStatus.Text = success ? "✓ Connected!" : "✗ Failed";
            lblStatus.ForeColor = success ? UIHelper.Success : UIHelper.Danger;
        }

        private async void BtnSave_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtHost.Text) || string.IsNullOrWhiteSpace(txtDatabase.Text))
            {
                UIHelper.ShowError("Server IP and Database are required.");
                return;
            }

            SaveSettings();
            lblStatus.Text = "Connecting...";

            bool success = await Task.Run(() => DatabaseService.TestConnection());

            if (success)
            {
                await AuthService.EnsureDefaultUserAsync();
                DialogResult = DialogResult.OK;
                Close();
            }
            else
            {
                lblStatus.Text = "✗ Connection failed";
                lblStatus.ForeColor = UIHelper.Danger;
            }
        }
    }
}
