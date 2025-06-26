using System.Collections.Generic;
using DataContainer.DatabaseSys.Databases.TagDatabase;
using XMLSystem;

namespace DataContainer.DatabaseSys.Databases.TemplateDatabase
{
    public partial class TemplateData
    {
        private readonly struct FileData(in string[] path, in string fileReference)
        {
            public readonly string[] path = path;
            public readonly string fileReference = fileReference;
            public readonly bool IsFolder => fileReference == null;
            public readonly Dictionary<string, string> metadata = [];

            public override string ToString()
            {
                string results = string.Join('/', path);
                if (IsFolder)
                    results += '/';
                else if (fileReference != null
                    && (fileReference.Length > 0 || metadata.Count > 0))
                {
                    results += $" [ {fileReference}";
                    if (metadata.Count > 0)
                    {
                        foreach (KeyValuePair<string, string> entry in metadata)
                            results += "{ " + $"{entry.Key} : {entry.Value}" + " }";
                    }
                    results += " ]";
                }
                return results + '\n';
            }
        }

        private HashSet<TagKey> _projectTags = [];
        private HashSet<TagKey> _softwareTags = [];
        private List<FileData> _buildInstructions = [];

        public string Name { get; private set; }

        private TemplateData() { }

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
            if (!rootNode.HasSubNode("Tags") || !rootNode.HasSubNode("Structure"))
            {
                LogError("Missing <Tags/> and/or <Structure/>");
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
    }
}