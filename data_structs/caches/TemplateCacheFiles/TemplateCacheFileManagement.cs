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

        if (!CheckForDefaultFiles())
            InitializeDefaultTemplates();

        ReadAssetFile();
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
        bool iconCreated = false;
        if (!File.Exists(iconPath))
        {
            File.Copy(_ICON_SVG_PATH, iconPath);
            iconCreated = true;
        }

        if (!_fileDatabase.ContainsKey("icon.svg") && iconCreated)
            _fileDatabase.Add("icon.svg", iconPath);

        if (iconCreated)
            WriteAssetFile();

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
        OSAPI.CreateDirectoryIfNotExists(_ASSET_DIRECTORY);

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

        OSAPI.CreateDirectoryIfNotExists(_TEMPLATE_DIRECTORY);

        foreach (KeyValuePair<string, TemplateStructure> entry in _ROM)
        {
            string filePath = $"{_TEMPLATE_DIRECTORY}/{entry.Key}.gdhub";
            StreamWriter file = new(filePath);
            file.WriteLine($"Version={VERSION_FLAG}");

            // Write to file
            WriteTemplateFilesHelper(ref file, entry.Value.RootFolder, "");

            file.Close();
        }
    }

    private bool ReadAssetFile()
    {
        if (!OSAPI.CreateDirectoryIfNotExists(_ASSET_DIRECTORY)) return false;
        if (!DoesFileExistAndContainData(_ASSET_METADATA_FILE_PATH)) return false;

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
        string templateName = unixPath.Split("/", StringSplitOptions.RemoveEmptyEntries)
                            .Last()
                            .Replace(".gdhub", "");

        string versionStr = file.ReadLine();
        if (versionStr == null) return;
        if (!int.TryParse(versionStr.Split("=")[1], out int result)) return;
        if (result != VERSION_FLAG) return;

        // Load Template
        LoadROMTemplate(templateName);
        string line = file.ReadLine();
        while (line != null)
        {
            string[] parts = line.Split(" | ", StringSplitOptions.RemoveEmptyEntries);
            string fileName = parts[0];
            string fileTag = parts[1];
            string structurePath = "";
            if (parts.Length > 2) structurePath = parts[2];

            _ROM[templateName].LoadFileData(ref fileName, ref fileTag, ref structurePath);

            line = file.ReadLine();
        }
        file.Close();
    }

    private void LoadROMTemplate(string templateName)
    {
        if (_ROM.ContainsKey(templateName)) return;
        _ROM.Add(templateName, new(templateName));
    }

    private bool CheckForDefaultFiles()
    {
        if (!OSAPI.CreateDirectoryIfNotExists(_ASSET_DIRECTORY)) return false;
        if (!DoesFileExistAndContainData(_ASSET_METADATA_FILE_PATH)) return false;
        if (!OSAPI.CreateDirectoryIfNotExists(_TEMPLATE_DIRECTORY)) return false;
        if (Directory.GetFiles(_TEMPLATE_DIRECTORY).Length == 0) return false;

        return true;
    }

    private static void WriteTemplateFilesHelper(ref StreamWriter file, TemplateStructure.Folder currentFolder,
        string path)
    {
        foreach (string fileName in currentFolder.FileNames)
            // FileName | FileTag | Path to PWD
            file.WriteLine($"{fileName} | {currentFolder.GetFileTag(fileName)} | {path}");

        foreach (string folderName in currentFolder.FolderNames)
            WriteTemplateFilesHelper(ref file, currentFolder.GetFolder(folderName), $"{path}/{folderName}");
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

    private static bool DoesFileExistAndContainData(string path)
    {
        if (!File.Exists(path)) return false;
        if (new FileInfo(path).Length == 0) return false;
        return true;
    }
}