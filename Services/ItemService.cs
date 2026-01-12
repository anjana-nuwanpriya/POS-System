using Dapper;
using HardwareShopPOS.Helpers;
using HardwareShopPOS.Models;

namespace HardwareShopPOS.Services
{
    public static class ItemService
    {
        public static async Task<IEnumerable<Item>> GetItemsAsync(string? search = null, Guid? categoryId = null)
        {
            var sql = @"SELECT i.*, c.name as category_name,
                       COALESCE(s.quantity_on_hand, 0) as stock_quantity
                       FROM items i
                       LEFT JOIN categories c ON i.category_id = c.id
                       LEFT JOIN item_store_stock s ON i.id = s.item_id AND s.store_id = @StoreId
                       WHERE i.is_active = true";

            if (!string.IsNullOrEmpty(search))
                sql += @" AND (LOWER(i.code) LIKE LOWER(@Search) 
                         OR LOWER(i.name) LIKE LOWER(@Search)
                         OR LOWER(i.barcode) LIKE LOWER(@Search))";

            if (categoryId.HasValue)
                sql += " AND i.category_id = @CategoryId";

            sql += " ORDER BY i.name";

            return await DatabaseService.QueryAsync<Item>(sql, new
            {
                StoreId = AppSettings.CurrentStoreId,
                Search = $"%{search}%",
                CategoryId = categoryId
            });
        }

        public static async Task<Item?> GetItemByIdAsync(Guid id)
        {
            var sql = @"SELECT i.*, c.name as category_name,
                       COALESCE(s.quantity_on_hand, 0) as stock_quantity
                       FROM items i
                       LEFT JOIN categories c ON i.category_id = c.id
                       LEFT JOIN item_store_stock s ON i.id = s.item_id AND s.store_id = @StoreId
                       WHERE i.id = @Id";

            return await DatabaseService.QueryFirstOrDefaultAsync<Item>(sql, new { Id = id, StoreId = AppSettings.CurrentStoreId });
        }

        public static async Task<Item?> GetItemByCodeAsync(string code)
        {
            var sql = @"SELECT i.*, c.name as category_name,
                       COALESCE(s.quantity_on_hand, 0) as stock_quantity
                       FROM items i
                       LEFT JOIN categories c ON i.category_id = c.id
                       LEFT JOIN item_store_stock s ON i.id = s.item_id AND s.store_id = @StoreId
                       WHERE (LOWER(i.code) = LOWER(@Code) OR LOWER(i.barcode) = LOWER(@Code))
                       AND i.is_active = true";

            return await DatabaseService.QueryFirstOrDefaultAsync<Item>(sql, new { Code = code, StoreId = AppSettings.CurrentStoreId });
        }

        public static async Task<IEnumerable<Item>> SearchItemsAsync(string query, int limit = 20)
        {
            var sql = @"SELECT i.*, c.name as category_name,
                       COALESCE(s.quantity_on_hand, 0) as stock_quantity
                       FROM items i
                       LEFT JOIN categories c ON i.category_id = c.id
                       LEFT JOIN item_store_stock s ON i.id = s.item_id AND s.store_id = @StoreId
                       WHERE i.is_active = true
                       AND (LOWER(i.code) LIKE LOWER(@Query) 
                            OR LOWER(i.name) LIKE LOWER(@Query)
                            OR LOWER(i.barcode) LIKE LOWER(@Query))
                       ORDER BY i.name
                       LIMIT @Limit";

            return await DatabaseService.QueryAsync<Item>(sql, new
            {
                Query = $"%{query}%",
                StoreId = AppSettings.CurrentStoreId,
                Limit = limit
            });
        }

        public static async Task<IEnumerable<Category>> GetCategoriesAsync()
        {
            return await DatabaseService.QueryAsync<Category>("SELECT * FROM categories WHERE is_active = true ORDER BY name");
        }

        public static async Task<int> GetLowStockCountAsync()
        {
            var sql = @"SELECT COUNT(*) FROM items i
                       LEFT JOIN item_store_stock s ON i.id = s.item_id AND s.store_id = @StoreId
                       WHERE i.is_active = true
                       AND COALESCE(s.quantity_on_hand, 0) <= i.reorder_level";

            return await DatabaseService.ExecuteScalarAsync<int>(sql, new { StoreId = AppSettings.CurrentStoreId });
        }

        public static async Task<Guid> CreateItemAsync(Item item)
        {
            var sql = @"INSERT INTO items (code, name, description, category_id, cost_price, retail_price, 
                       wholesale_price, unit_of_measure, barcode, reorder_level, tax_rate)
                       VALUES (@Code, @Name, @Description, @CategoryId, @CostPrice, @RetailPrice,
                       @WholesalePrice, @UnitOfMeasure, @Barcode, @ReorderLevel, @TaxRate)
                       RETURNING id";

            var id = await DatabaseService.ExecuteScalarAsync<Guid>(sql, item);
            await DatabaseService.LogAuditAsync("CREATE", "items", id, null, item);
            return id;
        }

        public static async Task<bool> UpdateItemAsync(Item item)
        {
            var sql = @"UPDATE items SET 
                       code = @Code, name = @Name, description = @Description, 
                       category_id = @CategoryId, cost_price = @CostPrice, 
                       retail_price = @RetailPrice, wholesale_price = @WholesalePrice,
                       unit_of_measure = @UnitOfMeasure, barcode = @Barcode, 
                       reorder_level = @ReorderLevel, tax_rate = @TaxRate,
                       updated_at = CURRENT_TIMESTAMP
                       WHERE id = @Id";

            var result = await DatabaseService.ExecuteAsync(sql, item);
            if (result > 0)
            {
                await DatabaseService.LogAuditAsync("UPDATE", "items", item.Id);
                return true;
            }
            return false;
        }

        public static async Task<bool> DeleteItemAsync(Guid id)
        {
            var sql = "UPDATE items SET is_active = false, updated_at = CURRENT_TIMESTAMP WHERE id = @Id";
            var result = await DatabaseService.ExecuteAsync(sql, new { Id = id });
            if (result > 0)
            {
                await DatabaseService.LogAuditAsync("DELETE", "items", id);
                return true;
            }
            return false;
        }
    }
}
