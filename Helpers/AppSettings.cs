using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace HardwareShopPOS.Helpers
{
    public static class AppSettings
    {
        private static readonly string SettingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");

        // Database settings
        public static string DbHost { get; set; } = "localhost";
        public static int DbPort { get; set; } = 5432;
        public static string DbName { get; set; } = "hardwareshop";
        public static string DbUser { get; set; } = "postgres";
        public static string DbPassword { get; set; } = "";

        // Application settings
        public static string StoreName { get; set; } = "Hardware Shop";
        public static string Currency { get; set; } = "Rs.";
        public static string InvoicePrefix { get; set; } = "INV";
        public static int ReceiptWidth { get; set; } = 48;

        // Printer settings
        public static bool PrinterEnabled { get; set; } = false;
        public static string PrinterName { get; set; } = "";
        public static bool AutoPrint { get; set; } = true;

        // Current session
        public static Guid? CurrentUserId { get; set; }
        public static string? CurrentUserName { get; set; }
        public static string? CurrentUserRole { get; set; }
        public static Guid? CurrentStoreId { get; set; }
        public static string? CurrentStoreName { get; set; }

        public static string ConnectionString =>
            $"Host={DbHost};Port={DbPort};Database={DbName};Username={DbUser};Password={DbPassword};Timeout=30;Pooling=true;";

        public static void Load()
        {
            try
            {
                if (File.Exists(SettingsPath))
                {
                    string json = File.ReadAllText(SettingsPath);
                    var settings = JObject.Parse(json);

                    DbHost = settings["Database"]?["Host"]?.ToString() ?? DbHost;
                    DbPort = settings["Database"]?["Port"]?.Value<int>() ?? DbPort;
                    DbName = settings["Database"]?["Database"]?.ToString() ?? DbName;
                    DbUser = settings["Database"]?["Username"]?.ToString() ?? DbUser;
                    DbPassword = settings["Database"]?["Password"]?.ToString() ?? DbPassword;

                    StoreName = settings["Application"]?["StoreName"]?.ToString() ?? StoreName;
                    Currency = settings["Application"]?["Currency"]?.ToString() ?? Currency;
                    InvoicePrefix = settings["Application"]?["InvoicePrefix"]?.ToString() ?? InvoicePrefix;
                    ReceiptWidth = settings["Application"]?["ReceiptWidth"]?.Value<int>() ?? ReceiptWidth;

                    PrinterEnabled = settings["Printer"]?["Enabled"]?.Value<bool>() ?? PrinterEnabled;
                    PrinterName = settings["Printer"]?["PrinterName"]?.ToString() ?? PrinterName;
                    AutoPrint = settings["Printer"]?["AutoPrint"]?.Value<bool>() ?? AutoPrint;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading settings: {ex.Message}");
            }
        }

        public static void Save()
        {
            try
            {
                var settings = new
                {
                    Database = new { Host = DbHost, Port = DbPort, Database = DbName, Username = DbUser, Password = DbPassword },
                    Application = new { StoreName, Currency, InvoicePrefix, ReceiptWidth },
                    Printer = new { Enabled = PrinterEnabled, PrinterName, AutoPrint }
                };

                string json = JsonConvert.SerializeObject(settings, Formatting.Indented);
                File.WriteAllText(SettingsPath, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving settings: {ex.Message}");
            }
        }

        public static string FormatCurrency(decimal amount) => $"{Currency}{amount:N2}";
    }
}
