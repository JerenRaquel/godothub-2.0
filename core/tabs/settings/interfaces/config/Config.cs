using Godot;
using System;

public partial class Config : InterfaceBase
{
    private CheckButton _fullExecPathCheckButton;
    private CheckButton _absProjPathCheckButton;
    private OptionButton _HUBBehaviorOptionButton;
    private Button _saveDataButton;

    public override void _Ready()
    {
        _fullExecPathCheckButton = GetNode<CheckButton>("%FullExecPathCheckButton");
        _absProjPathCheckButton = GetNode<CheckButton>("%AbsProjPathCheckButton");
        _HUBBehaviorOptionButton = GetNode<OptionButton>("%HUBBehaviorOptionButton");
        _saveDataButton = GetNode<Button>("%SaveDataButton");

        _fullExecPathCheckButton.Toggled += _ => EmitSignal(SignalName.SettingChanged, "full_exec_path");
        _absProjPathCheckButton.Toggled += _ => EmitSignal(SignalName.SettingChanged, "abs_proj_path");
        _HUBBehaviorOptionButton.ItemSelected += _ => EmitSignal(SignalName.SettingChanged, "HUB_behavior");
        _saveDataButton.Pressed += () => OSAPI.OpenFolder(ProjectSettings.GlobalizePath("user://"));

    }

    public override string[] GetAllSettingTags() => ["full_exec_path", "abs_proj_path", "HUB_behavior"];

    public override SettingsData.Data GetData(string settingTag)
    {
        return settingTag switch
        {
            "full_exec_path" => _fullExecPathCheckButton.ButtonPressed,
            "abs_proj_path" => _absProjPathCheckButton.ButtonPressed,
            "HUB_behavior" => _HUBBehaviorOptionButton.Selected,
            _ => new()
        };
    }

    public override void SetData(string settingTag, SettingsData.Data data)
    {
        switch (settingTag)
        {
            case "full_exec_path": _fullExecPathCheckButton.SetPressedNoSignal(data); break;
            case "abs_proj_path": _absProjPathCheckButton.SetPressedNoSignal(data); break;
            case "HUB_behavior": _HUBBehaviorOptionButton.Select(data); break;
            default: break;
        }
    }
}
