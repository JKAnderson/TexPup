using System.Windows.Data;

namespace TexPup
{
    public class IsBusyBindingExtension : Binding
    {
        public IsBusyBindingExtension()
        {
            Initialize();
        }

        public IsBusyBindingExtension(string path) : base(path)
        {
            Initialize();
        }

        private void Initialize()
        {
            RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(MainWindow), 1);
            Path = new System.Windows.PropertyPath("Busy");
            Mode = BindingMode.OneWay;
        }
    }
}
