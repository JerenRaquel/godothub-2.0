using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public partial class TemplateCache : Cache
{
    private readonly static string[] AUTO_CREATED = ["project.godot", ".gitignore", ".gitattributes"];

    private Dictionary<string, string> _fileDatabase = [];
    private Dictionary<string, TemplateStructure> _RAM;
    private Dictionary<string, TemplateStructure> _ROM;

    public string[] TemplateNames => [.. _RAM.Keys];
    public string[] SortedTemplateNames
    {
        get
        {
            string[] names = [.. _RAM.Keys];
            Array.Sort(names);
            return names;
        }
    }

    public bool DoesAnyTemplateHaveThisTag(string tagName, bool isSoftware)
    {
        foreach (KeyValuePair<string, TemplateStructure> entry in _RAM)
            if (entry.Value.ContainsTag(tagName, isSoftware)) return true;
        return false;
    }

    public bool HasTemplate(string templateName) => _RAM.ContainsKey(templateName);

    public void AddTemplate(string templateName)
    {
        if (_RAM.ContainsKey(templateName)) return;
        _RAM.Add(templateName, new(templateName));
    }

    public void AddFileToTemplate(string templateName, string path, string fileTag)
    {
        if (_fileDatabase.ContainsKey(fileTag) && !AUTO_CREATED.Contains(fileTag)) return;
        if (!_RAM.TryGetValue(templateName, out TemplateStructure template)) return;

        template.AddFile(path, fileTag);
    }

    public TemplateStructure.Folder GetTemplateRootFolder(string templateName)
    {
        if (templateName == null) return new();
        if (!_RAM.TryGetValue(templateName, out TemplateStructure templateData)) return new();
        return templateData.RootFolder;
    }

    public TemplateStructure GetTemplate(string templateName)
    {
        if (templateName == null) return null;
        if (!_RAM.TryGetValue(templateName, out TemplateStructure templateData)) return null;
        return templateData;
    }

    public bool GetTemplateFillFoldersState(string templateName) => GetTemplate(templateName)?.FillFolders ?? false;

    public string GetFilePath(string fileName)
    {
        if (!_fileDatabase.TryGetValue(fileName, out string value)) return null;
        return value;
    }

}