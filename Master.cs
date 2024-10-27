using Godot;
using System;

public partial class Master : Node
{
    private LinkButton _godotHUB;

    public override void _EnterTree()
    {
        string userDirectory = ProjectSettings.GlobalizePath("user://");
        ProjectCache.Initialize(userDirectory);
        SettingsCache.Initialize(userDirectory);
    }

    // TEMP: Replace with Normal Write once Done
    public override void _ExitTree()
    {
        ProjectCache.Instance.ForceWrite();
        SettingsCache.Instance.ForceWrite();
    }

    public override void _Ready()
    {
        _godotHUB = GetNode<LinkButton>("%GodotHUB");

        _godotHUB.Text = ProjectSettings.GetSetting("application/config/version") + " | GodotHUB Github";
    }
}
