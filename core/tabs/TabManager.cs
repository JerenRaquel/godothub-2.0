using Godot;
using System;

public partial class TabManager : TabContainer
{
    [Export] private Projects _projectTab;
    // TODO: Templates
    [Export] private GodotVersions _versionsTab;
    // TODO: Software
    [Export] private Settings _settingsTab;

    public override void _Ready()
    {
        _settingsTab.SettingUpdated += OnSettingUpdated;

        SettingsCache.Instance.LoadData();
        _projectTab.LoadData();
        _versionsTab.LoadData();
        _settingsTab.LoadData();
    }

    private void OnSettingUpdated(string key)
    {
        switch (key)
        {
            case "Application/Config/abs_proj_path/BOOL":
                _projectTab.UpdatePaths();
                break;


            default: break;
        }
    }
}
