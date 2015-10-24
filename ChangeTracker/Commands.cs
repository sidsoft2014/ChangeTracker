using ChangeTracker.ViewModels;
using System;
using System.Windows.Input;

namespace ChangeTracker.Commands
{
    public sealed class SelectFilterMode : ICommand
    {
        private ViewModelBase _viewModel;

        public SelectFilterMode(ViewModelBase viewModel)
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
            _viewModel.SelectFilterMode(parameter as string);
        }
    }

    public sealed class SelectScanMode : ICommand
    {
        private MainViewModel _viewModel;

        public SelectScanMode(MainViewModel viewModel)
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
            _viewModel.SelectScanMode(parameter as string);
        }
    }

    public sealed class LaunchFilterEditor : ICommand
    {
        private MainViewModel _viewModel;

        public LaunchFilterEditor(MainViewModel viewModel)
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
            _viewModel.LaunchFilterEditor();
        }
    }

    public sealed class SelectFolder : ICommand
    {
        private MainViewModel _viewModel;

        public SelectFolder(MainViewModel viewModel)
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

    public sealed class SaveList : ICommand
    {
        private MainViewModel _viewModel;

        public SaveList(MainViewModel viewModel)
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
        private MainViewModel _viewModel;

        public CopyFiles(MainViewModel viewModel)
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

    public sealed class AddFilter : ICommand
    {
        private EditorViewModel _viewModel;

        public AddFilter(EditorViewModel viewModel)
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
            return _viewModel.CanAddFilter(parameter as string);
        }

        public void Execute(object parameter)
        {
            _viewModel.AddFilter(parameter as string);
        }
    }

    public sealed class RemoveFilter : ICommand
    {
        private EditorViewModel _viewModel;

        public RemoveFilter(EditorViewModel viewModel)
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
            return _viewModel.CanRemoveFilter(parameter as string);
        }

        public void Execute(object parameter)
        {
            _viewModel.RemoveFilter(parameter as string);
        }
    }

    public sealed class SaveFilters : ICommand
    {
        private EditorViewModel _viewModel;

        public SaveFilters(EditorViewModel viewModel)
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
            return _viewModel.CanSaveFilters;
        }

        public void Execute(object parameter)
        {
            _viewModel.SaveFilters();
        }
    }
}
