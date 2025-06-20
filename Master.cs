using DataContainer.DatabaseSys.Databases.SettingDatabase;
using DataContainer.DatabaseSys.Databases.TagDatabase;
using DataContainer.DatabaseSys.Databases.VersionDatabase;
using Godot;

public partial class Master : Node
{
    private LinkButton _godotHUB;

    public override void _EnterTree()
    {
        OSAPI.Initialize();
        string userDirectory = ProjectSettings.GlobalizePath("user://");

        SettingsDatabase.Initialize(userDirectory);
        VersionDatabase.Initialize(userDirectory);
        TagCache.Initialize(userDirectory); // TEMP
        TagDatabase.Initialize(userDirectory);
        ProjectCache.Initialize(userDirectory);
        TemplateCache.Initialize(userDirectory);
    }

    // TEMP: Replace with Normal Write once Done
    public override void _ExitTree()
    {
        ProjectCache.Instance.ForceWrite();
        SettingsDatabase.Instance.WriteData();
        TagCache.Instance.ForceWrite(); // TEMP
        TagDatabase.Instance.WriteData();
        VersionDatabase.Instance.WriteData();
        TemplateCache.Instance.ForceWrite();
    }

    public override void _Ready()
    {
        _godotHUB = GetNode<LinkButton>("%GodotHUB");

        _godotHUB.Text = ProjectSettings.GetSetting("application/config/version") + " | GodotHUB Github";
    }
}
