using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Npgsql;
using HardwareShopPOS.Models;

namespace HardwareShopPOS.Services
{
    /// <summary>
    /// Extended RetailSaleService with comprehensive sales management
    /// Replaces Next.js /api/sales-retail route
    /// </summary>
    public class RetailSaleServiceExtended
    {
        // ==================== GET OPERATIONS ====================

        /// <summary>
        /// Get list of retail sales with optional filters
        /// Replaces: GET /api/sales-retail
        /// </summary>
        public async Task<(List<dynamic> Sales, int Total)> GetRetailSalesAsync(
            string? paymentStatus = null,
            Guid? storeId = null,
            DateTime? dateFrom = null,
            DateTime? dateTo = null,
            int limit = 50,
            int offset = 0)
        {
            using (var connection = DatabaseService.GetConnection())
            {
                var query = @"
                    SELECT 
                        sr.id,
                        sr.invoice_number,
                        sr.invoice_date,
                        sr.sale_date,
                        sr.customer_id,
                        sr.store_id,
                        sr.employee_id,
                        sr.payment_method,
                        sr.payment_status,
                        sr.subtotal,
                        sr.discount,
                        sr.tax,
                        sr.total_amount,
                        sr.description,
                        sr.is_active,
                        sr.created_at,
                        -- Store data
                        s.id as store_id_fk,
                        s.code as store_code,
                        s.name as store_name,
                        s.address as store_address,
                        -- Customer data
                        c.id as customer_id_fk,
                        c.name as customer_name,
                        c.type as customer_type,
                        c.phone as customer_phone,
                        c.email as customer_email,
                        -- Employee data
                        e.id as employee_id_fk,
                        e.name as employee_name,
                        e.email as employee_email
                    FROM sales_retail sr
                    LEFT JOIN stores s ON sr.store_id = s.id
                    LEFT JOIN customers c ON sr.customer_id = c.id
                    LEFT JOIN employees e ON sr.employee_id = e.id
                    WHERE sr.is_active = true";

                // Apply filters
                var parameters = new DynamicParameters();

                if (!string.IsNullOrEmpty(paymentStatus))
                {
                    query += " AND sr.payment_status = @PaymentStatus";
                    parameters.Add("@PaymentStatus", paymentStatus);
                }

                if (storeId.HasValue)
                {
                    query += " AND sr.store_id = @StoreId";
                    parameters.Add("@StoreId", storeId.Value);
                }

                if (dateFrom.HasValue)
                {
                    query += " AND sr.sale_date >= @DateFrom";
                    parameters.Add("@DateFrom", dateFrom.Value.Date);
                }

                if (dateTo.HasValue)
                {
                    query += " AND sr.sale_date <= @DateTo";
                    parameters.Add("@DateTo", dateTo.Value.Date);
                }

                // Get total count
                var countQuery = $"SELECT COUNT(*) FROM sales_retail WHERE is_active = true";
                var countParams = new DynamicParameters();

                if (!string.IsNullOrEmpty(paymentStatus))
                    countParams.Add("@PaymentStatus", paymentStatus);
                if (storeId.HasValue)
                    countParams.Add("@StoreId", storeId.Value);
                if (dateFrom.HasValue)
                    countParams.Add("@DateFrom", dateFrom.Value.Date);
                if (dateTo.HasValue)
                    countParams.Add("@DateTo", dateTo.Value.Date);

                var total = await connection.QueryFirstOrDefaultAsync<int>(countQuery, countParams);

                // Apply sorting and pagination
                query += " ORDER BY sr.invoice_date DESC";
                query += $" LIMIT {limit} OFFSET {offset}";

                var sales = (await connection.QueryAsync<dynamic>(query, parameters))?.ToList() ?? new List<dynamic>();

                return (sales, total);
            }
        }

        /// <summary>
        /// Get sale details with items by ID
        /// </summary>
        public async Task<(dynamic Sale, List<dynamic> Items)> GetRetailSaleWithItemsAsync(Guid saleId)
        {
            using (var connection = DatabaseService.GetConnection())
            {
                // Get sale
                var saleQuery = @"
                    SELECT 
                        sr.*,
                        s.code as store_code,
                        s.name as store_name,
                        c.name as customer_name,
                        e.name as employee_name
                    FROM sales_retail sr
                    LEFT JOIN stores s ON sr.store_id = s.id
                    LEFT JOIN customers c ON sr.customer_id = c.id
                    LEFT JOIN employees e ON sr.employee_id = e.id
                    WHERE sr.id = @SaleId";

                var sale = await connection.QueryFirstOrDefaultAsync<dynamic>(saleQuery, new { SaleId = saleId });

                // Get items
                var itemsQuery = @"
                    SELECT 
                        sri.*,
                        i.code,
                        i.name,
                        i.unit_of_measure
                    FROM sales_retail_items sri
                    LEFT JOIN items i ON sri.item_id = i.id
                    WHERE sri.sales_retail_id = @SaleId
                    ORDER BY sri.created_at ASC";

                var items = (await connection.QueryAsync<dynamic>(itemsQuery, new { SaleId = saleId }))?.ToList() ?? new List<dynamic>();

                return (sale, items);
            }
        }

        // ==================== CREATE OPERATION ====================

        /// <summary>
        /// Create new retail sale with items and inventory updates
        /// Replaces: POST /api/sales-retail
        /// Handles complete transaction with stock deduction
        /// </summary>
        public async Task<(bool Success, string Message, Guid SaleId)> CreateRetailSaleAsync(
            Guid storeId,
            Guid? customerId,
            Guid? employeeId,
            string paymentMethod,
            string? description,
            List<RetailSaleItemDto> items)
        {
            // Validation
            if (storeId == Guid.Empty)
                return (false, "Store ID is required", Guid.Empty);

            if (items == null || items.Count == 0)
                return (false, "At least one item is required", Guid.Empty);

            // Validate each item
            foreach (var item in items)
            {
                if (item.ItemId == Guid.Empty || item.Quantity <= 0 || item.UnitPrice <= 0)
                    return (false, "All items must have: ItemId, Quantity > 0, UnitPrice > 0", Guid.Empty);
            }

            using (var connection = DatabaseService.GetConnection())
            {
                await connection.OpenAsync();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Calculate totals
                        decimal subtotal = 0;
                        decimal totalDiscount = 0;

                        var validatedItems = new List<ValidatedLineItem>();

                        foreach (var item in items)
                        {
                            var discountPercent = item.DiscountPercent ?? 0;
                            var lineTotal = item.UnitPrice * item.Quantity;
                            var discountValue = item.DiscountValue ?? ((lineTotal * discountPercent) / 100);
                            var netValue = lineTotal - discountValue;

                            subtotal += lineTotal;
                            totalDiscount += discountValue;

                            validatedItems.Add(new ValidatedLineItem
                            {
                                ItemId = item.ItemId,
                                Quantity = item.Quantity,
                                UnitPrice = item.UnitPrice,
                                DiscountPercent = discountPercent,
                                DiscountValue = discountValue,
                                NetValue = netValue,
                                BatchNo = item.BatchNo
                            });
                        }

                        // Get store code for invoice number
                        const string getStoreQuery = "SELECT code FROM stores WHERE id = @StoreId";
                        var storeCode = await connection.QueryFirstOrDefaultAsync<string>(
                            getStoreQuery,
                            new { StoreId = storeId },
                            transaction);

                        if (string.IsNullOrEmpty(storeCode))
                            return (false, "Store not found", Guid.Empty);

                        // Generate invoice number
                        var invoiceNumber = await GenerateInvoiceNumberAsync(storeCode, connection, transaction);

                        decimal totalTax = 0;
                        decimal totalAmount = subtotal - totalDiscount + totalTax;
                        var saleId = Guid.NewGuid();

                        // Insert sales_retail record
                        const string insertSaleQuery = @"
                            INSERT INTO sales_retail 
                            (id, invoice_number, invoice_date, sale_date, customer_id, store_id, employee_id,
                             payment_method, payment_status, subtotal, discount, tax, total_amount, description, is_active, created_at)
                            VALUES 
                            (@Id, @InvoiceNumber, @InvoiceDate, @SaleDate, @CustomerId, @StoreId, @EmployeeId,
                             @PaymentMethod, @PaymentStatus, @Subtotal, @Discount, @Tax, @TotalAmount, @Description, @IsActive, @CreatedAt)";

                        await connection.ExecuteAsync(insertSaleQuery, new
                        {
                            Id = saleId,
                            InvoiceNumber = invoiceNumber,
                            InvoiceDate = DateTime.Now,
                            SaleDate = DateTime.Now.Date,
                            CustomerId = customerId,
                            StoreId = storeId,
                            EmployeeId = employeeId,
                            PaymentMethod = paymentMethod ?? "cash",
                            PaymentStatus = "paid",
                            Subtotal = subtotal,
                            Discount = totalDiscount,
                            Tax = totalTax,
                            TotalAmount = totalAmount,
                            Description = description,
                            IsActive = true,
                            CreatedAt = DateTime.Now
                        }, transaction);

                        // Insert sales_retail_items
                        const string insertItemsQuery = @"
                            INSERT INTO sales_retail_items
                            (id, sales_retail_id, item_id, batch_no, quantity, unit_price, discount_percent, discount_value, tax_value, net_value, created_at)
                            VALUES
                            (@Id, @SalesRetailId, @ItemId, @BatchNo, @Quantity, @UnitPrice, @DiscountPercent, @DiscountValue, @TaxValue, @NetValue, @CreatedAt)";

                        foreach (var item in validatedItems)
                        {
                            await connection.ExecuteAsync(insertItemsQuery, new
                            {
                                Id = Guid.NewGuid(),
                                SalesRetailId = saleId,
                                ItemId = item.ItemId,
                                BatchNo = item.BatchNo,
                                Quantity = item.Quantity,
                                UnitPrice = item.UnitPrice,
                                DiscountPercent = item.DiscountPercent,
                                DiscountValue = item.DiscountValue,
                                TaxValue = 0m,
                                NetValue = item.NetValue,
                                CreatedAt = DateTime.Now
                            }, transaction);
                        }

                        // ✅ CRITICAL: Deduct stock and create inventory transactions
                        foreach (var item in validatedItems)
                        {
                            const string getStockQuery = @"
                                SELECT * FROM item_store_stock
                                WHERE item_id = @ItemId AND store_id = @StoreId";

                            var stockRecord = await connection.QueryFirstOrDefaultAsync<dynamic>(
                                getStockQuery,
                                new { ItemId = item.ItemId, StoreId = storeId },
                                transaction);

                            if (stockRecord != null)
                            {
                                var newQuantity = Math.Max(0, stockRecord.quantity_on_hand - item.Quantity);

                                // Update stock
                                const string updateStockQuery = @"
                                    UPDATE item_store_stock
                                    SET quantity_on_hand = @NewQuantity, updated_at = @UpdatedAt
                                    WHERE id = @Id";

                                await connection.ExecuteAsync(updateStockQuery, new
                                {
                                    NewQuantity = newQuantity,
                                    UpdatedAt = DateTime.Now,
                                    Id = stockRecord.id
                                }, transaction);

                                // Create inventory transaction
                                const string insertTransactionQuery = @"
                                    INSERT INTO inventory_transactions
                                    (id, item_id, store_id, transaction_type, quantity, batch_no, reference_id, reference_type, notes, created_by, created_at)
                                    VALUES
                                    (@Id, @ItemId, @StoreId, @TransactionType, @Quantity, @BatchNo, @ReferenceId, @ReferenceType, @Notes, @CreatedBy, @CreatedAt)";

                                await connection.ExecuteAsync(insertTransactionQuery, new
                                {
                                    Id = Guid.NewGuid(),
                                    ItemId = item.ItemId,
                                    StoreId = storeId,
                                    TransactionType = "sale",
                                    Quantity = -item.Quantity,
                                    BatchNo = item.BatchNo,
                                    ReferenceId = saleId,
                                    ReferenceType = "sales_retail",
                                    Notes = $"Sale: {invoiceNumber}",
                                    CreatedBy = employeeId,
                                    CreatedAt = DateTime.Now
                                }, transaction);

                                System.Diagnostics.Debug.WriteLine(
                                    $"✅ Stock deducted: Item {item.ItemId}, Qty {item.Quantity}, New: {newQuantity}");
                            }
                        }

                        // Log audit
                        const string insertAuditQuery = @"
                            INSERT INTO audit_logs
                            (id, user_id, action, table_name, record_id, new_values, created_at)
                            VALUES
                            (@Id, @UserId, @Action, @TableName, @RecordId, @NewValues, @CreatedAt)";

                        await connection.ExecuteAsync(insertAuditQuery, new
                        {
                            Id = Guid.NewGuid(),
                            UserId = employeeId,
                            Action = "CREATE",
                            TableName = "sales_retail",
                            RecordId = saleId,
                            NewValues = $"{{invoice_number:{invoiceNumber},store_id:{storeId},total_amount:{totalAmount},items_count:{validatedItems.Count}}}",
                            CreatedAt = DateTime.Now
                        }, transaction);

                        transaction.Commit();
                        return (true, "Sale created successfully", saleId);
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        System.Diagnostics.Debug.WriteLine($"Error creating sale: {ex.Message}");
                        return (false, ex.Message, Guid.Empty);
                    }
                }
            }
        }

        // ==================== HELPER METHODS ====================

        private async Task<string> GenerateInvoiceNumberAsync(
            string storeCode,
            NpgsqlConnection connection,
            NpgsqlTransaction transaction)
        {
            const string query = @"
                SELECT invoice_number FROM sales_retail
                WHERE invoice_number LIKE @Pattern
                ORDER BY created_at DESC
                LIMIT 1";

            var pattern = $"{storeCode}-SINV-%";
            var lastInvoice = await connection.QueryFirstOrDefaultAsync<string>(
                query,
                new { Pattern = pattern },
                transaction);

            if (string.IsNullOrEmpty(lastInvoice))
                return $"{storeCode}-SINV-000001";

            var lastNum = int.Parse(lastInvoice.Split('-').Last());
            return $"{storeCode}-SINV-{(lastNum + 1).ToString("D6")}";
        }

        // ==================== DTOs ====================

        public class RetailSaleItemDto
        {
            public Guid ItemId { get; set; }
            public decimal Quantity { get; set; }
            public decimal UnitPrice { get; set; }
            public decimal? DiscountPercent { get; set; }
            public decimal? DiscountValue { get; set; }
            public string? BatchNo { get; set; }
        }

        private class ValidatedLineItem
        {
            public Guid ItemId { get; set; }
            public decimal Quantity { get; set; }
            public decimal UnitPrice { get; set; }
            public decimal DiscountPercent { get; set; }
            public decimal DiscountValue { get; set; }
            public decimal NetValue { get; set; }
            public string? BatchNo { get; set; }
        }
    }
}