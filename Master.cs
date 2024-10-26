using Godot;
using System;

public partial class Master : Node
{
    private LinkButton _godotHUB;

    public override void _EnterTree() => ProjectCache.Initialize(ProjectSettings.GlobalizePath("user://"));

    public override void _ExitTree()
    {
        // TEMP: Remove once integrated
        ProjectCache.Instance.ForceWrite();
    }

    public override void _Ready()
    {
        _godotHUB = GetNode<LinkButton>("%GodotHUB");

        _godotHUB.Text = ProjectSettings.GetSetting("application/config/version") + " | GodotHUB Github";
    }
}
