using HardwareShopPOS.Helpers;
using HardwareShopPOS.Services;

namespace HardwareShopPOS.Forms
{
    public partial class MainForm : Form
    {
        private Panel sidebarPanel = null!;
        private Panel contentPanel = null!;
        private UserControl? currentContent;
        private Dictionary<string, Panel> sectionPanels = new();
        private Dictionary<string, Button> sectionToggleButtons = new();

        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            SetupFormProperties();
            BuildSidebar();
            ShowDashboard();
        }

        private void SetupFormProperties()
        {
            // Form settings
            Text = "";
            Icon = null;
            ControlBox = true;
            MinimizeBox = true;
            MaximizeBox = true;
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            BackColor = UIHelper.BgPrimary;
            KeyPreview = true;

            // Content panel
            contentPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = UIHelper.BgPrimary,
                Padding = new Padding(20)
            };
            Controls.Add(contentPanel);

            // Header panel
            var headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = Color.White,
                Padding = new Padding(20, 10, 20, 10)
            };

            var btnNewSale = UIHelper.CreateButton("🛒 New Sale (F2)", UIHelper.Success, 140, 40);
            btnNewSale.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnNewSale.Location = new Point(headerPanel.Width - 160, 10);
            btnNewSale.Click += (s, e) => ShowBilling();
            headerPanel.Controls.Add(btnNewSale);
            headerPanel.Resize += (s, e) => btnNewSale.Location = new Point(headerPanel.Width - 160, 10);

            Controls.Add(headerPanel);
        }

        private void BuildSidebar()
        {
            sidebarPanel = new Panel
            {
                Dock = DockStyle.Left,
                Width = 220,
                BackColor = Color.FromArgb(248, 250, 252),
                AutoScroll = true
            };

            // Header
            var headerPanel = new Panel
            {
                Height = 90,
                Dock = DockStyle.Top,
                BackColor = Color.FromArgb(30, 90, 200),
                Padding = new Padding(15)
            };

            var titleLabel = new Label
            {
                Text = "Hardware Shop",
                Font = new Font("Segoe UI", 13, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(15, 10),
                AutoSize = true,
                BackColor = Color.Transparent
            };

            var subtitleLabel = new Label
            {
                Text = "Management System",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(200, 220, 255),
                Location = new Point(15, 38),
                AutoSize = true,
                BackColor = Color.Transparent
            };

            headerPanel.Controls.Add(titleLabel);
            headerPanel.Controls.Add(subtitleLabel);
            sidebarPanel.Controls.Add(headerPanel);

            // Menu
            var menuPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                Padding = new Padding(10)
            };

            int yPos = 10;

            // Dashboard
            yPos = AddMenuButton(menuPanel, "📊 Dashboard", Color.FromArgb(30, 90, 200), yPos, ShowDashboard);

            // Masters
            var masterItems = new MenuItem[]
            {
                new MenuItem { Text = "🏪 Stores", Action = () => ShowPanel("Stores") },
                new MenuItem { Text = "👥 Customers", Action = () => ShowPanel("Customers") },
                new MenuItem { Text = "🚚 Suppliers", Action = () => ShowPanel("Suppliers") },
                new MenuItem { Text = "📦 Items", Action = () => ShowPanel("Items") },
                new MenuItem { Text = "📋 Categories", Action = () => ShowPanel("Categories") },
                new MenuItem { Text = "👨 Employees", Action = () => ShowPanel("Employees") }
            };
            yPos = AddMenuSection(menuPanel, "⚙️ Masters", masterItems, yPos);

            // Sales
            var salesItems = new MenuItem[]
            {
                new MenuItem { Text = "🛒 New Sale (F2)", Action = ShowBilling },
                new MenuItem { Text = "📋 Retail Sales", Action = ShowRetailSales },
                new MenuItem { Text = "🏢 Wholesale Sales", Action = () => ShowPanel("Wholesale Sales") },
                new MenuItem { Text = "📝 Quotations", Action = () => ShowPanel("Quotations") },
                new MenuItem { Text = "↩️ Sales Returns", Action = () => ShowPanel("Sales Returns") }
            };
            yPos = AddMenuSection(menuPanel, "🛒 Sales", salesItems, yPos);

            // Inventory
            var inventoryItems = new MenuItem[]
            {
                new MenuItem { Text = "📊 Current Stock", Action = ShowInventory },
                new MenuItem { Text = "📥 Opening Stock", Action = () => ShowPanel("Opening Stock") },
                new MenuItem { Text = "⚙️ Adjustments", Action = () => ShowPanel("Stock Adjustments") },
                new MenuItem { Text = "📮 Dispatch", Action = () => ShowPanel("Dispatch") }
            };
            yPos = AddMenuSection(menuPanel, "📦 Inventory", inventoryItems, yPos);

            // Purchase
            var purchaseItems = new MenuItem[]
            {
                new MenuItem { Text = "🛍️ Purchase Orders", Action = () => ShowPanel("Purchase Orders") },
                new MenuItem { Text = "📦 Goods Received", Action = () => ShowPanel("Goods Received") },
                new MenuItem { Text = "↩️ Purchase Returns", Action = () => ShowPanel("Purchase Returns") }
            };
            yPos = AddMenuSection(menuPanel, "📩 Purchase", purchaseItems, yPos);

            // Reports
            var reportItems = new MenuItem[]
            {
                new MenuItem { Text = "📊 Daily Sales", Action = ShowReports },
                new MenuItem { Text = "📦 Stock Report", Action = () => ShowPanel("Stock Report") },
                new MenuItem { Text = "💰 Financial Report", Action = () => ShowPanel("Financial Report") }
            };
            yPos = AddMenuSection(menuPanel, "📈 Reports", reportItems, yPos);

            // Settings & Logout
            yPos = AddMenuButton(menuPanel, "⚙️ Settings", Color.FromArgb(100, 100, 100), yPos, ShowSettings);
            yPos = AddMenuButton(menuPanel, "🚪 Logout", Color.FromArgb(180, 50, 50), yPos, DoLogout);

            sidebarPanel.Controls.Add(menuPanel);
            Controls.Add(sidebarPanel);
        }

        private int AddMenuButton(Panel parent, string text, Color bgColor, int yPos, Action action)
        {
            var btn = new Button
            {
                Text = text,
                Size = new Size(190, 42),
                Location = new Point(5, yPos),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                BackColor = bgColor,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(10, 0, 0, 0),
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = AdjustBrightness(bgColor, 1.2);
            btn.Click += (s, e) => action?.Invoke();
            parent.Controls.Add(btn);
            return yPos + 48;
        }

        private int AddMenuSection(Panel parent, string title, MenuItem[] items, int yPos)
        {
            var headerBtn = new Button
            {
                Text = $"{title} ▼",
                Size = new Size(190, 40),
                Location = new Point(5, yPos),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.FromArgb(33, 47, 61),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                BackColor = Color.FromArgb(200, 210, 220),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(10, 0, 0, 0),
                Cursor = Cursors.Hand
            };
            headerBtn.FlatAppearance.BorderSize = 0;
            headerBtn.MouseEnter += (s, e) => headerBtn.BackColor = Color.FromArgb(180, 190, 210);
            headerBtn.MouseLeave += (s, e) => headerBtn.BackColor = Color.FromArgb(200, 210, 220);
            parent.Controls.Add(headerBtn);
            yPos += 45;

            var itemsPanel = new Panel { Location = new Point(5, yPos), Size = new Size(190, 0), BackColor = Color.Transparent };
            parent.Controls.Add(itemsPanel);
            sectionPanels[title] = itemsPanel;

            int itemY = 0;
            foreach (var item in items)
            {
                var itemBtn = new Button
                {
                    Text = $"  • {item.Text}",
                    Size = new Size(180, 35),
                    Location = new Point(10, itemY),
                    FlatStyle = FlatStyle.Flat,
                    ForeColor = Color.FromArgb(80, 90, 110),
                    Font = new Font("Segoe UI", 9),
                    BackColor = Color.White,
                    TextAlign = ContentAlignment.MiddleLeft,
                    Cursor = Cursors.Hand
                };
                itemBtn.FlatAppearance.BorderSize = 0;
                itemBtn.MouseEnter += (s, e) => itemBtn.BackColor = Color.FromArgb(240, 245, 250);
                itemBtn.MouseLeave += (s, e) => itemBtn.BackColor = Color.White;
                itemBtn.Click += (s, e) => item.Action?.Invoke();
                itemsPanel.Controls.Add(itemBtn);
                itemY += 38;
            }
            itemsPanel.Height = itemY;

            headerBtn.Click += (s, e) =>
            {
                bool isExpanded = itemsPanel.Height > 0;
                headerBtn.Text = isExpanded ? $"{title} ►" : $"{title} ▼";
                itemsPanel.Height = isExpanded ? 0 : itemY;
            };

            return yPos + itemsPanel.Height;
        }

        private Color AdjustBrightness(Color color, double brightness)
        {
            return Color.FromArgb(
                Math.Min(255, (int)(color.R * brightness)),
                Math.Min(255, (int)(color.G * brightness)),
                Math.Min(255, (int)(color.B * brightness))
            );
        }

        private void ShowContent(UserControl content)
        {
            if (contentPanel == null) return;
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

        private void ShowPanel(string panelName)
        {
            var placeholder = new UserControl { Dock = DockStyle.Fill, BackColor = UIHelper.BgPrimary };
            var label = new Label
            {
                Text = $"{panelName}\n\n(Coming Soon)",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = UIHelper.TextSecondary,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = UIHelper.BgPrimary
            };
            placeholder.Controls.Add(label);
            ShowContent(placeholder);
        }

        // Display methods
        private void ShowDashboard() => ShowContent(new DashboardPanel());
        private void ShowBilling() => ShowContent(new BillingPanel());
        private void ShowRetailSales() => ShowContent(new AdvancedRetailSalesPanel());
        private void ShowInventory() => ShowContent(new InventoryPanel());
        private void ShowReports() => ShowContent(new ReportsPanel());
        private void ShowSettings() { using var form = new SettingsForm(false); form.ShowDialog(); }
        private async void DoLogout()
        {
            if (UIHelper.Confirm("Logout?")) { await AuthService.LogoutAsync(); Application.Restart(); }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.F1: ShowDashboard(); e.Handled = true; break;
                case Keys.F2: ShowBilling(); e.Handled = true; break;
                case Keys.F3: ShowInventory(); e.Handled = true; break;
                case Keys.F4: ShowReports(); e.Handled = true; break;
            }
            base.OnKeyDown(e);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing && !UIHelper.Confirm("Exit?")) e.Cancel = true;
            base.OnFormClosing(e);
        }
    }

    internal class MenuItem
    {
        public string Text { get; set; } = "";
        public Action? Action { get; set; }
    }
}