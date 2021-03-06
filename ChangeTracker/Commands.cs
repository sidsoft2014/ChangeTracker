﻿using ChangeTracker.ViewModels;
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

    public sealed class GetChangesManual : ICommand
    {
        private MainViewModel _viewModel;

        public GetChangesManual(MainViewModel viewModel)
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
            return _viewModel.CanGetChangesManual;
        }

        public void Execute(object parameter)
        {
            _viewModel.GetChangesManual();
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

    public sealed class ViewHistory : ICommand
    {
        private MainViewModel _viewModel;

        public ViewHistory(MainViewModel viewModel)
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
            _viewModel.LaunchJobHistoryWindow();
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

    public sealed class ClearList : ICommand
    {
        private MainViewModel _viewModel;

        public ClearList(MainViewModel viewModel)
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
            return _viewModel.ChangedFiles.Count > 0;
        }

        public void Execute(object parameter)
        {
            _viewModel.ClearList();
        }
    }

    public sealed class GetChangesSince : ICommand
    {
        private MainViewModel _viewModel;

        public GetChangesSince(MainViewModel viewModel)
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
            return _viewModel.CanGetChangesSince;
        }

        public void Execute(object parameter)
        {
            _viewModel.LaunchHistoricChangesWindow();
        }
    }

    public sealed class AddOrSaveFilterMode : ICommand
    {
        private EditorViewModel _viewModel;

        public AddOrSaveFilterMode(EditorViewModel viewModel)
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
            _viewModel.AddOrSaveFilterModeButton_OnClick(parameter as string);
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

    public sealed class ClearDay : ICommand
    {
        private HistoryViewModel _viewModel;

        public ClearDay(HistoryViewModel viewModel)
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
            return _viewModel.CanClearDay;
        }

        public void Execute(object parameter)
        {
            _viewModel.ClearDay();
        }
    }

    public sealed class ClearAll : ICommand
    {
        private HistoryViewModel _viewModel;

        public ClearAll(HistoryViewModel viewModel)
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
            return _viewModel.CanClearAll;
        }

        public void Execute(object parameter)
        {
            _viewModel.ClearAll();
        }
    }

}
