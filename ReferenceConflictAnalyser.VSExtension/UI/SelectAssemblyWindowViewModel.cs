
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using ReferenceConflictAnalyser.VSExtension.UI.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;

namespace ReferenceConflictAnalyser.VSExtension.UI
{
    public class SelectAssemblyWindowViewModel : INotifyPropertyChanged
    {

        public SelectAssemblyWindowViewModel(ToolWindowPane window)
        {
            SelectAssemblyCommand = new GenericCommand<SelectAssemblyWindowViewModel, object>(this, SelectAssembly, (vm, p) => true);
            SelectConfigCommand = new GenericCommand<SelectAssemblyWindowViewModel, object>(this, SelectConfig, CanSelectConfig);
            AnalyzeConfigCommand = new GenericCommand<SelectAssemblyWindowViewModel, object>(this, Analyze, CanAnalyze);

            IgnoreSystemAssemblies = true;

            _window = window;
        }

        public ICommand SelectAssemblyCommand { get; private set; }
        public ICommand SelectConfigCommand { get; private set; }
        public ICommand AnalyzeConfigCommand { get; private set; }

        public string AssemblyPath
        {
            get { return _assemblyPath; }
            set { SetProperty(ref _assemblyPath, value, "AssemblyPath"); }
        }

        public string ConfigPath
        {
            get { return _configPath; }
            set { SetProperty(ref _configPath, value, "ConfigPath"); }
        }

        public bool IgnoreSystemAssemblies
        {
            get { return _ignoreSystemAssemblies; }
            set { SetProperty(ref _ignoreSystemAssemblies, value, "IgnoreSystemAssemblies"); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        #region private

        private string _assemblyPath;
        private string _configPath;
        private bool _ignoreSystemAssemblies;
        private ToolWindowPane _window;

        private void SetProperty<T>(ref T field, T newValue, string propertyName)
           where T : IComparable
        {
            if ((field == null && newValue != null)
                || field.CompareTo(newValue) != 0)
            {
                field = newValue;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void SelectAssembly(SelectAssemblyWindowViewModel vm, object parameter)
        {
            var dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.Filter = "DLL, executable|*.dll;*.exe";
            if (dlg.ShowDialog() == true)
            {
                AssemblyPath = dlg.FileName;

                string configPath;
                if (ConfigurationHelper.TrySuggestConfigFile(AssemblyPath, out configPath))
                    ConfigPath = configPath;
                else
                    ConfigPath = "";
            }
        }

        private void SelectConfig(SelectAssemblyWindowViewModel vm, object parameter)
        {
            var dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.Filter = "Configuration files|*.config";
            dlg.InitialDirectory = Path.GetDirectoryName(AssemblyPath);
            if (dlg.ShowDialog() == true)
            {
                ConfigPath = dlg.FileName;
            }
        }

        private bool CanSelectConfig(SelectAssemblyWindowViewModel vm, object parameter)
        {
            return !string.IsNullOrWhiteSpace(AssemblyPath);
        }

        private void Analyze(SelectAssemblyWindowViewModel vm, object parameter)
        {
            RunAnalysis();
            CloseWindow();
        }

        private void CloseWindow()
        {
            ((IVsWindowFrame)_window.Frame).Hide();
        }

        private void RunAnalysis()
        {
            try
            {
                var reader = new ReferenceReader();
                var result = reader.Read(AssemblyPath, ConfigPath, IgnoreSystemAssemblies);

                var builder = new GraphBuilder();
                var doc = builder.BuildDgml(result);

                var path = Path.GetTempFileName();
                path = Path.ChangeExtension(path, ".dgml");
                doc.Save(path);

                Process.Start(path);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool CanAnalyze(SelectAssemblyWindowViewModel vm, object parameter)
        {
            return !string.IsNullOrWhiteSpace(AssemblyPath);
        }





        #endregion
    }
}
