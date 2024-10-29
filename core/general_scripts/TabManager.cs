using Godot;
using System;

public partial class TabManager : TabContainer
{
    private Projects _projectTab;
    // TODO: Templates
    private GodotVersions _versionsTab;
    // TODO: Software
    private Settings _settingsTab;

    public override void _Ready()
    {
        _projectTab = GetNode<Projects>("%Projects");

        _versionsTab = GetNode<GodotVersions>("%Godot Versions");

        _settingsTab = GetNode<Settings>("%Settings");
        _settingsTab.SettingUpdated += OnSettingUpdated;
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
