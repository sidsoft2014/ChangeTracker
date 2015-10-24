using ChangeTracker.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChangeTracker
{
    public static class Globals
    {
        public static SettingsCollection WebSettings = new SettingsCollection { Name = "Web" };
        public static SettingsCollection CodeSettings = new SettingsCollection { Name = "Code" };
        public static SettingsCollection GeneralSettings = new SettingsCollection { Name = "General" };

        public static string SettingsFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Settings");
        public static string SavedWebSettings = Path.Combine(SettingsFolder, "web.json");
        public static string SavedCodeSettings = Path.Combine(SettingsFolder, "code.json");
        public static string SavedGeneralSettings = Path.Combine(SettingsFolder, "general.json");

        public static void Init()
        {
            if (!Directory.Exists(SettingsFolder))
                Directory.CreateDirectory(SettingsFolder);

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

        private static void PopulateBaseGeneralFilters()
        {
            if (GeneralSettings == null)
                GeneralSettings = new SettingsCollection { Name = "General" };

            GeneralSettings.FilteredDirectories = new HashSet<string>();
            GeneralSettings.FilteredExtensions = new HashSet<string>();
            GeneralSettings.FilteredStrings = new HashSet<string>();
        }

        private static void PopulateBaseWebFilters()
        {
            if (WebSettings == null)
                WebSettings = new SettingsCollection { Name = "Web" };

            WebSettings.FilteredExtensions = new HashSet<string>
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
        }
    }
}
