using System.IO;
using System.Collections.Generic;
using Godot;
using System;
using System.Linq;

public partial class ProjectCache : Cache
{
    private readonly int[] READABLE_CONFIG_VERSIONS = [5];
    public enum ImportError { OK, PROJECT_READ_FAIL, PROJECT_DUPLICATE };

    private Dictionary<string, ProjectDataState> _projects = [];

    public List<string> ProjectNames => [.. _projects.Keys];
    public int ProjectCount => _projects.Count;

    #region Singleton Instance
    private static ProjectCache _instance;

    public static ProjectCache Instance => _instance;

    private ProjectCache(string userDirectory) : base(userDirectory, "/ProjectCache.gdhub") => LoadData();

    public static ProjectCache Initialize(string userDirectory)
    {
        lock (padlock)
        {
            _instance ??= new ProjectCache(userDirectory);
            return _instance;
        }
    }
    #endregion

    public bool DoesAnyProjectHaveThisTag(string tagName, bool isSoftware)
    {
        foreach (KeyValuePair<string, ProjectDataState> entry in _projects)
        {
            if (entry.Value.ContainsTag(tagName, isSoftware)) return true;
        }
        return false;
    }

    public void ScanProjects(string[] paths)
    {
        _isDirty = true;

        List<string> cachedProjects = [];
        foreach (KeyValuePair<string, ProjectDataState> projectEntry in _projects)
            if (projectEntry.Value.Build != VersionData.BuildType.UNKNOWN)
                cachedProjects.Add(projectEntry.Key);

        Dictionary<string, ProjectDataState> tempProjects = [];
        foreach (string path in paths)
        {
            if (!Directory.Exists(path)) continue;

            IEnumerable<string> folderPaths = Directory.EnumerateDirectories(path);
            foreach (string folderPath in folderPaths)
            {
                string sanitizedPath = folderPath.Replace("\\", "/");
                ProjectDataState project = CreateProjectFromConfig(sanitizedPath);
                if (project == null) continue;  // Skip non project folders

                // Skip dupes
                if (tempProjects.ContainsKey(project.projectName)) continue;

                // See if build data can be transfered over
                if (cachedProjects.Contains(project.projectName))
                {
                    ProjectDataState oldProject = _projects[project.projectName];
                    if (oldProject.VersionData == project.VersionData &&
                        oldProject.Renderer == project.Renderer)
                    {
                        project.SetBuild(oldProject.Build);
                        project.IsFavorited = oldProject.IsFavorited;
                    }
                }

                tempProjects.Add(project.projectName, project);
            }
        }
        _projects.Clear();
        _projects = tempProjects;
    }

    public ImportError ImportProject(string path)
    {
        string sanitizedPath = SanitizeProjectPath(path);
        ProjectDataState project = CreateProjectFromConfig(sanitizedPath);
        if (project == null) return ImportError.PROJECT_READ_FAIL;  // Skip non project folders

        // Skip dupes
        if (_projects.ContainsKey(project.projectName)) return ImportError.PROJECT_DUPLICATE;

        _projects.Add(project.projectName, project);
        return ImportError.OK;
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
        if (!_isDirty) return;

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

    private ProjectDataState CreateProjectFromConfig(string directoryPath)
    {
        // Load config file
        Tuple<ConfigFile, string> configLoadData = LoadProjectConfig(directoryPath);
        // Check if failed to load
        if (configLoadData == null)
        {
            // GD.PushError("Failed to load ", directoryPath);
            return null;
        }
        // Check if valid config
        int configVersion = (int)configLoadData.Item1.GetValue("", "config_version", -1);
        if (!READABLE_CONFIG_VERSIONS.Contains(configVersion)) return null; // Invalid version

        // Create a project object
        ProjectDataState project = new();
        if (!project.LoadUncached(ref configLoadData, ref directoryPath)) return null;

        return project;
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

    private static string SanitizeProjectPath(string path)
    {
        string unixPath = path.Replace("\\", "/");
        if (unixPath.EndsWith(".gdhub"))
            return unixPath.Replace("/.gdhub", "");
        else if (unixPath.EndsWith("project.godot"))
            return unixPath.Replace("/project.godot", "");

        return unixPath;
    }
}