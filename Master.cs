using DataContainer.DatabaseSys.Databases.ProjectDatabase;
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
        TagDatabase.Initialize(userDirectory);
        ProjectCache.Initialize(userDirectory); // TODO: Remove once ProjectDatabase is integrated
        ProjectDatabase.Initialize(userDirectory);
        TemplateCache.Initialize(userDirectory);
    }

    // TEMP: Replace with Normal Write once Done
    public override void _ExitTree()
    {
        ProjectCache.Instance.ForceWrite(); // TODO: Remove once ProjectDatabase is integrated
        ProjectDatabase.Instance.WriteData();
        SettingsDatabase.Instance.WriteData();
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
