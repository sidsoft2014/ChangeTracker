using ChangeTracker.Models;
using Pri.LongPath;
using System.Text.RegularExpressions;

namespace ChangeTracker.Helpers
{
    public static class FilterCollectionExtensions
    {
        /// <summary>
        /// Checks if the given fileinfo object passes the collections set of filters.
        /// </summary>
        /// <param name="file">FileInfo object to check against filters.</param>
        /// <returns></returns>
        public static bool FilePassesFilter(this FilterCollection collection, FileInfo file)
        {
            if (file == null)
                return false;

            // Check for filtered extensions.
            if (collection.FilteredExtensions.Contains(file.Extension.ToLower()))
                return false;

            // Check for filtered file names without extension.
            if (collection.FilteredStrings.Contains(file.Name.ToLower().Replace(file.Extension, "")))
                return false;

            // Check for filtered file name with extension.
            if (collection.FilteredStrings.Contains(file.Name.ToLower()))
                return false;

            // Check for filtered directories.
            var dirs = file.FullName.Split('\\');
            foreach (var dir in dirs)
            {
                if (collection.FilteredDirectories.Contains(dir.ToLower()))
                    return false;
            }

            // Check for filtered regex expressions.
            foreach (var regex in collection.FilteredRegex)
            {
                try
                {
                    if (Regex.IsMatch(file.FullName, regex))
                        return false;
                }
                // Incase invalid regex causes exception.
                catch
                {

                }
            }

            return true;
        }
        public static bool FilePassesFilter(this FilterCollection collection, System.IO.FileInfo file)
        {


            return true;
        }
    }
}
