using HardwareShopPOS.Helpers;
using HardwareShopPOS.Services;

namespace HardwareShopPOS.Forms
{
    public class MainForm : Form
    {
        private Panel sidebarPanel = null!;
        private Panel contentPanel = null!;
        private UserControl? currentContent;

        public MainForm()
        {
            InitializeComponent();
            ShowDashboard();
        }

        private void InitializeComponent()
        {
            Text = $"Hardware Shop - {AppSettings.StoreName}";
            Size = new Size(1400, 850);
            MinimumSize = new Size(1200, 700);
            StartPosition = FormStartPosition.CenterScreen;
            WindowState = FormWindowState.Maximized;
            BackColor = UIHelper.BgPrimary;

            // Main Layout
            var mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = UIHelper.BgPrimary
            };
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 220));
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            Controls.Add(mainLayout);

            // Sidebar
            sidebarPanel = BuildSidebar();
            mainLayout.Controls.Add(sidebarPanel, 0, 0);

            // Content Area
            var contentArea = new Panel { Dock = DockStyle.Fill, BackColor = UIHelper.BgPrimary };

            // Header
            var header = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = Color.White };
            var userLabel = new Label
            {
                Text = $"👤 {AppSettings.CurrentUserName ?? "User"}  |  🏪 {AppSettings.CurrentStoreName ?? "Store"}",
                Font = UIHelper.FontBold,
                Location = new Point(20, 18),
                AutoSize = true
            };
            header.Controls.Add(userLabel);

            var btnNewSale = UIHelper.CreateButton("🛒 New Sale (F2)", UIHelper.Success, 140, 40);
            btnNewSale.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnNewSale.Location = new Point(header.Width - 160, 10);
            btnNewSale.Click += (s, e) => ShowBilling();
            header.Controls.Add(btnNewSale);
            header.Resize += (s, e) => btnNewSale.Location = new Point(header.Width - 160, 10);

            contentArea.Controls.Add(header);

            // Main Content
            contentPanel = new Panel { Dock = DockStyle.Fill, BackColor = UIHelper.BgPrimary, Padding = new Padding(20) };
            contentArea.Controls.Add(contentPanel);

            mainLayout.Controls.Add(contentArea, 1, 0);

            // Keyboard Shortcuts
            KeyPreview = true;
            KeyDown += (s, e) =>
            {
                switch (e.KeyCode)
                {
                    case Keys.F1: ShowDashboard(); e.Handled = true; break;
                    case Keys.F2: ShowBilling(); e.Handled = true; break;
                    case Keys.F3: ShowInventory(); e.Handled = true; break;
                    case Keys.F4: ShowReports(); e.Handled = true; break;
                }
            };
        }

        private Panel BuildSidebar()
        {
            var sidebar = new Panel { Dock = DockStyle.Fill, BackColor = UIHelper.PrimaryBlue, AutoScroll = true };

            var logo = new Panel { Height = 60, Dock = DockStyle.Top, BackColor = UIHelper.PrimaryDark };
            logo.Controls.Add(new Label
            {
                Text = "🔧 Hardware Shop",
                Font = new Font("Segoe UI", 13, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(15, 18),
                AutoSize = true,
                BackColor = Color.Transparent
            });
            sidebar.Controls.Add(logo);

            var menuPanel = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent, Padding = new Padding(10) };

            int y = 10;
            string[] menus = { "📊 Dashboard", "🛒 New Sale", "📦 Inventory", "📈 Reports", "⚙️ Settings", "🚪 Logout" };
            Action[] actions = { ShowDashboard, ShowBilling, ShowInventory, ShowReports, ShowSettings, DoLogout };

            for (int i = 0; i < menus.Length; i++)
            {
                var btn = new Button
                {
                    Text = menus[i],
                    Size = new Size(190, 42),
                    Location = new Point(5, y),
                    FlatStyle = FlatStyle.Flat,
                    ForeColor = Color.White,
                    Font = UIHelper.FontNormal,
                    BackColor = Color.Transparent,
                    TextAlign = ContentAlignment.MiddleLeft,
                    Padding = new Padding(10, 0, 0, 0),
                    Cursor = Cursors.Hand
                };
                btn.FlatAppearance.BorderSize = 0;
                btn.FlatAppearance.MouseOverBackColor = UIHelper.PrimaryDark;

                int idx = i;
                btn.Click += (s, e) => actions[idx]();
                menuPanel.Controls.Add(btn);
                y += 48;
            }

            sidebar.Controls.Add(menuPanel);
            return sidebar;
        }

        private void ShowContent(UserControl content)
        {
            contentPanel.SuspendLayout();
            if (currentContent != null)
            {
                contentPanel.Controls.Remove(currentContent);
                currentContent.Dispose();
            }
            currentContent = content;
            content.Dock = DockStyle.Fill;
            contentPanel.Controls.Add(content);
            contentPanel.ResumeLayout();
        }

        private void ShowDashboard() => ShowContent(new DashboardPanel());
        private void ShowBilling() => ShowContent(new BillingPanel());
        private void ShowInventory() => ShowContent(new InventoryPanel());
        private void ShowReports() => ShowContent(new ReportsPanel());

        private void ShowSettings()
        {
            using var form = new SettingsForm(false);
            form.ShowDialog();
        }

        private async void DoLogout()
        {
            if (UIHelper.Confirm("Are you sure you want to logout?"))
            {
                await AuthService.LogoutAsync();
                Application.Restart();
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing && !UIHelper.Confirm("Exit application?"))
                e.Cancel = true;
            base.OnFormClosing(e);
        }
    }
}
