using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public partial class TemplateCache : Cache
{
    private const int VERSION_FLAG = 1;

    private readonly string _ASSET_DIRECTORY;
    private readonly string _ASSET_METADATA_FILE_PATH;
    private readonly string _TEMPLATE_DIRECTORY;
    private readonly string _ICON_SVG_PATH;

    #region Singleton Instance
    private static TemplateCache _instance;

    public static TemplateCache Instance => _instance;

    private TemplateCache(string userDirectory, string iconSVG) : base(userDirectory, "/TemplateCache.gdhub")
    {
        _ASSET_DIRECTORY = userDirectory + "template_assets";
        _ASSET_METADATA_FILE_PATH = _ASSET_DIRECTORY + "/metadata.gdhub";
        _TEMPLATE_DIRECTORY = userDirectory + "templates";
        _ICON_SVG_PATH = iconSVG;
        LoadData();
    }

    public static TemplateCache Initialize(string userDirectory, string iconSVG)
    {
        lock (padlock)
        {
            _instance ??= new TemplateCache(userDirectory, iconSVG);
            return _instance;
        }
    }
    #endregion

    public override bool LoadData()
    {
        _ROM = [];
        _RAM = [];

        if (!ReadAssetFile()) InitializeDefaultTemplates();
        OverWrite(_RAM, _ROM);

        string[] files = Directory.GetFiles(_TEMPLATE_DIRECTORY);
        foreach (string filePath in files)
            ReadTemplateFile(filePath);
        OverWrite(_ROM, _RAM);

        return true;
    }

    public override void WriteData()
    {
        // Check if dirty, no point to write if not
        if (!_isDirty) return;

        ForceWrite();
    }

    public override void ForceWrite()
    {
        OverWrite(_RAM, _ROM);

        WriteAssetFile();
        WriteTemplateFiles();
        _isDirty = false;
    }

    private void InitializeDefaultTemplates()
    {
        string iconPath = _ASSET_DIRECTORY + "/icon.svg";
        if (!File.Exists(iconPath))
        {
            File.Copy(_ICON_SVG_PATH, iconPath);
            _fileDatabase.Add("icon.svg", iconPath);
            WriteAssetFile();
        }

        AddTemplate("Default - Empty");
        AddFileToTemplate("Default - Empty", "", "project.godot");
        AddFileToTemplate("Default - Empty", "", "icon.svg");

        AddTemplate("Default - Git");
        AddFileToTemplate("Default - Git", "", "project.godot");
        AddFileToTemplate("Default - Git", "", "icon.svg");
        AddFileToTemplate("Default - Git", "", ".gitignore");
        AddFileToTemplate("Default - Git", "", ".gitattributes");
    }

    private void WriteAssetFile()
    {
        if (!Directory.Exists(_ASSET_DIRECTORY))
            Directory.CreateDirectory(_ASSET_DIRECTORY);

        using StreamWriter file = new(_ASSET_METADATA_FILE_PATH, false);
        if (_fileDatabase.Count == 0)
        {
            file.Close();
            return;
        }

        foreach (KeyValuePair<string, string> fileData in _fileDatabase)
        {
            file.WriteLine(fileData.Key);   // Name
            file.WriteLine(fileData.Value); // Path
        }
    }

    private void WriteTemplateFiles()
    {
        OverWrite(_RAM, _ROM);

        if (!Directory.Exists(_TEMPLATE_DIRECTORY))
            Directory.CreateDirectory(_TEMPLATE_DIRECTORY);

        using (Dictionary<string, TemplateData.DataNode>.Enumerator romEnumerator = _ROM.RawTemplateData)
        {
            while (romEnumerator.MoveNext())
            {
                KeyValuePair<string, TemplateData.DataNode> entry = romEnumerator.Current;
                if (entry.Value.IsNull) continue;

                string filePath = _TEMPLATE_DIRECTORY + "/" + entry.Key + ".gdhub";

                StreamWriter file = new(filePath);
                file.WriteLine($"Version={VERSION_FLAG}");
                TemplateData.DataNode.WriteToFile("", entry.Value, ref file);
                file.Close();
            }
        }
    }

    private bool ReadAssetFile()
    {
        if (!Directory.Exists(_ASSET_DIRECTORY))
        {
            Directory.CreateDirectory(_ASSET_DIRECTORY);
            return false;
        }
        if (!File.Exists(_ASSET_METADATA_FILE_PATH)) return false;
        if (new FileInfo(_ASSET_METADATA_FILE_PATH).Length == 0) return false;

        using StreamReader file = new(_ASSET_METADATA_FILE_PATH);
        string name = file.ReadLine();
        string path = file.ReadLine();
        while (name != null && path != null)
        {
            _fileDatabase.Add(name, path);
            name = file.ReadLine();
            path = file.ReadLine();
        }

        file.Close();
        return true;
    }

    private void ReadTemplateFile(string path)
    {
        string unixPath = path.Replace("\\", "/");
        using StreamReader file = new(unixPath);
        string fileName = unixPath.Split("/", StringSplitOptions.RemoveEmptyEntries)
                            .Last()
                            .Replace(".gdhub", "");

        string versionStr = file.ReadLine();
        if (versionStr == null) return;
        if (!int.TryParse(versionStr.Split("=")[1], out int result)) return;
        if (result != VERSION_FLAG) return;

        // TODO: Load Template
    }

    private static void OverWrite(Dictionary<string, TemplateStructure> copyFrom,
        Dictionary<string, TemplateStructure> copyTo)
    {
        foreach (KeyValuePair<string, TemplateStructure> entry in copyFrom)
        {
            if (copyTo.ContainsKey(entry.Key))
                copyTo[entry.Key].OverwriteWith(entry.Value);
            else
                copyTo.Add(entry.Key, entry.Value);
        }
    }
}