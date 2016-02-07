using ChangeTracker.Commands;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Input;

namespace ChangeTracker.ViewModels
{
    // pragma_warning.cs
    public class EditorViewModel : ViewModelBase
    {
        private ObservableCollection<string> _directories;
        private ObservableCollection<string> _extensions;
        private ObservableCollection<string> _regexes;
        private ObservableCollection<string> _strings;
        private string _mode = "web";
        private string _textBoxExtension;
        private string _textBoxTempFiles;
        private string _textBoxDirectories;
        private string _textBoxRegex;
        private ICommand _cmdAddFilterString;
        private ICommand _cmdRemoveFilterString;
        private ICommand _cmdSelectMode;
        private ICommand _cmdSaveFilters;
        private string _selectedFilterMode;
        private ICommand _cmdAddOrSaveFilterModeButton_OnClick;
        private string _addSaveBtnText;

        public EditorViewModel()
        {
            // Try clause needed to prevent VS complaining.
            try
            {
                // May as well use the method we already have to build initial lists.
                SelectedFilterMode = FilterModes.Count > 0 ? FilterModes[0] : "";
            }
            finally
            {

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
        public ObservableCollection<string> Regexes
        {
            get
            {
                return _regexes;
            }
            private set
            {
                _regexes = value;
                OnChanged();
            }
        }
        public List<string> FilterModes
        {
            get
            {
                return Globals.FilterCollections.Keys.ToList();
            }
        }
        public string SelectedFilterMode
        {
            get
            {
                return _selectedFilterMode;
            }
            set
            {
                if (value != _selectedFilterMode)
                {
                    _selectedFilterMode = value;
                    SelectFilterMode(_selectedFilterMode);
                    OnChanged();
                }
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
        public string TextBoxRegex
        {
            get
            {
                return _textBoxRegex;
            }
            set
            {
                if (value != _textBoxRegex)
                {
                    _textBoxRegex = value;
                    OnChanged();
                }
            }
        }
        public string AddSaveBtnText
        {
            get
            {
                if (string.IsNullOrEmpty(_addSaveBtnText))
                    _addSaveBtnText = "New";
                return _addSaveBtnText;
            }
            set
            {
                if(value != _addSaveBtnText)
                {
                    _addSaveBtnText = value;
                    OnChanged();
                }
            }
        }

        public ICommand cmdAddFilterString
        {
            get
            {
                if (_cmdAddFilterString == null)
                    _cmdAddFilterString = new AddFilter(this);
                return _cmdAddFilterString;
            }
        }
        public ICommand cmdRemoveFilterString
        {
            get
            {
                if (_cmdRemoveFilterString == null)
                    _cmdRemoveFilterString = new RemoveFilter(this);
                return _cmdRemoveFilterString;
            }
        }
        public ICommand cmdSelectMode
        {
            get
            {
                if (_cmdSelectMode == null)
                    _cmdSelectMode = new SelectFilterMode(this);
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
        public ICommand cmdAddOrSaveFilterModeButton_OnClick
        {
            get
            {
                if (_cmdAddOrSaveFilterModeButton_OnClick == null)
                    _cmdAddOrSaveFilterModeButton_OnClick = new AddOrSaveFilterMode(this);
                return _cmdAddOrSaveFilterModeButton_OnClick;
            }
        }

        internal bool CanSaveFilters
        {
            get
            {
                var collection = Globals.FilterCollections
                    .FirstOrDefault(p => p.Key.ToLower() == _mode.ToLower()).Value;

                if (collection == null)
                    return false;

                return !collection.FilteredDirectories.SequenceEqual(Directories)
                                || !collection.FilteredExtensions.SequenceEqual(Extensions)
                                || !collection.FilteredStrings.SequenceEqual(Strings);                
            }
        }

        internal void AddOrSaveFilterModeButton_OnClick(string v)
        {
            if (string.IsNullOrEmpty(v))
                return;

            switch (AddSaveBtnText.ToLower())
            {
                case "new":
                    AddSaveBtnText = "Save";
                    break;
                default:
                case "save":
                    if (!string.IsNullOrEmpty(SelectedFilterMode))
                    {
                        foreach (var key in Globals.FilterCollections.Keys)
                        {
                            if (key.ToLower() == SelectedFilterMode.ToLower())
                                return;
                        }
                        Globals.FilterCollections
                            .Add(
                            SelectedFilterMode,
                            new Models.FilterCollection
                            {
                                Name = SelectedFilterMode
                            });

                        SaveFilters();
                        OnChanged("FilterModes");
                    }
                    AddSaveBtnText = "New";
                    break;
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

        internal void AddFilter(string v)
        {
            switch (v.ToLower())
            {
                case "extensions":
                    {
                        if (string.IsNullOrEmpty(TextBoxExtension))
                            return;

                        // Add leading dot if not present.
                        if (!TextBoxExtension.StartsWith("."))
                            TextBoxExtension = "." + TextBoxExtension;

                        Extensions.Add(TextBoxExtension.ToLower());
                        TextBoxExtension = string.Empty;
                        break;
                    }
                case "directories":
                    {
                        if (string.IsNullOrEmpty(TextBoxDirectories))
                            return;

                        Directories.Add(TextBoxDirectories.ToLower());
                        TextBoxDirectories = string.Empty;
                        break;
                    }
                case "regex":
                    {
                        if (string.IsNullOrEmpty(TextBoxRegex))
                            return;

                        Regexes.Add(TextBoxRegex.ToLower());
                        TextBoxRegex = string.Empty;
                        break;
                    }
                case "strings":
                    {
                        if (string.IsNullOrEmpty(TextBoxStrings))
                            return;

                        Strings.Add(TextBoxStrings.ToLower());
                        TextBoxStrings = string.Empty;
                        break;
                    }
                default:
                    break;
            }
        }

        internal void RemoveFilter(string v)
        {
            switch (v.ToLower())
            {
                case "extensions":
                    {
                        if (string.IsNullOrEmpty(TextBoxExtension))
                            return;

                        Extensions.Remove(TextBoxExtension);
                        TextBoxExtension = string.Empty;
                        break;
                    }
                case "directories":
                    {
                        if (string.IsNullOrEmpty(TextBoxDirectories))
                            return;

                        Directories.Remove(TextBoxDirectories);
                        TextBoxDirectories = string.Empty;
                        break;
                    }
                case "regex":
                    {
                        if (string.IsNullOrEmpty(TextBoxRegex))
                            return;

                        Regexes.Remove(TextBoxRegex);
                        TextBoxRegex = string.Empty;
                        break;
                    }
                case "strings":
                    {
                        if (string.IsNullOrEmpty(TextBoxStrings))
                            return;

                        Strings.Remove(TextBoxStrings);
                        TextBoxStrings = string.Empty;
                        break;
                    }
                default:
                    break;
            }
        }

        internal void SaveFilters()
        {
            if (string.IsNullOrEmpty(_mode))
                return;

            var collection = Globals.FilterCollections
                .FirstOrDefault(p => p.Key.ToLower() == _mode.ToLower()).Value;

            if (collection == null)
                return;

            collection.FilteredDirectories = new HashSet<string>(Directories);
            collection.FilteredExtensions = new HashSet<string>(Extensions);
            collection.FilteredStrings = new HashSet<string>(Strings);
            collection.FilteredRegex = new HashSet<string>(Regexes);

            string json = JsonConvert.SerializeObject(Globals.FilterCollections);
            File.WriteAllText(Globals.SavedSettingsCollections, json);
        }

        internal override void SelectFilterMode(string parameter)
        {
            if (parameter == null)
                return;

            _mode = parameter.ToLower();

            var collection = Globals.FilterCollections
                .FirstOrDefault(p => p.Key.ToLower() == _mode).Value;

            if (collection == null)
            {
                Extensions = new ObservableCollection<string>();
                Strings = new ObservableCollection<string>();
                Directories = new ObservableCollection<string>();
                Regexes = new ObservableCollection<string>();
            }
            else
            {
                Extensions = new ObservableCollection<string>(collection.FilteredExtensions);
                Strings = new ObservableCollection<string>(collection.FilteredStrings);
                Directories = new ObservableCollection<string>(collection.FilteredDirectories);
                Regexes = new ObservableCollection<string>(collection.FilteredRegex);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _extensions = null;
                _directories = null;
                _strings = null;
                _cmdAddFilterString = null;
                _cmdRemoveFilterString = null;
                _cmdSaveFilters = null;
                _cmdSelectMode = null;
            }

            base.Dispose(disposing);
        }
    }
}
