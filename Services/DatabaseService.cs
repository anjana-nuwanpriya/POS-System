using Npgsql;
using Dapper;
using HardwareShopPOS.Helpers;

namespace HardwareShopPOS.Services
{
    public static class DatabaseService
    {
        public static NpgsqlConnection GetConnection() => new NpgsqlConnection(AppSettings.ConnectionString);

        public static bool TestConnection()
        {
            try
            {
                using var conn = GetConnection();
                conn.Open();
                return true;
            }
            catch { return false; }
        }

        public static async Task<IEnumerable<T>> QueryAsync<T>(string sql, object? param = null)
        {
            using var conn = GetConnection();
            return await conn.QueryAsync<T>(sql, param);
        }

        public static async Task<T?> QueryFirstOrDefaultAsync<T>(string sql, object? param = null)
        {
            using var conn = GetConnection();
            return await conn.QueryFirstOrDefaultAsync<T>(sql, param);
        }

        public static async Task<int> ExecuteAsync(string sql, object? param = null)
        {
            using var conn = GetConnection();
            return await conn.ExecuteAsync(sql, param);
        }

        public static async Task<T?> ExecuteScalarAsync<T>(string sql, object? param = null)
        {
            using var conn = GetConnection();
            return await conn.ExecuteScalarAsync<T>(sql, param);
        }

        public static IEnumerable<T> Query<T>(string sql, object? param = null)
        {
            using var conn = GetConnection();
            return conn.Query<T>(sql, param);
        }

        public static T? QueryFirstOrDefault<T>(string sql, object? param = null)
        {
            using var conn = GetConnection();
            return conn.QueryFirstOrDefault<T>(sql, param);
        }

        public static int Execute(string sql, object? param = null)
        {
            using var conn = GetConnection();
            return conn.Execute(sql, param);
        }

        public static async Task<bool> ExecuteTransactionAsync(Func<NpgsqlConnection, NpgsqlTransaction, Task<bool>> action)
        {
            using var conn = GetConnection();
            await conn.OpenAsync();
            using var transaction = await conn.BeginTransactionAsync();

            try
            {
                bool result = await action(conn, transaction);
                if (result)
                {
                    await transaction.CommitAsync();
                    return true;
                }
                await transaction.RollbackAsync();
                return false;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public static async Task LogAuditAsync(string action, string? tableName = null, Guid? recordId = null,
            object? oldValues = null, object? newValues = null)
        {
            try
            {
                var sql = @"INSERT INTO audit_logs (user_id, action, table_name, record_id, old_values, new_values)
                           VALUES (@UserId, @Action, @TableName, @RecordId, @OldValues::jsonb, @NewValues::jsonb)";

                await ExecuteAsync(sql, new
                {
                    UserId = AppSettings.CurrentUserId,
                    Action = action,
                    TableName = tableName,
                    RecordId = recordId,
                    OldValues = oldValues != null ? Newtonsoft.Json.JsonConvert.SerializeObject(oldValues) : null,
                    NewValues = newValues != null ? Newtonsoft.Json.JsonConvert.SerializeObject(newValues) : null
                });
            }
            catch { }
        }
    }
}
