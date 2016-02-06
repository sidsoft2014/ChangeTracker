using ChangeTracker.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace ChangeTracker
{
    public static class Globals
    {
        public static readonly string SettingsFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Settings");
        public static readonly string HistoryFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "History");
        public static readonly string SavedSettingsCollections = Path.Combine(SettingsFolder, "collections.json");

        public static Dictionary<string, FilterCollection> FilterCollections;

        public static List<HistoryRecord> History;

        public static FilterCollection SelectedFilter { get; set; }

        public static bool Init()
        {
            if (!Directory.Exists(SettingsFolder))
                Directory.CreateDirectory(SettingsFolder);

            InitSettingsCollections();
            InitHistory();

            SelectedFilter = FilterCollections.FirstOrDefault().Value;

            return true;
        }

        public static void XmlToHistory(string path, out List<HistoryRecord> output)
        {
            if (!string.IsNullOrEmpty(path))
            {
                using (XmlReader reader = XmlReader.Create(path))
                {
                    XmlSerializer serialiser = new XmlSerializer(typeof(List<HistoryRecord>));

                    if (serialiser.CanDeserialize(reader))
                    {
                        var list = serialiser.Deserialize(reader) as List<HistoryRecord>;
                        if (list != null)
                        {
                            output = list;
                            return;
                        }
                    }

                }
            }

            output = new List<HistoryRecord>();
        }

        /// <summary>
        /// Method that needs to be called on application close in order to save history.
        /// </summary>
        /// <returns></returns>
        public static bool OnClose()
        {
            if (History != null && History.Count > 0)
            {
                DateTime now = DateTime.Now;
                string fileName = string.Format("{0}-{1}-{2}.xml", now.Year, now.Month, now.Day);
                string path = Path.Combine(HistoryFolder, fileName);

                using (TextWriter writer = new StreamWriter(path, false))
                {
                    XmlSerializer serialiser = new XmlSerializer(typeof(List<HistoryRecord>));

                    serialiser.Serialize(writer, History);
                }
            }

            return true;
        }

        private static void InitSettingsCollections()
        {
            FilterCollections = new Dictionary<string, FilterCollection>();

            if (File.Exists(SavedSettingsCollections))
            {
                string json = File.ReadAllText(SavedSettingsCollections);
                try
                {
                    FilterCollections = JsonConvert.DeserializeObject<Dictionary<string, FilterCollection>>(json);
                    return;
                }
                catch (Exception ex)
                {

                }
            }

            FilterCollections.Add("Code", BaseCodeFilters());
            FilterCollections.Add("General", BaseGeneralFilters());
            FilterCollections.Add("Web", BaseWebFilters());

        }

        private static void InitHistory()
        {
            if (!Directory.Exists(HistoryFolder))
                Directory.CreateDirectory(HistoryFolder);

            var files = new DirectoryInfo(HistoryFolder).GetFiles();
            DateTime today = DateTime.Now;
            FileInfo file = null;

            foreach (var item in files)
            {
                if (item.CreationTime.Year == today.Year
                    && item.CreationTime.Month == today.Month
                    && item.CreationTime.Day == today.Day)
                {
                    file = item;
                    break;
                }
            }

            if (file != null)
            {
                string path = file.FullName;
                XmlToHistory(path, out History);
            }
            else
            {
                History = new List<HistoryRecord>();
            }
        }

        private static FilterCollection BaseGeneralFilters()
        {
            FilterCollection generalSettings = new FilterCollection { Name = "General" };

            if (generalSettings == null)
                generalSettings = new FilterCollection { Name = "General" };

            generalSettings.FilteredDirectories = new HashSet<string>();
            generalSettings.FilteredExtensions = new HashSet<string>();
            generalSettings.FilteredStrings = new HashSet<string>();
            generalSettings.FilteredRegex = new HashSet<string>();

            return generalSettings;
        }

        private static FilterCollection BaseCodeFilters()
        {
            FilterCollection codeSettings = new FilterCollection { Name = "Code" };
            if (codeSettings == null)
                codeSettings = new FilterCollection { Name = "Web" };

            codeSettings.FilteredExtensions = new HashSet<string>
                {
                    ".cs",
                    ".vb",
                    ".pdb",
                    ".sln",
                    ".csproj",
                    ".vbproj",
                    ".suo",
                    ".sync",
                    ".temp",
                    ".tmp"
                };
            codeSettings.FilteredStrings = new HashSet<string>
                {
                    ".dll.config"
                };
            codeSettings.FilteredDirectories = new HashSet<string>
                {
                    "obj",
                    "debug",
                    "release"
                };
            codeSettings.FilteredRegex = new HashSet<string>();

            return codeSettings;
        }

        private static FilterCollection BaseWebFilters()
        {
            FilterCollection webSettings = new FilterCollection { Name = "Web" };
            if (webSettings == null)
                webSettings = new FilterCollection { Name = "Web" };

            webSettings.FilteredExtensions = new HashSet<string>
                {
                    ".pdb",
                    ".sln",
                    ".csproj",
                    ".vbproj",
                    ".suo",
                    ".sync",
                    ".temp",
                    ".tmp"
                };
            webSettings.FilteredStrings = new HashSet<string>
                {
                    ".dll.config"
                };
            webSettings.FilteredDirectories = new HashSet<string>
                {
                    "obj",
                    "debug",
                    "release"
                };
            webSettings.FilteredRegex = new HashSet<string>();

            return webSettings;
        }
    }
}
