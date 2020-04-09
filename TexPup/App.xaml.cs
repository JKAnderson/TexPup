using System.Windows;

namespace TexPup
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static readonly Properties.Settings Settings = TexPup.Properties.Settings.Default;

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            if (Settings.UpgradeRequired)
            {
                Settings.Upgrade();
                Settings.UpgradeRequired = false;
            }
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            Settings.Save();
        }
    }
}
