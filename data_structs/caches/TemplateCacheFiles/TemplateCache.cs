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

    public bool CopyFileToPath(string fileTag, string absPath)
    {
        if (AUTO_CREATED.Contains(fileTag))
        {
            // TODO: Create at destination

            return true;
        }
        // Couldn't find file to copy over
        if (!_fileDatabase.ContainsKey(fileTag)) return false;

        // TODO: Copy file over

        return true;    // Success
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

    public string GetFilePath(string fileName)
    {
        if (!_fileDatabase.TryGetValue(fileName, out string value)) return null;
        return value;
    }

}