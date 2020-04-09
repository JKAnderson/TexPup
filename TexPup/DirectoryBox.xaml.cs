using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace TexPup
{
    /// <summary>
    /// Interaction logic for DirectoryBox.xaml
    /// </summary>
    public partial class DirectoryBox : UserControl
    {
        public static readonly DependencyProperty DescriptionProperty
            = DependencyProperty.Register(nameof(Description), typeof(string), typeof(DirectoryBox));

        public string Description
        {
            get => (string)GetValue(DescriptionProperty);
            set => SetValue(DescriptionProperty, value);
        }

        public static readonly DependencyProperty IsBrowsingEnabledProperty
            = DependencyProperty.Register(nameof(IsBrowsingEnabled), typeof(bool), typeof(DirectoryBox));

        public bool IsBrowsingEnabled
        {
            get => (bool)GetValue(IsBrowsingEnabledProperty);
            set => SetValue(IsBrowsingEnabledProperty, value);
        }

        public static readonly DependencyProperty PathProperty
           = DependencyProperty.Register(nameof(Path), typeof(string), typeof(DirectoryBox));

        public string Path
        {
            get => (string)GetValue(PathProperty);
            set => SetValue(PathProperty, value);
        }

        public static readonly DependencyProperty ShowNewFolderButtonProperty
            = DependencyProperty.Register(nameof(ShowNewFolderButton), typeof(bool), typeof(DirectoryBox));

        public bool ShowNewFolderButton
        {
            get => (bool)GetValue(ShowNewFolderButtonProperty);
            set => SetValue(ShowNewFolderButtonProperty, value);
        }

        public static readonly DependencyProperty TitleProperty
            = DependencyProperty.Register(nameof(Title), typeof(string), typeof(DirectoryBox));

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public DirectoryBox()
        {
            InitializeComponent();
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            var browser = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog
            {
                Description = Description,
                ShowNewFolderButton = ShowNewFolderButton,
                RootFolder = Environment.SpecialFolder.MyComputer,
                SelectedPath = Path,
                UseDescriptionForTitle = true,
            };

            if (browser.ShowDialog() == true)
            {
                Path = browser.SelectedPath;
            }
        }

        private void ExploreButton_Click(object sender, RoutedEventArgs e)
        {
            if (Directory.Exists(Path))
            {
                Process.Start("explorer", $"\"{Path}\"");
            }
            else
            {
                MessageBox.Show($"Directory not found:\n\"{Path}\"", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
