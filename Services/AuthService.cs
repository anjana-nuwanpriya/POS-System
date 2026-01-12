using HardwareShopPOS.Helpers;
using HardwareShopPOS.Models;

namespace HardwareShopPOS.Services
{
    public static class AuthService
    {
        public static async Task<Employee?> LoginAsync(string employeeCode)
        {
            try
            {
                var sql = @"SELECT e.*, s.name as store_name 
                           FROM employees e 
                           LEFT JOIN stores s ON e.store_id = s.id
                           WHERE LOWER(e.employee_code) = LOWER(@Code) 
                           AND e.is_active = true";

                var employee = await DatabaseService.QueryFirstOrDefaultAsync<Employee>(sql, new { Code = employeeCode });

                if (employee != null)
                {
                    await DatabaseService.ExecuteAsync(
                        "UPDATE employees SET last_login_at = CURRENT_TIMESTAMP WHERE id = @Id",
                        new { employee.Id });

                    AppSettings.CurrentUserId = employee.Id;
                    AppSettings.CurrentUserName = employee.Name;
                    AppSettings.CurrentUserRole = employee.Role;
                    AppSettings.CurrentStoreId = employee.StoreId;
                    AppSettings.CurrentStoreName = employee.StoreName;

                    await DatabaseService.LogAuditAsync("LOGIN", "employees", employee.Id);
                    return employee;
                }
                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Login error: {ex.Message}");
                return null;
            }
        }

        public static async Task LogoutAsync()
        {
            if (AppSettings.CurrentUserId.HasValue)
                await DatabaseService.LogAuditAsync("LOGOUT", "employees", AppSettings.CurrentUserId);

            AppSettings.CurrentUserId = null;
            AppSettings.CurrentUserName = null;
            AppSettings.CurrentUserRole = null;
            AppSettings.CurrentStoreId = null;
            AppSettings.CurrentStoreName = null;
        }

        public static async Task<bool> EnsureDefaultUserAsync()
        {
            try
            {
                var count = await DatabaseService.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM employees");
                if (count == 0)
                {
                    await DatabaseService.ExecuteAsync(
                        "INSERT INTO employees (name, employee_code, role, is_active) VALUES ('Administrator', 'admin', 'admin', true)");
                    return true;
                }
                return false;
            }
            catch { return false; }
        }

        public static async Task<IEnumerable<Store>> GetStoresAsync()
        {
            return await DatabaseService.QueryAsync<Store>("SELECT * FROM stores WHERE is_active = true ORDER BY name");
        }

        public static async Task SetCurrentStoreAsync(Guid storeId)
        {
            var store = await DatabaseService.QueryFirstOrDefaultAsync<Store>(
                "SELECT * FROM stores WHERE id = @Id", new { Id = storeId });

            if (store != null)
            {
                AppSettings.CurrentStoreId = store.Id;
                AppSettings.CurrentStoreName = store.Name;
            }
        }
    }
}
