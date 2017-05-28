using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ReferenceConflictAnalyser.VSExtension.UI.Utils
{
    public class GenericCommand<TViewModel, TCommandParameter> : ICommand
       where TViewModel : INotifyPropertyChanged
    {
        Func<TViewModel, TCommandParameter, bool> _predicate;
        Action<TViewModel, TCommandParameter> _execute;
        TViewModel _viewModel;

        public GenericCommand(TViewModel viewModel, Action<TViewModel, TCommandParameter> execute, Func<TViewModel, TCommandParameter, bool> canExecute)
        {
            _viewModel = viewModel;
            _predicate = canExecute;
            _execute = execute;

            _viewModel.PropertyChanged += _viewModel_PropertyChanged;
        }

        private void _viewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            CanExecuteChanged?.Invoke(this, e);
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return _predicate(_viewModel, (TCommandParameter)parameter);
        }

        public void Execute(object parameter)
        {
            _execute(_viewModel, (TCommandParameter)parameter);
        }
    }
}
