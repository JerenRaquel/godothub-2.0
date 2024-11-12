using System.Collections.Generic;

public partial class TemplateCache : Cache
{
    private Dictionary<string, string> _fileDatabase = [];
    private TemplateData _RAM;
    private TemplateData _ROM;

    public string[] TemplateNames => _RAM.Templates;

    private string GetFilePath(string fileName)
    {
        if (!_fileDatabase.TryGetValue(fileName, out string value)) return null;
        return value;
    }
}