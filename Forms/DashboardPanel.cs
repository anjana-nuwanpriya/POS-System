using HardwareShopPOS.Helpers;
using HardwareShopPOS.Services;

namespace HardwareShopPOS.Forms
{
    /// <summary>
    /// Dashboard Panel - Complete Business Overview
    /// Displays sales summary, invoices, receivables, and low stock items
    /// </summary>
    public class DashboardPanel : UserControl
    {
        private Panel cardTodaySales = null!;
        private Panel cardInvoices = null!;
        private Panel cardReceivables = null!;
        private Panel cardLowStock = null!;
        private DataGridView dgvRecentSales = null!;
        private Label lblLastUpdated = null!;
        private Panel headerPanel;
        private Label titleLabel;
        private Label subtitleLabel;
        private FlowLayoutPanel cardsPanel;
        private Panel tablePanel;
        private Panel tableHeaderPanel;
        private Label tableTitle;
        private DataGridViewTextBoxColumn dataGridViewTextBoxColumn1;
        private DataGridViewTextBoxColumn dataGridViewTextBoxColumn2;
        private DataGridViewTextBoxColumn dataGridViewTextBoxColumn3;
        private DataGridViewTextBoxColumn dataGridViewTextBoxColumn4;
        private DataGridViewTextBoxColumn dataGridViewTextBoxColumn5;
        private System.Windows.Forms.Timer refreshTimer = null!;

        public DashboardPanel()
        {
            InitializeComponent();
            SetupRefreshTimer();
        }

        private void InitializeComponent()
        {
            DataGridViewCellStyle dataGridViewCellStyle2 = new DataGridViewCellStyle();
            headerPanel = new Panel();
            titleLabel = new Label();
            subtitleLabel = new Label();
            cardsPanel = new FlowLayoutPanel();
            tablePanel = new Panel();
            tableHeaderPanel = new Panel();
            tableTitle = new Label();
            lblLastUpdated = new Label();
            dgvRecentSales = new DataGridView();
            dataGridViewTextBoxColumn1 = new DataGridViewTextBoxColumn();
            dataGridViewTextBoxColumn2 = new DataGridViewTextBoxColumn();
            dataGridViewTextBoxColumn3 = new DataGridViewTextBoxColumn();
            dataGridViewTextBoxColumn4 = new DataGridViewTextBoxColumn();
            dataGridViewTextBoxColumn5 = new DataGridViewTextBoxColumn();
            headerPanel.SuspendLayout();
            cardsPanel.SuspendLayout();
            tablePanel.SuspendLayout();
            tableHeaderPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvRecentSales).BeginInit();
            SuspendLayout();
            // 
            // headerPanel
            // 
            headerPanel.Controls.Add(subtitleLabel);
            headerPanel.Location = new Point(3, 26);
            headerPanel.Name = "headerPanel";
            headerPanel.Size = new Size(200, 100);
            headerPanel.TabIndex = 0;
            // 
            // titleLabel
            // 
            titleLabel.Location = new Point(3, 0);
            titleLabel.Name = "titleLabel";
            titleLabel.Size = new Size(100, 23);
            titleLabel.TabIndex = 0;
            // 
            // subtitleLabel
            // 
            subtitleLabel.Location = new Point(0, 0);
            subtitleLabel.Name = "subtitleLabel";
            subtitleLabel.Size = new Size(100, 23);
            subtitleLabel.TabIndex = 1;
            // 
            // cardsPanel
            // 
            cardsPanel.Controls.Add(titleLabel);
            cardsPanel.Controls.Add(headerPanel);
            cardsPanel.Location = new Point(0, 0);
            cardsPanel.Name = "cardsPanel";
            cardsPanel.Size = new Size(200, 100);
            cardsPanel.TabIndex = 1;
            // 
            // tablePanel
            // 
            tablePanel.Controls.Add(tableHeaderPanel);
            tablePanel.Controls.Add(dgvRecentSales);
            tablePanel.Location = new Point(0, 0);
            tablePanel.Name = "tablePanel";
            tablePanel.Size = new Size(200, 100);
            tablePanel.TabIndex = 2;
            // 
            // tableHeaderPanel
            // 
            tableHeaderPanel.Controls.Add(tableTitle);
            tableHeaderPanel.Controls.Add(lblLastUpdated);
            tableHeaderPanel.Location = new Point(0, 0);
            tableHeaderPanel.Name = "tableHeaderPanel";
            tableHeaderPanel.Size = new Size(200, 100);
            tableHeaderPanel.TabIndex = 0;
            // 
            // tableTitle
            // 
            tableTitle.Location = new Point(0, 0);
            tableTitle.Name = "tableTitle";
            tableTitle.Size = new Size(100, 23);
            tableTitle.TabIndex = 0;
            // 
            // lblLastUpdated
            // 
            lblLastUpdated.Location = new Point(0, 0);
            lblLastUpdated.Name = "lblLastUpdated";
            lblLastUpdated.Size = new Size(100, 23);
            lblLastUpdated.TabIndex = 1;
            // 
            // dgvRecentSales
            // 
            dgvRecentSales.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            dgvRecentSales.ColumnHeadersHeight = 40;
            dgvRecentSales.Columns.AddRange(new DataGridViewColumn[] { dataGridViewTextBoxColumn1, dataGridViewTextBoxColumn2, dataGridViewTextBoxColumn3, dataGridViewTextBoxColumn4, dataGridViewTextBoxColumn5 });
            dgvRecentSales.Location = new Point(0, 0);
            dgvRecentSales.Name = "dgvRecentSales";
            dgvRecentSales.RowTemplate.Height = 35;
            dgvRecentSales.Size = new Size(240, 150);
            dgvRecentSales.TabIndex = 1;
            // 
            // dataGridViewTextBoxColumn1
            // 
            dataGridViewTextBoxColumn1.HeaderText = "Invoice #";
            dataGridViewTextBoxColumn1.Name = "dataGridViewTextBoxColumn1";
            // 
            // dataGridViewTextBoxColumn2
            // 
            dataGridViewTextBoxColumn2.HeaderText = "Time";
            dataGridViewTextBoxColumn2.Name = "dataGridViewTextBoxColumn2";
            // 
            // dataGridViewTextBoxColumn3
            // 
            dataGridViewTextBoxColumn3.HeaderText = "Customer";
            dataGridViewTextBoxColumn3.Name = "dataGridViewTextBoxColumn3";
            // 
            // dataGridViewTextBoxColumn4
            // 
            dataGridViewTextBoxColumn4.HeaderText = "Amount";
            dataGridViewTextBoxColumn4.Name = "dataGridViewTextBoxColumn4";
            // 
            // dataGridViewTextBoxColumn5
            // 
            dataGridViewTextBoxColumn5.HeaderText = "Status";
            dataGridViewTextBoxColumn5.Name = "dataGridViewTextBoxColumn5";
            // 
            // DashboardPanel
            // 
            AutoScroll = true;
            BackColor = Color.FromArgb(245, 247, 250);
            Controls.Add(cardsPanel);
            Controls.Add(tablePanel);
            Name = "DashboardPanel";
            Padding = new Padding(20);
            Size = new Size(1543, 572);
            headerPanel.ResumeLayout(false);
            cardsPanel.ResumeLayout(false);
            tablePanel.ResumeLayout(false);
            tableHeaderPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvRecentSales).EndInit();
            ResumeLayout(false);
        }

        /// <summary>
        /// Creates a styled statistics card
        /// </summary>
        private Panel CreateStatCard(string title, string value, Color accentColor, string tooltip)
        {
            var card = new Panel
            {
                Size = new Size(300, 110),
                BackColor = Color.White,
                BorderStyle = BorderStyle.None,
                Margin = new Padding(10),
                Padding = new Padding(15)
            };

            // Left accent bar
            var accentBar = new Panel
            {
                Size = new Size(4, 110),
                BackColor = accentColor,
                Dock = DockStyle.Left,
                Margin = new Padding(0)
            };
            card.Controls.Add(accentBar);

            // Title label
            var titleLabel = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.FromArgb(33, 47, 61),
                AutoSize = true,
                Location = new Point(20, 10),
                BackColor = Color.Transparent
            };
            card.Controls.Add(titleLabel);

            // Value label
            var valueLabel = new Label
            {
                Text = value,
                Font = new Font("Segoe UI", 24, FontStyle.Bold),
                ForeColor = accentColor,
                AutoSize = true,
                Location = new Point(20, 35),
                BackColor = Color.Transparent
            };
            card.Controls.Add(valueLabel);

            // Tooltip label
            var tooltipLabel = new Label
            {
                Text = tooltip,
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(120, 130, 150),
                AutoSize = true,
                Location = new Point(20, 75),
                BackColor = Color.Transparent
            };
            card.Controls.Add(tooltipLabel);

            return card;
        }

        private void SetupRefreshTimer()
        {
            refreshTimer = new System.Windows.Forms.Timer();
            refreshTimer.Interval = 30000; // 30 seconds
            refreshTimer.Tick += RefreshTimer_Tick;
            refreshTimer.Start();
        }

        private void RefreshTimer_Tick(object sender, EventArgs e)
        {
            LoadData();
        }

        private async void LoadData()
        {
            try
            {
                // Get statistics
                var todaySales = await SalesService.GetTodaySalesTotalAsync();
                var todayInvoices = await SalesService.GetTodayInvoiceCountAsync();
                var receivables = await SalesService.GetReceivablesAsync();
                var lowStock = await ItemService.GetLowStockCountAsync();

                // Update cards
                UpdateStatCard(cardTodaySales, AppSettings.FormatCurrency(todaySales));
                UpdateStatCard(cardInvoices, todayInvoices.ToString());
                UpdateStatCard(cardReceivables, AppSettings.FormatCurrency(receivables));
                UpdateStatCard(cardLowStock, lowStock.ToString());

                // Load recent sales
                var recentSales = await SalesService.GetTodaySalesAsync();
                dgvRecentSales.Rows.Clear();

                if (recentSales != null && recentSales.Any())
                {
                    foreach (var sale in recentSales.Take(20))
                    {
                        dgvRecentSales.Rows.Add(
                            sale.InvoiceNumber,
                            sale.InvoiceDate.ToString("HH:mm:ss"),
                            sale.CustomerName ?? "Walk-In",
                            AppSettings.FormatCurrency(sale.TotalAmount),
                            sale.PaymentStatus == "paid" ? "✅ Paid" : "⏳ Pending"
                        );
                    }
                }

                // Update last updated time
                lblLastUpdated.Text = $"Updated at {DateTime.Now:HH:mm:ss}";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Dashboard error: {ex.Message}");
                lblLastUpdated.Text = $"Error loading data: {ex.Message}";
            }
        }

        private void UpdateStatCard(Panel card, string value)
        {
            if (card == null) return;

            foreach (Control ctrl in card.Controls)
            {
                if (ctrl is Label lbl && lbl.Font.Size == 24)
                {
                    lbl.Text = value;
                    break;
                }
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            LoadData();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                refreshTimer?.Stop();
                refreshTimer?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}