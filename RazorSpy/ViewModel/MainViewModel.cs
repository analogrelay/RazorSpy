using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Threading;
using RazorSpy.Contracts;
using RazorSpy.Contracts.SyntaxTree;

namespace RazorSpy.ViewModel
{
    [Export]
    public class MainViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<Lazy<IRazorEngine, IRazorEngineMetadata>> _engines;
        private Lazy<IRazorEngine, IRazorEngineMetadata> _selectedEngine;
        private RazorLanguage _selectedLanguage;
        private bool _designTimeMode;
        private string _razorCode;

        private IEnumerable<Block> _generatedTree;
        private string _generatedCode;
        private string _status;

        public event PropertyChangedEventHandler PropertyChanged;

        private readonly DispatcherTimer _timer;

        public MainViewModel()
        {
            PropertyChanged += MainViewModel_PropertyChanged;

            _timer = new DispatcherTimer(DispatcherPriority.Background);
            _timer.Tick += Regenerate;
            _timer.Interval = TimeSpan.FromSeconds(.1);
        }

        private bool RaiseAndSetIfChanged<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (!object.Equals(field, value))
            {
                field = value;
                Notify(propertyName);
                return true;
            }

            return false;
        }

        private void Notify(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public string Status
        {
            get { return _status; }
            set { RaiseAndSetIfChanged(ref _status, value); }
        }

        public string RazorCode
        {
            get { return _razorCode; }
            set { RaiseAndSetIfChanged(ref _razorCode, value); }
        }

        [ImportMany]
        public ObservableCollection<Lazy<IRazorEngine, IRazorEngineMetadata>> Engines
        {
            get { return _engines; }
            set
            {
                _engines = value;
                _engines.CollectionChanged += (sender, args) => Initialize();
            }
        }

        public Lazy<IRazorEngine, IRazorEngineMetadata> SelectedEngine
        {
            get { return _selectedEngine; }
            set
            {
                if (RaiseAndSetIfChanged(ref _selectedEngine, value))
                {
                    EnsureLanguage();

                    Notify("Languages");
                    Notify("SelectedLanguage");
                }
            }
        }

        public RazorLanguage SelectedLanguage
        {
            get { return _selectedLanguage; }
            set { RaiseAndSetIfChanged(ref _selectedLanguage, value); }
        }

        public bool DesignTimeMode
        {
            get { return _designTimeMode; }
            set { RaiseAndSetIfChanged(ref _designTimeMode, value); }
        }

        public IEnumerable<Block> GeneratedTree
        {
            get { return _generatedTree; }
            private set { RaiseAndSetIfChanged(ref _generatedTree, value); }
        }

        public string GeneratedCode
        {
            get { return _generatedCode; }
            private set { RaiseAndSetIfChanged(ref _generatedCode, value); }
        }

        public IEnumerable<RazorLanguage> Languages
        {
            get
            {
                return SelectedEngine != null ? SelectedEngine.Value.Languages : null;
            }
        }

        private void MainViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "DesignTimeMode":
                case "SelectedEngine":
                case "SelectedLanguage":
                case "RazorCode":
                    _timer.Stop();
                    _timer.Start();
                    break;
            }
        }

        private void EnsureLanguage()
        {
            RazorLanguage selected = SelectedLanguage;

            if (selected == null)
            {
                SelectedLanguage = Languages.FirstOrDefault();
            }
            else if (!Languages.Contains(selected))
            {
                SelectedLanguage = Languages.FirstOrDefault(l => l.Id == selected.Id) ?? Languages.FirstOrDefault();
            }
        }

        private void Initialize()
        {
            _selectedEngine = Engines.FirstOrDefault();

            if (_selectedEngine != null)
            {
                EnsureLanguage();
            }
        }

        private void Regenerate(object source, EventArgs args)
        {
            _timer.Stop();

            if (SelectedEngine != null && !String.IsNullOrEmpty(RazorCode))
            {
                EnsureLanguage();

                Status = "Compiling...";
                // Configure the host
                IRazorEngine engine = SelectedEngine.Value;
                ITemplateHost host = engine.CreateHost();
                host.Language = SelectedLanguage;
                host.DesignTimeMode = DesignTimeMode;

                // Generate the template
                GenerationResult result;
                using (TextReader reader = new StringReader(RazorCode))
                {
                    result = SelectedEngine.Value.Generate(reader, host);
                }
                if (result != null)
                {
                    GeneratedCode = result.Code.GenerateString(SelectedLanguage.CreateCodeDomProvider());
                    GeneratedTree = new[] { result.Document };
                    Status = result.Success ? "Success" : "Errors during compilation";
                    return;
                }
            }
            GeneratedCode = String.Empty;
            GeneratedTree = null;
        }
    }
}
