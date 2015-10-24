using System;
using Directory = Pri.LongPath.Directory;
using File = Pri.LongPath.File;
using FileInfo = Pri.LongPath.FileInfo;
using DirectoryInfo = Pri.LongPath.DirectoryInfo;
using Path = Pri.LongPath.Path;
using IOException = System.IO.IOException;
using SeekOrigin = System.IO.SeekOrigin;
using FileOptions = System.IO.FileOptions;
using FileAttributes = System.IO.FileAttributes;
using FileMode = System.IO.FileMode;
using FileAccess = System.IO.FileAccess;
using FileNotFoundException = System.IO.FileNotFoundException;
using DirectoryNotFoundException = System.IO.DirectoryNotFoundException;

namespace ChangeTracker.Models
{
    public sealed class ChangedFile : IEquatable<ChangedFile>, IDisposable
    {
        internal FileInfo File { get; private set; }

        public ChangedFile(FileInfo file)
        {
            File = file;
        }

        /// <summary>
        /// Gets the absolute path of the file.
        /// </summary>
        public string FullPath
        {
            get
            {
                return File.FullName;
            }
        }

        public DateTime CreationTime
        {
            get
            {
                return File.CreationTime;
            }
        }

        public DateTime CreationTimeUtc
        {
            get
            {
                return File.CreationTimeUtc;
            }
        }

        public DateTime LastAccessTime
        {
            get
            {
                return File.LastAccessTime;
            }
        }

        public DateTime LastAccessTimeUtc
        {
            get
            {
                return File.LastAccessTimeUtc;
            }
        }

        public DateTime LastWriteTime
        {
            get
            {
                return File.LastWriteTime;
            }
        }

        public DateTime LastWriteTimeUtc
        {
            get
            {
                return File.LastWriteTimeUtc;
            }
        }

        /// <summary>
        /// Gets the name of the file with the extension.
        /// </summary>
        public string Name
        {
            get
            {
                return File.Name;
            }
        }

        /// <summary>
        /// Gets the name of the file without the extension.
        /// </summary>
        public string ShortName
        {
            get
            {
                return Path.GetFileNameWithoutExtension(File.Name);
            }
        }
        
        /// <summary>
        /// Gets the extension of the file.
        /// </summary>
        public string FileExtension
        {
            get
            {
                return File.Extension;
            }
        }

        /// <summary>
        /// Gets the name of the parent directory.
        /// </summary>
        public string DirectoryName
        {
            get
            {
                return File.Directory.Name;
            }
        }
        
        /// <summary>
        /// Determines if the absolute path points to an existing file.
        /// </summary>
        public bool Exists
        {
            get
            { 
                return File.Exists;
            }
        }

        /// <summary>
        /// Deletes the file.
        /// </summary>
        public void Delete()
        {
            if (File.Exists)
                File.Delete();
        }

        /// <summary>
        /// Copys the file to the given destination, optionally overwritting any existing version.
        /// </summary>
        /// <param name="destination">The absolute destionation to copy to.</param>
        /// <param name="overwrite">If true will overwrite an existing file if one exists.</param>
        public void Copy(string destination, bool overwrite)
        {
            string source = FullPath.Replace(@"\\?\", "");
            File.CopyTo(destination, overwrite);
        }

        public bool Equals(ChangedFile other)
        {
            if (ReferenceEquals(null, other))
                return false;
            if (ReferenceEquals(this, other))
                return true;

            return this.FullPath == other.FullPath;
        }

        public override bool Equals(object obj)
        {
            var other = obj as ChangedFile;
            return Equals(other);
        }

        public override int GetHashCode()
        {
            return FullPath.GetHashCode();
        }

        public static implicit operator ChangedFile(FileInfo file)
        {
            return new ChangedFile(file);
        }

        #region IDisposable Support
        private bool disposedValue = false;
        public void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    File = null;
                }

                disposedValue = true;
            }
        }
        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}
