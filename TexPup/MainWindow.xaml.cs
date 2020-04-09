using Semver;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace TexPup
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public bool Busy
        {
            get { return (bool)GetValue(BusyProperty); }
            set { SetValue(BusyProperty, value); }
        }

        public static readonly DependencyProperty BusyProperty =
            DependencyProperty.Register("Busy", typeof(bool), typeof(MainWindow));

        private UnpackGame[] Games { get; }

        private IProgress<ProgressReport> Progress { get; }

        private IProgress<string> ErrorsProgress { get; }

        private CancellationTokenSource CancelSource { get; set; }

        public MainWindow()
        {
            Progress = new Progress<ProgressReport>(UpdateProgress);
            ErrorsProgress = new Progress<string>(UpdateErrors);
            InitializeComponent();
            Title = $"TexPup {System.Windows.Forms.Application.ProductVersion}";

            Games = new UnpackGame[]
            {
                LoadGame(GameType.DarkSouls3, "DarkSouls3Config.xml", Properties.Settings.Default.DarkSouls3Settings),
                LoadGame(GameType.Sekiro, "SekiroConfig.xml", Properties.Settings.Default.SekiroSettings),
            };
            GameComboBox.ItemsSource = Games;
        }

        private static UnpackGame LoadGame(GameType type, string configName, string settingsStr)
        {
            var config = GameConfig.XmlDeserialize(File.ReadAllText($@"res\{configName}"));
            var settings = GameSettings.JsonDeserialize(settingsStr);
            return new UnpackGame(type, config, settings);
        }

        private async void Window_Initialized(object sender, EventArgs e)
        {
            var gitHubClient = new Octokit.GitHubClient(new Octokit.ProductHeaderValue("TexPup"));
            try
            {
                Octokit.Release release = await gitHubClient.Repository.Release.GetLatest("JKAnderson", "TexPup");
                if (SemVersion.Parse(release.TagName) > System.Windows.Forms.Application.ProductVersion)
                {
                    UpdateHyperlink.NavigateUri = new Uri(release.HtmlUrl);
                    UpdateStatusBarItem.Visibility = Visibility.Visible;
                }
            }
            catch { }
        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        private void UpdateProgress(ProgressReport report)
        {
            ProgressBar.Value = report.Value * ProgressBar.Maximum;
            ProgressLabel.Content = report.Status;
        }

        private void UpdateErrors(string text)
        {
            if (ErrorsTextBox.Text.Length > 0)
                ErrorsTextBox.AppendText("\n\n");
            ErrorsTextBox.AppendText(text);
            if (!ErrorsTextBox.IsFocused)
                ErrorsTextBox.ScrollToEnd();
        }

        private void GameComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var game = (UnpackGame)e.AddedItems[0];

            FiltersPanel.Children.Clear();
            foreach (UnpackFilter filter in game.Config.Filters)
            {
                CheckBox cbx = MakeFilterCheckBox(filter);
                FiltersPanel.Children.Add(cbx);
            }

            MapFiltersPanel.Children.Clear();
            foreach (UnpackFilter filter in game.Config.MapFilters)
            {
                CheckBox cbx = MakeFilterCheckBox(filter);
                MapFiltersPanel.Children.Add(cbx);
            }
        }

        private static CheckBox MakeFilterCheckBox(UnpackFilter filter)
        {
            var cbx = new CheckBox { DataContext = filter };
            cbx.SetBinding(CheckBox.IsCheckedProperty, "Enabled");
            cbx.SetBinding(CheckBox.ContentProperty, "Name");
            return cbx;
        }

        private async void UnpackButton_Click(object sender, RoutedEventArgs e)
        {
            Busy = true;
            CancelSource = new CancellationTokenSource();
            using (var unpacker = new TexUnpacker((UnpackGame)DataContext))
            {
                await Task.Run(() => unpacker.Unpack(Progress, ErrorsProgress, CancelSource.Token));
            }
            CancelSource = null;
            Busy = false;
        }

        private async void PackButton_Click(object sender, RoutedEventArgs e)
        {
            Busy = true;
            CancelSource = new CancellationTokenSource();
            using (var packer = new TexPacker((UnpackGame)DataContext))
            {
                await Task.Run(() => packer.Pack(Progress, ErrorsProgress, CancelSource.Token));
            }
            CancelSource = null;
            Busy = false;
        }

        private void AbortButton_Click(object sender, RoutedEventArgs e)
        {
            CancelSource.Cancel();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Properties.Settings.Default.DarkSouls3Settings
                = Games.First(g => g.Type == GameType.DarkSouls3).Settings.JsonSerialize();
            Properties.Settings.Default.SekiroSettings
                = Games.First(g => g.Type == GameType.Sekiro).Settings.JsonSerialize();
        }
    }
}
