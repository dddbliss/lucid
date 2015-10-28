using Caliburn.Micro;
using Lucid.ViewModels;
using MahApps.Metro;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lucid
{
    public class AppBootstrapper : BootstrapperBase
    {
        public AppBootstrapper()
        {
            Initialize();
        }

        private CompositionContainer container;

        protected override void Configure()
        {
            container = new CompositionContainer(
            new AggregateCatalog(
                AssemblySource.Instance.Select(x => new AssemblyCatalog(x)).OfType<ComposablePartCatalog>()
                )
            );

            var batch = new CompositionBatch();

            batch.AddExportedValue<IWindowManager>(new WindowManager());
            batch.AddExportedValue<IEventAggregator>(new EventAggregator());
            batch.AddExportedValue(container);

            container.Compose(batch);
        }

        protected override object GetInstance(Type serviceType, string key)
        {
            string contract = string.IsNullOrEmpty(key) ? AttributedModelServices.GetContractName(serviceType) : key;
            var exports = container.GetExportedValues<object>(contract);

            if (exports.Any())
                return exports.First();

            throw new Exception(string.Format("Could not locate any instances of contract {0}.", contract));
        }

        protected override IEnumerable<object> GetAllInstances(Type serviceType)
        {
            return container.GetExportedValues<object>(AttributedModelServices.GetContractName(serviceType));
        }

        protected override void BuildUp(object instance)
        {
            container.SatisfyImportsOnce(instance);
        }

        protected override void OnStartup(object sender, System.Windows.StartupEventArgs e)
        {
            if (Properties.Settings.Default.UpgradeMe)
            {
                Properties.Settings.Default.Upgrade();
                Properties.Settings.Default.MS_ShopID = 1;
                Properties.Settings.Default.UpgradeMe = false;
                Properties.Settings.Default.Save();
            }

            ThemeManager.ChangeAppStyle(App.Current,
                                    ThemeManager.GetAccent(Properties.Settings.Default.UI_Accent),
                                    ThemeManager.GetAppTheme("BaseLight"));

            DisplayRootViewFor<IShell>();
        }
    }
}
