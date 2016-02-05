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
        public static SettingsCollection WebSettings = new SettingsCollection { Name = "Web" };
        public static SettingsCollection CodeSettings = new SettingsCollection { Name = "Code" };
        public static SettingsCollection GeneralSettings = new SettingsCollection { Name = "General" };

        public static string SettingsFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Settings");
        public static string HistoryFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "History");
        public static string SavedWebSettings = Path.Combine(SettingsFolder, "web.json");
        public static string SavedCodeSettings = Path.Combine(SettingsFolder, "code.json");
        public static string SavedGeneralSettings = Path.Combine(SettingsFolder, "general.json");

        public static List<HistoryRecord> History;

        public static SettingsCollection SelectedFilter { get; set; }

        public static void Init()
        {
            if (!Directory.Exists(SettingsFolder))
                Directory.CreateDirectory(SettingsFolder);

            InitGeneralMode();
            InitWebMode();
            InitHistory();

            SelectedFilter = WebSettings;
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

        public static bool OnClose()
        {
            if (History.Count > 0)
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

        private static void InitGeneralMode()
        {
            if (File.Exists(SavedGeneralSettings))
            {
                string json = File.ReadAllText(SavedGeneralSettings);
                try
                {
                    GeneralSettings = JsonConvert.DeserializeObject<SettingsCollection>(json);
                }
                catch (Exception ex)
                {
#if DEBUG
                    Console.WriteLine(ex.Message);
#endif
                    PopulateBaseGeneralFilters();
                }
            }
            else
            {
                PopulateBaseGeneralFilters();
            }
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

        private static void InitWebMode()
        {
            if (File.Exists(SavedWebSettings))
            {
                string json = File.ReadAllText(SavedWebSettings);
                try
                {
                    WebSettings = JsonConvert.DeserializeObject<SettingsCollection>(json);
                }
                catch (Exception ex)
                {
#if DEBUG
                    Console.WriteLine(ex.Message);
#endif
                    PopulateBaseWebFilters();
                }
            }
            else
            {
                PopulateBaseWebFilters();
            }
        }

        private static void PopulateBaseCodeFilters()
        {
            if (WebSettings == null)
                WebSettings = new SettingsCollection { Name = "Web" };

            CodeSettings.FilteredExtensions = new HashSet<string>
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
            CodeSettings.FilteredStrings = new HashSet<string>
                {
                    ".dll.config"
                };
            CodeSettings.FilteredDirectories = new HashSet<string>
                {
                    "obj",
                    "debug",
                    "release"
                };
            WebSettings.FilteredRegex = new HashSet<string>();
        }

        private static void PopulateBaseGeneralFilters()
        {
            if (GeneralSettings == null)
                GeneralSettings = new SettingsCollection { Name = "General" };

            GeneralSettings.FilteredDirectories = new HashSet<string>();
            GeneralSettings.FilteredExtensions = new HashSet<string>();
            GeneralSettings.FilteredStrings = new HashSet<string>();
            GeneralSettings.FilteredRegex = new HashSet<string>();
        }

        private static void PopulateBaseWebFilters()
        {
            if (WebSettings == null)
                WebSettings = new SettingsCollection { Name = "Web" };

            WebSettings.FilteredExtensions = new HashSet<string>
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
            WebSettings.FilteredStrings = new HashSet<string>
                {
                    ".dll.config"
                };
            WebSettings.FilteredDirectories = new HashSet<string>
                {
                    "obj",
                    "debug",
                    "release"
                };
            WebSettings.FilteredRegex = new HashSet<string>();
        }
    }
}
