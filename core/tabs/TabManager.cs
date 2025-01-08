using Godot;
using System;

public partial class TabManager : TabContainer
{
    [Export] private Projects _projectTab;
    [Export] private Templates _templateTab;
    [Export] private GodotVersions _versionsTab;
    [Export] private Software _softwareTab;
    [Export] private Settings _settingsTab;

    public override void _Ready()
    {
        //* Signals
        _projectTab.GoToVersionsRequested += OnGoToVersionsPressed;
        _softwareTab.EntryFavorited += _projectTab.UpdateQuickTools;
        _settingsTab.SettingUpdated += OnSettingUpdated;

        //* Load Data
        SettingsCache.Instance.LoadData();
        _projectTab.LoadData();
        _templateTab.LoadData();
        _versionsTab.LoadData();
        _softwareTab.LoadData();
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

    private void OnGoToVersionsPressed()
    {
        while (GetTabTitle(CurrentTab) != _versionsTab.Name)
        {
            SelectNextAvailable();
        }
    }
}
