using HardwareShopPOS.Helpers;
using HardwareShopPOS.Models;
using HardwareShopPOS.Services;

namespace HardwareShopPOS.Forms
{
    public class BillingPanel : UserControl
    {
        private TextBox txtBarcode = null!;
        private TextBox txtQuantity = null!;
        private ComboBox cmbCustomer = null!;
        private ComboBox cmbPaymentMethod = null!;
        private DataGridView dgvCart = null!;
        private Label lblSubtotal = null!;
        private Label lblDiscount = null!;
        private Label lblTax = null!;
        private Label lblTotal = null!;
        private Panel itemSearchPanel = null!;
        private ListBox lstSearchResults = null!;
        private System.Windows.Forms.Timer searchTimer = null!;

        private List<CartItem> cart = new();
        private List<Item> searchResults = new();

        public BillingPanel()
        {
            InitializeComponent();
            LoadCustomers();
        }

        private void InitializeComponent()
        {
            BackColor = UIHelper.BgPrimary;
            Dock = DockStyle.Fill;

            var header = UIHelper.CreateHeaderPanel("🛒 New Sale", "F2: Quick Sale | ESC: Clear | Enter: Add Item");
            header.Width = Width;
            header.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            Controls.Add(header);

            var mainLayout = new TableLayoutPanel
            {
                Location = new Point(0, 100),
                Size = new Size(Width, Height - 120),
                ColumnCount = 2,
                RowCount = 1,
                BackColor = Color.Transparent,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom
            };
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 65));
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35));

            // Left - Cart
            var cartPanel = UIHelper.CreateCard();
            cartPanel.Dock = DockStyle.Fill;
            cartPanel.Margin = new Padding(0, 0, 10, 0);

            var entryPanel = new Panel { Location = new Point(20, 15), Size = new Size(600, 70), BackColor = Color.Transparent };
            entryPanel.Controls.Add(new Label { Text = "Item Code / Barcode:", Font = UIHelper.FontBold, Location = new Point(0, 0), AutoSize = true });
            txtBarcode = new TextBox { Location = new Point(0, 25), Size = new Size(300, 35), Font = new Font("Segoe UI", 12) };
            txtBarcode.KeyDown += TxtBarcode_KeyDown;
            txtBarcode.TextChanged += TxtBarcode_TextChanged;
            entryPanel.Controls.Add(txtBarcode);

            entryPanel.Controls.Add(new Label { Text = "Qty:", Font = UIHelper.FontBold, Location = new Point(320, 0), AutoSize = true });
            txtQuantity = new TextBox { Location = new Point(320, 25), Size = new Size(60, 35), Font = new Font("Segoe UI", 12), Text = "1", TextAlign = HorizontalAlignment.Center };
            txtQuantity.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; AddItemToCart(); } };
            entryPanel.Controls.Add(txtQuantity);

            var btnAdd = UIHelper.CreateButton("+ Add", UIHelper.Success, 90, 35);
            btnAdd.Location = new Point(400, 25);
            btnAdd.Click += (s, e) => AddItemToCart();
            entryPanel.Controls.Add(btnAdd);
            cartPanel.Controls.Add(entryPanel);

            // Search dropdown
            itemSearchPanel = new Panel { Location = new Point(20, 95), Size = new Size(300, 150), BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle, Visible = false };
            lstSearchResults = new ListBox { Dock = DockStyle.Fill, Font = UIHelper.FontNormal, BorderStyle = BorderStyle.None };
            lstSearchResults.DoubleClick += (s, e) => SelectSearchResult();
            lstSearchResults.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; SelectSearchResult(); } else if (e.KeyCode == Keys.Escape) HideSearch(); };
            itemSearchPanel.Controls.Add(lstSearchResults);
            cartPanel.Controls.Add(itemSearchPanel);

            dgvCart = UIHelper.CreateDataGrid();
            dgvCart.Location = new Point(15, 100);
            dgvCart.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
            dgvCart.Columns.Add("code", "Code");
            dgvCart.Columns.Add("name", "Item Name");
            dgvCart.Columns.Add("qty", "Qty");
            dgvCart.Columns.Add("price", "Price");
            dgvCart.Columns.Add("total", "Total");
            dgvCart.Columns.Add(new DataGridViewButtonColumn { Name = "remove", Text = "X", UseColumnTextForButtonValue = true, Width = 40 });
            dgvCart.Columns["code"].Width = 80;
            dgvCart.Columns["name"].Width = 200;
            dgvCart.Columns["qty"].Width = 50;
            dgvCart.Columns["price"].Width = 80;
            dgvCart.Columns["total"].Width = 80;
            dgvCart.CellClick += (s, e) => { if (e.RowIndex >= 0 && e.ColumnIndex == dgvCart.Columns["remove"].Index) RemoveItem(e.RowIndex); };
            cartPanel.Controls.Add(dgvCart);

            cartPanel.Resize += (s, e) => dgvCart.Size = new Size(cartPanel.Width - 30, cartPanel.Height - 120);
            mainLayout.Controls.Add(cartPanel, 0, 0);

            // Right - Payment
            var paymentPanel = UIHelper.CreateCard();
            paymentPanel.Dock = DockStyle.Fill;
            paymentPanel.Margin = new Padding(10, 0, 0, 0);

            int y = 20;
            paymentPanel.Controls.Add(new Label { Text = "Customer:", Font = UIHelper.FontBold, Location = new Point(20, y), AutoSize = true });
            y += 25;
            cmbCustomer = new ComboBox { Location = new Point(20, y), Size = new Size(260, 35), DropDownStyle = ComboBoxStyle.DropDownList, Font = UIHelper.FontNormal };
            paymentPanel.Controls.Add(cmbCustomer);
            y += 45;

            paymentPanel.Controls.Add(new Label { Text = "Payment:", Font = UIHelper.FontBold, Location = new Point(20, y), AutoSize = true });
            y += 25;
            cmbPaymentMethod = new ComboBox { Location = new Point(20, y), Size = new Size(260, 35), DropDownStyle = ComboBoxStyle.DropDownList, Font = UIHelper.FontNormal };
            cmbPaymentMethod.Items.AddRange(new[] { "Cash", "Card", "UPI", "Credit" });
            cmbPaymentMethod.SelectedIndex = 0;
            paymentPanel.Controls.Add(cmbPaymentMethod);
            y += 55;

            var totalsPanel = new Panel { Location = new Point(20, y), Size = new Size(260, 130), BackColor = UIHelper.BgPrimary };
            lblSubtotal = new Label { Text = "Subtotal: Rs. 0.00", Font = UIHelper.FontNormal, Location = new Point(10, 10), AutoSize = true };
            lblDiscount = new Label { Text = "Discount: Rs. 0.00", Font = UIHelper.FontNormal, Location = new Point(10, 35), AutoSize = true };
            lblTax = new Label { Text = "Tax: Rs. 0.00", Font = UIHelper.FontNormal, Location = new Point(10, 60), AutoSize = true };
            lblTotal = new Label { Text = "TOTAL: Rs. 0.00", Font = new Font("Segoe UI", 16, FontStyle.Bold), ForeColor = UIHelper.PrimaryBlue, Location = new Point(10, 90), AutoSize = true };
            totalsPanel.Controls.AddRange(new Control[] { lblSubtotal, lblDiscount, lblTax, lblTotal });
            paymentPanel.Controls.Add(totalsPanel);
            y += 145;

            var btnComplete = UIHelper.CreateButton("✅ Complete (F10)", UIHelper.Success, 260, 50);
            btnComplete.Location = new Point(20, y);
            btnComplete.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            btnComplete.Click += BtnComplete_Click;
            paymentPanel.Controls.Add(btnComplete);
            y += 60;

            var btnClear = UIHelper.CreateButton("❌ Clear (ESC)", UIHelper.Danger, 260, 40);
            btnClear.Location = new Point(20, y);
            btnClear.Click += (s, e) => ClearCart();
            paymentPanel.Controls.Add(btnClear);

            mainLayout.Controls.Add(paymentPanel, 1, 0);
            Controls.Add(mainLayout);

            Resize += (s, e) =>
            {
                header.Width = Width;
                mainLayout.Size = new Size(Width, Height - 120);
            };

            // Search timer
            searchTimer = new System.Windows.Forms.Timer { Interval = 300 };
            searchTimer.Tick += async (s, e) => { searchTimer.Stop(); await SearchItems(); };
        }

        private async void LoadCustomers()
        {
            try
            {
                var customers = await SalesService.GetCustomersAsync();
                cmbCustomer.Items.Clear();
                cmbCustomer.Items.Add(new CustomerItem { Id = null, Name = "Walk-In Customer" });
                foreach (var c in customers)
                    cmbCustomer.Items.Add(new CustomerItem { Id = c.Id, Name = c.Name });
                cmbCustomer.SelectedIndex = 0;
                cmbCustomer.DisplayMember = "Name";
            }
            catch { }
        }

        private void TxtBarcode_TextChanged(object? sender, EventArgs e)
        {
            searchTimer.Stop();
            if (txtBarcode.Text.Length >= 2) searchTimer.Start();
            else HideSearch();
        }

        private async Task SearchItems()
        {
            if (string.IsNullOrEmpty(txtBarcode.Text)) return;
            try
            {
                var items = await ItemService.SearchItemsAsync(txtBarcode.Text, 10);
                searchResults = items.ToList();
                if (searchResults.Any())
                {
                    lstSearchResults.Items.Clear();
                    foreach (var item in searchResults)
                        lstSearchResults.Items.Add($"{item.Code} - {item.Name} ({AppSettings.FormatCurrency(item.RetailPrice)})");
                    itemSearchPanel.Visible = true;
                    itemSearchPanel.BringToFront();
                }
                else HideSearch();
            }
            catch { HideSearch(); }
        }

        private void HideSearch() => itemSearchPanel.Visible = false;

        private void TxtBarcode_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                if (itemSearchPanel.Visible && lstSearchResults.SelectedIndex >= 0)
                    SelectSearchResult();
                else
                    AddItemToCart();
            }
            else if (e.KeyCode == Keys.Down && itemSearchPanel.Visible)
            {
                e.SuppressKeyPress = true;
                lstSearchResults.Focus();
                if (lstSearchResults.Items.Count > 0) lstSearchResults.SelectedIndex = 0;
            }
            else if (e.KeyCode == Keys.Escape)
            {
                HideSearch();
                txtBarcode.Clear();
            }
        }

        private void SelectSearchResult()
        {
            if (lstSearchResults.SelectedIndex >= 0 && lstSearchResults.SelectedIndex < searchResults.Count)
            {
                var item = searchResults[lstSearchResults.SelectedIndex];
                txtBarcode.Text = item.Code;
                HideSearch();
                AddItemToCart(item);
            }
        }

        private async void AddItemToCart(Item? item = null)
        {
            string code = txtBarcode.Text.Trim();
            if (string.IsNullOrEmpty(code) && item == null) return;

            if (!decimal.TryParse(txtQuantity.Text, out decimal qty) || qty <= 0)
            {
                UIHelper.ShowError("Invalid quantity");
                txtQuantity.SelectAll();
                txtQuantity.Focus();
                return;
            }

            try
            {
                item ??= await ItemService.GetItemByCodeAsync(code);
                if (item == null)
                {
                    UIHelper.ShowError($"Item not found: {code}");
                    txtBarcode.SelectAll();
                    return;
                }

                var existing = cart.FirstOrDefault(c => c.ItemId == item.Id);
                if (existing != null)
                    existing.Quantity += qty;
                else
                    cart.Add(new CartItem
                    {
                        ItemId = item.Id,
                        ItemCode = item.Code,
                        ItemName = item.Name,
                        UnitPrice = item.RetailPrice,
                        Quantity = qty,
                        TaxRate = item.TaxRate,
                        AvailableStock = item.StockQuantity
                    });

                RefreshCart();
                txtBarcode.Clear();
                txtQuantity.Text = "1";
                txtBarcode.Focus();
                HideSearch();
            }
            catch (Exception ex)
            {
                UIHelper.ShowError($"Error: {ex.Message}");
            }
        }

        private void RefreshCart()
        {
            dgvCart.Rows.Clear();
            foreach (var item in cart)
            {
                dgvCart.Rows.Add(item.ItemCode, item.ItemName, item.Quantity.ToString("N0"),
                    AppSettings.FormatCurrency(item.UnitPrice), AppSettings.FormatCurrency(item.NetValue));
            }
            UpdateTotals();
        }

        private void UpdateTotals()
        {
            decimal subtotal = cart.Sum(c => c.UnitPrice * c.Quantity);
            decimal discount = cart.Sum(c => c.DiscountValue);
            decimal tax = cart.Sum(c => c.TaxValue);
            decimal total = subtotal - discount + tax;

            lblSubtotal.Text = $"Subtotal: {AppSettings.FormatCurrency(subtotal)}";
            lblDiscount.Text = $"Discount: {AppSettings.FormatCurrency(discount)}";
            lblTax.Text = $"Tax: {AppSettings.FormatCurrency(tax)}";
            lblTotal.Text = $"TOTAL: {AppSettings.FormatCurrency(total)}";
        }

        private void RemoveItem(int index)
        {
            if (index >= 0 && index < cart.Count)
            {
                cart.RemoveAt(index);
                RefreshCart();
            }
        }

        public void ClearCart()
        {
            if (cart.Count > 0 && !UIHelper.Confirm("Clear cart?")) return;
            cart.Clear();
            RefreshCart();
            txtBarcode.Clear();
            txtQuantity.Text = "1";
            txtBarcode.Focus();
        }

        private async void BtnComplete_Click(object? sender, EventArgs e)
        {
            if (!cart.Any())
            {
                UIHelper.ShowError("Cart is empty");
                txtBarcode.Focus();
                return;
            }

            if (!UIHelper.Confirm($"Complete sale for {lblTotal.Text}?")) return;

            try
            {
                var customer = cmbCustomer.SelectedItem as CustomerItem;
                var payment = cmbPaymentMethod.SelectedItem?.ToString() ?? "Cash";

                var sale = new SalesRetail
                {
                    CustomerId = customer?.Id,
                    PaymentMethod = payment,
                    PaymentStatus = payment == "Credit" ? "unpaid" : "paid"
                };

                var result = await SalesService.CreateSaleAsync(sale, cart);

                if (result != null)
                {
                    UIHelper.ShowInfo($"Sale completed!\n\nInvoice: {result.InvoiceNumber}\nTotal: {AppSettings.FormatCurrency(result.TotalAmount)}");

                    if (AppSettings.PrinterEnabled && AppSettings.AutoPrint)
                    {
                        var fullSale = await SalesService.GetSaleByIdAsync(result.Id);
                        if (fullSale != null) PrinterService.PrintReceipt(fullSale);
                    }

                    cart.Clear();
                    RefreshCart();
                    txtBarcode.Focus();
                }
                else
                    UIHelper.ShowError("Failed to complete sale");
            }
            catch (Exception ex)
            {
                UIHelper.ShowError($"Error: {ex.Message}");
            }
        }

        private class CustomerItem
        {
            public Guid? Id { get; set; }
            public string Name { get; set; } = "";
            public override string ToString() => Name;
        }
    }
}
