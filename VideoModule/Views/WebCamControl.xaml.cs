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
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CameraCapture.Interface;
using Microsoft.Practices.Prism.Mvvm;

namespace VideoModule.Views
{
    /// <summary>
    /// Interaction logic for WebCamControl.xaml
    /// </summary>
    [Export]
    public partial class WebCamControl : IPartImportsSatisfiedNotification
    {
        [ImportingConstructor]
        public WebCamControl([Import("WebCam")] BindableBase myBase)
        {
            DataContext = myBase;
            InitializeComponent();
        }

        public WebCamControl()
        {
            InitializeComponent();
        }

        public void OnImportsSatisfied()
        {
            var display = DataContext as IDisplayAdapter;
            if (display != null)
            {
                var windowHelper = new WindowInteropHelper(Application.Current.MainWindow);
                display.InitDisplay(DisplayPanel.Handle, windowHelper.Handle);
                var src = HwndSource.FromHwnd(windowHelper.Handle);
                src?.AddHook(display.WndProc);
            }
        }

        private void DisplayPanel_SizeChanged(object sender, EventArgs e)
        {
            if (DisplayPanel.Width == 0 || DisplayPanel.Height == 0)
                return;
            var display = DataContext as IDisplayAdapter;
            display?.OnResize();
        }
    }
}
