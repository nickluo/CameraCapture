using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CameraCapture.Interface;
using Microsoft.Practices.Prism.MefExtensions;
using Microsoft.Practices.Prism.Mvvm;
using Microsoft.Practices.ServiceLocation;

namespace ControlModule.Views
{
    /// <summary>
    /// Interaction logic for ConsoleControl.xaml
    /// </summary>
    [Export]
    public partial class ConsoleControl : UserControl
    {
        [ImportingConstructor]
        public ConsoleControl([Import("Console")] BindableBase myBase)
        {
            DataContext = myBase;
            InitializeComponent();
        }
        
    }
}
