using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DataContainer.DatabaseSys.Databases.TagDatabase;
using XMLSystem;

namespace DataContainer.DatabaseSys.Databases.TemplateDatabase
{
    public partial class TemplateData
    {
        private static readonly HashSet<string> SPECIAL_FILES = [
            ".gitignore", ".gitattributes",
            "icon.svg", "project.godot",
            ".temp",
            ".gdhub", ".gdextension"
        ];

        private HashSet<TagKey> _projectTags = [];
        private HashSet<TagKey> _softwareTags = [];
        private List<FileData> _buildInstructions = [];

        public string Name { get; private set; }

        private TemplateData() { }

        public TagKey[] GetTags(TagDatabase.TagDatabase.TagFlag flag)
        {
            return flag switch
            {
                TagDatabase.TagDatabase.TagFlag.PROJECT => [.. _projectTags],
                TagDatabase.TagDatabase.TagFlag.SOFTWARE => [.. _softwareTags],
                TagDatabase.TagDatabase.TagFlag.ANY => [.. _projectTags, .. _softwareTags],
                _ => [],
            };
        }

        public bool Generate(in string path)
        {
            FileData? gdhub = null;
            foreach (FileData fileData in _buildInstructions)
            {
                if (!fileData.IsFolder
                    && fileData.metadata.ContainsKey("godotProjectRelativePath"))
                {
                    gdhub = fileData;
                    break;
                }
            }

            foreach (FileData fileData in _buildInstructions)
            {
                //* Create the folders
                string directoryPath = path + "/";
                if (fileData.IsFolder)
                    directoryPath += string.Join('/', fileData.path);
                else
                {
                    Span<string> pathSpan = new(fileData.path);
                    Span<string> parts = pathSpan[..^2];
                    directoryPath += string.Join('/', parts.ToArray());
                }
                Directory.CreateDirectory(directoryPath);

                //* Create the files
                if (!fileData.IsFolder)
                    if (!CreateFile(in fileData, in directoryPath, in gdhub))
                        return false;
            }

            return true;
        }

        public override string ToString()
        {
            string results = Name + '\n';
            foreach (FileData fileData in _buildInstructions)
                results += $"\t{fileData}";
            return results;
        }

        public static TemplateData CreateFromXMLNode(in XMLNode rootNode,
                            in string fileName)
        {
            TemplateData template = new();

            //* Set the name
            if (!HandleTemplateElement(in template, in rootNode, in fileName))
                return null;

            //* Check to make sure only <Tags/> and <Structure/> exists
            if (rootNode.ChildCount != 2)
            {
                LogError("More than <Tags/> and <Structure/> is included.");
                return null;
            }

            //* Set the tags
            XMLNode tagsNode = rootNode.GetNode("Tags");
            if (!HandleTagsElement(in template, in tagsNode)) return null;

            //* Set the build instructions
            XMLNode structureNode = rootNode.GetNode("Structure");
            if (!HandleStructureElement(in template, in structureNode))
                return null;

            return template;
        }

        private static bool CreateFile(in FileData fileData, in string directory,
            in FileData? gdhubFile)
        {
            string fileName = fileData.path.Last();
            if (SPECIAL_FILES.Contains(fileName))
                return CreateSpecialFile(
                    in fileData,
                    in directory,
                    in fileName,
                    in gdhubFile
                );

            //* Copy the file from the ref location to the project location

            return true;
        }

        private static bool CreateSpecialFile(in FileData fileData,
            in string directory, in string fileName, in FileData? gdhubFile)
        {
            switch (fileName)
            {
                case ".gitignore":
                    string godotCache = ".godot/";
                    if (gdhubFile.HasValue)
                    {
                        godotCache
                            = gdhubFile.Value.metadata["godotProjectRelativePath"]
                            + ".godot/";
                    }
                    List<string> lines = [
                        "# Godot 4+ specific ignores",
                        godotCache,
                        "/android/",
                    ];

                    if (gdhubFile.HasValue)
                    {
                        lines.Add("");
                        lines.Add("# GDExtension - CPP");
                        lines.Add("bin/*");
                        lines.Add("godot-cpp/*");
                    }
                    return CreatePlainTextFile(fileName, directory, [.. lines]);

                case ".gitattributes":
                    return CreatePlainTextFile(
                        fileName, directory,
                        [
                            "# Normalize EOL for all files that Git considers text files.",
                            "* text=auto eol=lf"
                        ]
                    );

                case "icon.svg":
                    // TODO: Finish
                    break;

                case "project.godot":
                    // TODO: Finish
                    break;

                case ".temp":
                    return CreatePlainTextFile(
                        "ReplaceMeWithFiles.temp", directory, []
                    );

                case ".gdhub":
                    return CreatePlainTextFile(
                        fileName, directory,
                        [fileData.metadata["godotProjectRelativePath"]]
                    );

                case ".gdextension":
                    // TODO: Finish
                    break;

                default:
                    return false;
            }
            return true;
        }

        private static bool CreatePlainTextFile(in string fileName, in string path,
            in string[] lines)
        {
            try
            {
                StreamWriter file = new(path + fileName, false);
                foreach (string line in lines)
                    file.WriteLine(line);
                file.Close();
                return true;
            }
            catch
            { return false; }
        }
    }
}