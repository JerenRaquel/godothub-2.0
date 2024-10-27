using System;
using System.Collections.Generic;
using System.Linq;

public partial class ProjectData
{
    public enum Renderer { INVALID, COMPAT, MOBILE, FORWARD }

    private string _path;
    private string _projectPath;
    private bool _favorited;

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
    public bool IsFavorited => _favorited;
    public string RendererString => renderer switch
    {
        Renderer.COMPAT => "GL Compatibility",
        Renderer.MOBILE => "Mobile",
        Renderer.FORWARD => "Forward Plus",
        _ => "Unknown"
    };

    public ProjectData(string version, string renderer, string path, string gdextPathExtra,
        bool isFavorited, string[] projectTags, string[] softwareTags)
    {
        this._path = path;
        this._projectPath = gdextPathExtra;
        this._favorited = isFavorited;
        this.projectTags = new List<string>(projectTags);
        this.softwareTags = new List<string>(softwareTags);

        string[] parts = version.Split(".");
        try
        {
            this.version = new Version(Int32.Parse(parts[0]), Int32.Parse(parts[1]));
        }
        catch (System.Exception)
        {
            this.version = new Version();
        }

        this.renderer = renderer switch
        {
            "GL Compatibility" => Renderer.COMPAT,
            "Mobile" => Renderer.MOBILE,
            "Forward Plus" => Renderer.FORWARD,
            _ => Renderer.INVALID,
        };
    }

    public ProjectData(ProjectData copy)
    {
        _path = copy._path;
        _projectPath = copy._projectPath;
        _favorited = copy._favorited;
        projectTags = [.. copy.projectTags];
        softwareTags = [.. copy.softwareTags];
        version = new(copy.version);
        renderer = copy.renderer;
    }

    public override string ToString()
    {
        return "=================\n"
        + version.ToString() + "\n"
        + ProjectGodotPath + "\n"
        + "=================";
    }
}
