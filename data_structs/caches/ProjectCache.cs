using System.IO;
using System.Collections.Generic;
using Godot;
using System;
using System.Linq;

public partial class ProjectCache : Cache
{
    private readonly string SAVE_LOCATION;
    private readonly int[] READABLE_CONFIG_VERSIONS = { 5 };

    private Dictionary<string, ProjectDataState> _projects
        = new Dictionary<string, ProjectDataState>();

    public ProjectCache(string userDirectory)
    {
        SAVE_LOCATION = userDirectory + "/ProjectCache.gdhub";
    }

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
        return false;
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
            string jsonStr = entry.Value.Write();
            file.WriteLine(jsonStr);
        }
    }

    private void CreateProjectFromConfig(string directoryPath)
    {
        // Load config file
        Tuple<ConfigFile, string> configLoadData = LoadProjectConfig(directoryPath);
        // Check if failed to load
        if (configLoadData == null) return;
        // Check if valid config
        int configVersion = (int)configLoadData.Item1.GetValue("", "config_version", -1);
        if (!READABLE_CONFIG_VERSIONS.Contains<int>(configVersion)) return; // Invalid version

        // Create a project object
        ProjectDataState project = new ProjectDataState();
        if (!project.LoadUncached(ref configLoadData, ref directoryPath)) return;
        if (_projects.ContainsKey(project.projectName)) return;

        _projects.Add(project.projectName, project);
    }

    private Tuple<ConfigFile, string> LoadProjectConfig(string folderPath)
    {
        ConfigFile config = new ConfigFile();

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
        if (File.Exists(projectPath + "/" + relativeProjectPath))
            return new Tuple<ConfigFile, string>(config, relativeProjectPath);

        return null;
    }
}