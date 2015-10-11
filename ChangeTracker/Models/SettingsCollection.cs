using System;
using System.Collections.Generic;

namespace ChangeTracker.Models
{
    [Serializable]
    public class SettingsCollection
    {
        public string Name { get; set; }
        public List<string> FilteredExtensions { get; set; }
        public List<string> FilteredDirectories { get; set; }
        public List<string> FilteredStrings { get; set; }
    }
}
