using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public partial class TemplateCache : Cache
{
    private const string ICON_SVG = @"<svg height=""128"" width=""128"" xmlns=""http://www.w3.org/2000/svg""><rect x=""2"" y=""2""
width=""124"" height=""124"" rx=""14"" fill=""#363d52"" stroke=""#212532"" stroke-width=""4""/><g
transform=""scale(.101) translate(122 122)""><g fill=""#fff""><path d=""M105 673v33q407 354 814 0v-33z""/>
<path d=""m105 673 152 14q12 1 15 14l4 67 132 10 8-61q2-11 15-15h162q13 4 15 15l8 61 132-10 4-67q3-13
15-14l152-14V427q30-39 56-81-35-59-83-108-43 20-82 47-40-37-88-64 7-51 8-102-59-28-123-42-26 43-46
89-49-7-98 0-20-46-46-89-64 14-123 42 1 51 8 102-48 27-88 64-39-27-82-47-48 49-83 108 26 42 56 81zm0
33v39c0 276 813 276 814 0v-39l-134 12-5 69q-2 10-14 13l-162 11q-12 0-16-11l-10-65H446l-10 65q-4
11-16 11l-162-11q-12-3-14-13l-5-69z"" fill=""#478cbf""/><path d=""M483 600c0 34 58 34 58
0v-86c0-34-58-34-58 0z""/><circle cx=""725"" cy=""526"" r=""90""/><circle cx=""299"" cy=""526"" r=""90""/></g><g
fill=""#414042""><circle cx=""307"" cy=""532"" r=""60""/><circle cx=""717"" cy=""532"" r=""60""/></g></g></svg>";
    private const int VERSION_FLAG = 1;
    public static readonly string[] DEFAULT_TEMPLATES = ["Default - Empty", "Default - Git"];

    private readonly string _ASSET_DIRECTORY;
    private readonly string _ASSET_METADATA_FILE_PATH;
    private readonly string _TEMPLATE_DIRECTORY;

    #region Singleton Instance
    private static TemplateCache _instance;

    public static TemplateCache Instance => _instance;

    private TemplateCache(string userDirectory) : base(userDirectory, "/TemplateCache.gdhub")
    {
        _ASSET_DIRECTORY = userDirectory + "template_assets";
        _ASSET_METADATA_FILE_PATH = _ASSET_DIRECTORY + "/metadata.gdhub";
        _TEMPLATE_DIRECTORY = userDirectory + "templates";
        LoadData();
    }

    public static TemplateCache Initialize(string userDirectory)
    {
        lock (padlock)
        {
            _instance ??= new TemplateCache(userDirectory);
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

        if (Directory.Exists(_TEMPLATE_DIRECTORY))
        {
            string[] files = Directory.GetFiles(_TEMPLATE_DIRECTORY);
            foreach (string filePath in files)
                ReadTemplateFile(filePath);
        }
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
            GenerateIconSVG(iconPath);
            iconCreated = true;
        }

        if (!_fileDatabase.ContainsKey("icon.svg"))
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

            //* Write to file
            // Fill folder state
            file.WriteLine(entry.Value.FillFolders ? "true" : "false");

            // Project tags
            string projectTagsCompiled = CompileTagsToStr(entry.Value.ProjectTags);
            file.WriteLine(projectTagsCompiled);

            // Software tags
            string softwareTagsCompiled = CompileTagsToStr(entry.Value.SoftwareTags);
            file.WriteLine(softwareTagsCompiled);

            // Data
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
            if (!_fileDatabase.ContainsKey(name))
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
        int readLines = 1;
        string line = file.ReadLine();
        while (line != null)
        {
            //* State Data
            if (readLines == 1)
            {
                _ROM[templateName].FillFolders = line == "true";
            }
            else if (readLines < 4) //* Tag Data
            {
                if (line == "")
                {
                    line = file.ReadLine();
                    readLines++;
                    continue;
                }

                string[] tags = line.Split(" | ", StringSplitOptions.RemoveEmptyEntries);
                if (tags.Length == 0)
                {
                    line = file.ReadLine();
                    readLines++;
                    continue;
                }
                if (readLines == 2)
                    _ROM[templateName].BulkAddProjectTags(tags);
                else if (readLines == 3)
                    _ROM[templateName].BulkAddSoftwareTags(tags);
            }
            else    //* Template Files
            {
                string[] parts = line.Split(" | ", StringSplitOptions.RemoveEmptyEntries);
                string fileName = parts[0];
                string fileTag = parts[1];
                string structurePath = "";
                if (parts.Length > 2) structurePath = parts[2];

                _ROM[templateName].LoadFileData(ref fileName, ref fileTag, ref structurePath);
            }

            line = file.ReadLine();
            readLines++;
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

    private static string CompileTagsToStr(string[] tags)
    {
        string tagStr = "";
        int i = 0;
        int maxCount = tags.Length;
        foreach (string tag in tags)
        {
            tagStr += tag;
            if (i < maxCount - 1) tagStr += " | ";
            i++;
        }
        return tagStr;
    }

    private static void GenerateIconSVG(string location)
    {
        StreamWriter file = new(location);
        file.WriteLine(ICON_SVG);
        file.Close();
    }
}