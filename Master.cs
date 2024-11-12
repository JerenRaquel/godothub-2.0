using Godot;
using System;

public partial class Master : Node
{
    private LinkButton _godotHUB;

    public override void _EnterTree()
    {
        OSAPI.Initialize();
        string userDirectory = ProjectSettings.GlobalizePath("user://");
        SettingsCache.Initialize(userDirectory);
        VersionCache.Initialize(userDirectory);
        TagCache.Initialize(userDirectory);
        ProjectCache.Initialize(userDirectory);
        TemplateCache.Initialize(userDirectory, ProjectSettings.GlobalizePath("res://icon.svg"));
    }

    // TEMP: Replace with Normal Write once Done
    public override void _ExitTree()
    {
        ProjectCache.Instance.ForceWrite();
        SettingsCache.Instance.ForceWrite();
        TagCache.Instance.ForceWrite();
        VersionCache.Instance.ForceWrite();
        TemplateCache.Instance.ForceWrite();
    }

    public override void _Ready()
    {
        _godotHUB = GetNode<LinkButton>("%GodotHUB");

        _godotHUB.Text = ProjectSettings.GetSetting("application/config/version") + " | GodotHUB Github";
    }
}
