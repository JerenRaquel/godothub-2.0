using System.Collections.Generic;

public partial class TemplateStructure
{
    public struct Folder
    {
        private Dictionary<string, Folder> _folders;
        private Dictionary<string, string> _files;
        private bool _isNull;

        public string Name { get; set; }
        public readonly bool IsNull => _isNull;
        public readonly string[] FolderNames => [.. _folders.Keys];
        public readonly int FolderCount => _folders.Count;
        public readonly int FileCount => _files.Count;

        public Folder() { _isNull = true; }

        public Folder(string name)
        {
            Name = name;
            _folders = [];
            _files = [];
            _isNull = false;
        }

        public readonly string GetFileTag(string fileName)
        {
            if (_files.TryGetValue(fileName, out string fileTag)) return fileTag;
            return null;
        }

        public readonly Folder GetFolder(string folderName)
        {
            if (_folders.TryGetValue(folderName, out Folder result)) return result;
            return new();
        }

        public readonly bool FolderExists(string folderName)
        {
            if (_isNull) return false;
            return _folders.ContainsKey(folderName);
        }

        public readonly bool FileExists(string fileName)
        {
            if (_isNull) return false;
            return _files.ContainsValue(fileName);
        }

        public readonly string AddFolder(string folderName)
        {
            string name = folderName;
            if (FolderExists(folderName))
                name = GenerateDuplicateName(folderName, [.. _folders.Keys]);
            _folders.Add(name, new(name));
            return name;
        }

        public readonly void AddFile(string fileName)
        {
            string name = fileName;
            if (FileExists(fileName))
                name = GenerateDuplicateName(fileName, [.. _files.Keys]);
            _files.Add(name, fileName);
        }

        private static string GenerateDuplicateName(string originalName, string[] names)
        {
            // Search for the highest ID
            long id = 0;
            bool found = false;
            foreach (string name in names)
            {
                if (!name.StartsWith(originalName)) continue;

                string idStr = name.Replace(originalName + "_", "");
                if (!long.TryParse(idStr, out long result)) continue;

                if (result - id >= 1)
                {
                    id++;
                    found = true;
                    break;
                }
            }
            if (found)
                return originalName + $"_{id}";
            else
                return originalName + $"_{id + 1}";
        }
    }
}