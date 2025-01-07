using System;
using System.Collections.Generic;
using System.Linq;

public partial class ProjectData
{
    public enum Renderer { INVALID, COMPAT, MOBILE, FORWARD }

    private string _path;
    private string _projectPath;

    public List<string> projectTags;
    public List<string> softwareTags;
    public Version version;
    public Renderer renderer;

    public string RootPath => _path;
    public string ProjectPathAddtion => _projectPath;
    public string FullPath
    {
        get
        {
            if (_projectPath.Length == 0) return _path;
            return _path + "/" + _projectPath;
        }
    }
    public string ProjectGodotPath => FullPath + "/project.godot";
    public string RendererString => renderer switch
    {
        Renderer.COMPAT => "GL Compatibility",
        Renderer.MOBILE => "Mobile",
        Renderer.FORWARD => "Forward Plus",
        _ => "Unknown"
    };
    public bool IsFavorited { get; set; } = false;
    public VersionData.BuildType Build { get; set; } = VersionData.BuildType.UNKNOWN;

    public ProjectData(string version, string renderer, string path, string gdextPathExtra,
        bool isFavorited, string[] projectTags, string[] softwareTags)
    {
        _path = path;
        _projectPath = gdextPathExtra;
        IsFavorited = isFavorited;
        this.projectTags = new List<string>(projectTags);
        this.softwareTags = new List<string>(softwareTags);
        this.version = new(version);
        //* Godot's storage method
        this.renderer = renderer switch
        {
            "GL Compatibility" => Renderer.COMPAT,
            "Mobile" => Renderer.MOBILE,
            "Forward Plus" => Renderer.FORWARD,
            _ => Renderer.INVALID,
        };
        //* In the case where this is created from within the hub
        //* -- This is the hub's internal storage method
        if (this.renderer == Renderer.INVALID)
        {
            this.renderer = renderer switch
            {
                "Compatibility" => Renderer.COMPAT,
                "Mobile" => Renderer.MOBILE,
                "Forward+" => Renderer.FORWARD,
                _ => Renderer.INVALID,
            };
        }
    }

    public ProjectData(ProjectData copy)
    {
        _path = copy._path;
        _projectPath = copy._projectPath;
        IsFavorited = copy.IsFavorited;
        projectTags = [.. copy.projectTags];
        softwareTags = [.. copy.softwareTags];
        version = new(copy.version);
        renderer = copy.renderer;
        Build = copy.Build;
    }

    public override string ToString()
    {
        return "=================\n"
        + version.ToString() + "\n"
        + ProjectGodotPath + "\n"
        + "=================";
    }
}
