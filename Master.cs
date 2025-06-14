using DataContainer.DatabaseSys.Databases.VersionDatabase;
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
        VersionDatabase.Initialize(userDirectory);
        TagCache.Initialize(userDirectory);
        ProjectCache.Initialize(userDirectory);
        TemplateCache.Initialize(userDirectory);

    }

    // TEMP: Replace with Normal Write once Done
    public override void _ExitTree()
    {
        ProjectCache.Instance.ForceWrite();
        SettingsCache.Instance.ForceWrite();
        TagCache.Instance.ForceWrite();
        VersionDatabase.Instance.ForceWrite();
        TemplateCache.Instance.ForceWrite();

    }

    public override void _Ready()
    {
        _godotHUB = GetNode<LinkButton>("%GodotHUB");

        _godotHUB.Text = ProjectSettings.GetSetting("application/config/version") + " | GodotHUB Github";
    }
}
