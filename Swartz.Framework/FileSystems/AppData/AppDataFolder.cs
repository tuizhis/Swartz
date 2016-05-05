using System;
using System.Collections.Generic;
using System.IO;
using Swartz.Caching;
using Swartz.Logging;

namespace Swartz.FileSystems.AppData
{
    public class AppDataFolder : IAppDataFolder
    {
        private readonly IAppDataFolderRoot _root;

        public AppDataFolder(IAppDataFolderRoot root)
        {
            _root = root;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public IEnumerable<string> ListFiles(string path)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> ListDirectories(string path)
        {
            throw new NotImplementedException();
        }

        public string Combine(params string[] paths)
        {
            throw new NotImplementedException();
        }

        public bool FileExists(string path)
        {
            throw new NotImplementedException();
        }

        public void CreateFile(string path, string content)
        {
            throw new NotImplementedException();
        }

        public Stream CreateFile(string path)
        {
            throw new NotImplementedException();
        }

        public string ReadFile(string path)
        {
            throw new NotImplementedException();
        }

        public Stream OpenFile(string path)
        {
            throw new NotImplementedException();
        }

        public void StoreFile(string sourceFileName, string destinationPath)
        {
            throw new NotImplementedException();
        }

        public void DeleteFile(string path)
        {
            throw new NotImplementedException();
        }

        public DateTime GetFileLastWriteTimeUtc(string path)
        {
            throw new NotImplementedException();
        }

        public void CreateDirectory(string path)
        {
            throw new NotImplementedException();
        }

        public bool DirectoryExists(string path)
        {
            throw new NotImplementedException();
        }

        public IVolatileToken WhenPathChanges(string path)
        {
            throw new NotImplementedException();
        }

        public string MapPath(string path)
        {
            throw new NotImplementedException();
        }

        public string GetVirtualPath(string path)
        {
            throw new NotImplementedException();
        }
    }
}