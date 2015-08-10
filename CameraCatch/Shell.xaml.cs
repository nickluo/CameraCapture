using System.ComponentModel.Composition;

namespace CameraCapture
{
    /// <summary>
    /// Interaction logic for Shell.xaml
    /// </summary>
    [Export]
    public partial class Shell
    {
        [ImportingConstructor]
        public Shell(ShellViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;
        }
    }
}
