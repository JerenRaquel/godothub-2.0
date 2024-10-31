using Godot;
using System;

public partial class GodotVersionConfig : InterfaceBase
{
    public const string VIEW_MODE_TAG = "view_mode";

    private CheckButton _viewModeCheckButton;

    public override void _Ready()
    {
        _viewModeCheckButton = GetNode<CheckButton>("%ViewModeCheckButton");
        _viewModeCheckButton.Toggled += OnViewModeChanged;

        base._Ready();
    }

    public override string[] GetAllSettingTags() => [VIEW_MODE_TAG];

    public override SettingsData.Data GetData(string settingTag)
    {
        if (settingTag != VIEW_MODE_TAG) return null;

        return new SettingsData.Data(_viewModeCheckButton.ButtonPressed);
    }

    public override void SetData(string settingTag, SettingsData.Data data)
    {
        if (settingTag != VIEW_MODE_TAG) return;

        _viewModeCheckButton.SetPressedNoSignal(data);
    }

    private void OnViewModeChanged(bool toggled)
    {
        if (toggled)
            _viewModeCheckButton.Text = "Display As Cards";
        else
            _viewModeCheckButton.Text = "Display As List";

        EmitSignal(SignalName.SettingChanged, VIEW_MODE_TAG);
    }
}
