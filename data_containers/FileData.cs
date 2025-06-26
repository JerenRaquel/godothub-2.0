using System.Collections.Generic;

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
    }
}