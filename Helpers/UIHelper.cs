using System.Drawing.Drawing2D;

namespace HardwareShopPOS.Helpers
{
    public static class UIHelper
    {
        // Color Palette
        public static Color PrimaryBlue = Color.FromArgb(33, 97, 214);
        public static Color PrimaryDark = Color.FromArgb(28, 82, 181);
        public static Color Success = Color.FromArgb(40, 167, 69);
        public static Color Warning = Color.FromArgb(255, 133, 27);
        public static Color Danger = Color.FromArgb(220, 53, 69);
        public static Color Info = Color.FromArgb(0, 123, 255);
        public static Color Purple = Color.FromArgb(111, 66, 193);

        public static Color BgPrimary = Color.FromArgb(245, 247, 250);
        public static Color TextPrimary = Color.FromArgb(33, 37, 41);
        public static Color TextSecondary = Color.FromArgb(108, 117, 125);
        public static Color Border = Color.FromArgb(222, 226, 230);
        public static Color BorderLight = Color.FromArgb(233, 236, 239);

        // Fonts
        public static Font FontTitle = new Font("Segoe UI", 18, FontStyle.Bold);
        public static Font FontSubtitle = new Font("Segoe UI", 14, FontStyle.Bold);
        public static Font FontNormal = new Font("Segoe UI", 10);
        public static Font FontBold = new Font("Segoe UI", 10, FontStyle.Bold);
        public static Font FontSmall = new Font("Segoe UI", 9);
        public static Font FontLarge = new Font("Segoe UI", 24, FontStyle.Bold);

        public static Button CreateButton(string text, Color bgColor, int width = 120, int height = 40)
        {
            var btn = new Button
            {
                Text = text,
                Size = new Size(width, height),
                FlatStyle = FlatStyle.Flat,
                BackColor = bgColor,
                ForeColor = Color.White,
                Font = FontBold,
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = ControlPaint.Light(bgColor, 0.15f);
            return btn;
        }

        public static TextBox CreateTextBox(int width = 200)
        {
            return new TextBox
            {
                Size = new Size(width, 35),
                Font = FontNormal,
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
        }

        public static ComboBox CreateComboBox(int width = 200)
        {
            return new ComboBox
            {
                Size = new Size(width, 35),
                Font = FontNormal,
                BackColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
        }

        public static Label CreateLabel(string text, bool bold = false)
        {
            return new Label
            {
                Text = text,
                Font = bold ? FontBold : FontNormal,
                ForeColor = TextSecondary,
                AutoSize = true,
                BackColor = Color.Transparent
            };
        }

        public static DataGridView CreateDataGrid()
        {
            var dgv = new DataGridView
            {
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                RowHeadersVisible = false,
                AllowUserToAddRows = false,
                AllowUserToResizeRows = false,
                EnableHeadersVisualStyles = false,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                ColumnHeadersHeight = 45,
                ReadOnly = true
            };
            dgv.RowTemplate.Height = 40;

            dgv.DefaultCellStyle.BackColor = Color.White;
            dgv.DefaultCellStyle.ForeColor = TextPrimary;
            dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(230, 240, 255);
            dgv.DefaultCellStyle.SelectionForeColor = TextPrimary;
            dgv.DefaultCellStyle.Font = FontNormal;
            dgv.DefaultCellStyle.Padding = new Padding(8, 5, 8, 5);

            dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 249, 250);

            dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(233, 236, 239);
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = TextSecondary;
            dgv.ColumnHeadersDefaultCellStyle.Font = FontBold;
            dgv.ColumnHeadersDefaultCellStyle.Padding = new Padding(8, 10, 8, 10);

            dgv.GridColor = BorderLight;

            return dgv;
        }

        public static Panel CreateCard()
        {
            var card = new Panel { BackColor = Color.White };
            card.Paint += (s, e) =>
            {
                using var path = CreateRoundedRectangle(new Rectangle(0, 0, card.Width - 1, card.Height - 1), 6);
                card.Region = new Region(CreateRoundedRectangle(new Rectangle(0, 0, card.Width, card.Height), 6));
                using var pen = new Pen(Border, 1);
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                e.Graphics.DrawPath(pen, path);
            };
            return card;
        }

        public static Panel CreateStatCard(string title, string value, Color bgColor, int width = 220, int height = 90)
        {
            var card = new Panel
            {
                Size = new Size(width, height),
                BackColor = bgColor,
                Margin = new Padding(0, 0, 15, 0),
                Cursor = Cursors.Hand
            };

            card.Paint += (s, e) =>
            {
                using var path = CreateRoundedRectangle(new Rectangle(0, 0, card.Width, card.Height), 6);
                card.Region = new Region(path);
            };

            var titleLabel = new Label
            {
                Text = title,
                Font = FontNormal,
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(15, 12),
                BackColor = Color.Transparent
            };
            card.Controls.Add(titleLabel);

            var valueLabel = new Label
            {
                Text = value,
                Font = FontLarge,
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(15, 40),
                BackColor = Color.Transparent,
                Tag = "value"
            };
            card.Controls.Add(valueLabel);

            return card;
        }

        public static void UpdateStatCard(Panel card, string value)
        {
            foreach (Control ctrl in card.Controls)
            {
                if (ctrl is Label lbl && lbl.Tag?.ToString() == "value")
                {
                    lbl.Text = value;
                    break;
                }
            }
        }

        public static Panel CreateHeaderPanel(string title, string subtitle = "")
        {
            var header = new Panel
            {
                Height = 80,
                Dock = DockStyle.Top,
                BackColor = PrimaryBlue
            };

            header.Paint += (s, e) =>
            {
                using var path = CreateRoundedRectangle(new Rectangle(0, 0, header.Width, header.Height), 8);
                header.Region = new Region(path);
            };

            var titleLabel = new Label
            {
                Text = title,
                Font = FontTitle,
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(25, string.IsNullOrEmpty(subtitle) ? 25 : 15),
                BackColor = Color.Transparent
            };
            header.Controls.Add(titleLabel);

            if (!string.IsNullOrEmpty(subtitle))
            {
                var subtitleLabel = new Label
                {
                    Text = subtitle,
                    Font = FontNormal,
                    ForeColor = Color.FromArgb(220, 220, 220),
                    AutoSize = true,
                    Location = new Point(25, 45),
                    BackColor = Color.Transparent
                };
                header.Controls.Add(subtitleLabel);
            }

            return header;
        }

        public static GraphicsPath CreateRoundedRectangle(Rectangle rect, int radius)
        {
            var path = new GraphicsPath();
            int d = radius * 2;
            path.AddArc(rect.X, rect.Y, d, d, 180, 90);
            path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
            path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        public static void ShowInfo(string message, string title = "Information")
            => MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Information);

        public static void ShowError(string message, string title = "Error")
            => MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Error);

        public static bool Confirm(string message, string title = "Confirm")
            => MessageBox.Show(message, title, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
    }
}
