using System.ComponentModel.Composition.Hosting;
using System.Windows;
using Microsoft.Practices.Prism.MefExtensions;
using Microsoft.Practices.Prism.Modularity;
using Microsoft.Practices.ServiceLocation;

namespace CameraCapture
{
    class Bootstrapper: MefBootstrapper
    {
        protected override DependencyObject CreateShell()
        {
            return ServiceLocator.Current.GetInstance<Shell>();
        }

        protected override void InitializeShell()
        {
            Application.Current.MainWindow = Shell as Window;
            Application.Current.MainWindow?.Show();
        }

        protected override IModuleCatalog CreateModuleCatalog()
        {
            return new ConfigurationModuleCatalog();
        }

        protected override void ConfigureModuleCatalog()
        {
            base.ConfigureModuleCatalog();

            var moduleCatalog = ModuleCatalog as ModuleCatalog;
            moduleCatalog?.AddModule(typeof(VideoModule.VideoModule));
            moduleCatalog?.AddModule(typeof(ControlModule.ControlModule));
        }

        protected override void ConfigureAggregateCatalog()
        {
            base.ConfigureAggregateCatalog();
            AggregateCatalog.Catalogs.Add(new AssemblyCatalog(typeof(Shell).Assembly));
            AggregateCatalog.Catalogs.Add(new AssemblyCatalog(typeof(VideoModule.MonikerProvider).Assembly));
            AggregateCatalog.Catalogs.Add(new AssemblyCatalog(typeof(ControlModule.ControlModule).Assembly));
        }
    }
}
