using HardwareShopPOS.Forms;
using HardwareShopPOS.Helpers;
using HardwareShopPOS.Services;

namespace HardwareShopPOS
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Load settings
            AppSettings.Load();

            // Test database connection and show appropriate form
            if (!DatabaseService.TestConnection())
            {
                using (var settingsForm = new SettingsForm(true))
                {
                    if (settingsForm.ShowDialog() != DialogResult.OK)
                    {
                        return;
                    }
                }
            }

            // Show login form
            using (var loginForm = new LoginForm())
            {
                if (loginForm.ShowDialog() == DialogResult.OK)
                {
                    Application.Run(new MainForm());
                }
            }
        }
    }
}
