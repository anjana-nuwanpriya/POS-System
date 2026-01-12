using HardwareShopPOS.Helpers;
using HardwareShopPOS.Models;
using HardwareShopPOS.Services;

namespace HardwareShopPOS.Forms
{
    public class LoginForm : Form
    {
        private TextBox txtEmployeeCode = null!;
        private ComboBox cmbStore = null!;
        private Button btnLogin = null!;
        private Label lblStatus = null!;

        public LoginForm()
        {
            InitializeComponent();
            LoadStores();
        }

        private void InitializeComponent()
        {
            Text = "Hardware Shop - Login";
            Size = new Size(400, 380);
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            BackColor = UIHelper.BgPrimary;

            var panel = new Panel { Location = new Point(20, 20), Size = new Size(345, 300), BackColor = Color.White };

            var header = new Panel { Size = new Size(345, 80), BackColor = UIHelper.PrimaryBlue, Dock = DockStyle.Top };
            var title = new Label
            {
                Text = "🔧 Hardware Shop",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(50, 25),
                AutoSize = true,
                BackColor = Color.Transparent
            };
            header.Controls.Add(title);
            panel.Controls.Add(header);

            int y = 100;

            panel.Controls.Add(new Label { Text = "Select Store", Location = new Point(30, y), AutoSize = true });
            y += 22;
            cmbStore = new ComboBox { Location = new Point(30, y), Size = new Size(285, 35), DropDownStyle = ComboBoxStyle.DropDownList, Font = UIHelper.FontNormal };
            panel.Controls.Add(cmbStore);
            y += 45;

            panel.Controls.Add(new Label { Text = "Employee Code", Location = new Point(30, y), AutoSize = true });
            y += 22;
            txtEmployeeCode = new TextBox { Location = new Point(30, y), Size = new Size(285, 35), Font = new Font("Segoe UI", 12) };
            panel.Controls.Add(txtEmployeeCode);
            y += 50;

            btnLogin = UIHelper.CreateButton("🔐 Login", UIHelper.PrimaryBlue, 285, 45);
            btnLogin.Location = new Point(30, y);
            btnLogin.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            btnLogin.Click += BtnLogin_Click;
            panel.Controls.Add(btnLogin);
            y += 55;

            lblStatus = new Label { Location = new Point(30, y), Size = new Size(285, 25), ForeColor = UIHelper.Danger, TextAlign = ContentAlignment.MiddleCenter };
            panel.Controls.Add(lblStatus);

            Controls.Add(panel);

            KeyPreview = true;
            KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter) BtnLogin_Click(this, EventArgs.Empty);
                if (e.KeyCode == Keys.Escape) { DialogResult = DialogResult.Cancel; Close(); }
            };

            Shown += (s, e) => txtEmployeeCode.Focus();
        }

        private async void LoadStores()
        {
            try
            {
                var stores = await AuthService.GetStoresAsync();
                cmbStore.Items.Clear();

                foreach (var store in stores)
                    cmbStore.Items.Add(new StoreItem { Id = store.Id, Name = store.Name });

                if (cmbStore.Items.Count == 0)
                    cmbStore.Items.Add(new StoreItem { Id = Guid.Empty, Name = "Default Store" });

                cmbStore.SelectedIndex = 0;
                cmbStore.DisplayMember = "Name";
            }
            catch
            {
                cmbStore.Items.Add(new StoreItem { Id = Guid.Empty, Name = "Default Store" });
                cmbStore.SelectedIndex = 0;
            }
        }

        private async void BtnLogin_Click(object? sender, EventArgs e)
        {
            string code = txtEmployeeCode.Text.Trim();
            if (string.IsNullOrEmpty(code))
            {
                lblStatus.Text = "Enter employee code";
                txtEmployeeCode.Focus();
                return;
            }

            btnLogin.Enabled = false;
            lblStatus.Text = "Logging in...";
            lblStatus.ForeColor = UIHelper.TextSecondary;

            try
            {
                var store = cmbStore.SelectedItem as StoreItem;
                if (store != null && store.Id != Guid.Empty)
                    await AuthService.SetCurrentStoreAsync(store.Id);

                var employee = await AuthService.LoginAsync(code);

                if (employee != null)
                {
                    DialogResult = DialogResult.OK;
                    Close();
                }
                else
                {
                    lblStatus.Text = "Invalid employee code";
                    lblStatus.ForeColor = UIHelper.Danger;
                    txtEmployeeCode.SelectAll();
                    txtEmployeeCode.Focus();
                }
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Login failed";
                lblStatus.ForeColor = UIHelper.Danger;
                System.Diagnostics.Debug.WriteLine($"Login error: {ex.Message}");
            }
            finally
            {
                btnLogin.Enabled = true;
            }
        }

        private class StoreItem
        {
            public Guid Id { get; set; }
            public string Name { get; set; } = "";
            public override string ToString() => Name;
        }
    }
}
