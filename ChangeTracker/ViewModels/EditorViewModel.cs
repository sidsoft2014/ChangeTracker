using ChangeTracker.Commands;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Input;

namespace ChangeTracker.ViewModels
{
    public class EditorViewModel : ViewModelBase
    {
        private ObservableCollection<string> _extensions;
        private ObservableCollection<string> _strings;
        private ObservableCollection<string> _directories;
        private string _textBoxExtension;
        private string _textBoxTempFiles;
        private string _textBoxDirectories;
        private string _mode;
        private ICommand _cmdAddFilter;
        private ICommand _cmdRemoveFilter;
        private ICommand _cmdSelectMode;
        private ICommand _cmdSaveFilters;

        public EditorViewModel()
        {
            // Added try-catch to stop app.xaml complaining.
            try
            {
                SelectMode("web");
            }
            catch
            {
                throw;
            }
        }

        public ObservableCollection<string> Extensions
        {
            get
            {
                return _extensions;
            }
            private set
            {
                _extensions = value;
                OnChanged();
            }
        }
        public ObservableCollection<string> Strings
        {
            get
            {
                return _strings;
            }
            private set
            {
                _strings = value;
                OnChanged();
            }
        }
        public ObservableCollection<string> Directories
        {
            get
            {
                return _directories;
            }
            private set
            {
                _directories = value;
                OnChanged();
            }
        }

        public string TextBoxExtension
        {
            get
            {
                return _textBoxExtension;
            }
            set
            {
                if (value != _textBoxExtension)
                {
                    _textBoxExtension = value;
                    OnChanged();
                }
            }
        }
        public string TextBoxStrings
        {
            get
            {
                return _textBoxTempFiles;
            }
            set
            {
                if (value != _textBoxTempFiles)
                {
                    _textBoxTempFiles = value;
                    OnChanged();
                }
            }
        }
        public string TextBoxDirectories
        {
            get
            {
                return _textBoxDirectories;
            }
            set
            {
                if (value != _textBoxDirectories)
                {
                    _textBoxDirectories = value;
                    OnChanged();
                }
            }
        }

        public ICommand cmdAddFilter
        {
            get
            {
                if (_cmdAddFilter == null)
                    _cmdAddFilter = new AddFilter(this);
                return _cmdAddFilter;
            }
        }
        public ICommand cmdRemoveFilter
        {
            get
            {
                if (_cmdRemoveFilter == null)
                    _cmdRemoveFilter = new RemoveFilter(this);
                return _cmdRemoveFilter;
            }
        }
        public ICommand cmdSelectMode
        {
            get
            {
                if (_cmdSelectMode == null)
                    _cmdSelectMode = new SelectMode(this);
                return _cmdSelectMode;
            }
        }
        public ICommand cmdSaveFilters
        {
            get
            {
                if (_cmdSaveFilters == null)
                    _cmdSaveFilters = new SaveFilters(this);
                return _cmdSaveFilters;
            }
        }

        internal bool CanAddFilter(string v)
        {
            switch (v.ToLower())
            {
                case "extensions":
                    {
                        return !String.IsNullOrEmpty(TextBoxExtension)
                            && !Extensions.Contains(TextBoxExtension);
                    }
                case "directories":
                    {
                        return !String.IsNullOrEmpty(TextBoxDirectories)
                            && !Directories.Contains(TextBoxDirectories);
                    }
                case "strings":
                    {
                        return !String.IsNullOrEmpty(TextBoxStrings)
                            && !Strings.Contains(TextBoxStrings);
                    }
                default:
                    return false;
            }
        }

        internal void AddFilter(string v)
        {
            switch (v.ToLower())
            {
                case "extensions":
                    {
                        Extensions.Add(TextBoxExtension);
                        TextBoxExtension = string.Empty;
                        break;
                    }
                case "directories":
                    {
                        Directories.Add(TextBoxDirectories);
                        TextBoxDirectories = string.Empty;
                        break;
                    }
                case "strings":
                    {
                        Strings.Add(TextBoxStrings);
                        TextBoxStrings = string.Empty;
                        break;
                    }
                default:
                    break;
            }
        }

        internal bool CanRemoveFilter(string v)
        {

            switch (v.ToLower())
            {
                case "extensions":
                    {
                        return !String.IsNullOrEmpty(TextBoxExtension)
                            && Extensions.Contains(TextBoxExtension);
                    }
                case "directories":
                    {
                        return !String.IsNullOrEmpty(TextBoxDirectories)
                            && Directories.Contains(TextBoxDirectories);
                    }
                case "strings":
                    {
                        return !String.IsNullOrEmpty(TextBoxStrings)
                            && Strings.Contains(TextBoxStrings);
                    }
                default:
                    return false;
            }
        }

        internal void RemoveFilter(string v)
        {
            switch (v.ToLower())
            {
                case "extensions":
                    {
                        Extensions.Remove(TextBoxExtension);
                        TextBoxExtension = string.Empty;
                        break;
                    }
                case "directories":
                    {
                        Directories.Remove(TextBoxDirectories);
                        TextBoxDirectories = string.Empty;
                        break;
                    }
                case "strings":
                    {
                        Strings.Remove(TextBoxStrings);
                        TextBoxStrings = string.Empty;
                        break;
                    }
                default:
                    break;
            }
        }

        internal override void SelectMode(string parameter)
        {
            _mode = parameter.ToLower();
            switch (_mode)
            {
                case "web":
                    Extensions = new ObservableCollection<string>(Globals.WebSettings.FilteredExtensions);
                    Strings = new ObservableCollection<string>(Globals.WebSettings.FilteredStrings);
                    Directories = new ObservableCollection<string>(Globals.WebSettings.FilteredDirectories);
                    break;
                case "general":
                default:
                    Extensions = new ObservableCollection<string>(Globals.GeneralSettings.FilteredExtensions);
                    Strings = new ObservableCollection<string>(Globals.GeneralSettings.FilteredStrings);
                    Directories = new ObservableCollection<string>(Globals.GeneralSettings.FilteredDirectories);
                    break;
            }
        }

        internal void SaveFilters()
        {
            if (string.IsNullOrEmpty(_mode))
                return;

            switch (_mode)
            {
                case "web":
                    {
                        if (Globals.WebSettings == null)
                            Globals.WebSettings = new Models.SettingsCollection { Name = "Web" };

                        Globals.WebSettings.FilteredDirectories = new List<string>(Directories);
                        Globals.WebSettings.FilteredExtensions = new List<string>(Extensions);
                        Globals.WebSettings.FilteredStrings = new List<string>(Strings);

                        string json = JsonConvert.SerializeObject(Globals.WebSettings);
                        File.WriteAllText(Globals.SavedWebSettings, json);
                        break;
                    }
                case "general":
                    {
                        if (Globals.GeneralSettings == null)
                            Globals.GeneralSettings = new Models.SettingsCollection { Name = "General" };

                        Globals.GeneralSettings.FilteredDirectories = new List<string>(Directories);
                        Globals.GeneralSettings.FilteredExtensions = new List<string>(Extensions);
                        Globals.GeneralSettings.FilteredStrings = new List<string>(Strings);

                        string json = JsonConvert.SerializeObject(Globals.GeneralSettings);
                        File.WriteAllText(Globals.SavedGeneralSettings, json);
                        break;
                    }
                default:
                    break;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if(disposing)
            {
                _extensions = null;
                _directories = null;
                _strings = null;
                _cmdAddFilter = null;
                _cmdRemoveFilter = null;
                _cmdSaveFilters = null;
                _cmdSelectMode = null;
            }

            base.Dispose(disposing);
        }
    }
}
