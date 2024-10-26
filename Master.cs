using Godot;
using System;

public partial class Master : Node
{
    private string[] paths = { "C:/Users/Jeren/Godot Projects" };
    private ProjectCache _cache;

    // TEMP: Remove once integrated
    public override void _ExitTree()
    {
        _cache.ForceWrite();
    }

    public override void _Ready()
    {
        _cache = new ProjectCache(ProjectSettings.GlobalizePath("user://"));
        _cache.ScanProjects(paths);
    }
}
