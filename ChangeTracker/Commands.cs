using System;
using System.Windows.Input;

namespace ChangeTracker
{
    public sealed class SelectFolder : ICommand
    {
        private ViewModel _viewModel;

        public SelectFolder(ViewModel viewModel)
        {
            _viewModel = viewModel;
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            _viewModel.SelectFolder();
        }
    }

    public sealed class SelectMode : ICommand
    {
        private ViewModel _viewModel;

        public SelectMode(ViewModel viewModel)
        {
            _viewModel = viewModel;
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            _viewModel.SelectMode((string)parameter);
        }
    }

    public sealed class SaveList : ICommand
    {
        private ViewModel _viewModel;

        public SaveList(ViewModel viewModel)
        {
            _viewModel = viewModel;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameter)
        {
            return _viewModel.CanSaveList;
        }

        public void Execute(object parameter)
        {
            _viewModel.SaveList();
        }
    }

    public sealed class CopyFiles : ICommand
    {
        private ViewModel _viewModel;

        public CopyFiles(ViewModel viewModel)
        {
            _viewModel = viewModel;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameter)
        {
            return _viewModel.CanSaveList;
        }

        public void Execute(object parameter)
        {
            _viewModel.CopyFiles();
        }
    }
}
