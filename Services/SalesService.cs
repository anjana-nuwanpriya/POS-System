using Dapper;
using HardwareShopPOS.Helpers;
using HardwareShopPOS.Models;

namespace HardwareShopPOS.Services
{
    public static class SalesService
    {
        public static async Task<string> GenerateInvoiceNumberAsync()
        {
            var prefix = AppSettings.InvoicePrefix;
            var year = DateTime.Now.Year;
            var month = DateTime.Now.Month;

            var sql = @"SELECT invoice_number FROM sales_retail 
                       WHERE invoice_number LIKE @Pattern
                       ORDER BY created_at DESC LIMIT 1";

            var pattern = $"{prefix}-{year}{month:D2}-%";
            var lastInvoice = await DatabaseService.QueryFirstOrDefaultAsync<string>(sql, new { Pattern = pattern });

            int sequence = 1;
            if (!string.IsNullOrEmpty(lastInvoice))
            {
                var parts = lastInvoice.Split('-');
                if (parts.Length >= 3 && int.TryParse(parts[2], out int lastSeq))
                    sequence = lastSeq + 1;
            }

            return $"{prefix}-{year}{month:D2}-{sequence:D4}";
        }

        public static async Task<SalesRetail?> CreateSaleAsync(SalesRetail sale, List<CartItem> items)
        {
            if (!items.Any()) return null;

            return await DatabaseService.ExecuteTransactionAsync(async (conn, trans) =>
            {
                sale.InvoiceNumber = await GenerateInvoiceNumberAsync();
                sale.InvoiceDate = DateTime.Now;
                sale.SaleDate = DateTime.Today;
                sale.StoreId = AppSettings.CurrentStoreId ?? Guid.Empty;
                sale.EmployeeId = AppSettings.CurrentUserId;

                sale.Subtotal = items.Sum(i => i.UnitPrice * i.Quantity);
                sale.Discount = items.Sum(i => i.DiscountValue);
                sale.Tax = items.Sum(i => i.TaxValue);
                sale.TotalAmount = sale.Subtotal - sale.Discount + sale.Tax;

                var saleSql = @"INSERT INTO sales_retail 
                               (invoice_number, invoice_date, sale_date, customer_id, store_id, employee_id,
                                payment_method, payment_status, subtotal, discount, tax, total_amount, description)
                               VALUES 
                               (@InvoiceNumber, @InvoiceDate, @SaleDate, @CustomerId, @StoreId, @EmployeeId,
                                @PaymentMethod, @PaymentStatus, @Subtotal, @Discount, @Tax, @TotalAmount, @Description)
                               RETURNING id";

                sale.Id = await conn.ExecuteScalarAsync<Guid>(saleSql, sale, trans);

                foreach (var item in items)
                {
                    var itemSql = @"INSERT INTO sales_retail_items 
                                   (sales_retail_id, item_id, quantity, unit_price, discount_percent, 
                                    discount_value, tax_value, net_value)
                                   VALUES 
                                   (@SalesRetailId, @ItemId, @Quantity, @UnitPrice, @DiscountPercent,
                                    @DiscountValue, @TaxValue, @NetValue)";

                    await conn.ExecuteAsync(itemSql, new
                    {
                        SalesRetailId = sale.Id,
                        item.ItemId,
                        item.Quantity,
                        item.UnitPrice,
                        item.DiscountPercent,
                        DiscountValue = item.DiscountValue,
                        TaxValue = item.TaxValue,
                        NetValue = item.NetValue
                    }, trans);

                    var stockSql = @"UPDATE item_store_stock 
                                    SET quantity_on_hand = quantity_on_hand - @Quantity,
                                        updated_at = CURRENT_TIMESTAMP
                                    WHERE item_id = @ItemId AND store_id = @StoreId";

                    await conn.ExecuteAsync(stockSql, new { item.ItemId, item.Quantity, StoreId = sale.StoreId }, trans);

                    var invLogSql = @"INSERT INTO inventory_transactions 
                                     (item_id, store_id, transaction_type, quantity, reference_id, reference_type, created_by)
                                     VALUES (@ItemId, @StoreId, 'sale', @Quantity, @ReferenceId, 'sales_retail', @CreatedBy)";

                    await conn.ExecuteAsync(invLogSql, new
                    {
                        item.ItemId,
                        StoreId = sale.StoreId,
                        Quantity = -item.Quantity,
                        ReferenceId = sale.Id,
                        CreatedBy = AppSettings.CurrentUserId
                    }, trans);
                }

                return true;
            }) ? sale : null;
        }

        public static async Task<SalesRetail?> GetSaleByIdAsync(Guid id)
        {
            var sql = @"SELECT sr.*, c.name as customer_name, e.name as employee_name
                       FROM sales_retail sr
                       LEFT JOIN customers c ON sr.customer_id = c.id
                       LEFT JOIN employees e ON sr.employee_id = e.id
                       WHERE sr.id = @Id";

            var sale = await DatabaseService.QueryFirstOrDefaultAsync<SalesRetail>(sql, new { Id = id });

            if (sale != null)
            {
                var itemsSql = @"SELECT sri.*, i.code as item_code, i.name as item_name
                                FROM sales_retail_items sri
                                JOIN items i ON sri.item_id = i.id
                                WHERE sri.sales_retail_id = @SaleId";

                var items = await DatabaseService.QueryAsync<SalesRetailItem>(itemsSql, new { SaleId = id });
                sale.Items = items.ToList();
            }

            return sale;
        }

        public static async Task<IEnumerable<SalesRetail>> GetTodaySalesAsync()
        {
            var sql = @"SELECT sr.*, c.name as customer_name, e.name as employee_name
                       FROM sales_retail sr
                       LEFT JOIN customers c ON sr.customer_id = c.id
                       LEFT JOIN employees e ON sr.employee_id = e.id
                       WHERE sr.sale_date = CURRENT_DATE
                       AND sr.store_id = @StoreId
                       AND sr.is_active = true
                       ORDER BY sr.created_at DESC";

            return await DatabaseService.QueryAsync<SalesRetail>(sql, new { StoreId = AppSettings.CurrentStoreId });
        }

        public static async Task<IEnumerable<SalesRetail>> GetSalesAsync(DateTime fromDate, DateTime toDate)
        {
            var sql = @"SELECT sr.*, c.name as customer_name, e.name as employee_name
                       FROM sales_retail sr
                       LEFT JOIN customers c ON sr.customer_id = c.id
                       LEFT JOIN employees e ON sr.employee_id = e.id
                       WHERE sr.sale_date >= @FromDate AND sr.sale_date <= @ToDate
                       AND sr.store_id = @StoreId
                       AND sr.is_active = true
                       ORDER BY sr.created_at DESC";

            return await DatabaseService.QueryAsync<SalesRetail>(sql, new
            {
                FromDate = fromDate,
                ToDate = toDate,
                StoreId = AppSettings.CurrentStoreId
            });
        }

        public static async Task<decimal> GetTodaySalesTotalAsync()
        {
            var sql = @"SELECT COALESCE(SUM(total_amount), 0) FROM sales_retail 
                       WHERE sale_date = CURRENT_DATE AND store_id = @StoreId AND is_active = true";

            return await DatabaseService.ExecuteScalarAsync<decimal>(sql, new { StoreId = AppSettings.CurrentStoreId });
        }

        public static async Task<int> GetTodayInvoiceCountAsync()
        {
            var sql = @"SELECT COUNT(*) FROM sales_retail 
                       WHERE sale_date = CURRENT_DATE AND store_id = @StoreId AND is_active = true";

            return await DatabaseService.ExecuteScalarAsync<int>(sql, new { StoreId = AppSettings.CurrentStoreId });
        }

        public static async Task<decimal> GetReceivablesAsync()
        {
            var sql = @"SELECT COALESCE(SUM(total_amount), 0) FROM sales_retail 
                       WHERE payment_status != 'paid' AND store_id = @StoreId AND is_active = true";

            return await DatabaseService.ExecuteScalarAsync<decimal>(sql, new { StoreId = AppSettings.CurrentStoreId });
        }

        public static async Task<IEnumerable<Customer>> GetCustomersAsync(string? search = null)
        {
            var sql = @"SELECT * FROM customers WHERE is_active = true";

            if (!string.IsNullOrEmpty(search))
                sql += @" AND (LOWER(name) LIKE LOWER(@Search) OR LOWER(phone) LIKE LOWER(@Search))";

            sql += " ORDER BY name";

            return await DatabaseService.QueryAsync<Customer>(sql, new { Search = $"%{search}%" });
        }
    }
}
