using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Windows;

namespace RazorSpy
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static CompositionContainer _container;

        public static CompositionContainer Container
        {
            get { return _container; }
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            //RxApp.GetFieldNameForPropertyNameFunc = p => "_" + Char.ToLower(p[0]) + p.Substring(1);

            AssemblyCatalog thisAsm = new AssemblyCatalog(typeof(App).Assembly);
            AggregateCatalog catalog = new AggregateCatalog();
            catalog.Catalogs.Add(thisAsm);
            if (Directory.Exists("Packages"))
            {
                foreach (string dir in Directory.GetDirectories("Packages"))
                {
                    catalog.Catalogs.Add(new DirectoryCatalog(dir));
                }
            }
            _container = new CompositionContainer(catalog);
            _container.Compose(new CompositionBatch());
        }
    }
}
