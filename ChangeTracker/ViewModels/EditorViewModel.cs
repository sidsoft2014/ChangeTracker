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
        private string _textBoxExtension;
        private string _textBoxTempFiles;
        private string _textBoxDirectories;
        private string _mode = "web";
        private ICommand _cmdAddFilter;
        private ICommand _cmdRemoveFilter;
        private ICommand _cmdSelectMode;
        private ICommand _cmdSaveFilters;

        public EditorViewModel()
        {
            // Try clause needed to prevent VS complaining.
            try
            {
                // May as well use the method we already have to build initial lists.
                SelectFilterMode("web");
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

        internal bool CanSaveFilters
        {
            get
            {
                switch (_mode)
                {
                    case "web":
                        try
                        {
                            return !Globals.WebSettings.FilteredDirectories.SequenceEqual(Directories)
                                || !Globals.WebSettings.FilteredExtensions.SequenceEqual(Extensions)
                                || !Globals.WebSettings.FilteredStrings.SequenceEqual(Strings);
                        }
                        catch
                        {
                            break;
                        }
                    case "general":
                        try
                        {
                            return !Globals.GeneralSettings.FilteredDirectories.SequenceEqual(Directories)
                                || !Globals.GeneralSettings.FilteredExtensions.SequenceEqual(Extensions)
                                || !Globals.GeneralSettings.FilteredStrings.SequenceEqual(Strings);
                        }
                        catch
                        {
                            break;
                        }
                    case "code":
                        try
                        {
                            return !Globals.CodeSettings.FilteredDirectories.SequenceEqual(Directories)
                                || !Globals.CodeSettings.FilteredExtensions.SequenceEqual(Extensions)
                                || !Globals.CodeSettings.FilteredStrings.SequenceEqual(Strings);
                        }
                        catch
                        {
                            break;
                        }
                    default:
                        break;
                }
                return false;
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
                        // Add leading dot if not present.
                        if (!TextBoxExtension.StartsWith("."))
                            TextBoxExtension = "." + TextBoxExtension;

                        Extensions.Add(TextBoxExtension.ToLower());
                        TextBoxExtension = string.Empty;
                        break;
                    }
                case "directories":
                    {
                        Directories.Add(TextBoxDirectories.ToLower());
                        TextBoxDirectories = string.Empty;
                        break;
                    }
                case "strings":
                    {
                        Strings.Add(TextBoxStrings.ToLower());
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

        internal override void SelectFilterMode(string parameter)
        {
            // If for some reason we have a null parameter we may aswell set a value to prevent exceptions later.
            if (parameter == null)
                parameter = "web";

            _mode = parameter.ToLower();
            switch (_mode)
            {
                case "web":
                    Extensions = new ObservableCollection<string>(Globals.WebSettings.FilteredExtensions);
                    Strings = new ObservableCollection<string>(Globals.WebSettings.FilteredStrings);
                    Directories = new ObservableCollection<string>(Globals.WebSettings.FilteredDirectories);
                    Regexes = new ObservableCollection<string>(Globals.WebSettings.FilteredRegex);
                    break;
                case "code":
                    Extensions = new ObservableCollection<string>(Globals.CodeSettings.FilteredExtensions);
                    Strings = new ObservableCollection<string>(Globals.CodeSettings.FilteredStrings);
                    Directories = new ObservableCollection<string>(Globals.CodeSettings.FilteredDirectories);
                    Regexes = new ObservableCollection<string>(Globals.CodeSettings.FilteredRegex);
                    break;
                case "general":
                default:
                    Extensions = new ObservableCollection<string>(Globals.GeneralSettings.FilteredExtensions);
                    Strings = new ObservableCollection<string>(Globals.GeneralSettings.FilteredStrings);
                    Directories = new ObservableCollection<string>(Globals.GeneralSettings.FilteredDirectories);
                    Regexes = new ObservableCollection<string>(Globals.GeneralSettings.FilteredRegex);
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

                        Globals.WebSettings.FilteredDirectories = new HashSet<string>(Directories);
                        Globals.WebSettings.FilteredExtensions = new HashSet<string>(Extensions);
                        Globals.WebSettings.FilteredStrings = new HashSet<string>(Strings);
                        Globals.WebSettings.FilteredRegex = new HashSet<string>(Regexes);

                        string json = JsonConvert.SerializeObject(Globals.WebSettings);
                        File.WriteAllText(Globals.SavedWebSettings, json);
                        break;
                    }
                case "code":
                    {
                        if (Globals.CodeSettings == null)
                            Globals.CodeSettings = new Models.SettingsCollection { Name = "Code" };

                        Globals.CodeSettings.FilteredDirectories = new HashSet<string>(Directories);
                        Globals.CodeSettings.FilteredExtensions = new HashSet<string>(Extensions);
                        Globals.CodeSettings.FilteredStrings = new HashSet<string>(Strings);
                        Globals.CodeSettings.FilteredRegex = new HashSet<string>(Regexes);

                        string json = JsonConvert.SerializeObject(Globals.CodeSettings);
                        File.WriteAllText(Globals.SavedCodeSettings, json);
                        break;
                    }
                case "general":
                    {
                        if (Globals.GeneralSettings == null)
                            Globals.GeneralSettings = new Models.SettingsCollection { Name = "General" };

                        Globals.GeneralSettings.FilteredDirectories = new HashSet<string>(Directories);
                        Globals.GeneralSettings.FilteredExtensions = new HashSet<string>(Extensions);
                        Globals.GeneralSettings.FilteredStrings = new HashSet<string>(Strings);
                        Globals.GeneralSettings.FilteredRegex = new HashSet<string>(Regexes);

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
