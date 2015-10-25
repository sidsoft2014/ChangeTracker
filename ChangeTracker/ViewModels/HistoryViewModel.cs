using ChangeTracker.Commands;
using ChangeTracker.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using WF = System.Windows.Forms;

namespace ChangeTracker.ViewModels
{
    public sealed class HistoryViewModel : ViewModelBase
    {
        private Dictionary<string, FileInfo> _xmlFiles;
        private List<HistoryRecord> _records;
        private HistoryRecord _selectedRecord;
        private string _selectedFileName;
        private ICommand _cmdClearDay;
        private ICommand _cmdClearAll;
        private string _status;
        private string _selectedDate;

        private delegate void SetUIStringDelegate(string text);

        public HistoryViewModel()
        {
            GetHistoryLogs();
        }

        public Dictionary<string, FileInfo> XmlFiles
        {
            get
            {
                if (_xmlFiles == null)
                    _xmlFiles = new Dictionary<string, FileInfo>();
                return _xmlFiles;
            }
            private set
            {
                _xmlFiles = value;
                OnChanged();
            }
        }

        public string SelectedFileName
        {
            get
            {
                return _selectedFileName;
            }
            set
            {
                if (value != _selectedFileName)
                {
                    _selectedFileName = value;
                    ReadXml(_selectedFileName);
                    OnChanged();
                }
            }
        }

        public List<HistoryRecord> Records
        {
            get
            {
                if (_records == null)
                    _records = new List<HistoryRecord>();
                return _records;
            }
            private set
            {
                _records = value;
                OnChanged();
                UpdateTotals();
            }
        }

        private void UpdateTotals()
        {
            OnChanged("TotalJobs");
            OnChanged("TotalTime");
            OnChanged("TotalChanges");

            if (Records == null || Records.Count == 0)
                SelectedDate = "No log loaded";
            else
            {
                SelectedDate = Records.First().Start.ToShortDateString();
            }
        }

        public HistoryRecord SelectedRecord
        {
            get
            {
                return _selectedRecord;
            }
            set
            {
                if (value != _selectedRecord)
                {
                    _selectedRecord = value;
                    OnChanged();
                }
            }
        }

        public string SelectedDate
        {
            get
            {
                if (Records == null)
                    return "No log loaded";
                return _selectedDate;
            }
            private set
            {
                if(value != _selectedDate)
                {
                    _selectedDate = value;
                    OnChanged();
                }
            }
        }

        public string TotalJobs
        {
            get
            {
                return Records.Count.ToString();
            }
        }

        public string TotalTime
        {
            get
            {
                TimeSpan ts = new TimeSpan(0, 0, 0);

                foreach (var record in Records)
                {
                    ts = ts.Add(record.TimeSpentAsTimeSpan);
                }

                int hours = ts.Hours;
                int minutes = ts.Minutes;

                if (minutes < 1)
                    minutes = 0;
                else if (minutes < 15)
                    minutes = 15;
                else if (minutes < 30)
                    minutes = 30;
                else if (minutes < 45)
                    minutes = 45;
                else
                {
                    minutes = 0;
                    ++hours;
                }

                var time = new TimeSpan(hours, minutes, 0).ToString().Remove(5);

                return time;
            }
        }

        public string TotalChanges
        {
            get
            {
                return Records.Sum(p => p.ChangedFilesCount).ToString();
            }
        }

        public bool CanClearDay
        {
            get
            {
                return SelectedFileName != null;
            }
        }

        public bool CanClearAll
        {
            get
            {
                return XmlFiles.Count > 0;
            }
        }

        public string Status
        {
            get
            {
                if (_status == null)
                    return string.Empty;
                return _status;
            }
            private set
            {
                if(value != _status)
                {
                    _status = value;
                    OnChanged();
                }
            }
        }

        public ICommand cmdClearDay
        {
            get
            {
                if (_cmdClearDay == null)
                    _cmdClearDay = new ClearDay(this);
                return _cmdClearDay;
            }
        }

        public ICommand cmdClearAll
        {
            get
            {
                if (_cmdClearAll == null)
                    _cmdClearAll = new ClearAll(this);
                return _cmdClearAll;
            }
        }

        internal void ClearDay()
        {
            var file = XmlFiles[SelectedFileName];

            if (file.Exists)
            {
                // Show dialog result.
                if (!GetDialogResult("Are you sure you wish to wipe this day from history log?" +
                    Environment.NewLine +
                    "If the selected record is for today any current history will also be lost.",
                    "Wipe day log?"))
                    return;

                // If the record is todays record, we want to clear global history as well
                // to prevent it re-saving on exit.
                if (Records.FirstOrDefault().Start.ToShortDateString() == DateTime.Now.ToShortDateString())
                    Globals.History = new List<HistoryRecord>();

                Records = new List<HistoryRecord>();
                file.Delete();
                GetHistoryLogs();

                SetTemporaryStatusMessage("Day erased from logs.");
            }

        }

        internal void ClearAll()
        {
            if (!Directory.Exists(Globals.HistoryFolder))
                return;

            if (!GetDialogResult("Are you sure you wish to wipe all history logs?"
                + Environment.NewLine
                + "Any unsaved jobs recorded today will also be lost.",
                "Wipe all logs"))
                return;

            // Clear local record list and global history.
            Globals.History = Records = new List<HistoryRecord>();

            var dInf = new DirectoryInfo(Globals.HistoryFolder);
            var files = dInf.GetFiles().Where(p => p.Extension == ".xml").Select(o => o.FullName);
            foreach (var item in files)
            {
                if(File.Exists(item))
                    File.Delete(item);
            }

            GetHistoryLogs();
            SetTemporaryStatusMessage("All history cleared.");
        }

        internal override void SelectFilterMode(string parameter)
        {
            
        }

        private void SetTemporaryStatusMessage(string message)
        {
            if (System.Windows.Application.Current == null)
                return;

            if (System.Windows.Application.Current.Dispatcher.CheckAccess())
            {
                if (Status != message)
                {
                    Status = message;

                    if (!string.IsNullOrEmpty(message))
                    {
                        System.Timers.Timer timer = new System.Timers.Timer(2000);

                        timer.AutoReset = false;
                        timer.Elapsed += (s, e) =>
                        {
                            SetTemporaryStatusMessage(string.Empty);
                            timer.Dispose();
                        };
                        timer.Start();
                    }
                }
            }
            else
            {
                SetUIStringDelegate del = new SetUIStringDelegate(SetTemporaryStatusMessage);
                System.Windows.Application.Current.Dispatcher.Invoke(del, new object[] { message });
            }
        }

        private bool GetDialogResult(string message, string title)
        {
            WF.DialogResult dlg = WF.MessageBox.Show(message, title, WF.MessageBoxButtons.YesNo, WF.MessageBoxIcon.Warning);

            if (dlg == WF.DialogResult.Yes)
                return true;

            return false;
        }

        private void ReadXml(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                Records = new List<HistoryRecord>();
                SelectedRecord = new HistoryRecord();
                return;
            }

            FileInfo file = XmlFiles[fileName];
            var temp = new List<HistoryRecord>();
            Globals.XmlToHistory(file.FullName, out temp);

            Records = temp;
            SelectedRecord = Records.FirstOrDefault();
        }

        private void GetHistoryLogs()
        {
            if (Directory.Exists(Globals.HistoryFolder))
            {
                DirectoryInfo dInf = new DirectoryInfo(Globals.HistoryFolder);

                var temp = new Dictionary<string, FileInfo>();
                foreach (var file in dInf.GetFiles().Where(p => p.Extension == ".xml"))
                {
                    temp.Add(file.Name, file);
                }
                XmlFiles = temp;
            }
            else
            {
                Records = Globals.History;
            }
        }
    }
}
