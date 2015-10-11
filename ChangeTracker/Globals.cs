﻿using ChangeTracker.Models;
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
        public static SettingsCollection GeneralSettings = new SettingsCollection { Name = "General" };

        public static string SettingsFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Settings");
        public static string SavedWebSettings = Path.Combine(SettingsFolder, "web.json");
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

            GeneralSettings.FilteredDirectories = new List<string>();
            GeneralSettings.FilteredExtensions = new List<string>();
            GeneralSettings.FilteredStrings = new List<string>();
        }

        private static void PopulateBaseWebFilters()
        {
            if (WebSettings == null)
                WebSettings = new SettingsCollection { Name = "Web" };

            WebSettings.FilteredExtensions = new List<string>
                {
                    ".cs",
                    ".vb",
                    ".pdb",
                    ".sln",
                    ".csproj",
                    ".vbproj",
                    ".suo"
                };
            WebSettings.FilteredStrings = new List<string>
                {
                    "temp",
                    ".dll.config",
                    "~"
                };
            WebSettings.FilteredDirectories = new List<string>
                {
                    "obj",
                    "debug",
                    "release"
                };
        }
    }
}