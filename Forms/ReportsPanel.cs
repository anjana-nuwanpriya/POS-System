using HardwareShopPOS.Helpers;
using HardwareShopPOS.Services;

namespace HardwareShopPOS.Forms
{
    public class ReportsPanel : UserControl
    {
        private Panel cardRevenue = null!;
        private Panel cardOrders = null!;
        private Panel cardAvgOrder = null!;
        private DateTimePicker dtpFrom = null!;
        private DateTimePicker dtpTo = null!;
        private DataGridView dgvReport = null!;

        public ReportsPanel()
        {
            InitializeComponent();
            LoadReport();
        }

        private void InitializeComponent()
        {
            BackColor = UIHelper.BgPrimary;
            Dock = DockStyle.Fill;

            var header = UIHelper.CreateHeaderPanel("📊 Sales Reports", "View and analyze sales");
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

            cardRevenue = UIHelper.CreateStatCard("Total Revenue", "Rs. 0", UIHelper.Success);
            cardOrders = UIHelper.CreateStatCard("Total Orders", "0", UIHelper.Info);
            cardAvgOrder = UIHelper.CreateStatCard("Avg. Order", "Rs. 0", UIHelper.Warning);
            statsPanel.Controls.AddRange(new Control[] { cardRevenue, cardOrders, cardAvgOrder });
            Controls.Add(statsPanel);

            var toolbar = new Panel
            {
                Location = new Point(0, 220),
                Size = new Size(Width, 50),
                BackColor = Color.White,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            toolbar.Controls.Add(new Label { Text = "From:", Location = new Point(20, 15), AutoSize = true });
            dtpFrom = new DateTimePicker { Location = new Point(60, 10), Size = new Size(130, 30), Format = DateTimePickerFormat.Short, Value = DateTime.Today.AddDays(-30) };
            dtpFrom.ValueChanged += (s, e) => LoadReport();
            toolbar.Controls.Add(dtpFrom);

            toolbar.Controls.Add(new Label { Text = "To:", Location = new Point(210, 15), AutoSize = true });
            dtpTo = new DateTimePicker { Location = new Point(240, 10), Size = new Size(130, 30), Format = DateTimePickerFormat.Short, Value = DateTime.Today };
            dtpTo.ValueChanged += (s, e) => LoadReport();
            toolbar.Controls.Add(dtpTo);

            var btnRefresh = UIHelper.CreateButton("🔄 Refresh", UIHelper.PrimaryBlue, 100, 35);
            btnRefresh.Location = new Point(400, 8);
            btnRefresh.Click += (s, e) => LoadReport();
            toolbar.Controls.Add(btnRefresh);

            var btnExport = UIHelper.CreateButton("📤 Export CSV", UIHelper.Success, 120, 35);
            btnExport.Location = new Point(510, 8);
            btnExport.Click += BtnExport_Click;
            toolbar.Controls.Add(btnExport);

            Controls.Add(toolbar);

            var gridPanel = UIHelper.CreateCard();
            gridPanel.Location = new Point(0, 280);
            gridPanel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
            gridPanel.Controls.Add(new Label { Text = "📅 Daily Sales", Font = UIHelper.FontSubtitle, Location = new Point(20, 15), AutoSize = true, BackColor = Color.Transparent });

            dgvReport = UIHelper.CreateDataGrid();
            dgvReport.Location = new Point(15, 50);
            dgvReport.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
            dgvReport.Columns.Add("date", "Date");
            dgvReport.Columns.Add("invoices", "Invoices");
            dgvReport.Columns.Add("sales", "Total Sales");
            dgvReport.Columns.Add("cash", "Cash");
            dgvReport.Columns.Add("card", "Card");
            foreach (DataGridViewColumn col in dgvReport.Columns)
                col.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            gridPanel.Controls.Add(dgvReport);

            gridPanel.Resize += (s, e) => dgvReport.Size = new Size(gridPanel.Width - 30, gridPanel.Height - 65);
            Controls.Add(gridPanel);

            Resize += (s, e) =>
            {
                header.Width = Width;
                statsPanel.Width = Width;
                toolbar.Width = Width;
                gridPanel.Size = new Size(Width, Height - 300);
            };
        }

        private async void LoadReport()
        {
            try
            {
                var sales = await SalesService.GetSalesAsync(dtpFrom.Value.Date, dtpTo.Value.Date);
                var salesList = sales.ToList();

                decimal totalRevenue = salesList.Sum(s => s.TotalAmount);
                int totalOrders = salesList.Count;
                decimal avgOrder = totalOrders > 0 ? totalRevenue / totalOrders : 0;

                UIHelper.UpdateStatCard(cardRevenue, AppSettings.FormatCurrency(totalRevenue));
                UIHelper.UpdateStatCard(cardOrders, totalOrders.ToString());
                UIHelper.UpdateStatCard(cardAvgOrder, AppSettings.FormatCurrency(avgOrder));

                dgvReport.Rows.Clear();

                var dailyData = salesList.GroupBy(s => s.SaleDate.Date).OrderByDescending(g => g.Key);

                foreach (var day in dailyData)
                {
                    var dayList = day.ToList();
                    var cash = dayList.Where(s => s.PaymentMethod?.ToLower() == "cash").Sum(s => s.TotalAmount);
                    var card = dayList.Where(s => s.PaymentMethod?.ToLower() == "card").Sum(s => s.TotalAmount);

                    dgvReport.Rows.Add(
                        day.Key.ToString("dd/MM/yyyy"),
                        dayList.Count.ToString(),
                        AppSettings.FormatCurrency(dayList.Sum(s => s.TotalAmount)),
                        AppSettings.FormatCurrency(cash),
                        AppSettings.FormatCurrency(card)
                    );
                }
            }
            catch (Exception ex)
            {
                UIHelper.ShowError($"Error: {ex.Message}");
            }
        }

        private void BtnExport_Click(object? sender, EventArgs e)
        {
            try
            {
                using var sfd = new SaveFileDialog { Filter = "CSV Files|*.csv", FileName = $"Report_{DateTime.Now:yyyyMMdd}.csv" };
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    using var writer = new System.IO.StreamWriter(sfd.FileName);

                    var headers = string.Join(",", dgvReport.Columns.Cast<DataGridViewColumn>().Select(c => $"\"{c.HeaderText}\""));
                    writer.WriteLine(headers);

                    foreach (DataGridViewRow row in dgvReport.Rows)
                    {
                        var line = string.Join(",", row.Cells.Cast<DataGridViewCell>().Select(c => $"\"{c.Value}\""));
                        writer.WriteLine(line);
                    }

                    UIHelper.ShowInfo($"Exported to:\n{sfd.FileName}");
                }
            }
            catch (Exception ex)
            {
                UIHelper.ShowError($"Export error: {ex.Message}");
            }
        }
    }
}
