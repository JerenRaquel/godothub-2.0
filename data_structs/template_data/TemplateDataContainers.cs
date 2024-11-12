using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public partial class TemplateData
{
    private readonly static string[] ONLY_ONCE = ["project.godot", ".gitignore", ".gitattributes"];

    public struct DataNode
    {
        private bool _isNull = true;
        private bool _isRoot = false;
        private Dictionary<string, DataNode> _folders;
        private Dictionary<string, string> _files;

        public string Name { get; private set; }
        public string[] Files
        {
            get
            {
                string[] data = [.. _files.Keys];
                Array.Sort(data);
                return data;
            }
        }
        public readonly bool IsNull => _isNull;
        public readonly bool IsRoot => _isRoot;
        public readonly long Count => !_isNull ? _folders.Count + _files.Count : 0;

        public DataNode() { }

        public DataNode(string name, bool isRoot = false)
        {
            Name = name;
            _folders = [];
            _files = [];
            _isNull = false;
            _isRoot = isRoot;
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

        public static void ReadFromFile(ref DataNode currentNode, StreamReader file)
        {
            string buffer = file.ReadLine();
            if (buffer == null) return;
            if (!buffer.StartsWith('[') || !buffer.EndsWith(']')) return;

            if (!currentNode.IsRoot)
            {

            }

            buffer = file.ReadLine();
            while (buffer != null)
            {
                if (IsNodeHeader(buffer))
                {
                    DataNode nextNode = currentNode.AddFolder(GetNodeName(buffer));
                    ReadFromFile(ref nextNode, file);
                }
                else
                {
                    string[] parts = buffer.Split(" | ", System.StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length == 2)
                        currentNode._files.Add(parts[0], parts[1]);
                }

                buffer = file.ReadLine();
            }
        }

        private static bool IsNodeHeader(string buffer)
        {
            return buffer.StartsWith('[') && buffer.EndsWith(']');
        }

        private static string GetNodeName(string buffer)
        {
            return buffer.Split("/", System.StringSplitOptions.RemoveEmptyEntries).Last();
        }
    }
}