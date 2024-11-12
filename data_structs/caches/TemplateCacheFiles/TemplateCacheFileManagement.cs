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
        _ROM = new();
        _RAM = new();

        if (!ReadAssetFile()) InitializeDefaultTemplates();
        _ROM.OverwriteWith(_RAM);

        string[] files = Directory.GetFiles(_TEMPLATE_DIRECTORY);
        foreach (string filePath in files)
            ReadTemplateFile(filePath);
        _RAM.OverwriteWith(_ROM);

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
        _ROM.OverwriteWith(_RAM);

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

        TemplateData.DataNode emptyRoot = _RAM.AddTemplate("Default - Empty");
        emptyRoot.AddFile("project.godot");
        emptyRoot.AddFile("icon.svg");

        TemplateData.DataNode gitRoot = _RAM.AddTemplate("Default - Git");
        gitRoot.AddFile("project.godot");
        gitRoot.AddFile("icon.svg");
        gitRoot.AddFile(".gitignore");
        gitRoot.AddFile(".gitattributes");
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
        _ROM.OverwriteWith(_RAM);

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

        TemplateData.DataNode root = _ROM.AddTemplate(fileName);
        TemplateData.DataNode.ReadFromFile(ref root, file);
    }

}