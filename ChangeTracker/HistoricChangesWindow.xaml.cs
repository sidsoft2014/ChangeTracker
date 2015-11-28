using ChangeTracker.Models;
using ChangeTracker.ViewModels;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using DirectoryInfo = Pri.LongPath.DirectoryInfo;

namespace ChangeTracker
{
    /// <summary>
    /// Interaction logic for HistoricChangesWindow.xaml
    /// </summary>
    public partial class HistoricChangesWindow : Window
    {
        private string _currentPath;
        private MainViewModel _mainVm;
        private List<ChangedFile> changes = new List<ChangedFile>();

        public HistoricChangesWindow(MainViewModel mainViewModel)
        {
            try
            {
                if (mainViewModel == null)
                    throw new NullReferenceException("mainViewModel");

                this._mainVm = mainViewModel;
                _currentPath = _mainVm.WatchedFolder;

                InitializeComponent();
            }
            catch
            {

            }
        }

        private void DatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(_currentPath))
                return;

            DatePicker dp = sender as DatePicker;
            if (dp == null)
            {

                lblStatus.Text = "Error getting selected date.";
                return;
            }

            try
            {
                changes = new List<ChangedFile>();
                lstChanegs.Items.Clear();
                btnSave.IsEnabled = false;
                btnSave.Visibility = Visibility.Hidden;

                DirectoryInfo dInf = new DirectoryInfo(_currentPath);
                foreach (var fileInfo in dInf.GetFiles("*", System.IO.SearchOption.AllDirectories))
                {
                    if (fileInfo.CreationTimeUtc >= dp.SelectedDate || fileInfo.LastWriteTimeUtc >= dp.SelectedDate)
                    {
                        if (Globals.SelectedFilter.FilePassesFilter(fileInfo))
                        {
                            changes.Add(fileInfo);
                            lstChanegs.Items.Add(fileInfo);
                        }
                    }
                }

                lblStatus.Text = "Total Changes: " + changes.Count;

                if(changes.Count > 0)
                {
                    btnSave.IsEnabled = true;
                    btnSave.Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                lblStatus.Text = ex.Message;
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (changes == null)
                return;

            try
            {
                _mainVm.AddNewChangeSet(changes);
            }
            catch (Exception ex)
            {
                lblStatus.Text = ex.Message;
            }

            try
            {
                var main = App.Current.MainWindow;
                main.Focus();
            }
            finally
            {
                Close();
            }
        }
    }
}
