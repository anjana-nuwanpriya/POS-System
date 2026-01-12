using System.Drawing.Printing;
using HardwareShopPOS.Helpers;
using HardwareShopPOS.Models;

namespace HardwareShopPOS.Services
{
    public static class PrinterService
    {
        private static List<string> _receiptLines = new();
        private static int _currentLine = 0;
        private static readonly Font _normalFont = new Font("Consolas", 9);

        public static bool PrintReceipt(SalesRetail sale)
        {
            if (!AppSettings.PrinterEnabled || string.IsNullOrEmpty(AppSettings.PrinterName))
                return false;

            try
            {
                _receiptLines = GenerateReceiptLines(sale);
                _currentLine = 0;

                PrintDocument printDoc = new PrintDocument();
                printDoc.PrinterSettings.PrinterName = AppSettings.PrinterName;
                printDoc.PrintPage += PrintPage;
                printDoc.Print();
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Print error: {ex.Message}");
                return false;
            }
        }

        public static List<string> GenerateReceiptLines(SalesRetail sale)
        {
            int width = AppSettings.ReceiptWidth;
            var lines = new List<string>
            {
                CenterText(AppSettings.StoreName.ToUpper(), width),
                new string('-', width),
                $"Invoice: {sale.InvoiceNumber}",
                $"Date: {sale.InvoiceDate:dd/MM/yyyy HH:mm}",
                $"Cashier: {sale.EmployeeName ?? AppSettings.CurrentUserName}",
                new string('-', width),
                FormatItemLine("Item", "Qty", "Price", "Total", width),
                new string('-', width)
            };

            foreach (var item in sale.Items)
            {
                lines.Add(item.ItemName ?? "");
                lines.Add(FormatItemLine("",
                    item.Quantity.ToString("N0"),
                    item.UnitPrice.ToString("N2"),
                    item.NetValue.ToString("N2"), width));
            }

            lines.Add(new string('-', width));
            lines.Add(FormatTotalLine("Subtotal:", sale.Subtotal, width));
            if (sale.Discount > 0)
                lines.Add(FormatTotalLine("Discount:", -sale.Discount, width));
            if (sale.Tax > 0)
                lines.Add(FormatTotalLine("Tax:", sale.Tax, width));

            lines.Add(new string('=', width));
            lines.Add(FormatTotalLine("TOTAL:", sale.TotalAmount, width));
            lines.Add(new string('=', width));
            lines.Add($"Payment: {sale.PaymentMethod ?? "Cash"}");
            lines.Add("");
            lines.Add(CenterText("Thank you for your purchase!", width));
            lines.Add("");

            return lines;
        }

        private static void PrintPage(object sender, PrintPageEventArgs e)
        {
            if (e.Graphics == null) return;

            float yPos = 0;
            float leftMargin = 5;
            float lineHeight = _normalFont.GetHeight(e.Graphics) + 2;

            while (_currentLine < _receiptLines.Count)
            {
                e.Graphics.DrawString(_receiptLines[_currentLine], _normalFont, Brushes.Black, leftMargin, yPos);
                yPos += lineHeight;
                _currentLine++;

                if (yPos > e.PageBounds.Height - lineHeight)
                {
                    e.HasMorePages = true;
                    return;
                }
            }
            e.HasMorePages = false;
        }

        private static string CenterText(string text, int width)
        {
            if (text.Length >= width) return text;
            int padding = (width - text.Length) / 2;
            return text.PadLeft(padding + text.Length).PadRight(width);
        }

        private static string FormatItemLine(string item, string qty, string price, string total, int width)
        {
            int qtyWidth = 5, priceWidth = 10, totalWidth = 10;
            int itemWidth = width - qtyWidth - priceWidth - totalWidth - 3;
            return $"{item.PadRight(itemWidth)} {qty.PadLeft(qtyWidth)} {price.PadLeft(priceWidth)} {total.PadLeft(totalWidth)}";
        }

        private static string FormatTotalLine(string label, decimal amount, int width)
        {
            string amountStr = AppSettings.FormatCurrency(amount);
            return label.PadRight(width - amountStr.Length) + amountStr;
        }

        public static List<string> GetPrinters()
        {
            var printers = new List<string>();
            foreach (string printer in PrinterSettings.InstalledPrinters)
                printers.Add(printer);
            return printers;
        }
    }
}
