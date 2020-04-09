using System.Windows.Data;

namespace TexPup
{
    public class SettingsBindingExtension : Binding
    {
        public SettingsBindingExtension()
        {
            Initialize();
        }

        public SettingsBindingExtension(string path) : base(path)
        {
            Initialize();
        }

        private void Initialize()
        {
            Source = Properties.Settings.Default;
            Mode = BindingMode.TwoWay;
        }
    }
}
