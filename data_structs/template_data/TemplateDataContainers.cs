using System.Collections.Generic;
using System.IO;
using System.Linq;

public partial class TemplateData
{
    private readonly static string[] ONLY_ONCE = ["project.godot", ".gitignore", ".gitattributes"];

    public struct DataNode
    {
        private bool _isNull = true;
        private Dictionary<string, DataNode> _folders;
        private Dictionary<string, string> _files;

        public string Name { get; private set; }
        public readonly bool IsNull => _isNull;
        public readonly long Count => !_isNull ? _folders.Count + _files.Count : 0;

        public DataNode() { }

        public DataNode(string name)
        {
            Name = name;
            _folders = [];
            _files = [];
            _isNull = false;
        }

        public readonly DataNode AddFolder(string folderName)
        {
            if (IsNull) return new();
            if (_folders.ContainsKey(folderName)) return new();

            DataNode node = new(folderName);
            _folders.Add(folderName, node);
            return node;
        }

        public readonly void AddFile(string fileName)
        {
            if (IsNull) return;

            string name = fileName;
            if (_files.ContainsKey(name))
            {
                // Not Adding more than one
                if (ONLY_ONCE.Contains(fileName)) return;

                long i = 1;
                name = $"{fileName} ({i})";
                while (_files.ContainsKey(name))
                {
                    i++;
                    name = $"{fileName} ({i})";
                }
            }

            _files.Add(name, fileName);
        }

        public static void WriteToFile(string nodePath, DataNode currentNode,
            ref StreamWriter file)
        {
            string newPath = $"[{nodePath}/{currentNode.Name}]";
            file.WriteLine(newPath);
            foreach (KeyValuePair<string, string> entry in currentNode._files)
                file.WriteLine(entry.Key + " | " + entry.Value);

            foreach (KeyValuePair<string, DataNode> entry in currentNode._folders)
                WriteToFile(newPath, entry.Value, ref file);
        }
    }
}