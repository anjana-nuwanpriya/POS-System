using HardwareShopPOS.Helpers;
using HardwareShopPOS.Services;

namespace HardwareShopPOS.Forms
{
    public class DashboardPanel : UserControl
    {
        private Panel cardTodaySales = null!;
        private Panel cardInvoices = null!;
        private Panel cardReceivables = null!;
        private Panel cardLowStock = null!;
        private DataGridView dgvRecentSales = null!;

        public DashboardPanel()
        {
            InitializeComponent();
            LoadData();
        }
         
        private void InitializeComponent()
        {
            BackColor = UIHelper.BgPrimary;
            Dock = DockStyle.Fill;

            var header = UIHelper.CreateHeaderPanel("📊 Dashboard", "Welcome! Here's your overview.");
            header.Width = Width;
            header.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            Controls.Add(header);

            var statsPanel = new FlowLayoutPanel
            {
                Location = new Point(0, 100),
                Size = new Size(Width, 110),
                BackColor = Color.Transparent,
                WrapContents = false,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            cardTodaySales = UIHelper.CreateStatCard("Today's Sales", "Rs. 0", UIHelper.Success);
            cardInvoices = UIHelper.CreateStatCard("Invoices Today", "0", UIHelper.Info);
            cardReceivables = UIHelper.CreateStatCard("Receivables", "Rs. 0", UIHelper.Warning);
            cardLowStock = UIHelper.CreateStatCard("Low Stock", "0", UIHelper.Danger);

            statsPanel.Controls.Add(cardTodaySales);
            statsPanel.Controls.Add(cardInvoices);
            statsPanel.Controls.Add(cardReceivables);
            statsPanel.Controls.Add(cardLowStock);
            Controls.Add(statsPanel);

            var gridCard = UIHelper.CreateCard();
            gridCard.Location = new Point(0, 230);
            gridCard.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
            gridCard.Controls.Add(new Label
            {
                Text = "📋 Recent Sales",
                Font = UIHelper.FontSubtitle,
                Location = new Point(20, 15),
                AutoSize = true,
                BackColor = Color.Transparent
            });

            dgvRecentSales = UIHelper.CreateDataGrid();
            dgvRecentSales.Location = new Point(15, 50);
            dgvRecentSales.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
            dgvRecentSales.Columns.Add("invoice", "Invoice #");
            dgvRecentSales.Columns.Add("time", "Time");
            dgvRecentSales.Columns.Add("customer", "Customer");
            dgvRecentSales.Columns.Add("amount", "Amount");
            dgvRecentSales.Columns.Add("status", "Status");
            foreach (DataGridViewColumn col in dgvRecentSales.Columns)
                col.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            gridCard.Controls.Add(dgvRecentSales);

            gridCard.Resize += (s, e) => dgvRecentSales.Size = new Size(gridCard.Width - 30, gridCard.Height - 65);
            Controls.Add(gridCard);

            Resize += (s, e) =>
            {
                header.Width = Width;
                statsPanel.Width = Width;
                gridCard.Size = new Size(Width, Height - 250);
            };
        }

        private async void LoadData()
        {
            try
            {
                var todaySales = await SalesService.GetTodaySalesTotalAsync();
                var todayInvoices = await SalesService.GetTodayInvoiceCountAsync();
                var receivables = await SalesService.GetReceivablesAsync();
                var lowStock = await ItemService.GetLowStockCountAsync();

                UIHelper.UpdateStatCard(cardTodaySales, AppSettings.FormatCurrency(todaySales));
                UIHelper.UpdateStatCard(cardInvoices, todayInvoices.ToString());
                UIHelper.UpdateStatCard(cardReceivables, AppSettings.FormatCurrency(receivables));
                UIHelper.UpdateStatCard(cardLowStock, lowStock.ToString());

                var recentSales = await SalesService.GetTodaySalesAsync();
                dgvRecentSales.Rows.Clear();
                foreach (var sale in recentSales)
                {
                    dgvRecentSales.Rows.Add(
                        sale.InvoiceNumber,
                        sale.InvoiceDate.ToString("HH:mm"),
                        sale.CustomerName ?? "Walk-In",
                        AppSettings.FormatCurrency(sale.TotalAmount),
                        sale.PaymentStatus == "paid" ? "✅ Paid" : "⏳ Pending"
                    );
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Dashboard error: {ex.Message}");
            }
        }
    }
}
