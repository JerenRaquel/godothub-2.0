using Godot;
using System;

public partial class Config : InterfaceBase
{
    private CheckButton _fullExecPathCheckButton;
    private CheckButton _absProjPathCheckButton;
    private OptionButton _HUBBehaviorOptionButton;

    public override void _ExitTree()
    {
        base._ExitTree();
        _fullExecPathCheckButton.Toggled -= FullExecPathNotify;
        _absProjPathCheckButton.Toggled -= AbsProjPathNotify;
        _HUBBehaviorOptionButton.ItemSelected -= HUBBehaviorPathNotify;
    }

    public override void _Ready()
    {
        _fullExecPathCheckButton = GetNode<CheckButton>("%FullExecPathCheckButton");
        _absProjPathCheckButton = GetNode<CheckButton>("%AbsProjPathCheckButton");
        _HUBBehaviorOptionButton = GetNode<OptionButton>("%HUBBehaviorOptionButton");

        _fullExecPathCheckButton.Toggled += FullExecPathNotify;
        _absProjPathCheckButton.Toggled += AbsProjPathNotify;
        _HUBBehaviorOptionButton.ItemSelected += HUBBehaviorPathNotify;

    }

    public override string[] GetAllSettingTags() => ["full_exec_path", "abs_proj_path", "HUB_behavior"];

    public override bool GetSettingBool(string settingTag)
    {
        return settingTag switch
        {
            "full_exec_path" => _fullExecPathCheckButton.ButtonPressed,
            "abs_proj_path" => _absProjPathCheckButton.ButtonPressed,
            _ => false
        };
    }

    public override int GetSettingOption(string settingTag)
    {
        return settingTag switch
        {
            "HUB_behavior" => _HUBBehaviorOptionButton.Selected,
            _ => 0
        };
    }

    private void FullExecPathNotify(bool _) => EmitSignal(SignalName.SettingChanged, "full_exec_path", (int)SettingType.BOOL);
    private void AbsProjPathNotify(bool _) => EmitSignal(SignalName.SettingChanged, "abs_proj_path", (int)SettingType.BOOL);
    private void HUBBehaviorPathNotify(long _) => EmitSignal(SignalName.SettingChanged, "HUB_behavior", (int)SettingType.OPTION);
}
