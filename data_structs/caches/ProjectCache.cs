using System.IO;
using System.Collections.Generic;
using Godot;
using System;
using System.Linq;

public partial class ProjectCache : Cache
{
    private readonly int[] READABLE_CONFIG_VERSIONS = [5];

    private Dictionary<string, ProjectDataState> _projects = [];

    public List<string> ProjectNames => [.. _projects.Keys];
    public int ProjectCount => _projects.Count;

    #region Singleton Instance
    private static ProjectCache _instance;

    public static ProjectCache Instance => _instance;

    private ProjectCache(string userDirectory) : base(userDirectory + "/ProjectCache.gdhub")
    {
        LoadData();
    }

    public static ProjectCache Initialize(string userDirectory)
    {
        lock (padlock)
        {
            _instance ??= new ProjectCache(userDirectory);
            return _instance;
        }
    }
    #endregion

    public void ScanProjects(string[] paths)
    {
        _projects.Clear();
        _isDirty = true;

        foreach (string path in paths)
        {
            if (!Directory.Exists(path)) continue;

            IEnumerable<string> folderPaths = Directory.EnumerateDirectories(path);
            foreach (string folderPath in folderPaths)
            {
                string sanitizedPath = folderPath.Replace("\\", "/");
                CreateProjectFromConfig(sanitizedPath);
            }

        }
    }

    public override bool LoadData()
    {
        if (!File.Exists(SAVE_LOCATION)) return false;

        // Load
        using StreamReader file = new(SAVE_LOCATION);
        string line = file.ReadLine();
        while (line != null)
        {
            CreateProjectFromFileEntry(line);
            line = file.ReadLine();
        }

        return true;
    }

    public override void WriteData()
    {
        // Check if dirty, no point to write if not
        if (!this._isDirty) return;

        ForceWrite();
    }

    public override void ForceWrite()
    {
        // If there's no projects, no point to write
        if (_projects.Count == 0) return;

        using StreamWriter file = new(SAVE_LOCATION);
        foreach (KeyValuePair<string, ProjectDataState> entry in _projects)
        {
            entry.Value.WriteToROM();
            string jsonStr = entry.Value.Write();
            file.WriteLine(jsonStr);
        }
        file.Close();
    }

    public string GetProjectVersion(string projectName) => GetProject(projectName)?.VersionStr ?? "Unknown";

    public string GetLocalTime(string projectName) => GetProject(projectName)?.LastEdited.ToLocalTime().ToString() ?? "Unknown";

    public Texture2D GetIcon(string projectName) => GetProject(projectName)?.Icon ?? null;

    public long GetRawTime(string projectName) => GetProject(projectName)?.LastEdited.Ticks ?? 0;

    public string GetProjectPath(string projectName, bool prettify = false)
    {
        ProjectDataState data = GetProject(projectName);
        if (data == null) return "";

        return data.GetFullPath(prettify);
    }

    public string GetRenderer(string projectName)
    {
        ProjectData.Renderer renderer = GetProject(projectName)?.Renderer ?? ProjectData.Renderer.INVALID;
        return renderer switch
        {
            ProjectData.Renderer.COMPAT => "Compatibility",
            ProjectData.Renderer.MOBILE => "Mobile",
            ProjectData.Renderer.FORWARD => "Forward+",
            _ => "Unkwown"
        };
    }

    public bool HasTags(string projectName) => GetProject(projectName)?.HasTags ?? false;

    public bool IsFavorited(string projectName) => GetProject(projectName)?.IsFavorited ?? false;

    public bool UsesDotNet(string projectName) => GetProject(projectName)?.IsDotNet ?? false;

    public bool UsesGDExt(string projectName) => GetProject(projectName)?.IsGDExt ?? false;

    public string ProjectNameToPartialKey(string projectName)
    {
        ProjectDataState data = GetProject(projectName);
        return VersionData.GeneratePartialKey(data.VersionData, data.IsDotNet);
    }

    public string GenerateProjectMetadataString(string projectName, bool center = false)
    {
        string versionStr = GetProjectVersion(projectName);

        // TEMP: Requires the godot version cache
        VersionData.BuildType buildType = VersionData.BuildType.UNKNOWN;

        string buildStr = VersionData.BuildEnumToString(buildType);
        string renderStr = GetRenderer(projectName);
        string colorCode = renderStr switch
        {
            "Compatibility" => ColorTheme.Compat,
            "Mobile" => ColorTheme.Mobile,
            "Forward+" => ColorTheme.Forward,
            _ => ColorTheme.Unknown
        };

        string mainTextMETA = projectName.BBCodeColor(ColorTheme.BaseBlue)
            + $" [ v{versionStr} | ".BBCodeColor(ColorTheme.BaseBlue)
            + buildStr.BBCodeColor(ColorTheme.GetColorFromBuild(buildType)) + " ] ".BBCodeColor(ColorTheme.BaseBlue)
            + $"[{renderStr}]".BBCodeColor(colorCode);

        if (UsesGDExt(projectName))
            mainTextMETA += " [Uses GDExtension]".BBCodeColor(ColorTheme.HighlightBlue);

        if (UsesDotNet(projectName))
            mainTextMETA += " [Uses .NET]".BBCodeColor(ColorTheme.CSharp);

        if (center)
            return $"[center]{mainTextMETA}[/center]";
        else
            return mainTextMETA;
    }

    private ProjectDataState GetProject(string projectName)
    {
        if (projectName == null || projectName.Length == 0) return null;
        if (!_projects.TryGetValue(projectName, out ProjectDataState value)) return null;

        return value;
    }

    private void CreateProjectFromConfig(string directoryPath)
    {
        // Load config file
        Tuple<ConfigFile, string> configLoadData = LoadProjectConfig(directoryPath);
        // Check if failed to load
        if (configLoadData == null)
        {
            GD.PushError("Failed to load ", directoryPath);
            return;
        }
        // Check if valid config
        int configVersion = (int)configLoadData.Item1.GetValue("", "config_version", -1);
        if (!READABLE_CONFIG_VERSIONS.Contains<int>(configVersion)) return; // Invalid version

        // Create a project object
        ProjectDataState project = new ProjectDataState();
        if (!project.LoadUncached(ref configLoadData, ref directoryPath)) return;
        if (_projects.ContainsKey(project.projectName)) return;

        _projects.Add(project.projectName, project);
    }

    private void CreateProjectFromFileEntry(string cachedData)
    {
        ProjectDataState project = new();
        if (!project.LoadCached(ref cachedData)) return;
        if (_projects.ContainsKey(project.projectName)) return;

        _projects.Add(project.projectName, project);
    }

    private static Tuple<ConfigFile, string> LoadProjectConfig(string folderPath)
    {
        ConfigFile config = new();

        // First Attempt
        string projectPath = folderPath + "/project.godot";
        // Check if it loaded
        if (config.Load(projectPath) == Error.Ok)
            return new Tuple<ConfigFile, string>(config, "");

        // Second Attempt -- GDExt Support/Meta Search (look for .gdhub)
        projectPath = folderPath + "/.gdhub";
        // Check if it exists
        if (!File.Exists(projectPath)) return null;  // Couldn't find it

        // Found it -- .gdhub
        StreamReader sr = new StreamReader(projectPath);
        string relativeProjectPath = sr.ReadLine();
        if (relativeProjectPath.Length == 0) return null;
        sr.Close();

        // Check if the relative path is valid
        projectPath = folderPath + "/" + relativeProjectPath + "/project.godot";
        if (File.Exists(projectPath))
            // Check if it loaded
            if (config.Load(projectPath) == Error.Ok)
                return new Tuple<ConfigFile, string>(config, relativeProjectPath);

        GD.PushError("Couldn't locate project.godot using path: ", projectPath);
        return null;
    }
}