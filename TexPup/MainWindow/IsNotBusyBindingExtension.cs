using System.Windows.Data;

namespace TexPup
{
    public class IsNotBusyBindingExtension : Binding
    {
        public IsNotBusyBindingExtension()
        {
            Initialize();
        }

        public IsNotBusyBindingExtension(string path) : base(path)
        {
            Initialize();
        }

        private void Initialize()
        {
            RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(MainWindow), 1);
            Path = new System.Windows.PropertyPath("Busy");
            Converter = new BoolNotConverter();
            Mode = BindingMode.OneWay;
        }
    }
}
