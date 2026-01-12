using HardwareShopPOS.Helpers;
using HardwareShopPOS.Models;
using HardwareShopPOS.Services;

namespace HardwareShopPOS.Forms
{
    public class InventoryPanel : UserControl
    {
        private TextBox txtSearch = null!;
        private ComboBox cmbCategory = null!;
        private DataGridView dgvItems = null!;
        private Label lblTotalItems = null!;
        private Label lblLowStock = null!;
        private System.Windows.Forms.Timer searchTimer = null!;

        public InventoryPanel()
        {
            InitializeComponent();
            LoadCategories();
            LoadItems();
        }

        private void InitializeComponent()
        {
            BackColor = UIHelper.BgPrimary;
            Dock = DockStyle.Fill;

            var header = UIHelper.CreateHeaderPanel("📦 Inventory", "View and manage stock");
            header.Width = Width;
            header.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            Controls.Add(header);

            var toolbar = new FlowLayoutPanel
            {
                Location = new Point(0, 100),
                Size = new Size(Width, 50),
                BackColor = Color.Transparent,
                WrapContents = false,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            var btnAdd = UIHelper.CreateButton("➕ Add Item", UIHelper.Success, 120, 40);
            btnAdd.Click += (s, e) => { using var form = new ItemEditForm(null); if (form.ShowDialog() == DialogResult.OK) LoadItems(); };
            toolbar.Controls.Add(btnAdd);

            var btnRefresh = UIHelper.CreateButton("🔄 Refresh", UIHelper.PrimaryBlue, 100, 40);
            btnRefresh.Margin = new Padding(10, 0, 0, 0);
            btnRefresh.Click += (s, e) => LoadItems();
            toolbar.Controls.Add(btnRefresh);

            toolbar.Controls.Add(new Label { Text = "Category:", Margin = new Padding(20, 12, 5, 0), AutoSize = true });
            cmbCategory = new ComboBox { Size = new Size(150, 35), DropDownStyle = ComboBoxStyle.DropDownList, Margin = new Padding(0, 5, 0, 0) };
            cmbCategory.SelectedIndexChanged += (s, e) => LoadItems();
            toolbar.Controls.Add(cmbCategory);

            toolbar.Controls.Add(new Label { Text = "Search:", Margin = new Padding(15, 12, 5, 0), AutoSize = true });
            txtSearch = new TextBox { Size = new Size(200, 35), Margin = new Padding(0, 5, 0, 0) };
            txtSearch.TextChanged += (s, e) => { searchTimer.Stop(); searchTimer.Start(); };
            toolbar.Controls.Add(txtSearch);

            Controls.Add(toolbar);

            var statsBar = new Panel
            {
                Location = new Point(0, 155),
                Size = new Size(Width, 35),
                BackColor = Color.White,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            lblTotalItems = new Label { Text = "Total: 0", Font = UIHelper.FontBold, Location = new Point(20, 8), AutoSize = true };
            lblLowStock = new Label { Text = "Low Stock: 0", Font = UIHelper.FontBold, ForeColor = UIHelper.Warning, Location = new Point(150, 8), AutoSize = true };
            statsBar.Controls.AddRange(new Control[] { lblTotalItems, lblLowStock });
            Controls.Add(statsBar);

            var gridPanel = UIHelper.CreateCard();
            gridPanel.Location = new Point(0, 200);
            gridPanel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;

            dgvItems = UIHelper.CreateDataGrid();
            dgvItems.Location = new Point(15, 15);
            dgvItems.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
            dgvItems.Columns.Add("code", "Code");
            dgvItems.Columns.Add("name", "Name");
            dgvItems.Columns.Add("category", "Category");
            dgvItems.Columns.Add("cost", "Cost");
            dgvItems.Columns.Add("price", "Price");
            dgvItems.Columns.Add("stock", "Stock");
            dgvItems.Columns.Add("status", "Status");
            dgvItems.Columns["code"].Width = 80;
            dgvItems.Columns["name"].Width = 200;
            dgvItems.Columns["category"].Width = 100;
            dgvItems.Columns["cost"].Width = 80;
            dgvItems.Columns["price"].Width = 80;
            dgvItems.Columns["stock"].Width = 60;
            dgvItems.Columns["status"].Width = 100;
            dgvItems.DoubleClick += (s, e) =>
            {
                if (dgvItems.CurrentRow != null)
                {
                    string code = dgvItems.CurrentRow.Cells["code"].Value?.ToString() ?? "";
                    using var form = new ItemEditForm(code);
                    if (form.ShowDialog() == DialogResult.OK) LoadItems();
                }
            };
            gridPanel.Controls.Add(dgvItems);

            gridPanel.Resize += (s, e) => dgvItems.Size = new Size(gridPanel.Width - 30, gridPanel.Height - 30);
            Controls.Add(gridPanel);

            Resize += (s, e) =>
            {
                header.Width = Width;
                toolbar.Width = Width;
                statsBar.Width = Width;
                gridPanel.Size = new Size(Width, Height - 220);
            };

            searchTimer = new System.Windows.Forms.Timer { Interval = 300 };
            searchTimer.Tick += (s, e) => { searchTimer.Stop(); LoadItems(); };
        }

        private async void LoadCategories()
        {
            try
            {
                var categories = await ItemService.GetCategoriesAsync();
                cmbCategory.Items.Clear();
                cmbCategory.Items.Add(new CategoryItem { Id = null, Name = "All" });
                foreach (var c in categories)
                    cmbCategory.Items.Add(new CategoryItem { Id = c.Id, Name = c.Name });
                cmbCategory.SelectedIndex = 0;
                cmbCategory.DisplayMember = "Name";
            }
            catch { }
        }

        private async void LoadItems()
        {
            try
            {
                var category = cmbCategory.SelectedItem as CategoryItem;
                var search = txtSearch.Text.Trim();

                var items = await ItemService.GetItemsAsync(
                    string.IsNullOrEmpty(search) ? null : search,
                    category?.Id
                );

                dgvItems.Rows.Clear();
                int lowCount = 0;

                foreach (var item in items)
                {
                    string status;
                    if (item.StockQuantity <= 0) { status = "❌ Out"; lowCount++; }
                    else if (item.StockQuantity <= item.ReorderLevel) { status = "⚠️ Low"; lowCount++; }
                    else status = "✅ OK";

                    dgvItems.Rows.Add(
                        item.Code, item.Name, item.CategoryName ?? "-",
                        AppSettings.FormatCurrency(item.CostPrice),
                        AppSettings.FormatCurrency(item.RetailPrice),
                        item.StockQuantity.ToString("N0"), status
                    );
                }

                lblTotalItems.Text = $"Total: {items.Count()}";
                lblLowStock.Text = $"Low Stock: {lowCount}";
            }
            catch (Exception ex)
            {
                UIHelper.ShowError($"Error: {ex.Message}");
            }
        }

        private class CategoryItem
        {
            public Guid? Id { get; set; }
            public string Name { get; set; } = "";
            public override string ToString() => Name;
        }
    }

    public class ItemEditForm : Form
    {
        private TextBox txtCode = null!;
        private TextBox txtName = null!;
        private ComboBox cmbCategory = null!;
        private TextBox txtCostPrice = null!;
        private TextBox txtRetailPrice = null!;
        private TextBox txtBarcode = null!;
        private TextBox txtReorderLevel = null!;
        private Item? _existingItem;
        private bool _isEdit;

        public ItemEditForm(string? itemCode)
        {
            _isEdit = !string.IsNullOrEmpty(itemCode);
            InitializeComponent();
            LoadCategories();
            if (_isEdit) LoadItem(itemCode!);
        }

        private void InitializeComponent()
        {
            Text = _isEdit ? "Edit Item" : "Add Item";
            Size = new Size(450, 420);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            BackColor = UIHelper.BgPrimary;

            var panel = new Panel { Location = new Point(20, 20), Size = new Size(395, 300), BackColor = Color.White };

            int y = 15;
            panel.Controls.Add(new Label { Text = "Code:", Location = new Point(15, y), AutoSize = true });
            txtCode = new TextBox { Location = new Point(120, y - 3), Size = new Size(250, 30) };
            panel.Controls.Add(txtCode);
            y += 35;

            panel.Controls.Add(new Label { Text = "Name:", Location = new Point(15, y), AutoSize = true });
            txtName = new TextBox { Location = new Point(120, y - 3), Size = new Size(250, 30) };
            panel.Controls.Add(txtName);
            y += 35;

            panel.Controls.Add(new Label { Text = "Category:", Location = new Point(15, y), AutoSize = true });
            cmbCategory = new ComboBox { Location = new Point(120, y - 3), Size = new Size(250, 30), DropDownStyle = ComboBoxStyle.DropDownList };
            panel.Controls.Add(cmbCategory);
            y += 35;

            panel.Controls.Add(new Label { Text = "Cost Price:", Location = new Point(15, y), AutoSize = true });
            txtCostPrice = new TextBox { Location = new Point(120, y - 3), Size = new Size(120, 30), Text = "0" };
            panel.Controls.Add(txtCostPrice);
            y += 35;

            panel.Controls.Add(new Label { Text = "Retail Price:", Location = new Point(15, y), AutoSize = true });
            txtRetailPrice = new TextBox { Location = new Point(120, y - 3), Size = new Size(120, 30), Text = "0" };
            panel.Controls.Add(txtRetailPrice);
            y += 35;

            panel.Controls.Add(new Label { Text = "Barcode:", Location = new Point(15, y), AutoSize = true });
            txtBarcode = new TextBox { Location = new Point(120, y - 3), Size = new Size(250, 30) };
            panel.Controls.Add(txtBarcode);
            y += 35;

            panel.Controls.Add(new Label { Text = "Reorder Level:", Location = new Point(15, y), AutoSize = true });
            txtReorderLevel = new TextBox { Location = new Point(120, y - 3), Size = new Size(80, 30), Text = "10" };
            panel.Controls.Add(txtReorderLevel);

            Controls.Add(panel);

            var btnSave = UIHelper.CreateButton("💾 Save", UIHelper.Success, 100, 40);
            btnSave.Location = new Point(210, 335);
            btnSave.Click += BtnSave_Click;
            Controls.Add(btnSave);

            var btnCancel = UIHelper.CreateButton("Cancel", UIHelper.Danger, 100, 40);
            btnCancel.Location = new Point(320, 335);
            btnCancel.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };
            Controls.Add(btnCancel);

            KeyPreview = true;
            KeyDown += (s, e) => { if (e.KeyCode == Keys.Escape) { DialogResult = DialogResult.Cancel; Close(); } };
        }

        private async void LoadCategories()
        {
            try
            {
                var categories = await ItemService.GetCategoriesAsync();
                cmbCategory.Items.Clear();
                cmbCategory.Items.Add(new CategoryItem { Id = null, Name = "(None)" });
                foreach (var c in categories)
                    cmbCategory.Items.Add(new CategoryItem { Id = c.Id, Name = c.Name });
                cmbCategory.SelectedIndex = 0;
                cmbCategory.DisplayMember = "Name";
            }
            catch { }
        }

        private async void LoadItem(string code)
        {
            try
            {
                _existingItem = await ItemService.GetItemByCodeAsync(code);
                if (_existingItem != null)
                {
                    txtCode.Text = _existingItem.Code;
                    txtCode.ReadOnly = true;
                    txtName.Text = _existingItem.Name;
                    txtCostPrice.Text = _existingItem.CostPrice.ToString("N2");
                    txtRetailPrice.Text = _existingItem.RetailPrice.ToString("N2");
                    txtBarcode.Text = _existingItem.Barcode;
                    txtReorderLevel.Text = _existingItem.ReorderLevel.ToString();

                    if (_existingItem.CategoryId.HasValue)
                    {
                        for (int i = 0; i < cmbCategory.Items.Count; i++)
                        {
                            if (cmbCategory.Items[i] is CategoryItem ci && ci.Id == _existingItem.CategoryId)
                            {
                                cmbCategory.SelectedIndex = i;
                                break;
                            }
                        }
                    }
                }
            }
            catch (Exception ex) { UIHelper.ShowError($"Error: {ex.Message}"); }
        }

        private async void BtnSave_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtCode.Text) || string.IsNullOrWhiteSpace(txtName.Text))
            {
                UIHelper.ShowError("Code and Name are required");
                return;
            }

            if (!decimal.TryParse(txtRetailPrice.Text, out decimal retailPrice) || retailPrice < 0)
            {
                UIHelper.ShowError("Invalid retail price");
                return;
            }

            try
            {
                var category = cmbCategory.SelectedItem as CategoryItem;
                var item = new Item
                {
                    Code = txtCode.Text.Trim(),
                    Name = txtName.Text.Trim(),
                    CategoryId = category?.Id,
                    CostPrice = decimal.TryParse(txtCostPrice.Text, out var cp) ? cp : 0,
                    RetailPrice = retailPrice,
                    WholesalePrice = retailPrice,
                    Barcode = txtBarcode.Text.Trim(),
                    ReorderLevel = int.TryParse(txtReorderLevel.Text, out var rl) ? rl : 10
                };

                if (_isEdit && _existingItem != null)
                {
                    item.Id = _existingItem.Id;
                    await ItemService.UpdateItemAsync(item);
                }
                else
                    await ItemService.CreateItemAsync(item);

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex) { UIHelper.ShowError($"Error: {ex.Message}"); }
        }

        private class CategoryItem
        {
            public Guid? Id { get; set; }
            public string Name { get; set; } = "";
            public override string ToString() => Name;
        }
    }
}
