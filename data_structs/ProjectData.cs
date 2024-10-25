using System;
using System.Collections.Generic;
using Godot;

public partial class ProjectData
{
    public struct Vec2I
    {
        public Vec2I(int major, int minor)
        {
            Major = major;
            Minor = minor;
        }

        public readonly int Major { get; }
        public readonly int Minor { get; }

        public override string ToString() => $"{Major}, {Minor}";
        public Vector2I ToGDVector() => new Vector2I(Major, Minor);
    }

    public enum Renderer { COMPAT, MOBILE, FORWARD }

    private string _path;
    private string _projectPath;
    private bool _favorited;

    public List<string> projectTags;
    public List<string> softwareTags;
    public Vec2I Version;
    public Renderer renderer;

    public ProjectData(string version, string renderer, string path, string gdextPathExtra,
        bool isFavorited, string[] projectTags, string[] softwareTags)
    {
        this._path = path;
        this._projectPath = gdextPathExtra;
        this._favorited = isFavorited;
        this.projectTags = new List<string>(projectTags);
        this.softwareTags = new List<string>(softwareTags);

        string[] parts = version.Split(".");
        int major, minor;
        try
        {
            major = Int32.Parse(parts[0]);
            minor = Int32.Parse(parts[1]);
        }
        catch (System.Exception) { throw; }
        this.Version = new Vec2I(major, minor);

        switch (renderer)
        {
            case "GL Compatibility":
                this.renderer = Renderer.COMPAT;
                break;
            case "Mobile":
                this.renderer = Renderer.MOBILE;
                break;
            case "Forward Plus":
                this.renderer = Renderer.FORWARD;
                break;
        }
    }
}
